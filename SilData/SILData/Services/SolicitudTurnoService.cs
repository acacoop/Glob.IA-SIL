using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Shared.ClassShared.BusinessExceptions;
using SILData.Controllers;
using SILData.DataAccess;
using SILData.Model;
using SILData.Model.SolicitudTurno;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using SILData.SilDataExceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using NuGet.Packaging;
using Shared.ClassShared.Interfaces;

namespace SILData.Services
{
  public class SolicitudTurnoService : ISolicitudTurnoService
  {
    private readonly ISolicitudTurnoStore _solicitudTurnoStore;
    private readonly ICuposDisponiblesService _CuposDisponiblesService;
    private IGeographicalAereaService _geographicalAereaService { get; set; }
    private readonly ILogger<SolicitudTurnoService> _logger;
    private readonly ISILCuposStore _silCuposStore;
    private readonly ACA.Matching.Engine.IMatchingEngine _matchingEngine;
    private readonly IZonaGeograficaResolver _zonaGeograficaResolver;
    private string? _connectionString;
    public SolicitudTurnoService(
      ISolicitudTurnoStore solicitudTurnoStore,
      ICuposDisponiblesService cuposDisponiblesService,
      IGeographicalAereaService geographicalAereaService,
      ILogger<SolicitudTurnoService> logger,
      IConfiguration configuration,
      ISILCuposStore silCuposStore,
      ACA.Matching.Engine.IMatchingEngine matchingEngine,
      IZonaGeograficaResolver zonaGeograficaResolver)
    {
      _connectionString = configuration.GetConnectionString("SilConnection");
      _solicitudTurnoStore = solicitudTurnoStore;
      _CuposDisponiblesService = cuposDisponiblesService;
      _geographicalAereaService = geographicalAereaService;
      _logger = logger;
      _silCuposStore = silCuposStore;
      _matchingEngine = matchingEngine;
      _zonaGeograficaResolver = zonaGeograficaResolver;
    }

    /// <summary>
    /// Construye un <see cref="CupoCatalogoLookup"/> en el momento de uso.
    /// Se instancia ad-hoc (no se inyecta por DI) porque depende de la
    /// connection string que este servicio ya tiene capturada. Mantenerlo
    /// afuera de este servicio preserva la cohesión: este archivo se
    /// ocupa del matching, no del catálogo de cuentas.
    /// </summary>
    private CupoCatalogoLookup BuildCupoCatalogoLookup()
    {
      using var loggerFactory = LoggerFactory.Create(b => { });
      return new CupoCatalogoLookup(
        loggerFactory.CreateLogger<CupoCatalogoLookup>(),
        _connectionString);
    }

    public async Task AddRequest(SolicitudTurnoCreate solicitudTurnoCreate)
    {
      IList<SolicitudTurno> inserciones = new List<SolicitudTurno>();
      IList<SolicitudTurno> actualizaciones = new List<SolicitudTurno>();
      IList<long> idsEliminarDuplicados = new List<long>();
      long disponibleHoy = 0;

      try
      {
        if (solicitudTurnoCreate.SolicitudesDias is not null && solicitudTurnoCreate.SolicitudesDias.Any() && solicitudTurnoCreate.SolicitudesDias.Sum(x => x.Cantidad) > 0)
        {
          if (solicitudTurnoCreate.TipoDestino == TipoDestino.Destino)
            throw new SilDataException(
                "Por el momento no se encuentra disponible la solicitud de turnos por destino. Intente con [Tipo Destino = 0].",
                StatusCodes.Status409Conflict);

          List<SolicitudTurnoDiaCreate> diasSolicitud = solicitudTurnoCreate.SolicitudesDias
            .OrderBy(x => x.FechaSolicitado)
            .ToList();

          DateTime fechaMin = diasSolicitud.Min(x => x.FechaSolicitado);
          DateTime fechaMax = diasSolicitud.Max(x => x.FechaSolicitado);

          SolicitudTurnosFilter filter = new SolicitudTurnosFilter
          {
            CuentaVendedor = solicitudTurnoCreate.CuentaVendedor,
            CuentaComprador = solicitudTurnoCreate.CuentaComprador,
            CuentaDestino = solicitudTurnoCreate.CuentaDestino,
            Desde = fechaMin,
            Hasta = fechaMax
          };

          List<SolicitudTurnoView> existentes = (await GetByFilterAsync(filter))
            .Where(x => x.CodigoGrano == solicitudTurnoCreate.CodigoGrano)
            .ToList();

          long cantidadExistenteTotal = 0;
          foreach (SolicitudTurnoDiaCreate dia in diasSolicitud)
          {
            SolicitudTurnoView? existente = BuscarSolicitudPendiente(existentes, solicitudTurnoCreate, dia.FechaSolicitado);
            if (existente is not null)
            {
              int cupoComprometido = existente.CantidadAceptada + existente.CantidadRechazada;
              if (dia.Cantidad < cupoComprometido)
              {
                throw new SilDataException(
                  $"La cantidad solicitada ({dia.Cantidad}) para el día {dia.FechaSolicitado:dd/MM/yyyy} es inferior a los cupos ya aceptados y rechazados ({cupoComprometido}). No se puede reducir por debajo de los cupos comprometidos.",
                  StatusCodes.Status409Conflict);
              }
              cantidadExistenteTotal += existente.Cantidad > 0 ? existente.Cantidad : 1;
            }
          }

          long solicitudesTotales = diasSolicitud.Sum(x => x.Cantidad);
          long incrementoNeto = solicitudesTotales - cantidadExistenteTotal;

          disponibleHoy = await _CuposDisponiblesService.GetCantidadDisponibles(
            solicitudTurnoCreate.CuentaVendedor,
            DateTime.Now.Date,
            solicitudTurnoCreate.CuentaComprador ?? 0,
            solicitudTurnoCreate.CodigoGrano,
            (solicitudTurnoCreate.TipoDestino == TipoDestino.ZonaPortuaria) ? (solicitudTurnoCreate.CuentaDestino ?? 0) : 0);

          if (solicitudTurnoCreate.EsFuturo)
          {
            long disponibleFuturo = await _CuposDisponiblesService.GetCantidadDisponibles(
              solicitudTurnoCreate.CuentaVendedor,
              solicitudTurnoCreate.FechaSolicitud,
              solicitudTurnoCreate.CuentaComprador ?? 0,
              solicitudTurnoCreate.CodigoGrano,
              (solicitudTurnoCreate.TipoDestino == TipoDestino.ZonaPortuaria) ? (solicitudTurnoCreate.CuentaDestino ?? 0) : 0);

            if (incrementoNeto > 0 && disponibleFuturo < incrementoNeto)
              throw new SilDataException(
                $"Demasiadas solicitudes: Usted puede solicitar un máximo de {disponibleFuturo} turnos a la fecha {solicitudTurnoCreate.FechaSolicitud:dd/MM/yyyy}.",
                StatusCodes.Status409Conflict);
          }
          else if (incrementoNeto > 0 && disponibleHoy < incrementoNeto)
          {
            throw new SilDataException(
              $"Demasiadas solicitudes: Usted puede solicitar un máximo de {disponibleHoy} turnos al día de hoy.",
              StatusCodes.Status409Conflict);
          }

          foreach (SolicitudTurnoDiaCreate solicitudTurnoDia in diasSolicitud)
          {
            int cantidad = solicitudTurnoDia.Cantidad;
            int cantidadHoy = (int)Math.Min(cantidad, Math.Max(disponibleHoy, 0));
            int cantidadFuturo = cantidad - cantidadHoy;

            if (cantidadFuturo > 0 && !solicitudTurnoCreate.EsFuturo)
              throw new SilDataException(
                $"Demasiadas solicitudes: Usted posee un disponible de {disponibleHoy} solicitudes el día de hoy.",
                StatusCodes.Status409Conflict);

            SolicitudTurno solicitud = new SolicitudTurno
            {
              CuentaVendedor = solicitudTurnoCreate.CuentaVendedor,
              CuentaComprador = solicitudTurnoCreate.CuentaComprador,
              CodigoGrano = solicitudTurnoCreate.CodigoGrano,
              CuentaDestino = solicitudTurnoCreate.CuentaDestino,
              FechaCreacion = DateTime.Now,
              FechaSolicitado = solicitudTurnoDia.FechaSolicitado,
              CodigoCentro = solicitudTurnoCreate.Centro,
              Observacion = solicitudTurnoCreate.Observacion,
              TipoDestino = TipoDestino.ZonaPortuaria,
              CupoId = null,
              Cantidad = cantidad,
              CantidadFuturo = cantidadFuturo,
              EsFuturo = cantidadFuturo > 0 && cantidadFuturo == cantidad
            };

            List<SolicitudTurnoView> coincidencias = existentes
              .Where(x => CoincideSolicitudPendiente(x, solicitudTurnoCreate, solicitudTurnoDia.FechaSolicitado))
              .OrderByDescending(x => x.Id)
              .ToList();

            if (coincidencias.Any())
            {
              SolicitudTurnoView principal = coincidencias.First();
              solicitud.Id = principal.Id;
              actualizaciones.Add(solicitud);

              idsEliminarDuplicados.AddRange(coincidencias.Skip(1).Select(x => x.Id));
            }
            else
            {
              inserciones.Add(solicitud);
            }

            disponibleHoy -= cantidadHoy;
          }

          await _solicitudTurnoStore.SaveAsync(
            inserciones,
            actualizaciones,
            idsEliminarDuplicados.Distinct().Where(id => !actualizaciones.Any(a => a.Id == id)).ToList());
        }
        else
        {
          throw new SilDataException("No hay solicitudes válidas para agregar.", StatusCodes.Status400BadRequest);
        }

      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en AddRequest. Vendedor: {Vendedor}", solicitudTurnoCreate.CuentaVendedor);
        throw new Exception("Error inesperado al agregar la solicitud.", ex);
      }
    }

    private static SolicitudTurnoView? BuscarSolicitudPendiente(
      IEnumerable<SolicitudTurnoView> existentes,
      SolicitudTurnoCreate solicitudTurnoCreate,
      DateTime fechaSolicitado)
    {
      return existentes
        .Where(x => CoincideSolicitudPendiente(x, solicitudTurnoCreate, fechaSolicitado))
        .OrderByDescending(x => x.Id)
        .FirstOrDefault();
    }

    private static bool CoincideSolicitudPendiente(
      SolicitudTurnoView existente,
      SolicitudTurnoCreate solicitudTurnoCreate,
      DateTime fechaSolicitado)
    {
      return existente.CuentaVendedor == solicitudTurnoCreate.CuentaVendedor
        && (existente.CuentaComprador ?? 0) == (solicitudTurnoCreate.CuentaComprador ?? 0)
        && (existente.CuentaDestino ?? 0) == (solicitudTurnoCreate.CuentaDestino ?? 0)
        && existente.CodigoGrano == solicitudTurnoCreate.CodigoGrano
        && string.Equals(existente.CodigoCentro, solicitudTurnoCreate.Centro, StringComparison.OrdinalIgnoreCase)
        && existente.FechaSolicitado.Date == fechaSolicitado.Date
        && (existente.TipoDestino ?? TipoDestino.ZonaPortuaria) == solicitudTurnoCreate.TipoDestino;
    }
    public async Task UpdateRequest(SolicitudTurnoCreate solicitudTurnoCreate)
    {
      try
      {
        SolicitudTurnoCreate solicitudTurnoForCreate = solicitudTurnoCreate.Clone();
        solicitudTurnoForCreate.SolicitudesDias = solicitudTurnoCreate.SolicitudesDias.Where(x => x.Cantidad > 0).ToList();


        SolicitudTurnoCreate solicitudTurnoForDelete = solicitudTurnoCreate.Clone();
        // Cantidad == 0 → eliminar todos los cupos pendientes del día.
        // Cantidad < 0 → eliminar |Cantidad| cupos (delta explícito).
        solicitudTurnoForDelete.SolicitudesDias = solicitudTurnoCreate.SolicitudesDias.Where(x => x.Cantidad <= 0).ToList();

        // Ejecutar tareas secuencialmente para manejar errores correctamente
        if (solicitudTurnoForCreate.SolicitudesDias.Any())
        {
          await AddRequest(solicitudTurnoForCreate);
        }

        if (solicitudTurnoForDelete.SolicitudesDias.Any())
        {
          await DeleteRequest(solicitudTurnoForDelete);
        }
      }
      catch (SilDataException sde)
      {
        throw; // Mantiene el stack trace original
      }
      catch (InvalidOperationException ex)
      {
        _logger.LogError(ex, "Operación inválida en UpdateRequest. Vendedor: {Vendedor}", solicitudTurnoCreate.CuentaVendedor);
        throw new Exception("Operación inválida al querer actualizar los datos.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en UpdateRequest. Vendedor: {Vendedor}", solicitudTurnoCreate.CuentaVendedor);
        throw new Exception("Ocurrió un error inesperado al actualizar la solicitud de turno.", ex);
      }
    }
    public async Task DeleteRequest(SolicitudTurnoCreate solicitudTurnoForDelete)
    {
      try
      {
        if (solicitudTurnoForDelete.SolicitudesDias is not null && solicitudTurnoForDelete.SolicitudesDias.Any(x => x.Cantidad <= 0))
        {
          DateTime min = solicitudTurnoForDelete.SolicitudesDias.Where(x => x.Cantidad <= 0).Min(x => x.FechaSolicitado);
          DateTime max = solicitudTurnoForDelete.SolicitudesDias.Where(x => x.Cantidad <= 0).Max(x => x.FechaSolicitado);
          List<SolicitudTurnoDiaCreate> amountRequestForDeleteByDate = solicitudTurnoForDelete.SolicitudesDias.Where(x => x.Cantidad <= 0).ToList();

          SolicitudTurnosFilter filter = new SolicitudTurnosFilter
          {
            CuentaVendedor = solicitudTurnoForDelete.CuentaVendedor,
            CuentaComprador = solicitudTurnoForDelete.CuentaComprador,
            CuentaDestino = solicitudTurnoForDelete.CuentaDestino,
            Desde = min,
            Hasta = max
          };
          IEnumerable<SolicitudTurnoView> ShiftRequest = await GetByFilterAsync(filter);
          ShiftRequest = ShiftRequest.Where(x => x.CodigoGrano == solicitudTurnoForDelete.CodigoGrano);

          List<Task> misDeletes = new List<Task>();
          if (ShiftRequest is not null && ShiftRequest.Any(x => x.EsPendiente))
          {
            List<SolicitudTurnoView> requestPending = ShiftRequest.Where(x => x.EsPendiente).ToList();
            foreach (SolicitudTurnoDiaCreate sol in amountRequestForDeleteByDate)
            {
              int cantidadAEliminar = Math.Abs(sol.Cantidad);
              List<SolicitudTurnoView> pendientesDelDia = requestPending
                .Where(x => x.FechaSolicitado == sol.FechaSolicitado)
                .OrderByDescending(x => x.Id)
                .ToList();

              int cuposPendientes = pendientesDelDia.Sum(x => x.Cantidad > 0 ? x.Cantidad : 1);
              // Cantidad == 0 → "eliminar todos los cupos pendientes del día".
              // Se interpreta como la cantidad total pendiente, no como 0.
              if (sol.Cantidad == 0)
                cantidadAEliminar = cuposPendientes;

              if (cuposPendientes < cantidadAEliminar)
              {
                throw new SilDataException(
                  $"Las solicitudes del día {sol.FechaSolicitado:dd/MM/yyyy} que desea cancelar ya fueron procesadas por el equipo de logística. Se descarta la cancelación de este día.",
                  StatusCodes.Status409Conflict);
              }

              int restante = cantidadAEliminar;
              List<long> idsAEliminar = new List<long>();
              Dictionary<long, int> cantidadesAActualizar = new Dictionary<long, int>();

              foreach (SolicitudTurnoView req in pendientesDelDia)
              {
                if (restante <= 0)
                  break;

                int cantidadRegistro = req.Cantidad > 0 ? req.Cantidad : 1;
                if (cantidadRegistro <= restante)
                {
                  idsAEliminar.Add(req.Id);
                  restante -= cantidadRegistro;
                }
                else
                {
                  cantidadesAActualizar[req.Id] = cantidadRegistro - restante;
                  restante = 0;
                }
              }

              if (idsAEliminar.Any())
                misDeletes.Add(_solicitudTurnoStore.DeleteAsync(idsAEliminar));

              foreach (KeyValuePair<long, int> actualizacion in cantidadesAActualizar)
                misDeletes.Add(_solicitudTurnoStore.UpdateCantidadAsync(actualizacion.Key, actualizacion.Value));
            }
          }
          else
          {
            throw new SilDataException(
              "Las solicitudes que desea eliminar ya fueron procesadas por el equipo de logística.",
              StatusCodes.Status409Conflict);
          }
          await Task.WhenAll(misDeletes);
        }
        else
        {
          throw new SilDataException("No hay solicitudes para eliminar.", StatusCodes.Status400BadRequest);
        }
      }
      catch (SilDataException)
      {
        throw; // Mantiene el mensaje y stack trace original
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en DeleteRequest. Vendedor: {Vendedor}", solicitudTurnoForDelete.CuentaVendedor);
        throw new Exception("Ocurrió un error inesperado al eliminar la solicitud de turno.", ex);
      }
    }
    public async Task<IEnumerable<SolicitudTurnoView>> GetAllAsync(DateTime desde, DateTime hasta)
    {
      return await _solicitudTurnoStore.GetAllAsync(desde, hasta);
    }
    public async Task<IEnumerable<SolicitudTurnoView>> GetByVendedorAsync(long cuentaVendedor)
    {
      IEnumerable<SolicitudTurnoView> result = await _solicitudTurnoStore
              .GetByVendedorAsync(cuentaVendedor, DateTime.Now.Date, DateTime.Now.Date.AddDays(7));

      // Defensive null — Dapper no debería devolver null pero por las dudas
      if (result is null)
        return Enumerable.Empty<SolicitudTurnoView>();

      try
      {
        IEnumerable<ZonaGeograficaView> zonas = await _geographicalAereaService.GetAllAsync();

        // zonas nunca debería ser null (GetAllAsync devuelve Empty), pero lo blindamos igual
        if (zonas is not null && zonas.Any())
        {
          result.ToList()
              .Where(x => x.CuentaDestino.HasValue
                       && x.CuentaDestino != 0
                       && string.IsNullOrEmpty(x.NombreDestino))
              .ToList()
              .ForEach(x =>
                  x.NombreDestino = zonas.Any(z => z.zonaGeoId == x.CuentaDestino)
                      ? zonas.First(z => z.zonaGeoId == x.CuentaDestino).Nombre
                      : "Zona Desconocida");
        }
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByVendedorAsync. Vendedor: {Vendedor}", cuentaVendedor);
      }
      return result;
    }
    public async Task<IEnumerable<SolicitudTurnoView>> GetByFilterAsync(SolicitudTurnosFilter solicitudTurnosFilter)
    {
      try
      {
        DateTime _desde = (solicitudTurnosFilter.Desde != DateTime.MinValue)
          ? solicitudTurnosFilter.Desde
          : DateTime.Now.Date;

        DateTime _hasta = (solicitudTurnosFilter.Hasta != DateTime.MinValue)
          ? solicitudTurnosFilter.Hasta
          : DateTime.Now.Date;

        return await _solicitudTurnoStore.GetByFilterAsync(solicitudTurnosFilter, _desde, _hasta);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByFilterAsync(filter). Vendedor: {Vendedor}", solicitudTurnosFilter.CuentaVendedor);
        throw new Exception("Error inesperado al consultar las solicitudes por filtro.", ex);
      }
    }

    public async Task<IEnumerable<SolicitudTurnoView>> GetByMatchesFilterAsync(MatchesSolicitudFilter filter)
    {
      if (filter is null)
        throw new SilDataException("El filtro no puede ser nulo.", StatusCodes.Status400BadRequest);

      if (filter.CodigoGrano <= 0)
        throw new SilDataException("CodigoGrano es obligatorio y debe ser positivo.", StatusCodes.Status400BadRequest);

      if (filter.Desde == DateTime.MinValue || filter.Hasta == DateTime.MinValue)
        throw new SilDataException("Desde y Hasta son obligatorios.", StatusCodes.Status400BadRequest);

      if (filter.Desde > filter.Hasta)
        throw new SilDataException("Desde no puede ser posterior a Hasta.", StatusCodes.Status400BadRequest);

      try
      {
        return await _solicitudTurnoStore.GetByMatchesFilterAsync(filter, filter.Desde, filter.Hasta);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByMatchesFilterAsync. Grano: {Grano}", filter.CodigoGrano);
        throw new Exception("Error inesperado al consultar las solicitudes para matching.", ex);
      }
    }

    public async Task<IEnumerable<SolicitudTurnoView>> GetByFilterAsync(SILSolicitudDeTurnosFilter solicitudTurnosFilter)
    {
      try
      {
        // TODO: validar si los centros son válidos contra un catálogo. 
        // TODO: agregar el filtro por estado cuando esté disponible.

        DateTime _desde = DateTime.Now.Date;
        DateTime _hasta = DateTime.Now.Date.AddDays(solicitudTurnosFilter.Dias);

        return await _solicitudTurnoStore.GetByFilterAsync(_desde, _hasta, solicitudTurnosFilter.Centros);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByFilterAsync(SILFilter). Días: {Dias}", solicitudTurnosFilter.Dias);
        throw new Exception("Error inesperado al consultar las solicitudes pendientes.", ex);
      }
    }

    public async Task<ShiftRequestAcceptResult> AcceptRequestsAsync(ShiftRequestAcceptData shiftRequestAcceptData)
    {
      if (shiftRequestAcceptData == null)
        throw new SilDataException("El cuerpo de la solicitud no puede ser nulo.", StatusCodes.Status400BadRequest);

      if (shiftRequestAcceptData.ShiftRequest == null || !shiftRequestAcceptData.ShiftRequest.Any())
        throw new SilDataException("La lista de solicitudes no puede estar vacía.", StatusCodes.Status400BadRequest);

      if (shiftRequestAcceptData.CuposToBeDistributed == null || !shiftRequestAcceptData.CuposToBeDistributed.Any())
        throw new SilDataException("La lista de cupos a distribuir no puede estar vacía.", StatusCodes.Status400BadRequest);

      // Validar que coincidan las cantidades
      int totalRequested = shiftRequestAcceptData.ShiftRequest.Sum(r => r.Cantidad > 0 ? r.Cantidad : 1);
      int totalCupos = shiftRequestAcceptData.CuposToBeDistributed.Count;

      if (totalRequested != totalCupos)
      {
        throw new SilDataException(
          $"La cantidad total de turnos solicitados ({totalRequested}) no coincide con la cantidad de cupos provistos ({totalCupos}).",
          StatusCodes.Status409Conflict);
      }

      // ── Motor de Matching: pre-evaluación (doc §5.2 + §8) ──────────────
      // Antes de gastar updates de cupos y de SOLTURNOS, validamos compatibilidad
      // semántica. Si un par (solicitud, cupo) es Incompatible, esa solicitud
      // falla entera (atomicidad por AcceptOperation: parent + splits).
      // Si es Condicional, lo registramos para que el front muestre el diálogo.
      var ctxZona = await _zonaGeograficaResolver.ResolverAsync(
        shiftRequestAcceptData.CuposToBeDistributed.Select(c => c.Id));

      var motivoFallaPorSolicitudId = new Dictionary<long, string>();
      var tipoMatchPorSolicitudId = new Dictionary<long, string>();

      {
        int idx = 0;
        foreach (var req in shiftRequestAcceptData.ShiftRequest)
        {
          int cant = req.Cantidad > 0 ? req.Cantidad : 1;
          var solicitudMatching = SolicitudMatchingAdapter.From(req);

          for (int i = 0; i < cant; i++)
          {
            var cupo = shiftRequestAcceptData.CuposToBeDistributed[idx++];
            var match = _matchingEngine.Evaluar(cupo, solicitudMatching, ctxZona);

            if (!match.Compatible)
            {
              if (!motivoFallaPorSolicitudId.ContainsKey(req.Id))
                motivoFallaPorSolicitudId[req.Id] = match.RazonIncompatibilidad ?? "Match incompatible";
            }
            else
            {
              if (!tipoMatchPorSolicitudId.ContainsKey(req.Id) && match.Tipo.HasValue)
                tipoMatchPorSolicitudId[req.Id] = match.Tipo.Value.ToString();
            }
          }
        }
      }

      var updatedCupos = new List<Domain.Entities.Externo.Cupo>();
      var operations = new List<AcceptOperation>();

      // Cache de cantidad pedida original por solicitud. La cantidad en `req.Cantidad`
      // representa cuánto quiere asignar el operador AHORA; necesitamos la cantidad
      // pedida ORIGINAL (de SOLTURNOS.CANTIDAD antes del Accept) para calcular
      // los pendientes en un Accept parcial.
      var cantidadOriginalPorSolicitudId = new Dictionary<long, int>();

      int cupoIndex = 0;
      foreach (var req in shiftRequestAcceptData.ShiftRequest)
      {
        // Si la solicitud tiene al menos un par Incompatible, falla entera.
        if (motivoFallaPorSolicitudId.ContainsKey(req.Id))
        {
          int cantSkip = req.Cantidad > 0 ? req.Cantidad : 1;
          cupoIndex += cantSkip; // consumir cupos sin procesarlos
          continue;
        }

        int cant = req.Cantidad > 0 ? req.Cantidad : 1;

        // Leer cantidad pedida ORIGINAL (de SOLTURNOS.CANTIDAD, FROZEN en este
        // modelo). Si la consulta falla (solicitud recién eliminada, etc.),
        // caemos a la cantidad del payload como defensa.
        int cantOriginal = cant;
        try
        {
          var solBd = await _solicitudTurnoStore.GetByIdAsync(req.Id);
          if (solBd != null && solBd.Cantidad > 0)
            cantOriginal = solBd.Cantidad;
        }
        catch
        {
          // defensa: usar cantidad del payload
        }
        cantidadOriginalPorSolicitudId[req.Id] = cantOriginal;

        // Validación: el operador no puede asignar MÁS de lo que pidió.
        if (cant > cantOriginal)
        {
          throw new SilDataException(
            $"La cantidad a asignar ({cant}) supera la cantidad pedida original ({cantOriginal}) para la solicitud {req.Id}.",
            StatusCodes.Status400BadRequest);
        }

        // ── Modelo "Detalle Acumulativo" — sin parent + splits ────────────
        // En el modelo nuevo, SOLTURNOS siempre es 1 fila por solicitud y
        // Cantidad/CantidadFuturo/EsFuturo están FROZEN. El acumulador
        // (CantidadAceptada o CantidadFuturoAceptada) y el detalle se
        // actualizan por separado en el store vía AcceptRequestsAsync.
        var cuposAsignadosOp = new List<long>(cant);

        for (int i = 0; i < cant; i++)
        {
          var cupoOriginal = shiftRequestAcceptData.CuposToBeDistributed[cupoIndex++];

          // Clonar el cupo antes de mutarlo: si el store no persiste por algún
          // motivo, no contaminamos la referencia del caller. La copia es
          // superficial (Clone() en Cupo.cs): alcanza para propiedades
          // value-type y strings, que es lo que mutamos.
          var cupo = cupoOriginal.Clone();

          // Completar la información del cupo a partir de la solicitud.
          // Esto materializa el cupo en CUPOSCORRE como OTORGADO con los
          // metadatos de la solicitud (Idéntico al modelo anterior).
          cupo.CodVendSIL = req.CuentaVendedor.ToString();
          cupo.CodCompSIL = req.CuentaComprador?.ToString();
          cupo.CodGrano = req.CodigoGrano.ToString();
          cupo.CodDestino = req.CuentaDestino?.ToString();
          cupo.Fecha = req.FechaSolicitado;
          cupo.CentroCupo = req.CodigoCentro;
          cupo.EstadoSIL = (short)EstadoCupoSIL.Otorgado;
          cupo.FechaInformadoSIL = DateTime.Now;

          updatedCupos.Add(cupo);
          cuposAsignadosOp.Add(cupo.Id);
        }

        operations.Add(new AcceptOperation
        {
          SolicitudId = req.Id,
          CuposAsignados = cuposAsignadosOp,
          SolicitudEsFuturo = req.EsFuturo
        });
      }

      await _silCuposStore.UpdateCuposDistributionAsync(updatedCupos);

      // El store aplica la concurrencia optimista y devuelve qué operaciones
      // prosperaron y cuáles fallaron por conflicto (otro operador ya actuó).
      IList<AcceptOperationResult> storeResults = await _solicitudTurnoStore.AcceptRequestsAsync(operations);

      var resultado = new ShiftRequestAcceptResult
      {
        TotalOperaciones = storeResults.Count
      };

      // Acumular contadores de cantidad parcial para reportar al frontend.
      int totalSolicitado = 0;
      int totalAsignado = 0;
      foreach (var kv in cantidadOriginalPorSolicitudId)
      {
        totalSolicitado += kv.Value;
        // Si la operación prosperó, se asignaron cant cupos; si no, 0.
        var op = operations.FirstOrDefault(o => o.SolicitudId == kv.Key);
        if (op != null && storeResults.Any(r => r.SolicitudId == kv.Key && r.Exitoso))
        {
          int cantAsign = op.CuposAsignados?.Count ?? 0;
          totalAsignado += cantAsign;
        }
      }
      resultado.CantidadSolicitadaTotal = totalSolicitado;
      resultado.CantidadAsignadaEnEsteAccept = totalAsignado;
      resultado.CantidadPendienteRestante = Math.Max(0, totalSolicitado - totalAsignado);

      foreach (AcceptOperationResult sr in storeResults)
      {
        if (sr.Exitoso)
        {
          resultado.Asignados.Add(new ShiftRequestAssignedItem
          {
            SolicitudId = sr.SolicitudId,
            CupoAsignadoId = sr.CupoAsignadoId ?? 0,
            CuposAsignados = sr.CuposAsignados ?? new List<long>(),
            TipoMatch = tipoMatchPorSolicitudId.TryGetValue(sr.SolicitudId, out var tipo) ? tipo : null
          });

          // Notificación al solicitante: por ahora registramos en log (audit
          // §7.2 + §3.1.5). El servicio de notificaciones de la plataforma
          // SIL se integrará en una iteración posterior.
          _logger.LogInformation(
            "Notificación de asignación enviada para solicitud {Id}. Cupos aceptados: {N}.",
            sr.SolicitudId, sr.CuposAsignados?.Count ?? 0);
        }
        else
        {
          resultado.Fallos.Add(new ShiftRequestAcceptFailure
          {
            SolicitudId = sr.SolicitudId,
            Motivo = sr.MotivoFalla ?? "Conflicto de concurrencia."
          });
        }
      }

      resultado.TotalAsignadas = resultado.Asignados.Count;
      resultado.TotalConflictos = resultado.Fallos.Count;

      // Agregar los Fallos por incompatibilidad del motor de matching.
      // Estos se reportan ANTES de los conflictos de concurrencia del store.
      foreach (var kv in motivoFallaPorSolicitudId)
      {
        resultado.Fallos.Add(new ShiftRequestAcceptFailure
        {
          SolicitudId = kv.Key,
          Motivo = kv.Value
        });
      }
      resultado.TotalConflictos = resultado.Fallos.Count;

      // Si TODAS las operaciones fallaron, elevamos 409 para que el cliente
      // pueda distinguir "ningún rechazo aplicado" de "rechazo parcial" (mismo
      // patrón que el rechazo masivo).
      if (resultado.TodosFallaron)
      {
        throw new SilDataException(
          "Ninguna solicitud pudo asignarse: todas fueron procesadas previamente por otro operador.",
          StatusCodes.Status409Conflict);
      }

      _logger.LogInformation(
        "AcceptRequestsAsync: {Asignadas} solicitudes asignadas, {Conflictos} con conflicto de concurrencia.",
        resultado.TotalAsignadas, resultado.TotalConflictos);

      return resultado;
    }

    // ====================================================================
    // Helpers para MVC — soporte del flujo de Pantalla 2
    // ====================================================================

    /// <summary>
    /// Devuelve la entidad <see cref="SolicitudTurno"/> por id. Usado por el
    /// MVC (<c>AcceptPayloadBuilder</c>) para armar el payload de Accept sin
    /// duplicar estado en el cliente.
    /// </summary>
    /// <param name="id">Id de la solicitud. Si es ≤ 0, devuelve <c>null</c>.</param>
    public async Task<SolicitudTurno?> GetByIdAsync(long id)
    {
      if (id <= 0) return null;
      return await _solicitudTurnoStore.GetByIdAsync(id);
    }

    /// <summary>
    /// Devuelve los cupos completos (entidad <see cref="Domain.Entities.Externo.Cupo"/>)
    /// cuya PK esté en la lista. Wrapper sobre <see cref="ISILCuposStore.GetCuposByIdsAsync"/>
    /// para que el MVC no toque stores directamente.
    /// </summary>
    /// <param name="ids">Ids de cupos. Vacía / nula → lista vacía.</param>
    public async Task<List<Domain.Entities.Externo.Cupo>> GetCuposByIdsAsync(List<long> ids)
    {
      var cupos = await _silCuposStore.GetCuposByIdsAsync(ids ?? new List<long>());
      return cupos?.ToList() ?? new List<Domain.Entities.Externo.Cupo>();
    }

    /// <summary>
    /// Devuelve el resumen de aceptación para una solicitud, calculado desde
    /// los acumuladores de <c>SOLTURNOS</c>. Útil para que la UI muestre
    /// cuántos cupos fueron asignados, cuántos quedaron pendientes y cuántos
    /// rechazados.
    /// </summary>
    /// <param name="solicitudId">Id de la solicitud.</param>
    public async Task<DetalleEstadoResumen> GetDetalleResumenAsync(long solicitudId)
    {
      if (solicitudId <= 0)
        return new DetalleEstadoResumen();
      return await _solicitudTurnoStore.GetDetalleResumenAsync(solicitudId);
    }

    /// <summary>
    /// Devuelve un mapa <c>solicitudId → List&lt;cupoId&gt;</c> con los cupos
    /// ya aceptados para cada solicitud, leyendo de <c>SOLTURNOS_DETALLE</c>
    /// (cada fila es una aceptación). Lo usa Pantalla 1 (grilla) y Pantalla 2
    /// para mostrar al operador cu&aacute;les cupos ya fueron otorgados (y
    /// descontarlos del conteo de matches del motor en la columna
    /// "Cupos compatibles").
    /// </summary>
    /// <param name="solicitudIds">Ids de solicitudes a consultar.</param>
    public async Task<Dictionary<long, List<long>>> GetCuposAceptadosPorSolicitudesAsync(
      IEnumerable<long> solicitudIds)
    {
      if (solicitudIds == null)
        return new Dictionary<long, List<long>>();
      return await _solicitudTurnoStore.GetCuposAceptadosPorSolicitudesAsync(solicitudIds);
    }

    public async Task<ShiftRequestRejectResult> RejectRequestsAsync(ShiftRequestRejectData shiftRequestRejectData)
    {
      // ── Validaciones de entrada ────────────────────────────────────────
      if (shiftRequestRejectData == null)
        throw new SilDataException("El cuerpo de la solicitud no puede ser nulo.", StatusCodes.Status400BadRequest);

      if (shiftRequestRejectData.SolicitudIds == null || !shiftRequestRejectData.SolicitudIds.Any())
        throw new SilDataException("La lista de solicitudes a rechazar no puede estar vacía.", StatusCodes.Status400BadRequest);

      // Defensa en profundidad: filtrar IDs no positivos en el servicio.
      var idsValidos = shiftRequestRejectData.SolicitudIds.Where(id => id > 0).Distinct().ToList();
      if (idsValidos.Count == 0)
        throw new SilDataException("Los IDs de solicitudes proporcionados no son válidos.", StatusCodes.Status400BadRequest);

      try
      {
        // El store aplica concurrencia optimista con la guardia
        // "CantidadAceptada + CantidadRechazada < Cantidad". Si el rowcount
        // por ID es 0, otro operador ya actuó (asignó o rechazó).
        IList<(long SolicitudId, bool Aceptado)> resultadosStore =
          await _solicitudTurnoStore.RejectRequestsAsync(idsValidos);

        var resultado = new ShiftRequestRejectResult
        {
          TotalProcesados = resultadosStore.Count
        };

        foreach (var (solicitudId, aceptado) in resultadosStore)
        {
          if (aceptado)
          {
            resultado.Rechazados.Add(solicitudId);
          }
          else
          {
            resultado.Fallos.Add(new ShiftRequestRejectFailure
            {
              SolicitudId = solicitudId,
              Motivo = "La solicitud ya no está en estado Pendiente (fue asignada o rechazada por otro operador)."
            });
          }
        }

        resultado.TotalRechazados = resultado.Rechazados.Count;
        resultado.TotalFallidos = resultado.Fallos.Count;

        // ── Notificación a los solicitantes afectados ─────────────────────
        // Las notificaciones se envían únicamente para los rechazos efectivos.
        // Por ahora registramos en log; el servicio de notificaciones de la
        // plataforma SIL se integrará en una iteración posterior.
        if (resultado.TieneExitos)
        {
          string motivoNotificacion = shiftRequestRejectData.Automatico
            ? "Rechazo automático por vencimiento del día operativo (20:00 hs)."
            : (string.IsNullOrWhiteSpace(shiftRequestRejectData.Motivo)
                ? "La solicitud fue rechazada por el operador de logística."
                : shiftRequestRejectData.Motivo);

          foreach (long id in resultado.Rechazados)
            _logger.LogInformation(
              "Notificación de rechazo enviada para solicitud {Id}. Motivo: {Motivo}. Automático: {Automatico}",
              id, motivoNotificacion, shiftRequestRejectData.Automatico);
        }

        if (resultado.TodosFallaron)
        {
          // Si ninguna solicitud pudo rechazarse, elevamos un 409 para que el
          // cliente pueda distinguir "ningún rechazo aplicado" de "rechazo parcial".
          throw new SilDataException(
            "Ninguna solicitud pudo rechazarse: todas fueron procesadas previamente por otro operador.",
            StatusCodes.Status409Conflict);
        }

        _logger.LogInformation(
          "RejectRequestsAsync: {Rechazados} rechazadas, {Fallidas} fallidas.",
          resultado.TotalRechazados, resultado.TotalFallidos);

        return resultado;
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en RejectRequestsAsync. IDs: {Ids}",
          string.Join(",", idsValidos));
        throw new Exception("Ocurrió un error inesperado al rechazar las solicitudes de turno.", ex);
      }
    }

    /// <summary>
    /// Busca todos los matches entre solicitudes pendientes y cupos disponibles
    /// para los filtros dados, ya clasificados por el motor como Directo / Parcial / Condicional.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Reglas de filtrado:
    /// </para>
    /// <list type="bullet">
    ///   <item><c>CodigoGrano</c> es el único filtro obligatorio.</item>
    ///   <item><c>CuentaComprador</c>, <c>ZonaGeograficaId</c> y <c>CuentaVendedor</c> son opcionales: si se omiten, se matchean solicitudes/cupos de cualquier valor para ese campo.</item>
    ///   <item>Solo se consideran solicitudes pendientes (derivado de los acumuladores: <c>CantidadAceptada + CantidadRechazada &lt; Cantidad</c>) y sin cupo asignado (CUPO_ID IS NULL).</item>
    ///   <item>El filtro "cupos pendientes por solicitud" del plan original es equivalente en este schema al chequeo de los acumuladores más CUPO_ID IS NULL (cada fila de SOLTURNOS es atómica; los splits se identifican por sus propios CUPO_ID).</item>
    /// </list>
    /// <para>
    /// El motor se invoca una vez por par (solicitud, cupo). El batch de zonas
    /// geográficas se pre-carga con <see cref="IZonaGeograficaResolver"/> para
    /// resolver la pertenencia cupo → zona (vía JOIN PUERTOPORZONA + ZONASGEOGRAFICAS).
    /// </para>
    /// </remarks>
    public async Task<MatchesResultDto> BuscarMatchesAsync(MatchesFilterDto filter)
    {
      if (filter is null)
        throw new SilDataException("El filtro no puede ser nulo.", StatusCodes.Status400BadRequest);

      // ── Validaciones ─────────────────────────────────────────────────
      // CodigoGrano es el único filtro obligatorio.
      if (filter.CodigoGrano <= 0)
        throw new SilDataException("CodigoGrano es obligatorio y debe ser positivo.", StatusCodes.Status400BadRequest);

      // Si llegan con valor, deben ser positivos. Si son null, se ignoran.
      if (filter.CuentaComprador is { } cc && cc <= 0)
        throw new SilDataException("Si se proporciona CuentaComprador, debe ser positivo.", StatusCodes.Status400BadRequest);

      //if (filter.ZonaGeograficaId is { } zg && zg <= 0)
      //  throw new SilDataException("Si se proporciona ZonaGeograficaId, debe ser positivo.", StatusCodes.Status400BadRequest);

      var fechaDesde = filter.FechaDesde ?? DateTime.Now.Date;
      var fechaHasta = filter.FechaHasta ?? DateTime.Now.Date.AddDays(7);
      if (fechaDesde > fechaHasta)
        throw new SilDataException("FechaDesde no puede ser posterior a FechaHasta.", StatusCodes.Status400BadRequest);

      // ── Resolución CuentaPuerto → zonas (NUEVO) ─────────────────────
      // Si llega CuentaPuerto, se resuelve a la lista de zonas a las que
      // pertenece el puerto (vía PUERTOPORZONA + ZONASGEOGRAFICAS). Esas
      // zonas se usan como filtro para las solicitudes. Si llega también
      // ZonaGeograficaId explícita, gana la resolución del puerto (más
      // específica, evita al operador tener que conocer la zona).
      // Si CuentaPuerto no mapea a ninguna zona, no hay matches posibles.
      List<long> zonasResueltas = new List<long>();
      long? zonaParaFiltroSolicitudes = filter.ZonaGeograficaId;

      if (filter.CuentaPuerto is long cp && cp > 0)
      {
        var zonasDelPuerto = await _geographicalAereaService.ResolveByDestinoAsync(cp);
        zonasResueltas = zonasDelPuerto?.Select(z => z.zonaGeoId).ToList() ?? new List<long>();

        if (zonasResueltas.Count == 0)
        {
          _logger.LogInformation(
            "BuscarMatchesAsync: CuentaPuerto={Id} no mapea a ninguna zona. Sin matches posibles.",
            cp);
          return EmptyMatchesResult(filter, fechaDesde, fechaHasta, zonasResueltas: new List<long> { cp });
        }

        // El SQL actual de GetByMatchesFilterAsync soporta un solo :destino
        // (ZonaGeograficaId), no IN. Por eso tomamos la primera zona
        // resuelta. Un puerto pertenecer a varias zonas es raro; si pasa,
        // se puede extender el store con un IN clause en una iteración
        // posterior (vía parámetro ZonasGeograficasIds[]).
        zonaParaFiltroSolicitudes = zonasResueltas[0];

        _logger.LogInformation(
          "BuscarMatchesAsync: CuentaPuerto={Id} → {Count} zonas, usando zona principal {ZonaId}.",
          cp, zonasResueltas.Count, zonaParaFiltroSolicitudes);
      }

      // ── Obtener solicitudes pendientes que matcheen los filtros base ─
      // Usamos GetByMatchesFilterAsync en lugar de GetByFilterAsync(SolicitudTurnosFilter)
      // porque este último exige CuentaVendedor y rompe el caso "todos los vendedores".
      var filterSolicitudes = new MatchesSolicitudFilter
      {
        CodigoGrano = filter.CodigoGrano,
        CuentaVendedor = filter.CuentaVendedor,
        CuentaComprador = filter.CuentaComprador,
        ZonaGeograficaId = zonaParaFiltroSolicitudes,
        Desde = fechaDesde,
        Hasta = fechaHasta
      };

      var todasSolicitudes = (await GetByMatchesFilterAsync(filterSolicitudes))
        .Where(s => s.EsPendiente
                 && !s.EstadoCupo.HasValue // CUPO_ID IS NULL (parent no asignado)
                 && s.CantidadAceptada < s.Cantidad) // aún no cubierta — si CantidadAceptada == Cantidad no hay cupos a asignar
        .ToList();

      if (todasSolicitudes.Count == 0)
      {
        _logger.LogInformation("BuscarMatchesAsync: 0 solicitudes matchean los filtros base.");
        return EmptyMatchesResult(filter, fechaDesde, fechaHasta, zonasResueltas: zonasResueltas);
      }

      // ── Obtener cupos disponibles para esos filtros ────────────────
      // NO usamos FindCuposByPeriod (carga CuposCorre.sql con JOINs a
      // MVCupos*, CuposStop, CUPOSCUITMV y Corretaje.CuposCorre, que NO
      // existen en este schema → ORA-00942). Usamos el método liviano
      // que consulta SOLO cuposcorre.
      var cuposDisponibles = await _silCuposStore.FindAvailableCuposByPeriodAsync(
        fechaDesde,
        fechaHasta,
        filter.CodigoGrano,
        filter.CuentaVendedor,
        cuentaDestino: filter.CuentaPuerto, // null si no se pidió, filtra cupos por destino concreto cuando llega
        cuentaComprador: filter.CuentaComprador, // null/0 = sin filtro por comprador
        estadoSil: 0, // El estado del cupo debe ser = 0
        zonaGeograficaId: zonaParaFiltroSolicitudes, // null/0 = sin filtro por zona
        centros: filter.Centros); // centros que el operador puede manipular; vacío = sin filtro

      if (cuposDisponibles.Count == 0)
      {
        _logger.LogInformation("BuscarMatchesAsync: 0 cupos disponibles para los filtros.");
        return EmptyMatchesResult(filter, fechaDesde, fechaHasta, totalSolicitudes: todasSolicitudes.Count, zonasResueltas: zonasResueltas);
      }

      // ── Resolver zonas + nombres en una sola query batch ────────────
      // El CodDestino del cupo es un código de puerto (ej. "100007"), no
      // un nombre legible. Traemos el nombre de la zona geográfica en el
      // mismo SELECT que la pertenencia (PUERTOPORZONA + ZONASGEOGRAFICAS)
      // para evitar un viaje extra al catálogo desde la UI.
      var ctxZonas = await _zonaGeograficaResolver.ResolverConNombresAsync(
        cuposDisponibles.Select(c => c.Id));

      // ── Hidratar nombres del CUPO ───────────────────────────────────
      // El nombre del comprador ya viene en la fila de cuposcorre
      // (NOMDESTINATARIO) y se mapea directo a NomCompSIL, por lo que no
      // hace falta salir a buscarlo a otro catálogo. La pertenencia a
      // zonas geográficas la resuelve IZonaGeograficaResolver más arriba.
      // Sólo queda lookup contra CUPOSVENDEDOR para los cupos que
      // tengan CodVendSIL poblado; los que no lo tengan quedan null y la
      // UI muestra "No informado".
      var cupoCatalogo = BuildCupoCatalogoLookup();

      Dictionary<long, string> nombresVendedor = await cupoCatalogo
        .ResolverNombresVendedorAsync(cuposDisponibles.Select(c => c.CodVendSIL));

      // ── Evaluar matches con el motor ────────────────────────────────
      var resultado = new MatchesResultDto
      {
        FiltrosAplicados = new MatchesFiltrosAplicados
        {
          CuentaComprador = filter.CuentaComprador,
          CodigoGrano = filter.CodigoGrano,
          // Si vino CuentaPuerto, reportamos la zona RESUELTA (no la explícita
          // que el caller pudo haber enviado) — es la zona efectiva que se
          // usó para filtrar las solicitudes.
          ZonaGeograficaId = filter.CuentaPuerto is > 0 ? zonaParaFiltroSolicitudes : filter.ZonaGeograficaId,
          CuentaPuerto = filter.CuentaPuerto,
          ZonasResueltas = zonasResueltas,
          CuentaVendedor = filter.CuentaVendedor,
          FechaDesde = fechaDesde,
          FechaHasta = fechaHasta,
          IncluirIncompatibles = filter.IncluirIncompatibles
        },
        Resumen = new MatchesResumen
        {
          TotalSolicitudesAnalizadas = todasSolicitudes.Count,
          TotalCuposAnalizados = cuposDisponibles.Count
        }
      };

      int directos = 0, parciales = 0, condicionales = 0, incompatibles = 0, descartadosPorFecha = 0;

      // ── Pre-agrupar cupos por Fecha.Date ──────────────────────────────
      // El nested foreach anterior hacía O(solicitudes × cupos) iteraciones
      // para descartar casi todas por fecha no coincidente (la regla de
      // negocio del motor exige fechas iguales). Reemplazamos por un lookup
      // O(1) por fecha: para cada solicitud sólo iteramos los cupos del
      // mismo día. Cupos con Fecha NULL quedan fuera (serían descartados
      // por FechasCoinciden de todos modos). desc: de N×M → N×K donde K
      // es la cantidad de cupos del mismo día (típicamente << M).
      var cuposPorFecha = new Dictionary<DateTime, List<Domain.Entities.Externo.Cupo>>(cuposDisponibles.Count);
      foreach (var c in cuposDisponibles)
      {
        if (!c.Fecha.HasValue) continue;
        var key = c.Fecha.Value.Date;
        if (!cuposPorFecha.TryGetValue(key, out var lista))
        {
          lista = new List<Domain.Entities.Externo.Cupo>();
          cuposPorFecha[key] = lista;
        }
        lista.Add(c);
      }

      foreach (var solicitudView in todasSolicitudes)
      {
        var solicitudMatching = SolicitudMatchingAdapter.From(solicitudView);

        // Si no hay cupos cargados para la fecha de esta solicitud, salteamos
        // el inner loop entero (antes igual iteraba M cupos para descartarlos
        // todos por fecha).
        if (!cuposPorFecha.TryGetValue(solicitudView.FechaSolicitado.Date, out var cuposDelDia))
          continue;

        foreach (var cupoDisponible in cuposDelDia)
        {
          // Cupos con Fecha NULL ya fueron filtrados al construir el Dictionary,
          // así que FechasCoinciden siempre es true por construcción.

          var match = _matchingEngine.Evaluar(cupoDisponible, solicitudMatching, ctxZonas);

          if (!match.Compatible)
          {
            incompatibles++;
            if (!filter.IncluirIncompatibles) continue;
          }

          resultado.Items.Add(new MatchItemDto
          {
            SolicitudId = solicitudView.Id,
            CupoId = cupoDisponible.Id,
            MatchType = match.Tipo?.ToString(),
            Razon = match.RazonIncompatibilidad,
            VendedorCoincide = match.VendedorCoincide,
            CompradorCoincide = match.CompradorCoincide,
            DestinoCoincide = match.DestinoCoincide,
            Solicitud = new MatchSolicitudCompleta
            {
              Id = solicitudView.Id,
              CuentaVendedor = solicitudView.CuentaVendedor,
              NombreVendedor = solicitudView.NombreVendedor,
              CuentaComprador = solicitudView.CuentaComprador,
              CodigoGrano = solicitudView.CodigoGrano,
              CuentaDestino = solicitudView.CuentaDestino,
              TipoDestino = solicitudView.TipoDestino?.ToString(),
              Cantidad = solicitudView.Cantidad,
              CantidadDisponible = Math.Max(0, solicitudView.Cantidad
                                              - solicitudView.CantidadAceptada
                                              - solicitudView.CantidadRechazada),
              CantidadRechazada = solicitudView.CantidadRechazada,
              FechaSolicitado = solicitudView.FechaSolicitado,
              Observacion = solicitudView.Observacion,
              CuposAsociados = new CuposAsociadosDesglose
              {
                Total = solicitudView.Cantidad,
                Otorgados = solicitudView.EstadoCupo.HasValue ? 1 : 0,
                Pendientes = solicitudView.EsPendiente ? 1 : 0,
                Rechazados = 0
              }
            },
            Cupo = new MatchCupoResumen
            {
              Id = cupoDisponible.Id,
              CodGrano = cupoDisponible.CodGrano,
              CodVendSIL = cupoDisponible.CodVendSIL,
              CodCompSIL = cupoDisponible.CodCompSIL,
              CodDestino = cupoDisponible.CodDestino,
              Fecha = cupoDisponible.Fecha,
              NombreVendedor = CupoCatalogoLookup.TryGetName(nombresVendedor, cupoDisponible.CodVendSIL),
              NombreComprador = cupoDisponible.NomCompSIL,
              NombreDestino = !string.IsNullOrWhiteSpace(cupoDisponible.NomDestino)
                                  ? cupoDisponible.NomDestino
                                  : ResolveNombreDestino(cupoDisponible.Id, ctxZonas)
            }
          });

          switch (match.Tipo)
          {
            case ACA.Matching.Modelos.MatchType.Directo: directos++; break;
            case ACA.Matching.Modelos.MatchType.Parcial: parciales++; break;
            case ACA.Matching.Modelos.MatchType.Condicional: condicionales++; break;
          }
        }
      }

      resultado.Resumen.MatchesDirectos = directos;
      resultado.Resumen.MatchesParciales = parciales;
      resultado.Resumen.MatchesCondicionales = condicionales;
      resultado.Resumen.Incompatibles = incompatibles;

      _logger.LogInformation(
        "BuscarMatchesAsync: {Solicitudes} solicitudes × {Cupos} cupos ({FechasCupo} fechas distintas) → {Items} items. Directos={D} Parciales={P} Condicionales={C} Incompatibles={I} DescartadosPorFecha={F}.",
        todasSolicitudes.Count, cuposDisponibles.Count, cuposPorFecha.Count, resultado.Items.Count,
        directos, parciales, condicionales, incompatibles, descartadosPorFecha);

      return resultado;
    }

    /// <summary>
    /// Matching bulk por VENTANA: resuelve en una sola llamada los pares
    /// (solicitud, cupo) compatibles de todas las solicitudes pendientes
    /// entre <c>FechaDesde</c> y <c>FechaDesde + Dias - 1</c>.
    ///
    /// Existe para que la grilla de Pantalla 1 deje de disparar una llamada a
    /// <see cref="BuscarMatchesAsync"/> por cada fila. Con R filas y G granos
    /// distintos, el costo en Oracle pasa de <c>R x 4</c> queries a
    /// <c>2 + G</c>: una lectura de SOLTURNOS, una de <c>cuposcorre</c> por
    /// grano y una resolucion batch de zonas.
    ///
    /// Preserva la semantica de <see cref="BuscarMatchesAsync"/>:
    /// <list type="bullet">
    ///   <item>Mismo SELECT de solicitudes y mismo post-filtro de pendientes.</item>
    ///   <item>Mismo conjunto de cupos por solicitud. La query se llama sin
    ///     filtros de vendedor, comprador ni zona (traeria un cupo por cada
    ///     combinacion, o sea de vuelta N queries) y esos tres filtros se
    ///     aplican en memoria, uno por solicitud: ver
    ///     <see cref="VendedorHabilitadoParaSolicitud"/>,
    ///     <see cref="CompradorHabilitadoParaSolicitud"/> y
    ///     <see cref="ZonaHabilitadaParaSolicitud"/>. Es equivalente al WHERE
    ///     porque los parametros que <c>BuscarMatchesAsync</c> recibe por fila
    ///     son los mismos valores que traen las solicitudes de esa fila: la
    ///     grilla agrupa justamente por grano, vendedor, comprador y destino.</item>
    ///   <item>Misma regla de fecha (cupo y solicitud el mismo dia) y mismo
    ///     descarte de incompatibles.</item>
    /// </list>
    ///
    /// Los filtros de comprador y zona no son opcionales para la paridad: el
    /// motor no descarta esos pares (ni <c>CompradorRule</c> ni
    /// <c>DestinoRule</c> cortan la cadena; un mismatch sale Parcial). Sin
    /// ellos, la grilla contaria mas Parciales de los que muestra el detalle.
    ///
    /// Diferencia deliberada con <c>BuscarMatchesAsync</c>: alla los filtros de
    /// comprador y zona se pasan por FILA, y como el parametro viaja como
    /// <c>valor ?? 0</c> contra un WHERE <c>(col = :p OR 0 = :p)</c>, una fila
    /// sin comprador apaga el filtro y termina contando matches de solicitudes
    /// de OTRAS filas. Aca el filtro es por solicitud, que es como esta escrito
    /// el requerimiento: la solicitud sin comprador ni zona matchea cualquier
    /// cupo del grano; con comprador, solo los de ese comprador; con zona,
    /// solo los cuyo destino cae en esa zona. Acumulativo.
    ///
    /// La atribucion de cada item a su fila de grilla queda del lado del
    /// consumidor, por <c>SolicitudId</c>: una fila es el conjunto de
    /// solicitudes de esa combinacion a lo largo de la ventana (una por fecha)
    /// y su contador es la suma sobre ellas.
    /// </summary>
    public async Task<MatchesVentanaResultDto> BuscarMatchesVentanaAsync(MatchesVentanaFilterDto filter)
    {
      if (filter is null)
        throw new SilDataException("El filtro no puede ser nulo.", StatusCodes.Status400BadRequest);

      if (filter.Dias <= 0)
        throw new SilDataException("Dias debe ser un numero positivo mayor a cero.", StatusCodes.Status400BadRequest);

      DateTime fechaDesde = (filter.FechaDesde ?? DateTime.Now.Date).Date;
      DateTime fechaHasta = fechaDesde.AddDays(filter.Dias - 1);

      var resultado = new MatchesVentanaResultDto
      {
        FechaDesde = fechaDesde,
        FechaHasta = fechaHasta
      };

      // -- 1) Una sola lectura de SOLTURNOS para toda la ventana ---------
      var solicitudes = (await _solicitudTurnoStore.GetVentanaParaMatchingAsync(fechaDesde, fechaHasta, filter.Centros))
        .Where(s => s.EsPendiente
                 && !s.EstadoCupo.HasValue           // CUPO_ID IS NULL (parent no asignado)
                 && s.CantidadAceptada < s.Cantidad) // aun no cubierta
        .ToList();

      resultado.Resumen.TotalSolicitudesAnalizadas = solicitudes.Count;

      if (solicitudes.Count == 0)
      {
        _logger.LogInformation(
          "BuscarMatchesVentanaAsync: 0 solicitudes pendientes entre {Desde} y {Hasta}.",
          fechaDesde, fechaHasta);
        return resultado;
      }

      // -- 2) Una lectura de cuposcorre por grano presente en la ventana --
      // No por fila de grilla: los granos distintos son un punado, las
      // combinaciones grano/vendedor/comprador/destino son decenas.
      var granos = solicitudes.Select(s => s.CodigoGrano).Distinct().ToList();
      resultado.Resumen.TotalGranos = granos.Count;

      var cuposPorGranoYFecha =
        new Dictionary<(int Grano, DateTime Fecha), List<Domain.Entities.Externo.Cupo>>();
      int totalCupos = 0;

      foreach (int grano in granos)
      {
        var cuposDelGrano = await _silCuposStore.FindAvailableCuposByPeriodAsync(
          fechaDesde,
          fechaHasta,
          grano,
          cuentaVendedor: 0,   // 0 = todos los vendedores; se filtra por solicitud mas abajo
          cuentaDestino: null, // la grilla nunca manda CuentaPuerto en este flujo
          estadoSil: 0,
          // El centro SI se filtra en el SQL, no en memoria: no depende de la
          // solicitud sino del operador, asi que vale para toda la ventana. De
          // paso achica el conjunto que despues resuelve zonas y evalua el motor.
          centros: filter.Centros);

        foreach (var cupo in cuposDelGrano)
        {
          // Un cupo sin fecha nunca satisface la regla de fecha obligatoria
          // (SolicitudMatchingAdapter.FechasCoinciden), asi que no lo indexamos.
          if (!cupo.Fecha.HasValue) continue;

          var clave = (grano, cupo.Fecha.Value.Date);
          if (!cuposPorGranoYFecha.TryGetValue(clave, out var lista))
          {
            lista = new List<Domain.Entities.Externo.Cupo>();
            cuposPorGranoYFecha[clave] = lista;
          }
          lista.Add(cupo);
          totalCupos++;
        }
      }

      resultado.Resumen.TotalCuposAnalizados = totalCupos;

      if (totalCupos == 0)
      {
        _logger.LogInformation(
          "BuscarMatchesVentanaAsync: 0 cupos disponibles para {Granos} granos entre {Desde} y {Hasta}.",
          granos.Count, fechaDesde, fechaHasta);
        return resultado;
      }

      // -- 3) Una sola resolucion batch de zonas para todos los cupos -----
      var ctxZonas = await _zonaGeograficaResolver.ResolverConNombresAsync(
        cuposPorGranoYFecha.Values.SelectMany(l => l).Select(c => c.Id));

      // -- 4) Evaluacion del motor, en memoria ---------------------------
      int directos = 0, parciales = 0, condicionales = 0, incompatibles = 0;

      foreach (var solicitud in solicitudes)
      {
        if (!cuposPorGranoYFecha.TryGetValue(
              (solicitud.CodigoGrano, solicitud.FechaSolicitado.Date), out var candidatos))
          continue;

        var solicitudMatching = SolicitudMatchingAdapter.From(solicitud);

        foreach (var cupo in candidatos)
        {
          if (!VendedorHabilitadoParaSolicitud(cupo.CodVendSIL, solicitud.CuentaVendedor))
            continue;

          if (!CompradorHabilitadoParaSolicitud(cupo.CodCompSIL, solicitud.CuentaComprador))
            continue;

          if (!ZonaHabilitadaParaSolicitud(cupo.Id, solicitud.CuentaDestino, ctxZonas))
            continue;

          var match = _matchingEngine.Evaluar(cupo, solicitudMatching, ctxZonas);

          if (!match.Compatible)
          {
            incompatibles++;
            continue;
          }

          resultado.Items.Add(new MatchVentanaItemDto
          {
            SolicitudId = solicitud.Id,
            CupoId = cupo.Id,
            MatchType = match.Tipo?.ToString(),
            CupoFecha = cupo.Fecha
          });

          switch (match.Tipo)
          {
            case ACA.Matching.Modelos.MatchType.Directo: directos++; break;
            case ACA.Matching.Modelos.MatchType.Parcial: parciales++; break;
            case ACA.Matching.Modelos.MatchType.Condicional: condicionales++; break;
          }
        }
      }

      resultado.Resumen.MatchesDirectos = directos;
      resultado.Resumen.MatchesParciales = parciales;
      resultado.Resumen.MatchesCondicionales = condicionales;
      resultado.Resumen.Incompatibles = incompatibles;

      _logger.LogInformation(
        "BuscarMatchesVentanaAsync: {Solicitudes} solicitudes / {Cupos} cupos ({Granos} granos) -> {Items} items. " +
        "Directos={D} Parciales={P} Condicionales={C} Incompatibles={I}.",
        solicitudes.Count, totalCupos, granos.Count, resultado.Items.Count,
        directos, parciales, condicionales, incompatibles);

      return resultado;
    }

    /// <summary>
    /// Reproduce en memoria el filtro de vendedor que
    /// <c>FindAvailableCuposByPeriodAsync</c> aplicaba en el WHERE cuando se
    /// lo llamaba una vez por fila de grilla:
    /// <c>c.VENDCTA = :cuentaVendedor OR c.VENDCTA IS NULL OR c.VENDCTA = 0</c>.
    ///
    /// Es valido moverlo del SQL a memoria porque el vendedor con el que se
    /// filtraba era siempre el de las solicitudes de esa fila: la query de
    /// solicitudes usaba <c>s.ctavend = :vendedor</c> con el mismo valor.
    ///
    /// <c>CodVendSIL</c> llega como string (mapeo de la columna NUMBER
    /// <c>VENDCTA</c>); un valor vacio o no numerico se trata como "cupo sin
    /// vendedor asignado", que es el caso que el WHERE dejaba pasar.
    /// </summary>
    private static bool VendedorHabilitadoParaSolicitud(string? codVendCupo, long cuentaVendedorSolicitud)
    {
      if (string.IsNullOrWhiteSpace(codVendCupo)) return true;

      if (!decimal.TryParse(
            codVendCupo.Trim(),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out decimal vendedorCupo))
        return true;

      return vendedorCupo == 0m || vendedorCupo == cuentaVendedorSolicitud;
    }

    /// <summary>
    /// Equivalente en memoria del filtro por comprador que
    /// <c>FindAvailableCuposByPeriodAsync</c> aplica en el WHERE:
    /// <c>(:cuentaComprador = 0 OR c.COMPCTA = :cuentaComprador)</c>.
    ///
    /// A diferencia del filtro de vendedor, acá un cupo SIN comprador NO pasa
    /// cuando la solicitud exige uno: en el SQL <c>NULL = :comprador</c> evalúa
    /// NULL y la fila queda afuera. Se replica esa semántica.
    ///
    /// Importa que el filtro exista: el motor no descarta estos pares por su
    /// cuenta. <c>CompradorRule</c> nunca corta la cadena — sólo baja la
    /// bandera <c>CompradorCoincide</c> y el clasificador emite Parcial. Sin
    /// este filtro, la ventana contaría como Parciales cupos que
    /// <c>/Matches</c> ya no devuelve.
    /// </summary>
    private static bool CompradorHabilitadoParaSolicitud(string? codCompCupo, long? cuentaCompradorSolicitud)
    {
      long compradorSolicitud = cuentaCompradorSolicitud ?? 0;
      if (compradorSolicitud == 0) return true; // la solicitud no exige comprador

      if (string.IsNullOrWhiteSpace(codCompCupo)) return false; // NULL = :comprador → afuera

      if (!decimal.TryParse(
            codCompCupo.Trim(),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out decimal compradorCupo))
        return false;

      return compradorCupo == compradorSolicitud;
    }

    /// <summary>
    /// Equivalente en memoria del filtro por zona geográfica que
    /// <c>FindAvailableCuposByPeriodAsync</c> aplica en el WHERE:
    /// <c>(:zonaGeograficaId = 0 OR EXISTS (SELECT 1 FROM PUERTOPORZONA pz
    /// WHERE pz.CUENTA = c.PUERTOCTA AND pz.ZONAGEOID = :zonaGeograficaId))</c>.
    ///
    /// No cuesta una query extra: <paramref name="ctxZonas"/> ya trae la
    /// pertenencia cupo → zonas, resuelta en el batch único de
    /// <c>IZonaGeograficaResolver</c>.
    ///
    /// Diferencia de borde respecto del SQL: el resolver hace INNER JOIN
    /// también contra <c>ZONASGEOGRAFICAS</c>, así que una fila de
    /// <c>PUERTOPORZONA</c> que apunte a una zona inexistente no aparece acá y
    /// sí pasaría el EXISTS de la query. Es un caso de integridad referencial
    /// rota; no lo compensamos.
    ///
    /// Igual que con el comprador, este filtro no es redundante con el motor:
    /// <c>DestinoRule</c> tampoco corta la cadena, marca el mismatch como
    /// Parcial.
    /// </summary>
    private static bool ZonaHabilitadaParaSolicitud(
      long cupoId,
      long? cuentaDestinoSolicitud,
      ACA.Matching.Contexto.IContextoZona ctxZonas)
    {
      long zonaSolicitud = cuentaDestinoSolicitud ?? 0;
      if (zonaSolicitud == 0) return true; // la solicitud no exige zona

      if (!ctxZonas.ZonasPorCupo.TryGetValue(cupoId, out var zonas) || zonas is null)
        return false;

      return zonas.Contains(zonaSolicitud);
    }

    /// <summary>
    /// Variante específica para la pantalla de Distribución (CuposMatchingController).
    /// Idéntico a <see cref="BuscarMatchesAsync"/> pero usa
    /// <see cref="ISolicitudTurnoStore.GetForMatchingAsync"/> para que las
    /// solicitudes sin comprador o sin destino (NULL) sean matchables como
    /// Parcial cuando el cupo sí los tiene poblados.
    ///
    /// Invariante de pendientes: una solicitud se considera pendiente mientras
    /// <c>CantidadDisponible &gt; 0</c>, es decir
    /// <c>Cantidad - CantidadAceptada - CantidadRechazada &gt; 0</c>. La columna
    /// <c>STATUS</c> está obsoleta (migrada en
    /// <c>ALTER_SOLTURNOS_DROP_REQUEST_STATUS.sql</c>); <c>CantidadAceptada</c>
    /// es un acumulado por asociación (<c>SOLTURNOS_DETALLE</c>). Esta es la
    /// única fuente de verdad para "cuánto le queda a esta solicitud".
    ///
    /// NO afecta a Pantalla 2 (Solicitudes), que sigue usando
    /// <c>BuscarMatchesAsync</c> con el query legacy.
    /// </summary>
    public async Task<MatchesResultDto> BuscarMatchesParaDistribucionAsync(MatchesFilterDto filter)
    {
      if (filter is null)
        throw new SilDataException("El filtro no puede ser nulo.", StatusCodes.Status400BadRequest);

      if (filter.CodigoGrano <= 0)
        throw new SilDataException("CodigoGrano es obligatorio y debe ser positivo.", StatusCodes.Status400BadRequest);

      if (filter.CuentaComprador is { } cc && cc <= 0)
        throw new SilDataException("Si se proporciona CuentaComprador, debe ser positivo.", StatusCodes.Status400BadRequest);

      var fechaDesde = filter.FechaDesde ?? DateTime.Now.Date;
      var fechaHasta = filter.FechaHasta ?? DateTime.Now.Date.AddDays(7);
      if (fechaDesde > fechaHasta)
        throw new SilDataException("FechaDesde no puede ser posterior a FechaHasta.", StatusCodes.Status400BadRequest);

      // ── Resolución CuentaPuerto → zonas ─────────────────────────
      List<long> zonasResueltas = new List<long>();
      long? zonaParaFiltroSolicitudes = filter.ZonaGeograficaId;

      if (filter.CuentaPuerto is long cp && cp > 0)
      {
        var zonasDelPuerto = await _geographicalAereaService.ResolveByDestinoAsync(cp);
        zonasResueltas = zonasDelPuerto?.Select(z => z.zonaGeoId).ToList() ?? new List<long>();

        if (zonasResueltas.Count == 0)
        {
          _logger.LogInformation(
            "BuscarMatchesParaDistribucionAsync: CuentaPuerto={Id} no mapea a ninguna zona. Sin matches posibles.",
            cp);
          return EmptyMatchesResult(filter, fechaDesde, fechaHasta, zonasResueltas: new List<long> { cp });
        }

        zonaParaFiltroSolicitudes = zonasResueltas[0];
      }

      // ── Obtener solicitudes pendientes (con soporte NULL para Parcial) ─
      long cuentaCompradorFiltro = filter.CuentaComprador ?? 0L;
      long zonaFiltro = zonaParaFiltroSolicitudes ?? 0;

      var todasSolicitudes = (await _solicitudTurnoStore.GetForMatchingAsync(
          filter.CodigoGrano,
          filter.CuentaVendedor ?? 0L,
          cuentaCompradorFiltro,
          zonaFiltro,
          fechaDesde,
          fechaHasta))
        .Where(s => s.EsPendiente
                 && s.CantidadAceptada < s.Cantidad
                 // CantidadDisponible = Cantidad - CantidadAceptada - CantidadRechazada.
                 // Mantener el filtro en el backend evita que el motor evalúe
                 // pares cuya solicitud ya está satisfecha (UC2 / UC3 post-Acept).
                 && (s.Cantidad - s.CantidadAceptada - s.CantidadRechazada) > 0)
        .ToList();

      if (todasSolicitudes.Count == 0)
      {
        _logger.LogInformation("BuscarMatchesParaDistribucionAsync: 0 solicitudes matchean los filtros base.");
        return EmptyMatchesResult(filter, fechaDesde, fechaHasta, zonasResueltas: zonasResueltas);
      }

      // ── Obtener cupos disponibles ────────────────────────────────
      var cuposDisponibles = await _silCuposStore.FindAvailableCuposByPeriodAsync(
        fechaDesde,
        fechaHasta,
        filter.CodigoGrano,
        filter.CuentaVendedor,
        cuentaDestino: filter.CuentaPuerto,
        estadoSil: 0);

      if (cuposDisponibles.Count == 0)
      {
        _logger.LogInformation("BuscarMatchesParaDistribucionAsync: 0 cupos disponibles para los filtros.");
        return EmptyMatchesResult(filter, fechaDesde, fechaHasta, totalSolicitudes: todasSolicitudes.Count, zonasResueltas: zonasResueltas);
      }

      // ── Resolver zonas + nombres en una sola query batch ────────────
      var ctxZonas = await _zonaGeograficaResolver.ResolverConNombresAsync(
        cuposDisponibles.Select(c => c.Id));

      // ── Hidratar nombres del CUPO ───────────────────────────────────
      var cupoCatalogo = BuildCupoCatalogoLookup();
      Dictionary<long, string> nombresVendedor = await cupoCatalogo
        .ResolverNombresVendedorAsync(cuposDisponibles.Select(c => c.CodVendSIL));

      // ── Evaluar matches con el motor ────────────────────────────────
      var resultado = new MatchesResultDto
      {
        FiltrosAplicados = new MatchesFiltrosAplicados
        {
          CuentaComprador = filter.CuentaComprador,
          CodigoGrano = filter.CodigoGrano,
          ZonaGeograficaId = filter.CuentaPuerto is > 0 ? zonaParaFiltroSolicitudes : filter.ZonaGeograficaId,
          CuentaPuerto = filter.CuentaPuerto,
          ZonasResueltas = zonasResueltas,
          CuentaVendedor = filter.CuentaVendedor,
          FechaDesde = fechaDesde,
          FechaHasta = fechaHasta,
          IncluirIncompatibles = filter.IncluirIncompatibles
        },
        Resumen = new MatchesResumen
        {
          TotalSolicitudesAnalizadas = todasSolicitudes.Count,
          TotalCuposAnalizados = cuposDisponibles.Count
        }
      };

      int directos = 0, parciales = 0, condicionales = 0, incompatibles = 0, descartadosPorFecha = 0;

      // ── Pre-agrupar cupos por Fecha.Date ──────────────────────────────
      // Misma optimización que BuscarMatchesAsync: pasamos de un cartesiano
      // O(solicitudes × cupos) a O(solicitudes × cuposDelMismoDia). Cupos
      // con Fecha NULL quedan fuera del Dictionary (serían descartados por
      // FechasCoinciden de todos modos).
      var cuposPorFecha = new Dictionary<DateTime, List<Domain.Entities.Externo.Cupo>>(cuposDisponibles.Count);
      foreach (var c in cuposDisponibles)
      {
        if (!c.Fecha.HasValue) continue;
        var key = c.Fecha.Value.Date;
        if (!cuposPorFecha.TryGetValue(key, out var lista))
        {
          lista = new List<Domain.Entities.Externo.Cupo>();
          cuposPorFecha[key] = lista;
        }
        lista.Add(c);
      }

      foreach (var solicitudView in todasSolicitudes)
      {
        var solicitudMatching = SolicitudMatchingAdapter.From(solicitudView);

        if (!cuposPorFecha.TryGetValue(solicitudView.FechaSolicitado.Date, out var cuposDelDia))
          continue;

        foreach (var cupoDisponible in cuposDelDia)
        {
          // Cupos con Fecha NULL ya fueron filtrados al construir el Dictionary,
          // así que FechasCoinciden siempre es true por construcción.

          var match = _matchingEngine.Evaluar(cupoDisponible, solicitudMatching, ctxZonas);

          if (!match.Compatible)
          {
            incompatibles++;
            if (!filter.IncluirIncompatibles) continue;
          }

          resultado.Items.Add(new MatchItemDto
          {
            SolicitudId = solicitudView.Id,
            CupoId = cupoDisponible.Id,
            MatchType = match.Tipo?.ToString(),
            Razon = match.RazonIncompatibilidad,
            VendedorCoincide = match.VendedorCoincide,
            CompradorCoincide = match.CompradorCoincide,
            DestinoCoincide = match.DestinoCoincide,
            Solicitud = new MatchSolicitudCompleta
            {
              Id = solicitudView.Id,
              CuentaVendedor = solicitudView.CuentaVendedor,
              NombreVendedor = solicitudView.NombreVendedor,
              CuentaComprador = solicitudView.CuentaComprador,
              CodigoGrano = solicitudView.CodigoGrano,
              CuentaDestino = solicitudView.CuentaDestino,
              TipoDestino = solicitudView.TipoDestino?.ToString(),
              Cantidad = solicitudView.Cantidad,
              CantidadDisponible = Math.Max(0, solicitudView.Cantidad
                                              - solicitudView.CantidadAceptada
                                              - solicitudView.CantidadRechazada),
              CantidadRechazada = solicitudView.CantidadRechazada,
              FechaSolicitado = solicitudView.FechaSolicitado,
              Observacion = solicitudView.Observacion,
              CuposAsociados = new CuposAsociadosDesglose
              {
                Total = solicitudView.Cantidad,
                Otorgados = solicitudView.EstadoCupo.HasValue ? 1 : 0,
                Pendientes = solicitudView.EsPendiente ? 1 : 0,
                Rechazados = 0
              }
            },
            Cupo = new MatchCupoResumen
            {
              Id = cupoDisponible.Id,
              CodGrano = cupoDisponible.CodGrano,
              CodVendSIL = cupoDisponible.CodVendSIL,
              CodCompSIL = cupoDisponible.CodCompSIL,
              CodDestino = cupoDisponible.CodDestino,
              Fecha = cupoDisponible.Fecha,
              NombreVendedor = CupoCatalogoLookup.TryGetName(nombresVendedor, cupoDisponible.CodVendSIL),
              NombreComprador = cupoDisponible.NomCompSIL,
              NombreDestino = !string.IsNullOrWhiteSpace(cupoDisponible.NomDestino)
                                  ? cupoDisponible.NomDestino
                                  : ResolveNombreDestino(cupoDisponible.Id, ctxZonas)
            }
          });

          switch (match.Tipo)
          {
            case ACA.Matching.Modelos.MatchType.Directo: directos++; break;
            case ACA.Matching.Modelos.MatchType.Parcial: parciales++; break;
            case ACA.Matching.Modelos.MatchType.Condicional: condicionales++; break;
          }
        }
      }

      resultado.Resumen.MatchesDirectos = directos;
      resultado.Resumen.MatchesParciales = parciales;
      resultado.Resumen.MatchesCondicionales = condicionales;
      resultado.Resumen.Incompatibles = incompatibles;

      _logger.LogInformation(
        "BuscarMatchesParaDistribucionAsync: {Solicitudes} solicitudes × {Cupos} cupos ({FechasCupo} fechas distintas) → {Items} items. Directos={D} Parciales={P} Condicionales={C} Incompatibles={I} DescartadosPorFecha={F}.",
        todasSolicitudes.Count, cuposDisponibles.Count, cuposPorFecha.Count, resultado.Items.Count,
        directos, parciales, condicionales, incompatibles, descartadosPorFecha);

      return resultado;
    }

    private static MatchesResultDto EmptyMatchesResult(
      MatchesFilterDto filter,
      DateTime fechaDesde,
      DateTime fechaHasta,
      int totalSolicitudes = 0,
      int totalCupos = 0,
      List<long> zonasResueltas = null)
    {
      return new MatchesResultDto
      {
        FiltrosAplicados = new MatchesFiltrosAplicados
        {
          CuentaComprador = filter.CuentaComprador,
          CodigoGrano = filter.CodigoGrano,
          ZonaGeograficaId = filter.ZonaGeograficaId,
          CuentaPuerto = filter.CuentaPuerto,
          ZonasResueltas = zonasResueltas ?? new List<long>(),
          CuentaVendedor = filter.CuentaVendedor,
          FechaDesde = fechaDesde,
          FechaHasta = fechaHasta,
          IncluirIncompatibles = filter.IncluirIncompatibles
        },
        Resumen = new MatchesResumen
        {
          TotalSolicitudesAnalizadas = totalSolicitudes,
          TotalCuposAnalizados = totalCupos
        }
      };
    }

    // ─── Hidratación de nombres para el payload de Matches ─────────────────
    // El nombre del comprador se popula desde cuposcorre (NOMDESTINATARIO)
    // y la pertenencia a zonas se resuelve vía IZonaGeograficaResolver.
    // El helper CupoCatalogoLookup (Services/CupoCatalogoLookup.cs) sólo
    // se ocupa del lookup de vendedor contra CUPOSVENDEDOR. Mantenerlo
    // afuera de este servicio preserva la cohesión: este archivo se
    // ocupa del matching, no del catálogo de cuentas.

    /// <summary>
    /// Fallback: resuelve el nombre legible del destino de un cupo desde la
    /// lista de zonas geográficas a las que pertenece. Sólo se usa cuando el
    /// SELECT principal no trajo <c>Cupo.NomDestino</c> (por ejemplo cupos
    /// huérfanos cuyo PuertoCta no está en cupospuerto).
    /// </summary>
    /// <remarks>
    /// Estrategia:
    /// <list type="number">
    ///   <item>Mirar la lista de nombres de zonas a las que pertenece el cupo
    ///         (vía <paramref name="ctxZonas"/>, una sola query batch con
    ///         JOIN PUERTOPORZONA + ZONASGEOGRAFICAS).</item>
    ///   <item>Devolver el primer nombre no nulo (la UI ya conoce el orden
    ///         paralelo a <c>ZonasPorCupo</c>).</item>
    ///   <item>Si el cupo no pertenece a ninguna zona o todos los nombres son
    ///         nulos, devuelve <c>null</c>: la UI mostrará el código crudo o
    ///         "—", nunca un valor inventado.</item>
    /// </list>
    /// </remarks>
    private static string? ResolveNombreDestino(
      long cupoId,
      ACA.Matching.Contexto.IContextoZonasConNombres ctxZonas)
    {
      if (!ctxZonas.NombresPorCupo.TryGetValue(cupoId, out var nombres) || nombres.Count == 0)
        return null;

      foreach (var nombre in nombres)
      {
        if (!string.IsNullOrWhiteSpace(nombre))
          return nombre;
      }

      return null;
    }

    /// <summary>
    /// Anula la distribución de uno o varios cupos (batch). Por cada cupo
    /// con solicitud asociada en <c>SOLTURNOS_DETALLE</c>: localiza la
    /// solicitud, la devuelve al estado Pendiente (decrementa
    /// <c>CANTIDAD_ACEPTADA</c>, incrementa <c>CANTIDAD</c>) y borra la
    /// fila de detalle. Los cupos sin solicitud asociada quedan como
    /// <c>Skipped</c> (el flujo legacy de <c>CuposDataController.Anular</c>
    /// ya los cubrió en CUPOSCORRE).
    /// </summary>
    /// <remarks>
    /// Este endpoint se consume desde la pantalla
    /// <c>Views/Cupos/Editar.cshtml</c> cuando el operador selecciona
    /// "Anular distribución" en el modal de motivos. Es complementario al
    /// Anular existente: aquel revierte el estado del cupo en CUPOSCORRE;
    /// éste re-habilita las solicitudes originales que tenían ese cupo
    /// asociado (quedaban "atrapadas" con <c>CANTIDAD_ACEPTADA</c> inflada
    /// aunque el cupo ya no les pertenece).
    /// </remarks>
    public async Task<AnularDistribucionResult> AnularDistribucionAsync(IList<long> cupoIds)
    {
      if (cupoIds == null || cupoIds.Count == 0)
        throw new SilDataException("La lista de cupos no puede estar vacía.", StatusCodes.Status400BadRequest);

      // Defensa adicional: el store ya deduplica, pero normalizamos acá
      // para tener un mensaje de error claro si llegan ids no positivos.
      var idsInvalidos = cupoIds.Where(id => id <= 0).ToList();
      if (idsInvalidos.Count > 0)
        throw new SilDataException(
          $"Los siguientes ids de cupo no son válidos: {string.Join(",", idsInvalidos)}.",
          StatusCodes.Status400BadRequest);

      try
      {
        var items = await _solicitudTurnoStore.AnularDistribucionPorCuposAsync(cupoIds);

        int exitos = items.Count(r => r.Estado == AnularDistribucionItemEstado.Exitoso);
        int skipped = items.Count(r => r.Estado == AnularDistribucionItemEstado.Skipped);
        int fallos = items.Count(r => r.Estado == AnularDistribucionItemEstado.Fallo);

        _logger.LogInformation(
          "AnularDistribucionAsync: {N} cupos procesados. Exitosos={E}, Skipped={S}, Fallos={F}.",
          items.Count, exitos, skipped, fallos);

        return new AnularDistribucionResult
        {
          AlMenosUnoExitoso = exitos > 0,
          CantidadExitosos = exitos,
          CantidadSkipped = skipped,
          CantidadFallos = fallos,
          Items = items
        };
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en AnularDistribucionAsync. CupoIds: {CupoIds}",
          string.Join(",", cupoIds));
        throw new Exception("Error al anular la distribución de los cupos.", ex);
      }
    }
  }
}

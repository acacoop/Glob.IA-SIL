using Domain.Entities.Externo;
using SILData.DataAccess;
using SILData.Model;
using SILData.Model.SolicitudTurno;
using SILData.SilDataExceptions;
using System.Collections.Generic;

namespace SILData.Services
{
  public class CuposDisponiblesService : ICuposDisponiblesService
  {
    private readonly ICuposDisponiblesStore _cuposDisponiblesStore;
    private readonly ISolicitudTurnoStore _solicitudTurnoStore;
    private readonly ILogger<CuposDisponiblesService> _logger;

    public CuposDisponiblesService(ICuposDisponiblesStore cuposDisponiblesStore, ILogger<CuposDisponiblesService> logger, ISolicitudTurnoStore solicitudTurnoStore)
    {
      _cuposDisponiblesStore = cuposDisponiblesStore;
      _logger = logger;
      _solicitudTurnoStore = solicitudTurnoStore;
    }

    public async Task<long> GetCantidadDisponibles(long cuentaVendedor, DateTime fecha, long cuentaComprador = 0, int codigoGrano = 0, long zonaGeografica = 0)
    {
      try
      {
        IEnumerable<CuposDisponiblesPorGrano> porGrano = await GetCuposDisponiblesForShiftRequest(
            cuentaVendedor, fecha, cuentaComprador, codigoGrano, zonaGeografica);

        string codigoGranoStr = codigoGrano.ToString();
        CuposDisponiblesPorGrano grano = porGrano.FirstOrDefault(x => x.CodigoGrano == codigoGranoStr);
        if (grano is null)
          return 0;

        if (cuentaComprador > 0 && zonaGeografica > 0)
        {
          // Fila detalle (comprador, zona)
          CuposDetalle det = grano.Detalles.FirstOrDefault(
              x => x.CuentaComprador == cuentaComprador &&
                   x.ZonaGeograficaId == (int)zonaGeografica);
          return Math.Max(0L, det?.CuposDisponibles ?? 0L);
        }

        if (cuentaComprador > 0)
        {
          // Síntesis del comprador (ZonaGeograficaId = 0)
          CuposDetalle det = grano.Detalles.FirstOrDefault(
              x => x.CuentaComprador == cuentaComprador &&
                   x.ZonaGeograficaId == 0);
          return Math.Max(0L, det?.CuposDisponibles ?? 0L);
        }

        // Sin comprador: total del grano (ya calculado en el árbol)
        return Math.Max(0L, grano.TotalGrano);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
            "Error inesperado en GetCantidadDisponibles. Vendedor: {Vendedor} Fecha: {Fecha}",
            cuentaVendedor, fecha);
        throw new Exception(
            "Ocurrió un error inesperado al calcular los cupos disponibles.", ex);
      }
    }

    public async Task<IEnumerable<CuposDisponible>> GetDisponibles(long cuentaVendedor, DateTime fecha, long cuentaComprador = 0, int codigoGrano = 0, long zonaGeografica = 0)
    {
      try
      {
        return await _cuposDisponiblesStore
          .GetDisponibles(cuentaVendedor, fecha, cuentaComprador, codigoGrano, zonaGeografica);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
          "Error inesperado en GetDisponibles. Vendedor: {Vendedor} Fecha: {Fecha}",
          cuentaVendedor, fecha);
        throw new Exception(
          "Ocurrió un error inesperado al obtener los cupos disponibles.", ex);
      }
    }

    public async Task<IEnumerable<CuposDisponiblesPorGrano>> GetCuposDisponiblesForShiftRequest(long cuentaVendedor, DateTime fecha, long cuentaComprador = 0, int codigoGrano = 0, long zonaGeografica = 0)
    {
      SolicitudTurnosFilter solicitudTurnosFilter = new SolicitudTurnosFilter
      {
        CuentaComprador = cuentaComprador,
        CuentaVendedor = cuentaVendedor,
        Desde = fecha,
        Hasta = fecha.AddDays(7),
        CuentaDestino = zonaGeografica
      };

      try
      {
        IEnumerable<SolicitudTurnoCuposDisponibles> CuposdDsponibles = await _cuposDisponiblesStore.GetCuposDisponiblesForShiftRequest(cuentaVendedor, fecha, cuentaComprador, codigoGrano, zonaGeografica);
        IEnumerable<SolicitudTurnoView> solicitudes = await _solicitudTurnoStore.GetByFilterAsync(solicitudTurnosFilter, solicitudTurnosFilter.Desde, solicitudTurnosFilter.Hasta);
        IEnumerable<SolicitudTurnoPorGranoView> solicitudTurnoPorGranoView = ConvertToSolicitudTurnoPorGranoView(solicitudes);
        List<AvailabilityByProduct> availabilityByProductsList = FromCuposDisponiblesToAvailabilityByProduct(CuposdDsponibles.ToList());
        List<SolicitudesAgrupadas> solicitudesAgrupadas = FromSolicitudesDeTurnoPorGranoToSolicitudDeTurnoAgrupadas(solicitudTurnoPorGranoView);

        // El vendedor es el mismo para todas las filas de la respuesta cruda (la SQL
        // filtra por :cuentavendedor). Lo levantamos de la primera fila para
        // propagarlo a cada CuposDisponiblesPorGrano (el agrupamiento en
        // AvailabilityByProduct lo pierde, porque esa clase no tiene el campo).
        var firstRow = CuposdDsponibles.FirstOrDefault();
        long cuentaVendedorFlat = firstRow?.CuentaVendedor ?? cuentaVendedor;
        string nombreVendedorFlat = firstRow?.NombreVendedor ?? "";

        var solicitudesPorGrano = solicitudesAgrupadas
          .GroupBy(x => x.CodigoGrano)
          .ToDictionary(
              g => g.Key,
              g => g.Sum(x => x.Cantidad));

        var solicitudesPorGranoComprador = solicitudesAgrupadas
            .Where(x => x.CuentaComprador > 0)
            .GroupBy(x => new
            {
              x.CodigoGrano,
              x.CuentaComprador
            })
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Cantidad));

        var solicitudesPorGranoCompradorZona = solicitudesAgrupadas
            .Where(x => x.CuentaComprador > 0 &&
                        x.ZonaGeograficaId > 0)
            .GroupBy(x => new
            {
              x.CodigoGrano,
              x.CuentaComprador,
              x.ZonaGeograficaId
            })
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Cantidad));

        foreach (var grano in availabilityByProductsList)
        {
          // Nivel 1: Grano
          // Nivel 1: descontar del total del grano
          if (solicitudesPorGrano.TryGetValue(grano.Id, out var totalGrano))
            grano.Shift = Math.Max(0, grano.Shift - totalGrano);

          foreach (var comprador in grano.CompradorAvailableList)
          {
            // Nivel 2: descontar del comprador
            var keyGranoComprador = new { CodigoGrano = grano.Id, CuentaComprador = comprador.Id };
            if (solicitudesPorGranoComprador.TryGetValue(keyGranoComprador, out var totalComprador))
              comprador.Pendiente = Math.Max(0, comprador.Pendiente - totalComprador);

            // Nivel 3: descontar de cada zona del comprador
            foreach (var zona in comprador.GeographicZoneAvailableList)
            {
              var key = new { CodigoGrano = grano.Id, CuentaComprador = comprador.Id, ZonaGeograficaId = (long)zona.Id };
              if (solicitudesPorGranoCompradorZona.TryGetValue(key, out var totalZona))
                zona.Pendiente = Math.Max(0, zona.Pendiente - totalZona);
            }
          }

          // Opcional: también podés ajustar grano.GeographicZoneAvailableList si necesitás mostrarlo visualmente
          grano.GeographicZoneAvailableList = grano.CompradorAvailableList
              .SelectMany(c => c.GeographicZoneAvailableList)
              .GroupBy(z => z.Id)
              .Select(zg => new GeographicZone
              {
                Id = zg.Key,
                Name = zg.First().Name,
                Pendiente = zg.Sum(z => z.Pendiente)
              })
              .ToList();
        }
        return BuildCuposDisponiblesPorGrano(availabilityByProductsList, cuentaVendedorFlat, nombreVendedorFlat);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
          "Error inesperado en GetCuposDisponiblesForShiftRequest. Vendedor: {Vendedor} Fecha: {Fecha}",
          cuentaVendedor, fecha);
        throw new Exception(
          "Ocurrió un error inesperado al obtener los cupos disponibles.", ex);
      }
    }

    private List<AvailabilityByProduct> FromCuposDisponiblesToAvailabilityByProduct(
    List<SolicitudTurnoCuposDisponibles> solicitudTurnoCuposDisponibles)
    {
      if (solicitudTurnoCuposDisponibles == null || !solicitudTurnoCuposDisponibles.Any())
        return new List<AvailabilityByProduct>();

      return solicitudTurnoCuposDisponibles
          .GroupBy(x => new { x.CodigoGrano, x.NombreGrano })
          .Select(granoGroup =>
          {
            var granoCupos = granoGroup.ToList();

            // Zonas del grano (sin duplicar "Sin zona")
            var zonasGrano = granoCupos
              .GroupBy(z => z.ZonaGeograficaId)
              .Select(zg => new GeographicZone
                  {
                    Id = zg.Key,
                    Name = zg.First().ZonaGeografica is null or ""
                         ? "Zona no identificada"
                         : zg.First().ZonaGeografica,
                    Pendiente = zg.Sum(x => x.CuposDisponibles)
                  })
              .ToList();

            // Compradores
            var compradores = granoCupos
              .GroupBy(x => new { x.CuentaComprador })
              .Select(compGroup =>
                  {
                    var compradorCupos = compGroup.ToList();

                    // Zonas del comprador (sin duplicar). Excluimos ZonaGeograficaId == 0
                    // ("sin zona"): ya están representadas en la síntesis del comprador
                    // (comprador.Pendiente es la suma de todos los cupos) y el descuento
                    // de nivel 3 nunca las toca (el diccionario filtra ZonaGeograficaId > 0).
                    // Si las dejáramos acá, BuildCuposDisponiblesPorGrano agregaría dos
                    // filas con ZonaGeograficaId = 0: la síntesis y este "detalle".
                    var zonasComprador = compradorCupos
                      .Where(z => z.ZonaGeograficaId > 0)
                      .GroupBy(z => z.ZonaGeograficaId)
                      .Select(zg => new GeographicZone
                          {
                            Id = zg.Key,
                            Name = zg.First().ZonaGeografica is null or ""
                                 ? "Zona no identificada"
                                 : zg.First().ZonaGeografica,
                            Pendiente = Math.Max(0, zg.Sum(x => x.CuposDisponibles))
                          })
                      .ToList();

                    return new Comprador
                    {
                      Id = compGroup.Key.CuentaComprador,
                      Name = compGroup.First().NombreComprador?.Trim() ?? "",
                      Pendiente = Math.Max(0, compradorCupos.Sum(x => x.CuposDisponibles)),
                      GeographicZoneAvailableList = zonasComprador
                    };
                  })
              .ToList();

            return new AvailabilityByProduct
            {
              Id = int.Parse(granoGroup.Key.CodigoGrano),
              ProductName = granoGroup.Key.NombreGrano is null or ""
                            ? "Grano no identificado"
                            : granoGroup.Key.NombreGrano,
              Shift = Math.Max(0, granoCupos.Sum(x => x.CuposDisponibles)),
              CompradorAvailableList = compradores,
              GeographicZoneAvailableList = zonasGrano
            };
          })
          .ToList();
    }

    private IEnumerable<SolicitudTurnoPorGranoView> ConvertToSolicitudTurnoPorGranoView(IEnumerable<SolicitudTurnoView> todasSolicitudes)
    {
      IEnumerable<SolicitudTurnoPorGranoView> solicitudes = todasSolicitudes
        .GroupBy(s => new { s.CodigoGrano, s.NombreGrano })
        .Select(solPorGrano => new SolicitudTurnoPorGranoView
        {
          CodigoGrano = solPorGrano.Key.CodigoGrano,
          NombreGrano = string.IsNullOrEmpty(solPorGrano.Key.NombreGrano)
            ? ""
            : solPorGrano.Key.NombreGrano,
          DetallePendientes = solPorGrano
            .Where(sol => sol.EsPendiente)
            .GroupBy(sol => new
            {
              CuentaComprador = sol.CuentaComprador ?? 0,
              sol.NombreComprador,
              sol.NombreDestino,
              CuentaDestino = sol.CuentaDestino ?? 0
            })
            .Select(sol => new SolicitudTurnoPendienteGrupoDetalleSolicitadoView
            {
              CuentaComprador = sol.Key.CuentaComprador == 0 ? null : sol.Key.CuentaComprador,
              NombreComprador = string.IsNullOrEmpty(sol.Key.NombreComprador) ? "" : sol.Key.NombreComprador,
              CuentaDestino = sol.Key.CuentaDestino == 0 ? null : sol.Key.CuentaDestino,
              NombreDestino = string.IsNullOrEmpty(sol.Key.NombreDestino) ? "" : sol.Key.NombreDestino,
              DetallesSolicitadosDia = sol
                .GroupBy(s => s.FechaSolicitado)
                .Select(s => new SolicitudTurnoGrupoDetallePendienteDiaView
                {
                  Fecha = s.Key.ToString("dd/MM/yyyy"),
                  Cantidad = s.Sum(r => r.Cantidad > 0 ? r.Cantidad : 1)
                })
            })
        });
      return solicitudes;
    }

    private List<SolicitudesAgrupadas> FromSolicitudesDeTurnoPorGranoToSolicitudDeTurnoAgrupadas(IEnumerable<SolicitudTurnoPorGranoView> solicitudes)
    {
      List<SolicitudesAgrupadas> solicitudesAgrupadas = new List<SolicitudesAgrupadas>();

      solicitudesAgrupadas = solicitudes
          .SelectMany(sol => sol.DetallePendientes, (sol, det) => new { sol.CodigoGrano, sol.NombreGrano, Detalle = det })
          .SelectMany(x => x.Detalle.DetallesSolicitadosDia, (x, dia) => new SolicitudesAgrupadas
          {
            CodigoGrano = x.CodigoGrano,
            CuentaComprador = x.Detalle.CuentaComprador ?? 0,
            ZonaGeograficaId = x.Detalle.CuentaDestino ?? 0,
            Cantidad = dia.Cantidad
          })
          .ToList();

      return solicitudesAgrupadas;
    }

    /// <summary>
    /// Construye la vista en árbol CuposDisponiblesPorGrano a partir del
    /// AvailabilityByProduct (después de aplicado el descuento de 3 niveles).
    /// Por cada grano emite:
    ///   - 1 entrada con TotalGrano (= grano.Shift, resumen ya listo)
    ///   - Por cada comprador: 1 fila de síntesis con ZonaGeograficaId = 0
    ///   - Por cada zona del comprador: 1 fila de detalle con ZonaGeograficaId > 0
    /// Propaga CuentaVendedor y NombreVendedor (mismo valor para todas las filas, lo
    /// levanta la primera fila cruda de la SQL — filtrada por :cuentavendedor).
    /// El frontend no necesita sumar nada: el total viene en TotalGrano.
    /// </summary>
    private static List<CuposDisponiblesPorGrano> BuildCuposDisponiblesPorGrano(
      IEnumerable<AvailabilityByProduct> availability,
      long cuentaVendedor,
      string nombreVendedor)
    {
      var result = new List<CuposDisponiblesPorGrano>();
      if (availability is null) return result;

      foreach (var grano in availability)
      {
        string codigoGranoStr = grano.Id.ToString();
        var entry = new CuposDisponiblesPorGrano
        {
          CuentaVendedor = cuentaVendedor,
          NombreVendedor = nombreVendedor,
          CodigoGrano = codigoGranoStr,
          NombreGrano = grano.ProductName,
          TotalGrano = grano.Shift
        };

        if (grano.CompradorAvailableList is null)
        {
          result.Add(entry);
          continue;
        }

        foreach (var comprador in grano.CompradorAvailableList)
        {
          // 1) Síntesis del comprador (ZonaGeograficaId = 0)
          entry.Detalles.Add(new CuposDetalle
          {
            CuentaComprador = comprador.Id,
            NombreComprador = comprador.Name,
            ZonaGeograficaId = 0,
            ZonaGeografica = "",
            CuposDisponibles = comprador.Pendiente
          });

          if (comprador.GeographicZoneAvailableList is null) continue;

          // 2) Detalle por zona del comprador
          foreach (var zona in comprador.GeographicZoneAvailableList)
          {
            entry.Detalles.Add(new CuposDetalle
            {
              CuentaComprador = comprador.Id,
              NombreComprador = comprador.Name,
              ZonaGeograficaId = zona.Id,
              ZonaGeografica = zona.Name,
              CuposDisponibles = zona.Pendiente
            });
          }
        }

        result.Add(entry);
      }

      return result;
    }
    /// <summary>
    /// Este servicio provee la informacion de los cupos disponibles para ser distribuidos.
    /// Los filtros inicialmente provienen unicamente de la pantalla de aceptacion/rechazo de una solicitud de turnos
    /// </summary>
    /// <returns></returns>
    public async Task<List<CuposCorreResult>> GetCuposPorEstado(CuposFilter cuposFilter)
    {
      try
      {
        // Estas validaciones son defensa en profundidad; el controller ya valida
        // antes de llegar aquí, pero el servicio no debe asumir eso.
        if (cuposFilter is null)
          throw new SilDataException("Debe especificar filtros.", StatusCodes.Status400BadRequest);

        if (string.IsNullOrWhiteSpace(cuposFilter.Grano))
          throw new SilDataException("Debe especificar el grano.", StatusCodes.Status400BadRequest);

        if (!long.TryParse(cuposFilter.Grano, out long granoParsed))
          throw new SilDataException(
            "El campo Grano debe ser un valor numérico válido.", StatusCodes.Status400BadRequest);

        CuposCorreFilter cuposCorreFilter = new CuposCorreFilter
        {
          FechaDesde = (cuposFilter.Fecha is not null && cuposFilter.Fecha != default)
            ? cuposFilter.Fecha.Value
            : DateTime.Now,
          FechaHasta = (cuposFilter.Fecha is not null && cuposFilter.Fecha != default)
            ? cuposFilter.Fecha.Value
            : DateTime.Now.AddDays(20),
          Grano = granoParsed,
          status = cuposFilter.status
        };

        IEnumerable<CuposCorreResult> result =
          await _cuposDisponiblesStore.GetCuposByStatus(cuposCorreFilter);

        List<CuposCorreResult> cupos = result?.ToList() ?? new List<CuposCorreResult>();

        if (cuposFilter.Destinos is not null && cuposFilter.Destinos.Any())
          cupos = cupos
            .Where(x => x.CodDestino is not null && cuposFilter.Destinos.Contains(x.CodDestino))
            .ToList();

        if (!string.IsNullOrWhiteSpace(cuposFilter.CuentaVendedor))
          cupos = cupos
            .Where(x => x.CodVendSIL is null || x.CodVendSIL.Equals(cuposFilter.CuentaVendedor))
            .ToList();

        if (!string.IsNullOrWhiteSpace(cuposFilter.CuentaComprador))
          cupos = cupos
            .Where(x => x.CodCompSIL is null || x.CodCompSIL.Equals(cuposFilter.CuentaComprador))
            .ToList();

        return cupos;
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
          "Error inesperado en GetCuposPorEstado. Vendedor: {Vendedor} Grano: {Grano}",
          cuposFilter?.CuentaVendedor, cuposFilter?.Grano);
        throw new Exception(
          "Ocurrió un error inesperado al obtener los cupos por estado.", ex);
      }
    }
    private DateTime GetDate(DateTime fecha)
    {
      return new DateTime(fecha.Year, fecha.Month, fecha.Day);
    }
  }
}

using Domain.Entities.Externo;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SILData.DataAccess;
using SILData.Model;
using SILData.Model.SolicitudTurno;
using SILData.SilDataExceptions;

namespace SILData.Services
{
  /// <summary>
  /// Servicio del flujo de matching V2 para Distribución (Pantalla 3).
  /// NO reemplaza ni modifica <see cref="SolicitudTurnoService"/>: ambos
  /// coexisten y exponen endpoints separados
  /// (<c>POST /api/ShiftRequest/MatchesDistribucion</c> legacy +
  /// <c>POST /api/ShiftRequest/MatchesDistribucionV2</c> nuevo).
  ///
  /// Diferencia clave: en lugar de hacer dos fetch separados
  /// (cupos + solicitudes) y dejar que el motor itere el cartesiano, este
  /// servicio usa el INNER JOIN nativo de
  /// <see cref="ISolicitudTurnoStore.GetCuposConMatchesAsync"/>, que ya
  /// devuelve sólo los pares (cupo, solicitud) candidatos que cumplen los
  /// filtros del operador y las invariantes obligatorias del modelo. El
  /// motor <c>ACA.Matching.Engine.MatchingEngine</c> sólo clasifica cada
  /// par (Directo / Parcial / Condicional).
  /// </summary>
  public class SolicitudTurnoMatchingV2Service
  {
    private readonly ISolicitudTurnoStore _solicitudTurnoStore;
    private readonly ILogger<SolicitudTurnoMatchingV2Service> _logger;
    private readonly ACA.Matching.Engine.IMatchingEngine _matchingEngine;
    private readonly IZonaGeograficaResolver _zonaGeograficaResolver;
    private readonly string? _connectionString;
    private readonly IConfiguration _configuration;
    private readonly ICatalogCache? _catalogCache;

    public SolicitudTurnoMatchingV2Service(
      ISolicitudTurnoStore solicitudTurnoStore,
      ILogger<SolicitudTurnoMatchingV2Service> logger,
      IConfiguration configuration,
      ACA.Matching.Engine.IMatchingEngine matchingEngine,
      IZonaGeograficaResolver zonaGeograficaResolver,
      ICatalogCache? catalogCache = null)
    {
      _solicitudTurnoStore = solicitudTurnoStore;
      _logger = logger;
      _connectionString = configuration.GetConnectionString("SilConnection");
      _configuration = configuration;
      _catalogCache = catalogCache;
      _matchingEngine = matchingEngine;
      _zonaGeograficaResolver = zonaGeograficaResolver;
    }

    /// <summary>
    /// Ejecuta el flujo V2:
    ///  1. Valida filtros obligatorios (grano, comprador, puerto, fechaDesde..Hasta).
    ///  2. Llama a <see cref="ISolicitudTurnoStore.GetCuposConMatchesAsync"/> para
    ///     obtener los pares (cupo, solicitud) ya filtrados por SQL.
    ///  3. Resuelve zonas geográficas y nombres en una sola query batch.
    ///  4. Construye los DTO <see cref="Cupo"/> y
    ///     <see cref="ACA.Matching.Modelos.SolicitudMatching"/> para cada par.
    ///  5. Pasa cada par al motor para clasificación.
    ///  6. Emite <see cref="MatchesResultDto"/> con la misma forma que el endpoint
    ///     legacy, de modo que el MVC (<c>CuposMatchingController</c>) no necesita
    ///     cambios de DTO.
    /// </summary>
    public async Task<MatchesResultDto> BuscarMatchesV2Async(MatchesFilterDto filter)
    {
      using var perf = new PerfScope(_logger, "BuscarMatchesV2Async");

      if (filter is null)
        throw new SilDataException("El filtro no puede ser nulo.", StatusCodes.Status400BadRequest);

      if (filter.CodigoGrano <= 0)
        throw new SilDataException("CodigoGrano es obligatorio y debe ser positivo.", StatusCodes.Status400BadRequest);
      if (!filter.CuentaComprador.HasValue || filter.CuentaComprador.Value <= 0)
        throw new SilDataException("CuentaComprador es obligatorio y debe ser positivo en el flujo V2.", StatusCodes.Status400BadRequest);
      if (!filter.CuentaPuerto.HasValue || filter.CuentaPuerto.Value <= 0)
        throw new SilDataException("CuentaPuerto es obligatorio y debe ser positivo en el flujo V2.", StatusCodes.Status400BadRequest);

      var fechaDesde = filter.FechaDesde ?? DateTime.Now.Date;
      var fechaHasta = filter.FechaHasta ?? DateTime.Now.Date.AddDays(7);
      if (fechaDesde > fechaHasta)
        throw new SilDataException("FechaDesde no puede ser posterior a FechaHasta.", StatusCodes.Status400BadRequest);

      long vendedor = filter.CuentaVendedor ?? 0L;
      long comprador = filter.CuentaComprador!.Value;
      string puerto = filter.CuentaPuerto!.Value.ToString();

      bool optimizada = FeatureFlags.IsEnabled(_configuration, FeatureFlags.DistribucionV2Optimizada);
      bool rangoVacio = false;
      if (optimizada)
      {
        (fechaDesde, fechaHasta, rangoVacio) = AcotarRangoFechas(fechaDesde, fechaHasta, DateTime.Now.Date);
      }

      // 1) Traer los pares (cupo, solicitud) candidatos vía INNER JOIN nativo.
      var pares = rangoVacio
        ? new List<CupoConSolicitudMatchDto>()
        : (await _solicitudTurnoStore.GetCuposConMatchesAsync(
        filter.CodigoGrano,
        vendedor,
        comprador,
        puerto,
        filter.Codcentro,
        filter.Codcentrodist,
        fechaDesde,
        fechaHasta)).ToList();
      perf.Mark("consultaCuposSolicitudes");

      var resultado = new MatchesResultDto
      {
        FiltrosAplicados = new MatchesFiltrosAplicados
        {
          CuentaComprador = filter.CuentaComprador,
          CodigoGrano = filter.CodigoGrano,
          ZonaGeograficaId = filter.ZonaGeograficaId,
          CuentaVendedor = filter.CuentaVendedor,
          FechaDesde = fechaDesde,
          FechaHasta = fechaHasta,
          IncluirIncompatibles = filter.IncluirIncompatibles,
          CuentaPuerto = filter.CuentaPuerto,
          ZonasResueltas = new List<long>()
        },
        Resumen = new MatchesResumen
        {
          TotalCuposAnalizados = pares.Select(p => p.CupoId).Distinct().Count(),
          TotalSolicitudesAnalizadas = pares.Select(p => p.SolicitudId).Distinct().Count()
        }
      };

      if (pares.Count == 0)
      {
        _logger.LogInformation(
          "BuscarMatchesV2Async: 0 pares candidato para grano={Grano} comprador={Comprador} puerto={Puerto} vendedor={Vendedor} entre {Desde} y {Hasta}.",
          filter.CodigoGrano, comprador, puerto, vendedor, fechaDesde, fechaHasta);
        return resultado;
      }

      // 2) Resolver zonas geográficas para los cupos involucrados (en un solo
      //    batch) — el motor lo necesita para evaluar DestinoRule cuando
      //    TIPODEST = ZonaPortuaria.
      // 3) Hidratar nombres del vendedor (el resto de los nombres ya viene en
      //    cada par: NombreComprador, NombrePuerto, NombreGrano).
      ACA.Matching.Contexto.IContextoZonasConNombres ctxZonas;
      Dictionary<long, string> nombresVendedor;
      if (optimizada)
      {
        // Flag DistribucionV2Optimizada: ambas consultas son independientes
        // (cada una abre su propia conexión), así que se ejecutan en paralelo.
        var zonasTask = _zonaGeograficaResolver.ResolverConNombresAsync(
          pares.Select(p => p.CupoId).Distinct().ToList());
        var nombresTask = ResolverNombresVendedorFlagAsync(
          pares.Select(p => p.CupoVendedor).ToList());
        await Task.WhenAll(zonasTask, nombresTask);
        ctxZonas = await zonasTask;
        nombresVendedor = await nombresTask;
        perf.Mark("zonasYNombres");
      }
      else
      {
        ctxZonas = await _zonaGeograficaResolver.ResolverConNombresAsync(
          pares.Select(p => p.CupoId).Distinct());
        perf.Mark("zonas");

        nombresVendedor = await ResolverNombresVendedorFlagAsync(
          pares.Select(p => p.CupoVendedor));
        perf.Mark("nombresVendedor");
      }

      // 4) Clasificar cada par con el motor. Los pares ya cumplen las
      //    invariantes obligatorias; aquí sólo emitimos el MatchType.
      int directos = 0, parciales = 0, condicionales = 0, incompatibles = 0;
      foreach (var par in pares)
      {
        var cupo = BuildCupoEntity(par);
        var solicitud = BuildSolicitudMatching(par);
        var match = _matchingEngine.Evaluar(cupo, solicitud, ctxZonas);

        if (!match.Compatible)
        {
          incompatibles++;
          if (!filter.IncluirIncompatibles) continue;
        }

        resultado.Items.Add(new MatchItemDto
        {
          SolicitudId = par.SolicitudId,
          CupoId = par.CupoId,
          MatchType = match.Tipo?.ToString(),
          Razon = match.RazonIncompatibilidad,
          VendedorCoincide = match.VendedorCoincide,
          CompradorCoincide = match.CompradorCoincide,
          DestinoCoincide = match.DestinoCoincide,
          Solicitud = new MatchSolicitudCompleta
          {
            Id = par.SolicitudId,
            CuentaVendedor = par.SolicitudVendedor,
            NombreVendedor = par.NombreVendedor,
            CuentaComprador = par.SolicitudComprador,
            CodigoGrano = par.SolicitudGrano,
            CuentaDestino = par.SolicitudDestino,
            TipoDestino = par.SolicitudTipoDestino?.ToString(),
            Cantidad = par.Cantidad,
            CantidadDisponible = Math.Max(0, par.Cantidad - par.CantidadAceptada - par.CantidadRechazada),
            CantidadRechazada = par.CantidadRechazada,
            FechaSolicitado = par.SolicitudFecha,
            Observacion = par.Observacion,
            CuposAsociados = new CuposAsociadosDesglose
            {
              Total = par.Cantidad,
              // El SQL ya filtra pendientes + cupos disponibles, así que
              // estos valores coinciden con la realidad del par devuelto.
              Otorgados = par.CantidadAceptada > 0 ? 1 : 0,
              Pendientes = (par.Cantidad - par.CantidadAceptada - par.CantidadRechazada) > 0 ? 1 : 0,
              Rechazados = par.CantidadRechazada > 0 ? 1 : 0
            }
          },
          Cupo = new MatchCupoResumen
          {
            Id = par.CupoId,
            CodGrano = par.CupoGrano,
            CodVendSIL = par.CupoVendedor?.ToString(),
            CodCompSIL = par.CupoComprador?.ToString(),
            CodDestino = par.CupoPuerto,
            Fecha = par.CupoFecha,
            NombreVendedor = par.CupoVendedor.HasValue
              ? (nombresVendedor.TryGetValue(par.CupoVendedor.Value, out var nombre) ? nombre : null)
              : null,
            NombreComprador = par.NombreComprador,
            NombreDestino = par.NombrePuerto,
            NombreGrano = par.NombreGrano
          }
        });

        switch (match.Tipo)
        {
          case ACA.Matching.Modelos.MatchType.Directo: directos++; break;
          case ACA.Matching.Modelos.MatchType.Parcial: parciales++; break;
          case ACA.Matching.Modelos.MatchType.Condicional: condicionales++; break;
        }
      }

      resultado.Resumen.MatchesDirectos = directos;
      resultado.Resumen.MatchesParciales = parciales;
      resultado.Resumen.MatchesCondicionales = condicionales;
      resultado.Resumen.Incompatibles = incompatibles;
      perf.Mark("matching");

      _logger.LogInformation(
        "BuscarMatchesV2Async: {Pares} pares (cupos={Cupos}, solicitudes={Sols}) → Directos={D} Parciales={P} Condicionales={C} Incompatibles={I}.",
        pares.Count, resultado.Resumen.TotalCuposAnalizados, resultado.Resumen.TotalSolicitudesAnalizadas,
        directos, parciales, condicionales, incompatibles);

      return resultado;
    }

    /// <summary>
    /// Construye la entidad <see cref="Cupo"/> que el motor espera a partir del
    /// par aplanado. Sólo se hidratan los campos que el motor lee (Fecha,
    /// CodGrano, CodDestino, CodCompSIL, CodVendSIL, EstadoSTOP opcional);
    /// el resto queda en default.
    /// </summary>
    private static Cupo BuildCupoEntity(CupoConSolicitudMatchDto par)
    {
      return new Cupo
      {
        Id = par.CupoId,
        Fecha = par.CupoFecha,
        CodGrano = par.CupoGrano,
        CodDestino = par.CupoPuerto,
        CodCompSIL = par.CupoComprador?.ToString(),
        CodVendSIL = par.CupoVendedor?.ToString(),
        EstadoSIL = par.CupoEstado ?? 0
      };
    }

    /// <summary>
    /// Construye el DTO <see cref="ACA.Matching.Modelos.SolicitudMatching"/>
    /// a partir del par aplanado.
    /// </summary>
    private static ACA.Matching.Modelos.SolicitudMatching BuildSolicitudMatching(CupoConSolicitudMatchDto par)
    {
      ACA.Matching.Modelos.TipoDestino? tipoDestino = par.SolicitudTipoDestino switch
      {
        TipoDestino.ZonaPortuaria => ACA.Matching.Modelos.TipoDestino.ZonaPortuaria,
        TipoDestino.Destino => ACA.Matching.Modelos.TipoDestino.Destino,
        _ => null
      };

      return new ACA.Matching.Modelos.SolicitudMatching(
        Id: par.SolicitudId,
        Grano: par.SolicitudGrano.ToString(),
        Vendedor: par.SolicitudVendedor.ToString(),
        Comprador: par.SolicitudComprador?.ToString(),
        Destino: par.SolicitudDestino?.ToString(),
        TipoDestino: tipoDestino,
        Observaciones: par.Observacion);
    }

    /// <summary>
    /// Flag <c>Features:DistribucionV2Optimizada</c>: acota el rango de fechas
    /// que manda el cliente (hoy llega 01/01/2001–01/12/2200) a
    /// [hoy - MaxDiasAtras, hoy + MaxDiasAdelante]. Cada límite sólo aplica
    /// si está configurado con un valor &gt; 0 (<c>Features:DistribucionV2MaxDiasAtras</c>,
    /// <c>Features:DistribucionV2MaxDiasAdelante</c>); por defecto NO se acota.
    /// Devuelve <c>vacio=true</c> si tras acotar el rango queda invertido.
    /// </summary>
    internal (DateTime Desde, DateTime Hasta, bool Vacio) AcotarRangoFechas(DateTime desde, DateTime hasta, DateTime hoy)
    {
      int maxAtras = FeatureFlags.GetInt(_configuration, FeatureFlags.DistribucionV2MaxDiasAtras, 0);
      int maxAdelante = FeatureFlags.GetInt(_configuration, FeatureFlags.DistribucionV2MaxDiasAdelante, 0);

      var nuevoDesde = desde;
      var nuevoHasta = hasta;
      if (maxAtras > 0 && nuevoDesde < hoy.AddDays(-maxAtras))
        nuevoDesde = hoy.AddDays(-maxAtras);
      if (maxAdelante > 0 && nuevoHasta > hoy.AddDays(maxAdelante))
        nuevoHasta = hoy.AddDays(maxAdelante);

      if (nuevoDesde != desde || nuevoHasta != hasta)
      {
        _logger.LogInformation(
          "BuscarMatchesV2Async: rango de fechas acotado de {Desde:d}–{Hasta:d} a {NuevoDesde:d}–{NuevoHasta:d}.",
          desde, hasta, nuevoDesde, nuevoHasta);
      }

      return (nuevoDesde, nuevoHasta, nuevoDesde > nuevoHasta);
    }

    /// <summary>
    /// Nombres de vendedor: con <c>Features:CatalogCache</c> usa IMemoryCache;
    /// sin el flag, el camino legacy <see cref="ResolverNombresVendedorAsync"/>.
    /// </summary>
    private async Task<Dictionary<long, string>> ResolverNombresVendedorFlagAsync(IEnumerable<long?> codigosVend)
    {
      if (_catalogCache is null || !FeatureFlags.IsEnabled(_configuration, FeatureFlags.CatalogCache))
        return await ResolverNombresVendedorAsync(codigosVend);

      var cuentas = new HashSet<long>();
      foreach (var c in codigosVend)
      {
        if (c.HasValue && c.Value > 0) cuentas.Add(c.Value);
      }
      if (cuentas.Count == 0) return new Dictionary<long, string>();

      try
      {
        using var loggerFactory = LoggerFactory.Create(b => { });
        var lookup = new CupoCatalogoLookup(loggerFactory.CreateLogger<CupoCatalogoLookup>(), _connectionString);
        return await _catalogCache.GetNombresVendedorAsync(cuentas, lookup.FetchNombresVendedorAsync);
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "ResolverNombresVendedorFlagAsync: falló el batch de {Count} cuentas.", cuentas.Count);
        return new Dictionary<long, string>();
      }
    }

    /// <summary>
    /// Resuelve los nombres de los vendedores en batch a partir de las
    /// cuentas del cupo (CupoVendedor). Misma estrategia que
    /// <see cref="CupoCatalogoLookup.ResolverNombresVendedorAsync"/>,
    /// duplicada acá para no tocar el servicio legacy. Si el catálogo
    /// no conoce la cuenta, el nombre queda null.
    /// </summary>
    private async Task<Dictionary<long, string>> ResolverNombresVendedorAsync(IEnumerable<long?> codigosVend)
    {
      var cuentas = new HashSet<long>();
      foreach (var c in codigosVend)
      {
        if (c.HasValue && c.Value > 0) cuentas.Add(c.Value);
      }
      if (cuentas.Count == 0) return new Dictionary<long, string>();

      try
      {
        var dict = new Dictionary<string, string?>
        {
          ["ConnectionStrings:SilConnection"] = _connectionString
        };
        var configuration = new ConfigurationBuilder()
          .AddInMemoryCollection(dict)
          .Build();

        using var loggerFactory = LoggerFactory.Create(b => { });
        var storeLogger = loggerFactory.CreateLogger<VendedorStore>();
        var store = new VendedorStore(configuration, storeLogger);
        var service = new AccountService(store);
        var rows = await service.GetVendedoresByCuentas(cuentas.ToList());
        return rows
          .Where(r => r is not null && !string.IsNullOrWhiteSpace(r.Nombre))
          .GroupBy(r => r.Cuenta)
          .ToDictionary(g => g.Key, g => g.First().Nombre);
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "ResolverNombresVendedorAsync: falló el batch de {Count} cuentas.", cuentas.Count);
        return new Dictionary<long, string>();
      }
    }
  }
}
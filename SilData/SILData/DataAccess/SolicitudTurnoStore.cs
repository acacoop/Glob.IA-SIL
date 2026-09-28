using Dapper;
using Oracle.ManagedDataAccess.Client;
using Shared.ClassShared.Interfaces;
using SILData.Model;
using SILData.Model.SolicitudTurno;
using SILData.SilDataExceptions;
using System.Data;

namespace SILData.DataAccess
{
  public class SolicitudTurnoStore : ISolicitudTurnoStore
  {
    private string? _connectionString;
    private ILogger<SolicitudTurnoStore> _logger;
    private readonly ISILCuposStore? _silCuposStore;

    public SolicitudTurnoStore(
      IConfiguration configuration,
      ILogger<SolicitudTurnoStore> logger,
      ISILCuposStore? silCuposStore = null)
    {
      _connectionString = configuration.GetConnectionString("SilConnection");
      _logger = logger;
      _silCuposStore = silCuposStore;

    }
    public async Task<IEnumerable<SolicitudTurnoView>> GetAllAsync(DateTime desde, DateTime hasta)
    {
      try
      {
        using OracleConnection connection = new OracleConnection(_connectionString);

        string strQuery = @"SELECT S.CTACOMP as ""CuentaComprador"", cc.nombre as ""NombreComprador"",
            S.CTAVEND as ""CuentaVendedor"", cv.nombre as ""NombreVendedor"",
            S.DEST as ""CuentaDestino"", cp.nombre as ""NombreDestino"",
            S.GRANO as ""CodigoGrano"", cg.nombre as ""NombreGrano"",
            s.Fechasolicitada as ""Fechasolicitado"",
            s.CENTRO as ""CodigoCentro"",
            NVL(s.CANTIDAD, 1) as ""Cantidad"",
            NVL(s.CANTIDAD_FUTURO, 0) as ""CantidadFuturo"",
            NVL(s.CANTIDAD_ACEPTADA, 0) as ""CantidadAceptada"",
            NVL(s.CANTIDAD_FUTURO_ACEPTADA, 0) as ""CantidadFuturoAceptada"",
            cupo.status ""EstadoCupo"", cupo.estadocupocnrt ""CtgCupo""
            FROM SOLTURNOS s
            LEFT OUTER JOIN MVCUPOSCOMPRADOR cc ON s.ctacomp = cc.cuenta
            INNER JOIN MVCUPOSVENDEDOR cv ON CV.CUENTA = S.CTAVEND
            LEFT OUTER JOIN MVCuposPuerto cp ON CP.CUENTA = s.dest
            INNER JOIN MVCuposGrano cg ON s.grano = cg.grano
            LEFT OUTER JOIN cuposcorre cupo ON s.cupo_id = cupo.id
            WHERE s.Fechasolicitada BETWEEN :desde AND :hasta";

        var dictionary = new Dictionary<string, object>
          {
            { "@desde", desde.Date },
            { "@hasta", hasta.Date }
          };

        var parameters = new DynamicParameters(dictionary);
        var solicitudes = connection.QueryAsync<SolicitudTurnoView>(strQuery, parameters);
        return await solicitudes;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex, "Error de base de datos en GetAllAsync. Desde: {Desde} Hasta: {Hasta}", desde, hasta);
        throw new Exception("Error al consultar las solicitudes de turno.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetAllAsync");
        throw;
      }
    }
    public async Task<IEnumerable<SolicitudTurnoView>> GetByFilterAsync(SolicitudTurnosFilter solicitudTurnosFilter, DateTime fechaDesde, DateTime fechaHasta)
    {
      try
      {
        using OracleConnection connection = new OracleConnection(_connectionString);

        //INNER JOIN cuposgrano cg ON s.grano = cg.grano ....ver....
        string strQuery = @"SELECT S.solturnos_id as ""Id"", S.CTACOMP as ""CuentaComprador"", cc.nombre as ""NombreComprador"",
            S.CTAVEND as ""CuentaVendedor"", cv.nombre as ""NombreVendedor"",
            NVL(S.DEST, 0) as ""CuentaDestino"", S.TIPODEST as ""TipoDestino"",
            CASE WHEN s.TIPODEST = 0 then  ZG.NOMBRE ELSE cp.nombre END as ""NombreDestino"",
            S.GRANO as ""CodigoGrano"", cg.nombre as ""NombreGrano"",
            s.Fechasolicitada as ""Fechasolicitado"",
            s.FechaCreacion as ""FechaCreacion"",
            s.CENTRO as ""CodigoCentro"",
            s.FUTURO as ""EsFuturo"",
            NVL(s.CANTIDAD, 1) as ""Cantidad"",
            NVL(s.CANTIDAD_FUTURO, 0) as ""CantidadFuturo"",
            NVL(s.CANTIDAD_ACEPTADA, 0) as ""CantidadAceptada"",
            NVL(s.CANTIDAD_FUTURO_ACEPTADA, 0) as ""CantidadFuturoAceptada"",
            NVL(s.CANTIDAD_RECHAZADA, 0) as ""CantidadRechazada"",
            NVL(s.CANTIDAD_FUTURO_RECHAZADA, 0) as ""CantidadFuturoRechazada"",
            cupo.status ""EstadoCupo"", cupo.estadocupocnrt ""CtgCupo""
            FROM SOLTURNOS s
            LEFT OUTER JOIN MVCUPOSCOMPRADOR cc ON s.ctacomp = cc.cuenta
            INNER JOIN MVCUPOSVENDEDOR cv ON CV.CUENTA = S.CTAVEND
            LEFT OUTER JOIN MVCuposPuerto cp ON CP.CUENTA = s.dest
            LEFT OUTER JOIN ZONASGEOGRAFICAS ZG ON ZG.ZONAGEOID = s.dest
            LEFT JOIN MVCuposGrano cg ON s.grano = cg.grano
            LEFT OUTER JOIN cuposcorre cupo ON s.cupo_id = cupo.id
            WHERE s.ctavend = :vendedor AND
            (s.ctacomp = : comprador OR  0 = : comprador) AND
            (s.dest =: destino OR 0=: destino) AND
            s.Fechasolicitada BETWEEN :desde AND :hasta";

        var dictionary = new Dictionary<string, object>
          {
            { "@vendedor", solicitudTurnosFilter.CuentaVendedor },
            { "@comprador", solicitudTurnosFilter.CuentaComprador?? 0 },
            { "@destino", solicitudTurnosFilter.CuentaDestino?? 0 },
            { "@desde", fechaDesde.Date.ToString("dd/MM/yyyy") },
            { "@hasta", fechaHasta.Date.ToString("dd/MM/yyyy") }
          };

        var parameters = new DynamicParameters(dictionary);
        var solicitudes = connection.QueryAsync<SolicitudTurnoView>(strQuery, parameters);
        return await solicitudes;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex,
          "Error de base de datos en GetByFilterAsync(filter). Vendedor: {Vendedor}",
          solicitudTurnosFilter.CuentaVendedor);
        throw new Exception("Error al consultar las solicitudes por filtro.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByFilterAsync(filter)");
        throw;
      }
    }

    public async Task<IEnumerable<SolicitudTurnoView>> GetByMatchesFilterAsync(MatchesSolicitudFilter filter, DateTime fechaDesde, DateTime fechaHasta)
    {
      try
      {
        if (filter is null)
          throw new ArgumentNullException(nameof(filter));

        if (filter.CodigoGrano <= 0)
          throw new SilDataException("CodigoGrano es obligatorio y debe ser positivo para el matching.", StatusCodes.Status400BadRequest);

        using OracleConnection connection = new OracleConnection(_connectionString);

        // A diferencia de GetByFilterAsync(SolicitudTurnosFilter), esta consulta:
        //   • NO filtra por CuentaVendedor (es opcional: 0 = wildcard).
        //   • SÍ filtra por CodigoGrano (obligatorio en el motor de matching).
        //   • Comprador y destino siguen siendo opcionales con el mismo patrón
        //     "(col = :param OR 0 = :param)" — el valor 0 desactiva el filtro.
        string strQuery = @"SELECT S.solturnos_id as ""Id"", S.CTACOMP as ""CuentaComprador"", cc.nombre as ""NombreComprador"",
            S.CTAVEND as ""CuentaVendedor"", cv.nombre as ""NombreVendedor"",
            NVL(S.DEST, 0) as ""CuentaDestino"", S.TIPODEST as ""TipoDestino"",
            CASE WHEN s.TIPODEST = 0 then  ZG.NOMBRE ELSE cp.nombre END as ""NombreDestino"",
            S.GRANO as ""CodigoGrano"", cg.nombre as ""NombreGrano"",
            s.Fechasolicitada as ""Fechasolicitado"",
            s.FechaCreacion as ""FechaCreacion"",
            s.CENTRO as ""CodigoCentro"",
            s.FUTURO as ""EsFuturo"",
            NVL(s.CANTIDAD, 1) as ""Cantidad"",
            NVL(s.CANTIDAD_FUTURO, 0) as ""CantidadFuturo"",
            NVL(s.CANTIDAD_ACEPTADA, 0) as ""CantidadAceptada"",
            NVL(s.CANTIDAD_FUTURO_ACEPTADA, 0) as ""CantidadFuturoAceptada"",
            NVL(s.CANTIDAD_RECHAZADA, 0) as ""CantidadRechazada"",
            NVL(s.CANTIDAD_FUTURO_RECHAZADA, 0) as ""CantidadFuturoRechazada"",
            cupo.status ""EstadoCupo"", cupo.estadocupocnrt ""CtgCupo""
            FROM SOLTURNOS s
            LEFT OUTER JOIN MVCUPOSCOMPRADOR cc ON s.ctacomp = cc.cuenta
            INNER JOIN MVCUPOSVENDEDOR cv ON CV.CUENTA = S.CTAVEND
            LEFT OUTER JOIN MVCUPOSPUERTO cp ON CP.CUENTA = s.dest
            LEFT OUTER JOIN ZONASGEOGRAFICAS ZG ON ZG.ZONAGEOID = s.dest
            LEFT JOIN MVCuposGrano cg ON s.grano = cg.grano
            LEFT OUTER JOIN cuposcorre cupo ON s.cupo_id = cupo.id
            WHERE s.grano = :grano AND
            (s.ctavend = :vendedor OR 0 = :vendedor) AND
            (s.ctacomp = :comprador OR 0 = :comprador) AND
            (s.dest = :destino OR 0 = :destino) AND
            s.Fechasolicitada BETWEEN :desde AND :hasta
            AND (NVL(s.CANTIDAD, 1) - NVL(s.CANTIDAD_ACEPTADA, 0) - NVL(s.CANTIDAD_RECHAZADA, 0)) > 0";

        var dictionary = new Dictionary<string, object>
        {
          { "@grano", filter.CodigoGrano },
          { "@vendedor", filter.CuentaVendedor ?? 0 },
          { "@comprador", filter.CuentaComprador ?? 0 },
          { "@destino", filter.ZonaGeograficaId ?? 0 },
          { "@desde", fechaDesde.Date.ToString("dd/MM/yyyy") },
          { "@hasta", fechaHasta.Date.ToString("dd/MM/yyyy") }
        };

        var parameters = new DynamicParameters(dictionary);
        var solicitudes = connection.QueryAsync<SolicitudTurnoView>(strQuery, parameters);
        return await solicitudes;
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex,
          "Error de base de datos en GetByMatchesFilterAsync. Grano: {Grano} Desde: {Desde} Hasta: {Hasta}",
          filter?.CodigoGrano, fechaDesde, fechaHasta);
        throw new Exception("Error al consultar las solicitudes para matching.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByMatchesFilterAsync");
        throw;
      }
    }

    /// <summary>
    /// Devuelve TODAS las solicitudes de la ventana [fechaDesde, fechaHasta],
    /// sin filtrar por grano / vendedor / comprador / destino. Es la lectura
    /// única que alimenta el matching bulk por ventana
    /// (<c>POST /api/ShiftRequest/MatchesVentana</c>), en reemplazo de las N
    /// llamadas a <see cref="GetByMatchesFilterAsync"/> — una por fila de la
    /// grilla — que disparaba Pantalla 1.
    ///
    /// La lista de columnas es deliberadamente idéntica a la de
    /// <see cref="GetByMatchesFilterAsync"/>, incluido el hecho de NO traer
    /// <c>OBSERVA</c>: el motor de matching hoy evalúa estas solicitudes con
    /// <c>Observaciones = null</c>, y agregar la columna cambiaría la
    /// clasificación de los pares.
    ///
    /// Diferencias respecto de <see cref="GetByMatchesFilterAsync"/>:
    /// <list type="bullet">
    ///   <item>WHERE sólo por rango de fechas (sin grano/vendedor/comprador/destino).</item>
    ///   <item>Las fechas se bindean como DATE y no como string, que es lo
    ///     correcto contra una columna DATE y evita depender del
    ///     <c>NLS_DATE_FORMAT</c> de la sesión.</item>
    ///   <item>Los catálogos se leen de las vistas materializadas
    ///     (<c>MVCuposComprador</c>, <c>MVCuposVendedor</c>, <c>MVCuposPuerto</c>,
    ///     <c>MVCuposGrano</c>) en lugar de las tablas base, siguiendo el mismo
    ///     criterio que <c>QuerysShared/CuposCorre.sql</c> y
    ///     <c>CuposStop.sql</c>. <c>SOLTURNOS</c>, <c>ZONASGEOGRAFICAS</c> y
    ///     <c>cuposcorre</c> no tienen MV equivalente y quedan como están.</item>
    /// </list>
    ///
    /// El JOIN a <c>MVCuposVendedor</c> se mantiene INNER, igual que era contra
    /// <c>cuposvendedor</c>. Con una MV eso implica que una solicitud de un
    /// vendedor todavía no replicado no aparecería en la grilla, pero el alta de
    /// vendedores es esporádica y el refresh de la MV llega mucho antes de que
    /// haya solicitudes de ese vendedor: no compensa relajarlo a LEFT OUTER.
    /// </summary>
    public async Task<IEnumerable<SolicitudTurnoView>> GetVentanaParaMatchingAsync(
      DateTime fechaDesde,
      DateTime fechaHasta,
      List<string>? centros = null)
    {
      try
      {
        using OracleConnection connection = new OracleConnection(_connectionString);

        string strQuery = @"SELECT S.solturnos_id as ""Id"", S.CTACOMP as ""CuentaComprador"", cc.nombre as ""NombreComprador"",
            S.CTAVEND as ""CuentaVendedor"", cv.nombre as ""NombreVendedor"",
            NVL(S.DEST, 0) as ""CuentaDestino"", S.TIPODEST as ""TipoDestino"",
            CASE WHEN s.TIPODEST = 0 then  ZG.NOMBRE ELSE cp.nombre END as ""NombreDestino"",
            S.GRANO as ""CodigoGrano"", cg.nombre as ""NombreGrano"",
            s.Fechasolicitada as ""Fechasolicitado"",
            s.FechaCreacion as ""FechaCreacion"",
            s.CENTRO as ""CodigoCentro"",
            s.FUTURO as ""EsFuturo"",
            NVL(s.CANTIDAD, 1) as ""Cantidad"",
            NVL(s.CANTIDAD_FUTURO, 0) as ""CantidadFuturo"",
            NVL(s.CANTIDAD_ACEPTADA, 0) as ""CantidadAceptada"",
            NVL(s.CANTIDAD_FUTURO_ACEPTADA, 0) as ""CantidadFuturoAceptada"",
            NVL(s.CANTIDAD_RECHAZADA, 0) as ""CantidadRechazada"",
            NVL(s.CANTIDAD_FUTURO_RECHAZADA, 0) as ""CantidadFuturoRechazada"",
            cupo.status ""EstadoCupo"", cupo.estadocupocnrt ""CtgCupo""
            FROM SOLTURNOS s
            LEFT OUTER JOIN MVCuposComprador cc ON s.ctacomp = cc.cuenta
            INNER JOIN MVCuposVendedor cv ON CV.CUENTA = S.CTAVEND
            LEFT OUTER JOIN MVCuposPuerto cp ON CP.CUENTA = s.dest
            LEFT OUTER JOIN ZONASGEOGRAFICAS ZG ON ZG.ZONAGEOID = s.dest
            LEFT JOIN MVCuposGrano cg ON s.grano = cg.grano
            LEFT OUTER JOIN cuposcorre cupo ON s.cupo_id = cupo.id
            WHERE s.Fechasolicitada BETWEEN :desde AND :hasta";

        var parameters = new DynamicParameters();
        parameters.Add("@desde", fechaDesde.Date);
        parameters.Add("@hasta", fechaHasta.Date);

        // Misma regla de visibilidad que GetByFilterAsync(centros): la
        // solicitud sin zona geográfica la puede tomar cualquier operador; la
        // que tiene zona, sólo el operador cuyo centro sea el de esa zona.
        // Acotarlo acá evita traer y evaluar solicitudes que el operador nunca
        // va a ver en la grilla.
        if (centros is not null && centros.Count > 0)
        {
          strQuery += @"
              AND (s.DEST IS NULL OR ZG.CENTROID IN :centros)";
          parameters.Add("@centros", centros);
        }

        var solicitudes = await connection.QueryAsync<SolicitudTurnoView>(strQuery, parameters);

        _logger.LogInformation(
          "GetVentanaParaMatchingAsync: {Count} solicitudes entre {Desde} y {Hasta}. Centros: {Centros}.",
          solicitudes.Count(), fechaDesde, fechaHasta,
          centros is null || centros.Count == 0 ? "(todos)" : string.Join(",", centros));

        return solicitudes;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex,
          "Error de base de datos en GetVentanaParaMatchingAsync. Desde: {Desde} Hasta: {Hasta}",
          fechaDesde, fechaHasta);
        throw new Exception("Error al consultar las solicitudes de la ventana para matching.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetVentanaParaMatchingAsync");
        throw;
      }
    }
    public async Task<IEnumerable<SolicitudTurnoView>> GetForMatchingAsync(
        int codigoGrano,
        long cuentaVendedor,
        long cuentaComprador,
        long zonaGeograficaId,
        DateTime fechaDesde,
        DateTime fechaHasta)
    {
      try
      {
        if (codigoGrano <= 0)
          throw new SilDataException("CodigoGrano es obligatorio y debe ser positivo para el matching.", StatusCodes.Status400BadRequest);

        using OracleConnection connection = new OracleConnection(_connectionString);

        // Query específica del matching bulk. A diferencia de GetByMatchesFilterAsync:
        //   • Filtra sólo por grano (obligatorio), vendedor (si > 0) y fecha.
        //   • Para comprador y destino: trae los que coincidan exactamente con el
        //     valor del cupo O estén NULL en la solicitud (eso será Parcial, lo
        //     evalúa el motor después). Descarta los que tienen un comprador o
        //     destino distinto al del cupo: eso no debería aparecer en Distribución.
        //   • Una sola tabla SOLTURNOS con joins a catálogos mínimos.
        //   • Incluye las columnas necesarias para que el frontend compute
        //     CantidadDisponible y el motor evalúe Vendedor/Comprador/Destino.
        string strQuery = @"SELECT S.solturnos_id as ""Id"", S.CTACOMP as ""CuentaComprador"", cc.nombre as ""NombreComprador"",
            S.CTAVEND as ""CuentaVendedor"", cv.nombre as ""NombreVendedor"",
            NVL(S.DEST, 0) as ""CuentaDestino"", S.TIPODEST as ""TipoDestino"",
            CASE WHEN s.TIPODEST = 0 then  ZG.NOMBRE ELSE cp.nombre END as ""NombreDestino"",
            S.GRANO as ""CodigoGrano"", cg.nombre as ""NombreGrano"",
            s.Fechasolicitada as ""Fechasolicitado"",
            s.FechaCreacion as ""FechaCreacion"",
            s.CENTRO as ""CodigoCentro"",
            s.FUTURO as ""EsFuturo"",
            NVL(s.CANTIDAD, 1) as ""Cantidad"",
            NVL(s.CANTIDAD_FUTURO, 0) as ""CantidadFuturo"",
            NVL(s.CANTIDAD_ACEPTADA, 0) as ""CantidadAceptada"",
            NVL(s.CANTIDAD_FUTURO_ACEPTADA, 0) as ""CantidadFuturoAceptada"",
            NVL(s.CANTIDAD_RECHAZADA, 0) as ""CantidadRechazada"",
            NVL(s.CANTIDAD_FUTURO_RECHAZADA, 0) as ""CantidadFuturoRechazada"",
            cupo.status ""EstadoCupo"", cupo.estadocupocnrt ""CtgCupo""
            FROM SOLTURNOS s
            LEFT OUTER JOIN MVCUPOSCOMPRADOR cc ON s.ctacomp = cc.cuenta
            INNER JOIN MVCUPOSVENDEDOR cv ON CV.CUENTA = S.CTAVEND
            LEFT OUTER JOIN MVCuposPuerto cp ON CP.CUENTA = s.dest
            LEFT OUTER JOIN ZONASGEOGRAFICAS ZG ON ZG.ZONAGEOID = s.dest
            LEFT JOIN MVCuposGrano cg ON s.grano = cg.grano
            LEFT OUTER JOIN cuposcorre cupo ON s.cupo_id = cupo.id
            WHERE s.grano = :grano
              AND (0 = :vendedor OR s.ctavend = :vendedor)
              AND (
                0 = :comprador
                OR s.ctacomp = :comprador
                OR s.ctacomp IS NULL
              )
              AND (
                0 = :destino
                OR s.dest = :destino
                OR s.dest IS NULL
              )
              AND s.Fechasolicitada BETWEEN :desde AND :hasta";

        var dictionary = new Dictionary<string, object>
        {
          { "@grano", codigoGrano },
          { "@vendedor", cuentaVendedor },
          { "@comprador", cuentaComprador },
          { "@destino", zonaGeograficaId },
          { "@desde", fechaDesde.Date.ToString("dd/MM/yyyy") },
          { "@hasta", fechaHasta.Date.ToString("dd/MM/yyyy") }
        };

        var parameters = new DynamicParameters(dictionary);
        var solicitudes = connection.QueryAsync<SolicitudTurnoView>(strQuery, parameters);
        return await solicitudes;
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex,
          "Error de base de datos en GetForMatchingAsync. Grano: {Grano} Comprador: {Comprador} Destino: {Destino}",
          codigoGrano, cuentaComprador, zonaGeograficaId);
        throw new Exception("Error al consultar las solicitudes para matching.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetForMatchingAsync");
        throw;
      }
    }

    public async Task<IEnumerable<CupoConSolicitudMatchDto>> GetCuposConMatchesAsync(
        int codigoGrano,
        long vendedor,
        long cuentaComprador,
        string cuentaPuerto,
        string codcentro,
        string codcentrodist,
        DateTime fechaDesde,
        DateTime fechaHasta)
    {
      try
      {
        if (codigoGrano <= 0)
          throw new SilDataException("CodigoGrano es obligatorio y debe ser positivo para el matching.", StatusCodes.Status400BadRequest);
        if (cuentaComprador <= 0)
          throw new SilDataException("cuentaComprador es obligatorio y debe ser positivo para el matching V2.", StatusCodes.Status400BadRequest);
        if (string.IsNullOrWhiteSpace(cuentaPuerto))
          throw new SilDataException("cuentaPuerto es obligatorio para el matching V2.", StatusCodes.Status400BadRequest);
        if (string.IsNullOrWhiteSpace(codcentro))
          throw new SilDataException("codcentro es obligatorio para el matching V2 (filtra cuposcorre por el centro que el operador está distribuyendo).", StatusCodes.Status400BadRequest);
        if (string.IsNullOrWhiteSpace(codcentrodist))
          throw new SilDataException("codcentrodist es obligatorio para el matching V2 (filtra cuposcorre por el centro de distribución del cupo).", StatusCodes.Status400BadRequest);

        using OracleConnection connection = new OracleConnection(_connectionString);

        // Query unificada V2: INNER JOIN entre cuposcorre y SOLTURNOS, aplicando
        // los filtros del operador y las invariantes obligatorias del modelo.
        // El motor ACA.Matching.Engine sólo clasifica cada par devuelto.
        //
        // Aliases Oracle entre comillas dobles para que Dapper mapee por
        // nombre al DTO CupoConSolicitudMatchDto (case-insensitive pero
        // respetando el casing del DTO).
        string strQuery = @"SELECT
            c.Id                              AS ""CupoId"",
            c.Fecha                           AS ""CupoFecha"",
            c.Grano                           AS ""CupoGrano"",
            c.VENDCTA                         AS ""CupoVendedor"",
            c.COMPCTA                         AS ""CupoComprador"",
            c.PUERTOCTA                       AS ""CupoPuerto"",
            c.STATUS                          AS ""CupoEstado"",
            s.solturnos_id                    AS ""SolicitudId"",
            s.CTAVEND                         AS ""SolicitudVendedor"",
            s.CTACOMP                         AS ""SolicitudComprador"",
            s.DEST                            AS ""SolicitudDestino"",
            s.TIPODEST                        AS ""SolicitudTipoDestino"",
            s.GRANO                           AS ""SolicitudGrano"",
            s.Fechasolicitada                 AS ""SolicitudFecha"",
            s.CENTRO                          AS ""SolicitudCentro"",
            CASE WHEN s.FUTURO = 1 THEN 1 ELSE 0 END AS ""EsFuturo"",
            NVL(s.CANTIDAD, 1)                AS ""Cantidad"",
            NVL(s.CANTIDAD_ACEPTADA, 0)       AS ""CantidadAceptada"",
            NVL(s.CANTIDAD_RECHAZADA, 0)      AS ""CantidadRechazada"",
            NVL(s.CANTIDAD_FUTURO, 0)         AS ""CantidadFuturo"",
            NVL(s.CANTIDAD_FUTURO_ACEPTADA,0) AS ""CantidadFuturoAceptada"",
            NVL(s.CANTIDAD_FUTURO_RECHAZADA,0)AS ""CantidadFuturoRechazada"",
            s.OBSERVA                     AS ""Observacion"",
            cc.NOMBRE                         AS ""NombreComprador"",
            cv.NOMBRE                         AS ""NombreVendedor"",
            cp.NOMBRE                         AS ""NombrePuerto"",
            cg.NOMBRE                         AS ""NombreGrano""
            FROM cuposcorre c
            INNER JOIN SOLTURNOS s
                   ON s.GRANO          = c.Grano
                  AND s.Fechasolicitada = c.Fecha
                  AND (
                       c.VENDCTA IS NULL OR c.VENDCTA = 0
                       OR s.CTAVEND = c.VENDCTA
                      )
                  AND (
                       s.CTACOMP IS NULL
                       OR s.CTACOMP = c.COMPCTA
                      )
                  AND (
                       s.DEST IS NULL
                       OR EXISTS (
                            SELECT 1 FROM puertoporzona ppz
                             WHERE ppz.cuenta  = c.PUERTOCTA
                               AND ppz.zonageoid = s.DEST
                          )
                      )
                  AND (NVL(s.CANTIDAD_ACEPTADA,0) + NVL(s.CANTIDAD_RECHAZADA,0)) < NVL(s.CANTIDAD, 1)
            LEFT OUTER JOIN MVCUPOSCOMPRADOR cc ON cc.CUENTA = s.CTACOMP
            INNER JOIN MVCUPOSVENDEDOR cv        ON cv.CUENTA  = s.CTAVEND
            LEFT OUTER JOIN MVCuposPuerto cp      ON cp.CUENTA  = c.PUERTOCTA
            INNER JOIN MVCuposGrano cg           ON cg.GRANO   = s.GRANO
            WHERE c.STATUS = 0
              AND c.TIPO  = 1
              AND c.Centro = :codcentro
              AND c.CentroDist = :codcentrodist
              AND c.Fecha BETWEEN :fechaDesde AND :fechaHasta
              AND c.Grano = :codigoGrano
              AND c.COMPCTA  = :cuentaComprador
              AND c.PUERTOCTA = :cuentaPuerto
              AND (
                   :vendedor > 0 AND c.VENDCTA = :vendedor
                   OR :vendedor = 0 AND (c.VENDCTA IS NULL OR c.VENDCTA = 0)
                  )";

        var dictionary = new Dictionary<string, object>
        {
          { "@codigoGrano", codigoGrano.ToString() },
          { "@vendedor", vendedor },
          { "@cuentaComprador", cuentaComprador },
          { "@cuentaPuerto", cuentaPuerto },
          { "@codcentro", codcentro },
          { "@codcentrodist", codcentrodist },
          { "@fechaDesde", fechaDesde.Date },
          { "@fechaHasta", fechaHasta.Date }
        };

        var parameters = new DynamicParameters(dictionary);
        var pares = await connection.QueryAsync<CupoConSolicitudMatchDto>(strQuery, parameters);
        return pares;
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex,
          "Error de base de datos en GetCuposConMatchesAsync. Grano: {Grano} Vendedor: {Vendedor} Comprador: {Comprador} Puerto: {Puerto} Centro: {Centro} CentroDist: {CentroDist}",
          codigoGrano, vendedor, cuentaComprador, cuentaPuerto, codcentro, codcentrodist);
        throw new Exception("Error al consultar cupos+solicitudes para matching V2.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetCuposConMatchesAsync");
        throw;
      }
    }

    public async Task<IEnumerable<SolicitudTurnoView>> GetByFilterAsync(DateTime fechaDesde, DateTime fechaHasta, List<string> centros)
    {
      try
      {
        if (centros is null || !centros.Any())
          throw new SilDataException("Debe indicar al menos un centro para filtrar las solicitudes.", StatusCodes.Status400BadRequest);

        using OracleConnection connection = new OracleConnection(_connectionString);

        string strQuery = @"SELECT S.solturnos_id as ""Id"", S.CTACOMP as ""CuentaComprador"", cc.nombre as ""NombreComprador"",
            S.CTAVEND as ""CuentaVendedor"", cv.nombre as ""NombreVendedor"",
            S.DEST as ""CuentaDestino"", S.TIPODEST as ""TipoDestino"", CASE WHEN s.TIPODEST = 0 then  ZG.NOMBRE ELSE cp.nombre END as ""NombreDestino"",
            S.GRANO as ""CodigoGrano"", cg.nombre as ""NombreGrano"",
            s.Fechasolicitada as ""Fechasolicitado"",
            s.FechaCreacion as ""FechaCreacion"",
            ZG.CENTROID as ""CodigoCentro"",
            ZG.NOMBRE as ""NombreCentro"",
            s.FUTURO as ""EsFuturo"",
            NVL(s.CANTIDAD, 1) as ""Cantidad"",
            NVL(s.CANTIDAD_FUTURO, 0) as ""CantidadFuturo"",
            NVL(s.CANTIDAD_ACEPTADA, 0) as ""CantidadAceptada"",
            NVL(s.CANTIDAD_FUTURO_ACEPTADA, 0) as ""CantidadFuturoAceptada"",
            NVL(s.CANTIDAD_RECHAZADA, 0) as ""CantidadRechazada"",
            NVL(s.CANTIDAD_FUTURO_RECHAZADA, 0) as ""CantidadFuturoRechazada"",
            cupo.status as ""EstadoCupo"",
            cupo.estadocupocnrt as ""CtgCupo"",
            s.OBSERVA as ""Observacion""
            FROM SOLTURNOS s
            LEFT OUTER JOIN MVCUPOSCOMPRADOR cc ON s.ctacomp = cc.cuenta
            INNER JOIN MVCUPOSVENDEDOR cv ON CV.CUENTA = S.CTAVEND
            LEFT OUTER JOIN MVCUPOSPUERTO cp ON CP.CUENTA = s.dest
			      LEFT OUTER JOIN ZONASGEOGRAFICAS ZG ON ZG.ZONAGEOID = s.dest
            LEFT JOIN MVCUPOSGRANO cg ON s.grano = cg.grano
            LEFT OUTER JOIN cuposcorre cupo ON s.cupo_id = cupo.id
            WHERE s.Fechasolicitada BETWEEN :desde AND :hasta
              -- Regla de centros para Pantalla 1 (Index de Solicitudes):
              --   • Si la solicitud NO tiene DEST (no se pidió por zona
              --     geográfica), se muestra siempre: no hay forma de
              --     asociarla a un centro vía ZONASGEOGRAFICAS.
              --   • Si la solicitud TIENE DEST (se pidió por zona
              --     geográfica), se resuelve la zona en ZONASGEOGRAFICAS
              --     por ZONAGEOID = s.DEST y se muestra únicamente si su
              --     CENTROID está en la lista de centros del operador.
              --   • Si s.DEST existe pero NO hay fila en ZONASGEOGRAFICAS,
              --     el LEFT OUTER JOIN deja ZG.CENTROID en NULL, el IN
              --     evalúa NULL y la fila queda excluida: sin zona
              --     resuelta no hay forma de saber a qué centro
              --     pertenece, así que no se muestra.
              -- El LEFT OUTER JOIN a ZONASGEOGRAFICAS ya está en el FROM
              -- de esta query (línea anterior), por lo tanto ZG.CENTROID
              -- es directamente accesible acá.
              AND (s.DEST IS NULL OR ZG.CENTROID IN :centros)
              AND (NVL(s.CANTIDAD, 1) - NVL(s.CANTIDAD_ACEPTADA, 0) - NVL(s.CANTIDAD_RECHAZADA, 0)) > 0";

        var dictionary = new Dictionary<string, object>
          {
            { "@centros", centros },
            { "@desde", fechaDesde.Date.ToString("dd/MM/yyyy") },
            { "@hasta", fechaHasta.Date.ToString("dd/MM/yyyy") }
          };

        var parameters = new DynamicParameters(dictionary);
        var solicitudes = connection.QueryAsync<SolicitudTurnoView>(strQuery, parameters);
        return await solicitudes;

      }
      catch (SilDataException)
      {
        throw;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex, "Error de base de datos en GetByFilterAsync(centros). Centros: {Centros}", string.Join(",", centros));
        throw new Exception("Error al consultar las solicitudes pendientes por centro.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByFilterAsync(centros)");
        throw;
      }
    }
    public async Task<IEnumerable<SolicitudTurnoView>> GetByVendedorAsync(long cuentaVendedor, DateTime fechaDesde, DateTime fechaHasta)
    {
      try
      {
        using OracleConnection connection = new OracleConnection(_connectionString);

        string strQuery = @"SELECT S.solturnos_id as ""Id"", S.CTACOMP as ""CuentaComprador"", cc.nombre as ""NombreComprador"",
            S.CTAVEND as ""CuentaVendedor"", cv.nombre as ""NombreVendedor"",
            S.DEST as ""CuentaDestino"", S.TIPODEST as ""TipoDestino"", cp.nombre as ""NombreDestino"", 
            S.GRANO as ""CodigoGrano"", cg.nombre as ""NombreGrano"",
            s.Fechasolicitada as ""Fechasolicitado"",
            s.CENTRO as ""CodigoCentro"",
            NVL(s.CANTIDAD, 1) as ""Cantidad"",
            NVL(s.CANTIDAD_FUTURO, 0) as ""CantidadFuturo"",
            NVL(s.CANTIDAD_ACEPTADA, 0) as ""CantidadAceptada"",
            NVL(s.CANTIDAD_FUTURO_ACEPTADA, 0) as ""CantidadFuturoAceptada"",
            NVL(s.CANTIDAD_RECHAZADA, 0) as ""CantidadRechazada"",
            NVL(s.CANTIDAD_FUTURO_RECHAZADA, 0) as ""CantidadFuturoRechazada"",
            cupo.status ""EstadoCupo"", cupo.estadocupocnrt ""CtgCupo""
            FROM SOLTURNOS s
            LEFT OUTER JOIN MVCUPOSCOMPRADOR cc ON s.ctacomp = cc.cuenta
            INNER JOIN MVCUPOSVENDEDOR cv ON CV.CUENTA = S.CTAVEND
            LEFT OUTER JOIN MVCUPOSPUERTO cp ON CP.CUENTA = s.dest
            INNER JOIN MVCUPOSGRANO cg ON s.grano = cg.grano
            LEFT OUTER JOIN cuposcorre cupo ON s.cupo_id = cupo.id
            WHERE s.ctavend = :vendedor
            AND s.Fechasolicitada BETWEEN :desde AND :hasta";

        var dictionary = new Dictionary<string, object>
          {
            { "@vendedor", cuentaVendedor },
            { "@desde", fechaDesde.Date.ToString("dd/MM/yyyy") },
            { "@hasta", fechaHasta.Date.ToString("dd/MM/yyyy") }
        };

        var parameters = new DynamicParameters(dictionary);
        var solicitudes = connection.QueryAsync<SolicitudTurnoView>(strQuery, parameters);
        return await solicitudes;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex, "Error de base de datos en GetByVendedorAsync. Vendedor: {Vendedor}", cuentaVendedor);
        throw new Exception("Error al consultar las solicitudes del vendedor.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByVendedorAsync. Vendedor: {Vendedor}", cuentaVendedor);
        throw;
      }
    }
    public async Task InsertAsync(IList<SolicitudTurno> solicitudes)
    {
      await SaveAsync(solicitudes ?? Array.Empty<SolicitudTurno>(), Array.Empty<SolicitudTurno>(), Array.Empty<long>());
    }

    public async Task SaveAsync(IList<SolicitudTurno> inserciones, IList<SolicitudTurno> actualizaciones, IList<long> idsEliminar)
    {
      inserciones ??= Array.Empty<SolicitudTurno>();
      actualizaciones ??= Array.Empty<SolicitudTurno>();
      idsEliminar ??= Array.Empty<long>();

      if (!inserciones.Any() && !actualizaciones.Any() && !idsEliminar.Any())
        throw new SilDataException("No hay solicitudes para guardar.", StatusCodes.Status400BadRequest);

      using OracleConnection connection = new OracleConnection(_connectionString);

      const string sqlInsert = @"INSERT INTO SOLTURNOS (CTACOMP, CTAVEND, DEST, TIPODEST, GRANO, FECHACREACION, FECHASOLICITADA, FUTURO, CENTRO, OBSERVA, CUPO_ID, CANTIDAD, CANTIDAD_FUTURO)
          VALUES (
          :CTACOMP,
          :CTAVEND,
          :DEST,
          :TIPODEST,
          :GRANO,
          :FECHACREACION,
          :FECHASOLICITADA,
          :FUTURO,
          :CENTRO,
          :OBSERVA,
          :CUPO_ID,
          :CANTIDAD,
          :CANTIDAD_FUTURO)";

      const string sqlUpdate = @"UPDATE SOLTURNOS
          SET CANTIDAD = :CANTIDAD,
              CANTIDAD_FUTURO = :CANTIDAD_FUTURO,
              FUTURO = :FUTURO,
              FECHACREACION = :FECHACREACION,
              OBSERVA = :OBSERVA
          WHERE solturnos_id = :Id";

      const string sqlDelete = @"DELETE FROM SOLTURNOS WHERE solturnos_id IN :Ids";

      var insertParams = inserciones.Select(solicitud => new
      {
        CTACOMP = solicitud.CuentaComprador,
        CTAVEND = solicitud.CuentaVendedor,
        DEST = solicitud.CuentaDestino,
        TIPODEST = TipoDestino.ZonaPortuaria,
        GRANO = solicitud.CodigoGrano,
        FECHACREACION = solicitud.FechaCreacion,
        FECHASOLICITADA = solicitud.FechaSolicitado.Date,
        FUTURO = solicitud.EsFuturo ? 1 : 0,
        CENTRO = solicitud.CodigoCentro,
        OBSERVA = solicitud.Observacion,
        CUPO_ID = solicitud.CupoId,
        CANTIDAD = solicitud.Cantidad > 0 ? solicitud.Cantidad : 1,
        CANTIDAD_FUTURO = solicitud.CantidadFuturo > 0 ? solicitud.CantidadFuturo : 0
      }).ToList();

      var updateParams = actualizaciones.Select(solicitud => new
      {
        Id = solicitud.Id,
        CANTIDAD = solicitud.Cantidad > 0 ? solicitud.Cantidad : 1,
        CANTIDAD_FUTURO = solicitud.CantidadFuturo > 0 ? solicitud.CantidadFuturo : 0,
        FUTURO = solicitud.EsFuturo ? 1 : 0,
        FECHACREACION = solicitud.FechaCreacion,
        OBSERVA = solicitud.Observacion
      }).ToList();

      await connection.OpenAsync();
      OracleTransaction transaction = connection.BeginTransaction();
      try
      {
        int totalRows = 0;

        if (idsEliminar.Any())
          totalRows += await connection.ExecuteAsync(sqlDelete, new { Ids = idsEliminar.ToList() }, transaction);

        if (updateParams.Any())
          totalRows += await connection.ExecuteAsync(sqlUpdate, updateParams, transaction);

        if (insertParams.Any())
          totalRows += await connection.ExecuteAsync(sqlInsert, insertParams, transaction);

        if (totalRows == 0)
        {
          await transaction.RollbackAsync();
          throw new SilDataException("No se guardó ninguna solicitud. Verifique los datos enviados.", StatusCodes.Status409Conflict);
        }

        await transaction.CommitAsync();
        _logger.LogInformation(
          "SaveAsync: {Insert} insertadas, {Update} actualizadas, {Delete} eliminadas.",
          insertParams.Count, updateParams.Count, idsEliminar.Count);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (OracleException ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error de base de datos en SaveAsync. Se realizó rollback.");
        throw new Exception("Error al guardar las solicitudes de turno en la base de datos.", ex);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error inesperado en SaveAsync. Se realizó rollback.");
        throw;
      }
    }
    public async Task UpdateCantidadAsync(long solicitudId, int cantidad)
    {
      if (solicitudId <= 0)
        throw new SilDataException("El ID de solicitud no es válido.", StatusCodes.Status400BadRequest);

      if (cantidad <= 0)
        throw new SilDataException("La cantidad debe ser mayor a cero.", StatusCodes.Status400BadRequest);

      using OracleConnection connection = new OracleConnection(_connectionString);
      const string sqlStatement = @"UPDATE SOLTURNOS SET CANTIDAD = :Cantidad WHERE solturnos_id = :Id";

      await connection.OpenAsync();
      OracleTransaction transaction = connection.BeginTransaction();
      try
      {
        int rowsAffected = await connection.ExecuteAsync(
          sqlStatement,
          new { Cantidad = cantidad, Id = solicitudId },
          transaction);

        if (rowsAffected == 0)
        {
          await transaction.RollbackAsync();
          throw new SilDataException(
            "No se actualizó la solicitud. Es posible que el ID indicado no exista.",
            StatusCodes.Status409Conflict);
        }

        await transaction.CommitAsync();
        _logger.LogInformation("UpdateCantidadAsync: solicitud {Id} actualizada a cantidad {Cantidad}.", solicitudId, cantidad);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (OracleException ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error de base de datos en UpdateCantidadAsync. Id: {Id}", solicitudId);
        throw new Exception("Error al actualizar la cantidad de la solicitud de turno.", ex);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error inesperado en UpdateCantidadAsync. Id: {Id}", solicitudId);
        throw;
      }
    }

    /// <summary>
    /// Devuelve la entidad <see cref="SolicitudTurno"/> por id. Usado por el MVC
    /// para construir el payload de Accept sin duplicar estado en el cliente.
    /// </summary>
    /// <remarks>
    /// Mapea el row de SOLTURNOS a la entidad. Sigue la convención del resto del
    /// store: <c>NUMBER(19)→long</c>, <c>DATE→DateTime</c>, <c>NUMBER(2)→int</c>.
    /// <c>TIPODEST</c> se mapea al enum <see cref="TipoDestino"/> (default
    /// <c>ZonaPortuaria</c> si la fila tiene NULL).
    /// </remarks>
    /// <param name="id">Id de la solicitud.</param>
    /// <returns>Entidad, o <c>null</c> si no existe.</returns>
    public async Task<SolicitudTurno?> GetByIdAsync(long id)
    {
      if (id <= 0) return null;

      const string sql = @"
        SELECT solturnos_id                AS ""Id"",
               CTACOMP                     AS ""CuentaComprador"",
               CTAVEND                     AS ""CuentaVendedor"",
               DEST                        AS ""CuentaDestino"",
               TIPODEST                    AS ""TipoDestino"",
               GRANO                       AS ""CodigoGrano"",
               FECHACREACION               AS ""FechaCreacion"",
               FECHASOLICITADA             AS ""FechaSolicitado"",
               FUTURO                      AS ""EsFuturo"",
               CENTRO                      AS ""CodigoCentro"",
               OBSERVA                     AS ""Observacion"",
               CUPO_ID                     AS ""CupoId"",
               CANTIDAD                    AS ""Cantidad"",
               CANTIDAD_FUTURO             AS ""CantidadFuturo"",
               CANTIDAD_ACEPTADA           AS ""CantidadAceptada"",
               CANTIDAD_FUTURO_ACEPTADA    AS ""CantidadFuturoAceptada"",
               CANTIDAD_RECHAZADA          AS ""CantidadRechazada"",
               CANTIDAD_FUTURO_RECHAZADA   AS ""CantidadFuturoRechazada""
          FROM SOLTURNOS
         WHERE solturnos_id = :Id";

      try
      {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.QuerySingleOrDefaultAsync<SolicitudTurno>(sql, new { Id = id });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en GetByIdAsync para solicitud {Id}", id);
        throw;
      }
    }

    /// <summary>Tamaño de lote para listas IN (Oracle corta en 1000, ORA-01795).</summary>
    internal const int TamanioLoteIn = 900;

    /// <summary>Mismas columnas/mapeo que <see cref="GetByIdAsync"/>.</summary>
    private const string SelectSolicitudTurnoSql = @"
        SELECT solturnos_id                AS ""Id"",
               CTACOMP                     AS ""CuentaComprador"",
               CTAVEND                     AS ""CuentaVendedor"",
               DEST                        AS ""CuentaDestino"",
               TIPODEST                    AS ""TipoDestino"",
               GRANO                       AS ""CodigoGrano"",
               FECHACREACION               AS ""FechaCreacion"",
               FECHASOLICITADA             AS ""FechaSolicitado"",
               FUTURO                      AS ""EsFuturo"",
               CENTRO                      AS ""CodigoCentro"",
               OBSERVA                     AS ""Observacion"",
               CUPO_ID                     AS ""CupoId"",
               CANTIDAD                    AS ""Cantidad"",
               CANTIDAD_FUTURO             AS ""CantidadFuturo"",
               CANTIDAD_ACEPTADA           AS ""CantidadAceptada"",
               CANTIDAD_FUTURO_ACEPTADA    AS ""CantidadFuturoAceptada"",
               CANTIDAD_RECHAZADA          AS ""CantidadRechazada"",
               CANTIDAD_FUTURO_RECHAZADA   AS ""CantidadFuturoRechazada""
          FROM SOLTURNOS";

    public async Task<Dictionary<long, SolicitudTurno>> GetByIdsAsync(IEnumerable<long> ids)
    {
      var resultado = new Dictionary<long, SolicitudTurno>();
      var lista = (ids ?? Enumerable.Empty<long>()).Where(id => id > 0).Distinct().ToList();
      if (lista.Count == 0) return resultado;

      string sql = SelectSolicitudTurnoSql + @"
         WHERE solturnos_id IN :Ids";

      try
      {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        foreach (var lote in lista.Chunk(TamanioLoteIn))
        {
          var rows = await connection.QueryAsync<SolicitudTurno>(sql, new { Ids = lote });
          foreach (var row in rows)
            resultado[row.Id] = row;
        }
        return resultado;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en GetByIdsAsync para {Count} solicitudes", lista.Count);
        throw;
      }
    }

    public async Task DeleteAsync(IList<long> solicitudIds)
    {
      if (solicitudIds is null || solicitudIds.Count == 0)
        throw new SilDataException("No se indicaron IDs de solicitudes para eliminar.", StatusCodes.Status400BadRequest);

      using OracleConnection connection = new OracleConnection(_connectionString);

      string sqlStatement = @"DELETE FROM SOLTURNOS WHERE solturnos_id IN :Ids";

      await connection.OpenAsync();
      OracleTransaction transaction = connection.BeginTransaction();

      try
      {
        int rowsAffected = await connection.ExecuteAsync(sqlStatement, new { Ids = solicitudIds }, transaction);

        if (rowsAffected == 0)
        {
          await transaction.RollbackAsync();
          throw new SilDataException(
            "No se eliminó ninguna solicitud. Es posible que los IDs indicados no existan o ya hayan sido procesados.",
            StatusCodes.Status409Conflict);
        }

        await transaction.CommitAsync();
        _logger.LogInformation("DeleteAsync: {Count} solicitudes eliminadas correctamente.", rowsAffected);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (OracleException ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error de base de datos en DeleteAsync. IDs: {Ids}. Se realizó rollback.", string.Join(",", solicitudIds));
        throw new Exception("Error al eliminar las solicitudes de turno en la base de datos.", ex);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error inesperado en DeleteAsync. Se realizó rollback.");
        throw;
      }

    }

    public async Task<IList<AcceptOperationResult>> AcceptRequestsAsync(IList<AcceptOperation> operations)
    {
      operations ??= Array.Empty<AcceptOperation>();

      var resultados = new List<AcceptOperationResult>();

      if (!operations.Any())
        return resultados;

      // ── Modelo "Detalle Acumulativo" (v2) ───────────────────────────────
      // Por cada AcceptOperation:
      //   1) IncrementAcceptedAsync: UPDATE de SOLTURNOS bajo la guardia
      //      "CantidadAceptada < Cantidad AND CantidadRechazada = 0 AND
      //      CUPO_ID IS NULL". Incrementa el acumulador correspondiente
      //      (cantidad_aceptada o cantidad_futuro_aceptada).
      //      rowcount=0 ⇒ conflicto de concurrencia (otro operador actuó antes).
      //   2) Si el UPDATE prosperó, se insertan N filas en SOLTURNOS_DETALLE
      //      (una por cada cupo aceptado). La constraint UNIQUE (solicitud_id,
      //      cupo_id) garantiza idempotencia.
      // Todo se ejecuta dentro de UNA transacción. Si cualquier paso falla,
      // rollback completo: ni el acumulador se incrementa ni se insertan
      // filas de detalle (evita inconsistencia entre SOLTURNOS y
      // SOLTURNOS_DETALLE).
      using var connection = new OracleConnection(_connectionString);
      await connection.OpenAsync();
      using var transaction = connection.BeginTransaction();
      try
      {
        int exitos = 0;
        int conflictos = 0;

        foreach (AcceptOperation op in operations)
        {
          // Defensa: filtrar cupos duplicados dentro de la misma operación
          // (debería venir limpio del servicio, pero resguardamos).
          var cuposUnicos = (op.CuposAsignados ?? new List<long>())
            .Where(id => id > 0)
            .Distinct()
            .ToList();

          if (cuposUnicos.Count == 0)
          {
            _logger.LogWarning(
              "AcceptRequestsAsync: la solicitud {Id} llegó sin cupos a asignar. Se omite.",
              op.SolicitudId);
            resultados.Add(new AcceptOperationResult
            {
              SolicitudId = op.SolicitudId,
              Exitoso = false,
              MotivoFalla = "La operación no incluye cupos a asignar."
            });
            conflictos++;
            continue;
          }

          // 1) Incrementar acumulador en SOLTURNOS (concurrencia optimista).
          bool ok = await IncrementAcceptedAsync(
            op.SolicitudId,
            op.SolicitudEsFuturo,
            cuposUnicos.Count,
            transaction);

          if (!ok)
          {
            conflictos++;
            _logger.LogWarning(
              "Conflicto de concurrencia en AcceptRequestsAsync: la solicitud {Id} ya no está en estado Pendiente o ya tiene un cupo asignado. Se omiten los {N} cupos de esta operación.",
              op.SolicitudId, cuposUnicos.Count);

            resultados.Add(new AcceptOperationResult
            {
              SolicitudId = op.SolicitudId,
              Exitoso = false,
              MotivoFalla = "La solicitud ya no está en estado Pendiente o ya tiene un cupo asignado."
            });
            continue;
          }

          // 2) Insertar líneas de detalle (una por cupo aceptado).
          await InsertAsignacionesDetalleAsync(
            op.SolicitudId,
            cuposUnicos,
            transaction);

          exitos++;

          resultados.Add(new AcceptOperationResult
          {
            SolicitudId = op.SolicitudId,
            Exitoso = true,
            CupoAsignadoId = cuposUnicos[0],
            CuposAsignados = cuposUnicos
          });
        }

        await transaction.CommitAsync();

        _logger.LogInformation(
          "AcceptRequestsAsync: {Exitos} operaciones exitosas, {Conflictos} con conflicto de concurrencia.",
          exitos, conflictos);

        return resultados;
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error al aceptar solicitudes en SOLTURNOS (modelo acumulativo).");
        throw;
      }
    }

    public async Task<IList<(long SolicitudId, bool Aceptado)>> RejectRequestsAsync(IList<long> solicitudIds)
    {
      solicitudIds ??= new List<long>();

      if (solicitudIds.Count == 0)
        throw new SilDataException("No se indicaron IDs de solicitudes para rechazar.", StatusCodes.Status400BadRequest);

      // Deduplicar y descartar IDs no positivos para evitar UPDATE inútil.
      var idsValidos = solicitudIds.Where(id => id > 0).Distinct().ToList();
      if (idsValidos.Count == 0)
        throw new SilDataException("Los IDs de solicitudes proporcionados no son válidos.", StatusCodes.Status400BadRequest);

      // Modelo "Detalle Acumulativo": rechazar una solicitud implica:
      //   1) Congelar el pendiente implícito como rechazado:
      //        cantidad_rechazada = cantidad - cantidad_aceptada (al momento)
      //        cantidad_futuro_rechazada = cantidad_futuro - cantidad_futuro_aceptada
      //      Esto permite saber cuántos cupos quedaron "sin asignar" al cierre
      //      de la solicitud y mantener el invariante
      //      cantidad_aceptada + cantidad_rechazada ≤ cantidad.
      //   2) La guardia es por acumuladores:
      //        NVL(cantidad_aceptada,0) + NVL(cantidad_rechazada,0) < NVL(cantidad,1)
      //      Garantiza que la solicitud aún tenga cupos por "congelar"; si ya
      //      fue aceptada o rechazada completamente, rowcount = 0.
      //
      // Lo que NO se hace (decisión confirmada con el usuario):
      //   - NO se borran filas de SOLTURNOS_DETALLE — los cupos ya aceptados
      //     conservan su historial (log inmutable de aceptaciones reales).
      //   - NO se libera cupo en CUPOSCORRE — las aceptaciones previas son
      //     reales; el rechazo significa "no se aceptan más cupos", no
      //     "deshacer todo".
      //   - NO se resetean los acumuladores cantidad_aceptada /
      //     cantidad_futuro_aceptada — preservan el histórico.
      const string sqlUpdate = @"
        UPDATE SOLTURNOS
           SET cantidad_rechazada        = NVL(cantidad, 0)        - NVL(cantidad_aceptada, 0),
               cantidad_futuro_rechazada = NVL(cantidad_futuro, 0) - NVL(cantidad_futuro_aceptada, 0)
         WHERE solturnos_id = :Id
           AND NVL(cantidad_aceptada, 0) + NVL(cantidad_rechazada, 0) < NVL(cantidad, 1)";

      var resultado = new List<(long, bool)>(idsValidos.Count);

      using var connection = new OracleConnection(_connectionString);
      await connection.OpenAsync();
      using var transaction = connection.BeginTransaction();
      try
      {
        foreach (long id in idsValidos)
        {
          int rowsAffected = await connection.ExecuteAsync(
            sqlUpdate,
            new { Id = id },
            transaction);

          resultado.Add((id, rowsAffected > 0));
        }

        await transaction.CommitAsync();

        int rechazadas = resultado.Count(r => r.Item2);
        int fallidas = resultado.Count(r => !r.Item2);
        _logger.LogInformation(
          "RejectRequestsAsync: {Rechazadas} rechazadas, {Fallidas} sin efecto (ya procesadas o sin pendientes).",
          rechazadas, fallidas);

        return resultado;
      }
      catch (SilDataException)
      {
        await transaction.RollbackAsync();
        throw;
      }
      catch (OracleException ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error de base de datos en RejectRequestsAsync. IDs: {Ids}", string.Join(",", idsValidos));
        throw new Exception("Error al rechazar las solicitudes de turno en la base de datos.", ex);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error inesperado en RejectRequestsAsync. Se realizó rollback.");
        throw;
      }
    }

    // ====================================================================
    // SOLTURNOS_DETALLE — soporte para acumulación de aceptados
    // ====================================================================

    /// <summary>
    /// UPDATE atómico de SOLTURNOS: incrementa el acumulador
    /// <c>CantidadAceptada</c> (si <paramref name="esFuturo"/> es false)
    /// o <c>CantidadFuturoAceptada</c> (si es true). La guardia por
    /// acumuladores <c>CantidadAceptada &lt; Cantidad AND CantidadRechazada
    /// = 0 AND CUPO_ID IS NULL</c> mantiene la concurrencia optimista: si la
    /// solicitud ya no está disponible, rowcount = 0 y se devuelve
    /// <c>false</c>.
    /// </summary>
    public async Task<bool> IncrementAcceptedAsync(
      long solicitudId,
      bool esFuturo,
      int incremento,
      IDbTransaction transaction)
    {
      if (solicitudId <= 0)
        throw new ArgumentOutOfRangeException(nameof(solicitudId), "Id de solicitud inválido.");
      if (incremento <= 0)
        throw new ArgumentOutOfRangeException(nameof(incremento), "El incremento debe ser positivo.");

      const string sql = @"
        UPDATE SOLTURNOS
           SET cantidad_aceptada         = cantidad_aceptada + CASE WHEN :EsFuturo = 0 THEN :Inc ELSE 0 END,
               cantidad_futuro_aceptada  = cantidad_futuro_aceptada + CASE WHEN :EsFuturo = 1 THEN :Inc ELSE 0 END
         WHERE solturnos_id = :SolicitudId
           AND NVL(cantidad_aceptada, 0) < NVL(cantidad, 1)
           AND NVL(cantidad_rechazada, 0) = 0
           AND cupo_id IS NULL";

      var connection = transaction.Connection;
      if (connection == null)
        throw new InvalidOperationException("La transacción no tiene conexión asociada.");

      int rowsAffected = await connection.ExecuteAsync(sql, new
      {
        SolicitudId = solicitudId,
        EsFuturo = esFuturo ? 1 : 0,
        Inc = incremento
      }, transaction);

      return rowsAffected > 0;
    }

    /// <summary>
    /// Inserta N filas en <c>SOLTURNOS_DETALLE</c>, una por cada cupo
    /// aceptado. Cada fila representa exclusivamente una solicitud que
    /// matcheó con un cupo y fue aceptada; el detalle no necesita una
    /// columna de estado. Se ejecuta dentro de la transacción del caller
    /// para mantener atomicidad con <see cref="IncrementAcceptedAsync"/>.
    /// </summary>
    /// <param name="solicitudId">Id de la solicitud.</param>
    /// <param name="cupoIds">Cupos aceptados (uno por fila detalle, sin duplicados).</param>
    /// <param name="transaction">Transacción abierta del caller.</param>
    public async Task InsertAsignacionesDetalleAsync(
      long solicitudId,
      IReadOnlyList<long> cupoIds,
      IDbTransaction transaction)
    {
      if (solicitudId <= 0)
        throw new ArgumentOutOfRangeException(nameof(solicitudId), "Id de solicitud inválido.");

      var cupoIdsList = (cupoIds ?? Array.Empty<long>())
        .Where(id => id > 0)
        .Distinct()
        .ToList();
      if (cupoIdsList.Count == 0) return;

      const string sql = @"
        INSERT INTO SOLTURNOS_DETALLE
          (solicitud_id, cupo_id, fecha_creacion)
        VALUES
          (:solicitud_id, :cupo_id, SYSDATE)";

      var insertParams = cupoIdsList.Select(cupoId => new
      {
        solicitud_id = solicitudId,
        cupo_id = cupoId
      }).ToList();

      var connection = transaction.Connection;
      if (connection == null)
        throw new InvalidOperationException("La transacción no tiene conexión asociada.");

      await connection.ExecuteAsync(sql, insertParams, transaction);

      _logger.LogInformation(
        "InsertAsignacionesDetalleAsync: {N} filas insertadas para solicitud {SolicitudId}.",
        insertParams.Count, solicitudId);
    }

    /// <summary>
    /// Devuelve el resumen del estado de aceptación para una solicitud,
    /// calculado desde los 3 acumuladores de SOLTURNOS:
    /// <list type="bullet">
    ///   <item><c>Asignados</c> = <c>CantidadAceptada</c> (siempre).</item>
    ///   <item><c>Pendientes</c> = <c>Cantidad - CantidadAceptada - CantidadRechazada</c>.</item>
    ///   <item><c>Rechazados</c> = <c>CantidadRechazada</c> (cero hasta que se rechace la solicitud).</item>
    /// </list>
    /// </summary>
    public async Task<Dictionary<long, List<long>>> GetCuposAceptadosPorSolicitudesAsync(
      IEnumerable<long> solicitudIds)
    {
      var resultado = new Dictionary<long, List<long>>();

      if (solicitudIds == null) return resultado;

      var ids = solicitudIds.Where(id => id > 0).Distinct().ToList();
      if (ids.Count == 0) return resultado;

      try
      {
        using OracleConnection connection = new OracleConnection(_connectionString);

        // Una sola query batched: trae todos los (solicitud_id, cupo_id) del
        // detalle para los ids solicitados. La tabla tiene
        // idx_soldet_solicitud, así que es barato. Estado fijo = 1 = Asignado
        // (los rechazados nunca viven en detalle).
        const string sql = @"
          SELECT d.solicitud_id AS ""SolicitudId"",
                 d.cupo_id      AS ""CupoId""
            FROM SOLTURNOS_DETALLE d
           WHERE d.solicitud_id IN :ids";

        var parameters = new
        {
          Ids = ids
        };

        var filas = await connection.QueryAsync<dynamic>(sql, parameters);

        foreach (var f in filas)
        {
          long solicitudId = Convert.ToInt64(f.SolicitudId);
          long cupoId = Convert.ToInt64(f.CupoId);
          if (!resultado.TryGetValue(solicitudId, out var lista))
          {
            lista = new List<long>();
            resultado[solicitudId] = lista;
          }
          lista.Add(cupoId);
        }

        _logger.LogInformation(
          "GetCuposAceptadosPorSolicitudesAsync: {N} cupos aceptados para {S} solicitudes.",
          resultado.Values.Sum(l => l.Count), ids.Count);
        return resultado;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex,
          "Error de BD en GetCuposAceptadosPorSolicitudesAsync. CantIds: {N}",
          ids.Count);
        throw new Exception("Error al consultar los cupos aceptados.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetCuposAceptadosPorSolicitudesAsync");
        throw;
      }
    }

    /// <summary>
    /// Devuelve el resumen del estado de aceptación para una solicitud,
    /// calculado desde los 3 acumuladores de SOLTURNOS:
    /// <list type="bullet">
    ///   <item><c>Asignados</c> = <c>CantidadAceptada</c> (siempre).</item>
    ///   <item><c>Pendientes</c> = <c>Cantidad - CantidadAceptada - CantidadRechazada</c>.</item>
    ///   <item><c>Rechazados</c> = <c>CantidadRechazada</c> (cero hasta que se rechace la solicitud).</item>
    /// </list>
    /// </summary>
    public async Task<DetalleEstadoResumen> GetDetalleResumenAsync(long solicitudId)
    {
      if (solicitudId <= 0)
        return new DetalleEstadoResumen();

      try
      {
        var sol = await GetByIdAsync(solicitudId);
        if (sol == null) return new DetalleEstadoResumen();

        int asignados = sol.CantidadAceptada;
        int rechazados = sol.CantidadRechazada;
        int pendientes = Math.Max(0, sol.Cantidad - sol.CantidadAceptada - sol.CantidadRechazada);

        return new DetalleEstadoResumen
        {
          Asignados = asignados,
          Pendientes = pendientes,
          Rechazados = rechazados
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en GetDetalleResumenAsync para solicitud {Id}", solicitudId);
        throw;
      }
    }

    // ====================================================================
    // Anulación de distribución (batch)
    // ====================================================================

    /// <summary>
    /// Anula la distribución de N cupos en una sola llamada HTTP. Cada
    /// cupo se procesa de forma independiente bajo su propia transacción:
    /// <list type="number">
    ///   <item>Lookup en <c>SOLTURNOS_DETALLE</c> para resolver
    ///     <c>solicitud_id</c>. Si no hay fila, el item se reporta como
    ///     <c>Skipped</c> y no se toca la BD (el cupo fue anulado por el
    ///     flujo legacy y no requiere reversión lógica).</item>
    ///   <item><c>UPDATE SOLTURNOS</c>: <c>CANTIDAD_ACEPTADA -= 1</c>,
    ///     <c>CANTIDAD += 1</c>, bajo la guardia <c>CANTIDAD_ACEPTADA &gt; 0
    ///     AND CANTIDAD_RECHAZADA = 0</c>. Si rowcount = 0, se hace
    ///     rollback y se reporta Fallo (la solicitud ya no estaba
    ///     pendiente).</item>
    ///   <item><c>DELETE SOLTURNOS_DETALLE</c> para el par
    ///     <c>(solicitud_id, cupo_id)</c>.</item>
    /// </list>
    /// NO toca CUPOSCORRE — el cupo ya fue marcado como anulado por el flujo
    /// legacy de Anular en CuposDataController.
    ///
    /// Procesamiento per-cupo (no per-batch): si la reversión de un cupo
    /// falla, no afecta a los demás. El orden de los items de salida
    /// coincide con el orden de los ids de entrada.
    /// </summary>
    public async Task<List<AnularDistribucionItemResult>> AnularDistribucionPorCuposAsync(IList<long> cupoIds)
    {
      // Deduplicar y descartar ids no positivos para evitar UPDATE inútil
      // y para que el orden de salida sea 1:1 con los ids únicos.
      var idsValidos = (cupoIds ?? new List<long>())
        .Where(id => id > 0)
        .Distinct()
        .ToList();

      var resultados = new List<AnularDistribucionItemResult>();

      if (idsValidos.Count == 0)
        return resultados;

      foreach (long cupoId in idsValidos)
      {
        resultados.Add(await AnularDistribucionSingleAsync(cupoId));
      }

      int exitos = resultados.Count(r => r.Estado == AnularDistribucionItemEstado.Exitoso);
      int skipped = resultados.Count(r => r.Estado == AnularDistribucionItemEstado.Skipped);
      int fallos = resultados.Count(r => r.Estado == AnularDistribucionItemEstado.Fallo);

      _logger.LogInformation(
        "AnularDistribucionPorCuposAsync: {N} cupos procesados. Exitosos={E}, Skipped={S}, Fallos={F}.",
        idsValidos.Count, exitos, skipped, fallos);

      return resultados;
    }

    /// <summary>
    /// Reversión de la distribución de UN solo cupo bajo su propia
    /// transacción. Helper privado de <see cref="AnularDistribucionPorCuposAsync"/>;
    /// aislado para que cada cupo sea atómico de forma independiente.
    /// </summary>
    private async Task<AnularDistribucionItemResult> AnularDistribucionSingleAsync(long cupoId)
    {
      // SQL 1 — Lookup en SOLTURNOS_DETALLE. Resuelve la solicitud asociada
      // al cupo dentro de la misma transacción (FOR UPDATE evita que entre
      // esta lectura y el UPDATE otro operador nos gane la palabra).
      const string sqlLookup = @"
        SELECT solicitud_id
          FROM SOLTURNOS_DETALLE
         WHERE cupo_id = :CupoId
         FOR UPDATE";

      // SQL 2 — UPDATE en SOLTURNOS: devuelve el cupo al pool pendiente.
      // La guardia verifica que la solicitud tenga al menos un cupo aceptado
      // para devolver. NO exige cantidad_rechazada = 0: la anulación de una
      // distribución puede (y suele) ocurrir DESPUÉS de un rechazo parcial,
      // por ejemplo:
      //   • estado previo:  cantidad=5, cantidad_aceptada=3,
      //                     cantidad_rechazada=2  (solicitud cerrada)
      //   • al anular 1 de los 3 aceptados:
      //                     cantidad=5, cantidad_aceptada=2,
      //                     cantidad_rechazada=2  → pendiente = 1 > 0
      //                     → la solicitud vuelve a aparecer en Pantalla 1.
      //
      // Importante: NO se modifica la columna `cantidad` porque representa
      // la cantidad solicitada original (es inmutable desde el Alta de la
      // solicitud). Lo único que vuelve al pool pendiente es el cupo
      // aceptado, no se "suman" cupos extra.
      //
      // Si la solicitud ya no tiene cupos aceptados para devolver
      // (cantidad_aceptada = 0) el rowcount del UPDATE será 0 y el item
      // se reporta como Fallo (otro operador ya revirtió antes).
      const string sqlUpdateSol = @"
        UPDATE SOLTURNOS
           SET cantidad_aceptada = cantidad_aceptada - 1
         WHERE solturnos_id      = :SolicitudId
           AND NVL(cantidad_aceptada, 0) > 0";

      // SQL 3 — DELETE en SOLTURNOS_DETALLE. La UNIQUE(solicitud_id, cupo_id)
      // garantiza que el WHERE afecta a lo sumo 1 fila.
      const string sqlDeleteDet = @"
        DELETE FROM SOLTURNOS_DETALLE
         WHERE solicitud_id = :SolicitudId
           AND cupo_id      = :CupoId";

      try
      {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        long solicitudId = await connection.ExecuteScalarAsync<long>(
          sqlLookup,
          new { CupoId = cupoId },
          transaction);

        if (solicitudId <= 0)
        {
          await transaction.RollbackAsync();
          // Cupo sin distribución activa — el flujo legacy de Anular ya
          // lo cubrió en CUPOSCORRE. No es error: Skipped.
          _logger.LogInformation(
            "AnularDistribucionPorCupoAsync: el cupo {CupoId} no tiene fila en SOLTURNOS_DETALLE. Skipped.",
            cupoId);
          return new AnularDistribucionItemResult
          {
            CupoId = cupoId,
            Estado = AnularDistribucionItemEstado.Skipped,
            SolicitudId = 0,
            MotivoFalla = "El cupo no tenía distribución activa en SOLTURNOS_DETALLE."
          };
        }

        int rowsUpdate = await connection.ExecuteAsync(
          sqlUpdateSol,
          new { SolicitudId = solicitudId },
          transaction);

        if (rowsUpdate == 0)
        {
          await transaction.RollbackAsync();
          _logger.LogWarning(
            "AnularDistribucionPorCupoAsync: el cupo {CupoId} apuntaba a la solicitud {SolicitudId}, pero la guardia del UPDATE no prosperó.",
            cupoId, solicitudId);
          return new AnularDistribucionItemResult
          {
            CupoId = cupoId,
            Estado = AnularDistribucionItemEstado.Fallo,
            SolicitudId = solicitudId,
            MotivoFalla = "La solicitud asociada ya no estaba en estado pendiente o no tenía cupos aceptados para devolver."
          };
        }

        int rowsDelete = await connection.ExecuteAsync(
          sqlDeleteDet,
          new { SolicitudId = solicitudId, CupoId = cupoId },
          transaction);

        await transaction.CommitAsync();

        _logger.LogInformation(
          "AnularDistribucionPorCupoAsync: cupo {CupoId} desasignado de solicitud {SolicitudId}. UPDATE filas={U}, DELETE filas={D}.",
          cupoId, solicitudId, rowsUpdate, rowsDelete);

        return new AnularDistribucionItemResult
        {
          CupoId = cupoId,
          Estado = AnularDistribucionItemEstado.Exitoso,
          SolicitudId = solicitudId,
          MotivoFalla = null
        };
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex,
          "Error de BD en AnularDistribucionPorCupoAsync. CupoId: {CupoId}",
          cupoId);
        return new AnularDistribucionItemResult
        {
          CupoId = cupoId,
          Estado = AnularDistribucionItemEstado.Fallo,
          SolicitudId = 0,
          MotivoFalla = "Error de base de datos al revertir el cupo."
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en AnularDistribucionPorCupoAsync. CupoId: {CupoId}", cupoId);
        return new AnularDistribucionItemResult
        {
          CupoId = cupoId,
          Estado = AnularDistribucionItemEstado.Fallo,
          SolicitudId = 0,
          MotivoFalla = "Error inesperado al revertir el cupo."
        };
      }
    }
  }
}

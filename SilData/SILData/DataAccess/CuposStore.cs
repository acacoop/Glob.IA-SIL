using Dapper;
using Domain.Entities.Externo;
using Domain.Entities.PanelControlLogistico;
using IdentityModel.Client;
using Oracle.ManagedDataAccess.Client;
using Shared.ClassShared.Interfaces;
using SILData.DataAccess.Map_OracleToSql;
using SILData.Model.SolicitudTurno;
using SILData.Model;
using SILData.Services;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SILData.DataAccess
{
  public class CuposStore : ISILCuposStore
  {
    private readonly IConfiguration _configuration;
    private readonly ILogger<CuposStore> _logger;
    private readonly string _connectionString;

    public CuposStore(IConfiguration configuration, ILogger<CuposStore> logger)
    {
      _logger = logger;
      _configuration = configuration;
      _connectionString = configuration.GetConnectionString("SilConnection")?? string.Empty;
    }
    public async Task<IList<Cupo>> FindCuposByPeriod(DateTime fechaDesde, DateTime fechaHasta)
    {
      try
      {
        _logger.LogInformation("SILData: busco datos en SIL");
        List<Cupo> cupos = new List<Cupo>();
        List<Task<IEnumerable<Cupo>>> tareas = new List<Task<IEnumerable<Cupo>>>();
        Task<IEnumerable<Cupo>> cuposCorre_ = (Task.Run(() => this.FindCuposCorreByPeriod(fechaDesde, fechaHasta, "QueryCupposCorre")));
        Task<IEnumerable<Cupo>> cuposStop_ = (Task.Run(() => this.FindCuposStopByPeriod(fechaDesde, fechaHasta)));
        tareas.Add(cuposCorre_);
        tareas.Add(cuposStop_);
        var continuation = Task.WhenAll(tareas);
        continuation.Wait();
        cupos.AddRange((await cuposCorre_).ToList());
        cupos.AddRange((await cuposStop_).ToList());
        _logger.LogInformation("SILData: obtengo " + cupos.Count() + " registros de SIL");
        return cupos;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message, ex.StackTrace);
        throw ex;
      }
    }

    public async Task<IList<Cupo>> FindAvailableCuposByPeriodAsync(
        DateTime fechaDesde,
        DateTime fechaHasta,
        int codigoGrano,
        long? cuentaVendedor,
        long? cuentaDestino,
        short? estadoSil, long? zonaGeograficaId = null, long? cuentaComprador = null,
        List<string>? centros = null)
    {
      try
      {
        if (codigoGrano <= 0)
          throw new ArgumentException("codigoGrano debe ser positivo.", nameof(codigoGrano));

        // Columnas devueltas: las mínimas que el motor de matching necesita,
        // más el nombre del puerto asociado al cupo (cuposcorre.PUERTOCTA →
        // cupospuerto.NOMBRE), que es lo que la UI muestra como "Destino" en
        // Pantalla 2. Antes el servicio rellenaba NombreDestino con el nombre
        // de la zona geográfica (PUERTOPORZONA → ZONASGEOGRAFICAS), pero la
        // zona NO es lo que el usuario ve en el card del cupo: lo que se
        // guarda como destino del cupo es el puerto, no la zona.
        //
        // El nombre del comprador viaja en la fila (NOMDESTINATARIO es el
        // nombre del destinatario, que coincide con el comprador) y se mapea
        // directo a NomCompSIL para que la UI no tenga que salir a buscarlo
        // a otro catálogo. El nombre del vendedor se sigue resolviendo en una
        // capa superior porque cuposcorre no tiene columna equivalente. La
        // pertenencia a zonas geográficas se resuelve con IZonaGeograficaResolver
        // (no en este query, para no contaminar el matching masivo).
        //
        // Filtro de VENDCTA: si el caller no manda cuentaVendedor (= 0) trae
        // cualquier vendedor. Si manda un vendedor puntual (N > 0), trae los
        // cupos de ese vendedor Y también los cupos sin vendedor asignado
        // (VENDCTA IS NULL ó 0) — el motor de matching los evalúa como
        // Parciales porque falta el dato, y la UI los ofrece para que el
        // operador los vea con su observación, no para que queden invisibles.
        //
        // Filtro de CUENTAPUERTO (nuevo): si el caller manda cuentaDestino
        // (> 0), restringe a cupos cuyo PuertoCta coincide con esa cuenta.
        // Si no, trae cualquier destino.
        string sql = @"
                SELECT
                    c.Id                              AS ""Id"",
                    c.NroCupo                         AS ""Alfanumerico"",
                    c.Fecha                           AS ""Fecha"",
                    c.Centro                          AS ""CentroCupo"",
                    c.Grano                           AS ""CodGrano"",
                    c.PuertoCta                       AS ""CodDestino"",
                    cp.NOMBRE                         AS ""NomDestino"",
                    c.VENDCTA                         AS ""CodVendSIL"",
                    c.COMPCTA                         AS ""CodCompSIL"",
                    CC.NOMBRE                         AS ""NomCompSIL"",
                    c.STATUS                          AS ""EstadoSIL"",
                    c.FECHAYHORAINFORMADO             AS ""FechaInformadoSIL""
                FROM cuposcorre c
                LEFT OUTER JOIN MVCUPOSPUERTO cp ON cp.CUENTA = c.PUERTOCTA
                LEFT OUTER JOIN MVCUPOSCOMPRADOR cc ON cc.CUENTA = c.COMPCTA
                WHERE c.Fecha BETWEEN :fechaDesde AND :fechaHasta
                  AND c.Grano = :codigoGrano
                  AND c.TIPO = 1
                  AND (
                    :cuentaVendedor = 0
                    OR c.VENDCTA = :cuentaVendedor
                    OR c.VENDCTA IS NULL
                    OR c.VENDCTA = 0
                  )
                  AND (:cuentaDestino = 0 OR c.PuertoCta = :cuentaDestino)
                  AND (:cuentaComprador = 0 OR c.COMPCTA = :cuentaComprador)
                  AND (:estadoSil IS NULL OR c.STATUS = :estadoSil)
                  AND (
                    :zonaGeograficaId IS NULL OR :zonaGeograficaId = 0
                    OR EXISTS (
                      SELECT 1 FROM PUERTOPORZONA pz
                      WHERE pz.CUENTA = c.PUERTOCTA
                        AND pz.ZONAGEOID = :zonaGeograficaId
                    )
                  )";

        var parameters = new DynamicParameters();
        parameters.Add("@fechaDesde", fechaDesde.Date);
        parameters.Add("@fechaHasta", fechaHasta.Date);
        parameters.Add("@codigoGrano", codigoGrano);
        parameters.Add("@cuentaVendedor", cuentaVendedor ?? 0);
        parameters.Add("@cuentaDestino", cuentaDestino ?? 0);
        parameters.Add("@cuentaComprador", cuentaComprador ?? 0);
        // Dapper requiere un tipo para los parámetros; pasamos NULL cuando
        // el caller no quiere filtrar por estado. La cláusula SQL lo ignora.
        parameters.Add("@estadoSil", (object?)estadoSil ?? DBNull.Value);
        parameters.Add("@zonaGeograficaId", zonaGeograficaId ?? 0);

        // Centro del cupo contra los centros que el operador puede manipular.
        // Es cuposcorre.Centro — el centro con el que se creó el cupo — y NO
        // CentroDist, que recién se asigna al momento de distribuir y por lo
        // tanto está vacío en los cupos que este matching evalúa.
        //
        // Va como predicado dinámico y no como el patrón "(:p = 0 OR ...)" que
        // usan los demás filtros porque es una lista: sin centros no hay valor
        // neutro que escribir en el IN.
        if (centros is not null && centros.Count > 0)
        {
          sql += @"
                  AND c.Centro IN :centros";
          parameters.Add("@centros", centros);
        }

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        var cupos = await connection.QueryAsync<Cupo>(sql, parameters);
        var result = cupos.ToList();

        _logger.LogInformation(
            "FindAvailableCuposByPeriodAsync: {Count} cupos para grano={Grano} vendedor={Vendedor} destino={Destino} estado={Estado} centros={Centros} entre {Desde} y {Hasta}.",
            result.Count, codigoGrano, cuentaVendedor, cuentaDestino, estadoSil,
            centros is null || centros.Count == 0 ? "(todos)" : string.Join(",", centros),
            fechaDesde, fechaHasta);

        return result;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex,
            "Error de BD en FindAvailableCuposByPeriodAsync. Grano: {Grano} Vendedor: {Vendedor} Estado: {Estado}.",
            codigoGrano, cuentaVendedor, estadoSil);
        throw new Exception("Error al consultar los cupos disponibles para matching.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en FindAvailableCuposByPeriodAsync");
        throw;
      }
    }

    private async Task<IEnumerable<Cupo>> FindCuposCorreByPeriod(DateTime fechaDesde, DateTime fechaHasta, string KeyForQuery)
    {
      try
      {
        string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string queryLocation = Path.Combine(executableLocation, _configuration[KeyForQuery]);
        string query;
        bool sqlFileCache = FeatureFlags.IsEnabled(_configuration, FeatureFlags.SqlFileCache);
        if (sqlFileCache)
        {
          query = SqlFileCache.GetOrLoad(queryLocation);
        }
        else
        {
          StreamReader fileReader = new StreamReader(queryLocation);
          //StreamReader fileReader = new StreamReader(_configuration["QueryCupposCorre"]);
          query = fileReader.ReadToEnd();
        }
        var dictionary = new Dictionary<string, object>();
        dictionary.Add("@fechaDesde", fechaDesde.Date.ToString("dd/MM/yyyy"));
        dictionary.Add("@fechaHasta", fechaHasta.Date.ToString("dd/MM/yyyy"));
        if (sqlFileCache)
        {
          SqlFileCache.EnsureTypeHandlersRegistered(_logger);
        }
        else
        {
          SqlMapper.AddTypeHandler(new BooleanTypeHandler());
          SqlMapper.AddTypeHandler(new DateTimeTypeHandler(_logger));
        }
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          await connection.OpenAsync();
          var parameters = new DynamicParameters(dictionary);
          return await connection.QueryAsync<Cupo>(query, parameters);
        }
      }
      catch (Exception e)
      {
        _logger.LogError(e.Message, e.StackTrace);
        throw e;

      }
    }

    private async Task<IEnumerable<Cupo>> FindCuposStopByPeriod(DateTime fechaDesde, DateTime fechaHasta)
    {
      try
      {
        string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string queryLocation = Path.Combine(executableLocation, _configuration["QueryCupposStop"]);
        string query;
        bool sqlFileCache = FeatureFlags.IsEnabled(_configuration, FeatureFlags.SqlFileCache);
        if (sqlFileCache)
        {
          query = SqlFileCache.GetOrLoad(queryLocation);
        }
        else
        {
          StreamReader fileReader = new StreamReader(queryLocation);
          //StreamReader fileReader = new StreamReader(_configuration["QueryCupposStop"]);
          query = fileReader.ReadToEnd();
        }
        var dictionary = new Dictionary<string, object>();
        dictionary.Add("@fechaDesde", fechaDesde.ToString("dd/MM/yyyy"));
        dictionary.Add("@fechaHasta", fechaHasta.ToString("dd/MM/yyyy"));
        if (sqlFileCache)
        {
          SqlFileCache.EnsureTypeHandlersRegistered(_logger);
        }
        else
        {
          SqlMapper.AddTypeHandler(new BooleanTypeHandler());
          SqlMapper.AddTypeHandler(new DateTimeTypeHandler(_logger));
        }
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          await connection.OpenAsync();
          var parameters = new DynamicParameters(dictionary);
          return await connection.QueryAsync<Cupo>(query, parameters);
        }
      }
      catch (Exception e)
      {
        _logger.LogError(e.Message, e.StackTrace);
        throw e;
      }
    }
    public async Task<IList<Cupo>> FindCuposForDasshboardByPeriodBy(ShiftSILBoardRequest filters)
    {
      try
      {
        _logger.LogInformation("SILData: busco datos en SIL");
        List<Cupo> cupos = new List<Cupo>();
        // Ejecuta todas las tareas asíncronas sin bloquear
        var tareas = new List<Task<IEnumerable<Cupo>>>
        {
            FindCuposCorreByPeriod(filters, "CuposCorreByDashboard")
        };

        var resultados = await Task.WhenAll(tareas);
        cupos = resultados.SelectMany(r => r).ToList();

        _logger.LogInformation("SILData: obtengo " + cupos.Count() + " registros de SIL");
        return cupos;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message, ex.StackTrace);
        throw ex;
      }
    }

    private async Task<IEnumerable<Cupo>> FindCuposCorreByPeriod(ShiftSILBoardRequest filters, string KeyForQuery)
    {
      try
      {
        //using (var connection = new OracleConnection(_connectionString))
        //{
        //  var result = await connection.QueryFirstOrDefaultAsync<bool>("SELECT 1 AS VALOR FROM DUAL");
        //  Console.WriteLine(result); // debería imprimir True
        //}

        string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string queryLocation = Path.Combine(executableLocation, _configuration[KeyForQuery]);
        bool sqlFileCache = FeatureFlags.IsEnabled(_configuration, FeatureFlags.SqlFileCache);
        string query = sqlFileCache
          ? SqlFileCache.GetOrLoad(queryLocation)
          : await File.ReadAllTextAsync(queryLocation);

        var parameters = new DynamicParameters();

        parameters.Add("@fechaDesde", filters.Desde.Date.ToString("dd/MM/yyyy"));
        parameters.Add("@fechaHasta", filters.Hasta.Date.ToString("dd/MM/yyyy"));

        parameters.Add("@compradores", ToStringConcat(filters.Compradores));
        parameters.Add("@vendedores", ToStringConcat(filters.Vendedores));
        parameters.Add("@productos", ToStringConcat(filters.Productos));
        parameters.Add("@destinos", ToStringConcat(filters.Destinos));
        parameters.Add("@centros", ToStringConcat(filters.Centros));

        if (sqlFileCache)
        {
          SqlFileCache.EnsureTypeHandlersRegistered(_logger);
        }
        else
        {
          SqlMapper.AddTypeHandler(new BooleanTypeHandler());
          SqlMapper.AddTypeHandler(new DateTimeTypeHandler(_logger));
        }

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        var command = new CommandDefinition(query, parameters, commandTimeout: 300); // 3 minutos
        var result = await connection.QueryAsync<Cupo>(command);
        return result.ToList();
      }
      catch (Exception e)
      {
        _logger.LogError(e.Message, e.StackTrace);
        throw e;
      }
    }

    public async Task<IList<Cupo>> GetCuposByIdsAsync(IList<long> ids)
    {
      if (ids == null || ids.Count == 0) return new List<Cupo>();

      // Sólo cuposcorre (TIPO = 1, idéntico al criterio de FindAvailableCuposByPeriodAsync).
      // Las cabeceras (TIPO = 0) no son cupos a asignar — no las traeremos.
      // Las columnas son las mínimas que el payload de Accept necesita
      // (mismo set que FindAvailableCuposByPeriodAsync), incluido el nombre
      // del puerto (cupospuerto.NOMBRE → NomDestino) que la UI muestra en
      // Pantalla 2 como "Destino" del cupo. El JOIN a cupospuerto sigue el
      // mismo patrón que el resto de las queries históricas del módulo.
      const string sql = @"
        SELECT
            c.Id                              AS ""Id"",
            c.NroCupo                         AS ""Alfanumerico"",
            c.Fecha                           AS ""Fecha"",
            c.Centro                          AS ""CentroCupo"",
            c.Grano                           AS ""CodGrano"",
            c.PuertoCta                       AS ""CodDestino"",
            cp.NOMBRE                         AS ""NomDestino"",
            c.VENDCTA                         AS ""CodVendSIL"",
            c.COMPCTA                         AS ""CodCompSIL"",
            c.NOMDESTINATARIO                 AS ""NomCompSIL"",
            c.STATUS                          AS ""EstadoSIL"",
            c.FECHAYHORAINFORMADO             AS ""FechaInformadoSIL""
        FROM cuposcorre c
        LEFT OUTER JOIN MVCUPOSPUERTO cp ON cp.CUENTA = c.PUERTOCTA
        WHERE c.Id IN :Ids
          AND c.TIPO = 1";

      try
      {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        var cupos = (await connection.QueryAsync<Cupo>(sql, new { Ids = ids.ToList() })).ToList();

        _logger.LogInformation(
          "GetCuposByIdsAsync: {Obtenidos} cupos de {Solicitados} solicitados.",
          cupos.Count, ids.Count);

        return cupos;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex, "Error de BD en GetCuposByIdsAsync. Solicitados: {Cantidad}.", ids.Count);
        throw new Exception("Error al consultar cupos por id.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetCuposByIdsAsync");
        throw;
      }
    }

    public async Task FreeCuposByIdsAsync(IList<long> cupoIds, IDbTransaction transaction)
    {
      if (cupoIds == null || cupoIds.Count == 0) return;

      // El cupo vuelve a estar libre:
      //   - STATUS = 0 (libre, equivalente a "no otorgado").
      //   - VENDCTA/COMPCTA/FECHAYHORAINFORMADO = NULL (ya no pertenece a nadie).
      // La guardia STATUS = 2 (Otorgado) evita liberar un cupo que ya está libre
      // (defensa contra dobles liberaciones en flujos concurrentes).
      const string sql = @"
        UPDATE CuposCorre
           SET STATUS = 0,
               VENDCTA = NULL,
               COMPCTA = NULL,
               FECHAYHORAINFORMADO = NULL
         WHERE Id IN :Ids
           AND STATUS = 2";

      var connection = transaction.Connection;
      if (connection == null)
        throw new InvalidOperationException("La transacción no tiene conexión asociada.");

      var rowsAffected = await connection.ExecuteAsync(sql, new { Ids = cupoIds.ToList() }, transaction);

      _logger.LogInformation(
        "FreeCuposByIdsAsync: {Liberados} cupos liberados de {Solicitados} solicitados.",
        rowsAffected, cupoIds.Count);
    }

    public async Task FreeCuposByIdsAsync(IList<long> cupoIds)
    {
      if (cupoIds == null || cupoIds.Count == 0) return;

      // Variante NO transaccional. Útil cuando el caller hizo un commit
      // previo y necesita liberar cupos como compensación best-effort.
      const string sql = @"
        UPDATE CuposCorre
           SET STATUS = 0,
               VENDCTA = NULL,
               COMPCTA = NULL,
               FECHAYHORAINFORMADO = NULL
         WHERE Id IN :Ids
           AND STATUS = 2";

      try
      {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        var rowsAffected = await connection.ExecuteAsync(sql, new { Ids = cupoIds.ToList() });

        _logger.LogInformation(
          "FreeCuposByIdsAsync(no-tx): {Liberados} cupos liberados de {Solicitados}.",
          rowsAffected, cupoIds.Count);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
          "Error en FreeCuposByIdsAsync(no-tx). Solicitados: {Cantidad}.", cupoIds.Count);
        throw;
      }
    }

    public async Task UpdateCuposDistributionAsync(IList<Cupo> cupos)
    {
      if (cupos == null || !cupos.Any())
        return;

      const string sqlUpdate = @"
        UPDATE CuposCorre
        SET VENDCTA = :CodVendSIL,
            COMPCTA = :CodCompSIL,
            STATUS = :EstadoSIL,
            FECHAYHORAINFORMADO = :FechaInformadoSIL
        WHERE ID = :Id";

      using var connection = new OracleConnection(_connectionString);
      await connection.OpenAsync();
      using var transaction = connection.BeginTransaction();
      try
      {
        var parameters = cupos.Select(c => new
        {
          CodVendSIL = long.TryParse(c.CodVendSIL, out long vend) ? vend : 0,
          CodCompSIL = long.TryParse(c.CodCompSIL, out long comp) ? comp : 0,
          EstadoSIL = c.EstadoSIL,
          FechaInformadoSIL = c.FechaInformadoSIL ?? DateTime.Now,
          Id = c.Id
        }).ToList();

        await connection.ExecuteAsync(sqlUpdate, parameters, transaction);
        await transaction.CommitAsync();
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error al actualizar la distribución de cupos en CuposCorre.");
        throw;
      }
    }

    public string? ToStringConcat(List<string>? filtros)
    {
      string? result = string.Empty;
      if (filtros is not null && filtros.Any())
        result = string.Join(",", filtros);
      else 
        result = null;
      return result; 
    }
  }
}

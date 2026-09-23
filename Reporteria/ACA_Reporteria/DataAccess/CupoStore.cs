using Dapper;
using Domain.Entities.Externo;
using Domain.Entities.PanelControlLogistico;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Reporteria.DataAccess.Map_ToModel;
using Shared.ClassShared.Requests;
using System.Data;
using System.Diagnostics;
using static Dapper.SqlBuilder;

namespace Reporteria.DataAccess
{
  public class CupoStore : IReportStore
  {
    private readonly string _connectionString;

    private readonly ILogger<CupoStore> _logger;

    public CupoStore(IConfiguration config, ILogger<CupoStore> logger)
    {
      _connectionString = config.GetConnectionString("DefaultConnection");
      _logger = logger;
    }

    // CommandTimeout para queries a Azure SQL (segundos). Subir si se justifica.
    private const int SqlCommandTimeoutSeconds = 60;

    public async Task<IEnumerable<Cupo>> GetCupos(CPEReportRequest? request = null)
    {
      var sw = Stopwatch.StartNew();
      try
      {
        bool orderDesc = false;
        using (SqlConnection connection = new(_connectionString))
        {
          if (connection.State == ConnectionState.Closed)
          {
            await connection.OpenAsync();
          }
          Template template = await BuildGetCupos(request);
          _logger.LogInformation("GetCupos - SQL: {Sql} Params: {P}", template.RawSql, JsonConvert.SerializeObject(template.Parameters));
          var prueba = await connection.QueryAsync(template.RawSql, template.Parameters, null, SqlCommandTimeoutSeconds);
          List<Cupo> cupos = (List<Cupo>)prueba;
          _logger.LogInformation("GetCupos OK - {Ms}ms - {Rows} filas", sw.ElapsedMilliseconds, cupos.Count);
          return cupos;
        }
      }
      catch (SqlException sqlEx) when (sqlEx.Number == -2 || sqlEx.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
      {
        _logger.LogError(sqlEx, "GetCupos TIMEOUT (>{S}s) tras {Ms}ms", SqlCommandTimeoutSeconds, sw.ElapsedMilliseconds);
        throw new TimeoutException($"La consulta a Azure SQL excedió los {SqlCommandTimeoutSeconds}s. Revisar índices y filtros.", sqlEx);
      }
      catch (Exception e)
      {
        _logger.LogError(e, "GetCupos ERROR tras {Ms}ms", sw.ElapsedMilliseconds);
        throw;
      }
    }
    public async Task<IEnumerable<Cupo>> GetCuposForDasboard(ShiftBoardRequest? request = null)
    {
      var sw = Stopwatch.StartNew();
      try
      {
        bool orderDesc = false;
        using (SqlConnection connection = new(_connectionString))
        {
          if (connection.State == ConnectionState.Closed)
          {
            await connection.OpenAsync();
          }
          Template template = BuildGetCuposForDasboard(request);
          _logger.LogInformation("GetCuposForDasboard - SQL: {Sql} Params: {P}", template.RawSql, JsonConvert.SerializeObject(template.Parameters));
          IEnumerable<Cupo> result = await connection.QueryAsync<Cupo>(template.RawSql, template.Parameters, null, SqlCommandTimeoutSeconds);
          IList<Cupo> cupos = (IList<Cupo>)result;
          _logger.LogInformation("GetCuposForDasboard OK - {Ms}ms - {Rows} filas", sw.ElapsedMilliseconds, cupos.Count);
          return cupos;
        }
      }
      catch (SqlException sqlEx) when (sqlEx.Number == -2 || sqlEx.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
      {
        _logger.LogError(sqlEx, "GetCuposForDasboard TIMEOUT (>{S}s) tras {Ms}ms", SqlCommandTimeoutSeconds, sw.ElapsedMilliseconds);
        throw new TimeoutException($"La consulta a Azure SQL excedió los {SqlCommandTimeoutSeconds}s. Revisar índices y filtros.", sqlEx);
      }
      catch (Exception e)
      {
        _logger.LogError(e, "GetCuposForDasboard ERROR tras {Ms}ms", sw.ElapsedMilliseconds);
        throw;
      }
    }
    public async Task<DataTable> GetCuposDT(CPEReportRequest? request = null, string? selectStatement = null)
    {
      var sw = Stopwatch.StartNew();
      try
      {
        DataTable dt = new DataTable();
        dt.TableName = "Cupos";
        bool orderDesc = false;
        using (SqlConnection connection = new(_connectionString))
        {
          if (connection.State == ConnectionState.Closed)
          {
            await connection.OpenAsync();
          }
          Template template = await BuildGetCupos(request, selectStatement);
          _logger.LogInformation("GetCuposDT - SQL: {Sql} Params: {P}", template.RawSql, JsonConvert.SerializeObject(template.Parameters));
          using var reader = await connection.ExecuteReaderAsync(template.RawSql, template.Parameters, null, SqlCommandTimeoutSeconds);
          dt.Load(reader);
          _logger.LogInformation("GetCuposDT OK - {Ms}ms - {Rows} filas", sw.ElapsedMilliseconds, dt.Rows.Count);
          return dt;
        }
      }
      catch (SqlException sqlEx) when (sqlEx.Number == -2 || sqlEx.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
      {
        _logger.LogError(sqlEx, "GetCuposDT TIMEOUT (>{S}s) tras {Ms}ms", SqlCommandTimeoutSeconds, sw.ElapsedMilliseconds);
        throw new TimeoutException($"La consulta a Azure SQL excedió los {SqlCommandTimeoutSeconds}s. Revisar índices y filtros.", sqlEx);
      }
      catch (Exception e)
      {
        _logger.LogError(e, "GetCuposDT ERROR tras {Ms}ms", sw.ElapsedMilliseconds);
        throw;
      }
    }
    public async Task<Template> BuildGetCupos(CPEReportRequest? request = null, string? selectStatement = null)
    {
      try
      {
        bool orderDesc = false;
        var builder = new SqlBuilder();
        string? selectQuery = selectStatement;

        if (string.IsNullOrEmpty(selectStatement))
        {
          selectQuery = "SELECT *";
        }

        if (request == null)
        {
          var selector = builder.AddTemplate($"{selectQuery} FROM CUPO");
          return selector;
        }
        else
        {
          // OPTION (RECOMPILE) fuerza a SQL Server a generar un plan nuevo por ejecucion.
          // Fix contra parameter sniffing: el plan cacheado puede ser suboptimo para los parametros
          // que Dapper pasa, mientras que valores hardcodeados en el portal usan otro plan.
          var selector = builder.AddTemplate($"{selectQuery} from cupo /**where**/ /**orderby**/ OPTION (RECOMPILE)");
          /*fechas*/
          builder.Where("fecha >= @fechaDesde", new { request.FechaDesde });
          builder.Where("fecha <= @fechaHasta", new { request.FechaHasta });

          if (request.Vendedores != null && request.Vendedores.Count > 0)
            builder.Where("CodVendSIL in @vendedores", new { vendedores = request.Vendedores });

          if (request.Compradores != null && request.Compradores.Count > 0)
            builder.Where("CodCompSIL in @compradores", new { compradores = request.Compradores });

          if (request.Destinos != null && request.Destinos.Count > 0)
            builder.Where("CodDestino in @puertos", new { puertos = request.Destinos });

          if (request.Productos != null && request.Productos.Count > 0)
            builder.Where("codgrano in @granos", new { granos = request.Productos });

          if (request.Centros != null && request.Centros.Count > 0)
            builder.Where("centroCupo in @centros", new { centros = request.Centros });

          if (request.EstadoDeCupoEnSTOP > -1)
            builder.Where("estadoStop = @EstadoDeCupoEnSTOP", new { request.EstadoDeCupoEnSTOP });
          switch (request.TipoDeReporte)
          {
            case 0: builder.Where("estaSil = 1 AND estaStop = 0"); break;
            case 1: builder.Where("estaSil = 0 AND estaStop = 1"); break;
            //case 2: builder.Where("estaSil = 1 AND estaStop = 1"); break; Todos
          }
          builder.OrderBy(string.Format("fecha {0}", orderDesc ? "desc" : "asc"));
          SqlMapper.AddTypeHandler(new NullableDateTimeHandler(_logger));
          return selector;
        }
      }
      catch (Exception e)
      {
        _logger.LogInformation(e.ToString());
        throw;
      }
    }
    public Template BuildGetCuposForDasboard(ShiftBoardRequest? filter = null, string? selectStatement = null)
    {
      try
      {
        bool orderDesc = false;
        var builder = new SqlBuilder();
        string? selectQuery = selectStatement;

        if (string.IsNullOrEmpty(selectStatement))
        {
          selectQuery = "SELECT *";
        }

        if (filter == null)
        {
          var selector = builder.AddTemplate($"{selectQuery} FROM CUPO");
          builder.Where("fecha >= @fechaDesde", new { fechaDesde = DateTime.Now.AddDays(-1).Date});
          builder.Where("fecha <= @fechaHasta", new { fechaHasta = DateTime.Now.AddDays(2).Date });
          return selector;
        }
        else
        {
          var selector = builder.AddTemplate($"{selectQuery} from cupo /**where**/");
          builder.Where("fecha >= @fechaDesde", new { fechaDesde = filter.Fecha.AddDays(-1).Date });
          builder.Where("fecha <= @fechaHasta", new { fechaHasta = filter.Fecha.AddDays(2).Date });

          if (filter.Vendedores != null && filter.Vendedores.Any())
            builder.Where("CodVendSIL in @vendedores", new { vendedores = filter.Vendedores });

          if (filter.Compradores != null && filter.Compradores.Any())
            builder.Where("CodCompSIL in @compradores", new { compradores = filter.Compradores });

          if (filter.Destinos != null && filter.Destinos.Any())
            builder.Where("CodDestino in @puertos", new { puertos = filter.Destinos });

          if (filter.Productos != null && filter.Productos.Any())
            builder.Where("codgrano in @granos", new { granos = filter.Productos });

          if (filter.Centros != null && filter.Centros.Count > 0)
            builder.Where("centroDist in @centros", new { centros = filter.Centros });

          builder.OrderBy(string.Format("fecha {0}", orderDesc ? "desc" : "asc"));
          SqlMapper.AddTypeHandler(new NullableDateTimeHandler(_logger));
          return selector;
        }
      }
      catch (Exception e)
      {
        _logger.LogInformation(e.ToString());
        throw;
      }
    }
    public async Task<IEnumerable<Cupo>> GetCuposWithPaging(int pageSize, int pageNumber, string columnForOrder, bool orderDesc , CPEReportRequest request)
    {
      try
      {
        string OrderByField = !string.IsNullOrEmpty(columnForOrder)? columnForOrder: "Id";        
        using (SqlConnection connection = new(_connectionString))
        {
          List<Cupo> cupos = new List<Cupo>();
          if (connection.State == ConnectionState.Closed)
          {
            await connection.OpenAsync();
          }

          var builder = new SqlBuilder();
          var selectTemplate = builder.AddTemplate(
            @$"SELECT *
              FROM Cupo 
              /**where**/
              /**orderby**/
              OFFSET {pageNumber * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
          builder.Where("fecha >= @fechaDesde", new { request.FechaDesde });
          builder.Where("fecha <= @fechaHasta", new { @request.FechaHasta });
          if (request.Vendedores != null && request.Vendedores.Count > 0)
            builder.Where("CodVendSIL in @vendedores", new { vendedores = request.Vendedores });
          if (request.Productos != null && request.Productos.Count > 0)
            builder.Where("codgrano in @granos", new { granos = request.Productos });
          if (request.Centros != null && request.Centros.Count > 0)
            builder.Where("centroCupo in @centros", new { centros = request.Centros });
          if (request.EstadoDeCupoEnSTOP > -1)
            builder.Where("estadoStop = @EstadoDeCupoEnSTOP", new { @request.EstadoDeCupoEnSTOP });
          switch (request.TipoDeReporte)
          {
            case 0: builder.Where("estaSil = 1 AND estaStop = 0"); break;
            case 1: builder.Where("estaSil = 0 AND estaStop = 1"); break;
            case 2: builder.Where("estaSil = 1 AND estaStop = 1"); break;
          }
          builder.OrderBy(string.Format("{0} {1}", OrderByField, orderDesc ? "desc" : "asc"));
          SqlMapper.AddTypeHandler(new NullableDateTimeHandler(_logger));
          cupos = (List<Cupo>)await connection.QueryAsync<Cupo>(selectTemplate.RawSql, selectTemplate.Parameters);
          return cupos;
        }
      }
      catch (Exception)
      {
        throw;
      }
    }
    public async Task<long> GetCountCupos(CPEReportRequest request)
    {
      using (SqlConnection connection = new(_connectionString))
      {
        if (connection.State == ConnectionState.Closed)
        {
          await connection.OpenAsync();
        }
        var builder = new SqlBuilder();
        var counter = builder.AddTemplate("select count(*) from cupo /**where**/ /**orderby**/");
        builder.Where("fecha >= @fechaDesde", new { request.FechaDesde.Date });
        builder.Where("fecha <= @fechaHasta", new { @request.FechaHasta.Date });
        if (request.Vendedores != null && request.Vendedores.Count > 0)
          builder.Where("CodVendSIL in @vendedores", new { vendedores = request.Vendedores });
        if (request.Productos != null && request.Productos.Count > 0)
          builder.Where("codgrano in @granos", new { granos = request.Productos });
        if (request.Centros != null && request.Centros.Count > 0)
          builder.Where("centroCupo in @centros", new { centros = request.Centros });
        if (request.EstadoDeCupoEnSTOP > -1)
          builder.Where("estadoStop = @EstadoDeCupoEnSTOP", new { @request.EstadoDeCupoEnSTOP });
        switch (request.TipoDeReporte)
        {
          case 0: builder.Where("estaSil = 1 AND estaStop = 0"); break;
          case 1: builder.Where("estaSil = 0 AND estaStop = 1"); break;
          case 2: builder.Where("estaSil = 1 AND estaStop = 1"); break;
        }
        SqlMapper.AddTypeHandler(new NullableDateTimeHandler(_logger));
        long cupos = await connection.ExecuteScalarAsync<long>(counter.RawSql, counter.Parameters) ;
        return cupos;

      }
    }

    /// <summary>
    /// Healthcheck: ejecuta SELECT 1 contra Azure SQL y mide el tiempo.
    /// Usado por /api/cupos/health para diagnosticar timeouts de conectividad.
    /// </summary>
    public async Task<(bool Ok, long Ms, string? Error)> PingSqlAsync()
    {
      var sw = Stopwatch.StartNew();
      try
      {
        using (SqlConnection connection = new(_connectionString))
        {
          if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();

          var result = await connection.ExecuteScalarAsync<int>("SELECT 1");
          sw.Stop();
          _logger.LogInformation("PingSql OK - {Ms}ms - result={R}", sw.ElapsedMilliseconds, result);
          return (true, sw.ElapsedMilliseconds, null);
        }
      }
      catch (Exception ex)
      {
        sw.Stop();
        _logger.LogError(ex, "PingSql ERROR tras {Ms}ms - {Msg}", sw.ElapsedMilliseconds, ex.Message);
        return (false, sw.ElapsedMilliseconds, ex.Message);
      }
    }

  }
}

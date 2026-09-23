using Dapper;
using Domain.Entities.Externo;
using HangFire.Stores.Sil.Map_OracleToSql;
using Oracle.ManagedDataAccess.Client;
using System.Reflection;

namespace HangFire.Stores.Sil
{
  public class CupoStore : ISilCupoStore
  {
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CupoStore> logger;

    public CupoStore(IConfiguration configuration, ILogger<CupoStore> logger)
    {
      _configuration = configuration;
      _connectionString = configuration.GetConnectionString("SilConnection");
      this.logger = logger;
    }

    /// <summary>
    /// Trae los cupos de la cupos corre y la cuposstop para el periodo indicado.
    /// </summary>
    /// <param name="fechaDesde"></param>
    /// <param name="fechaHasta"></param>
    /// <returns></returns>
    public async Task<IList<Cupo>> FindByPeriod(DateTime fechaDesde, DateTime fechaHasta)
    {
      try 
      {
        logger.LogInformation("HangFire: busco datos en SIL - FindByPeriod");
        List<Cupo> cupos = new List<Cupo>();
        List<Task<IEnumerable<Cupo>>> tareas = new List<Task<IEnumerable<Cupo>>>();
        Task<IEnumerable<Cupo>> cuposCorre_ = (Task.Run(() => this.FindCuposCorreByPeriod(fechaDesde, fechaHasta)));
        Task<IEnumerable<Cupo>> cuposStop_ = (Task.Run(() => this.FindCuposStopByPeriod(fechaDesde, fechaHasta)));      
        tareas.Add(cuposCorre_);
        tareas.Add(cuposStop_);
        var continuation = Task.WhenAll(tareas);
        continuation.Wait();
        cupos.AddRange((await cuposCorre_).ToList());
        cupos.AddRange((await cuposStop_).ToList());
        logger.LogInformation("HangFire: obtengo " + cupos.Count() +" registros de SIL - FindByPeriod");
        return cupos;
      }
      catch (Exception ex) 
      {
        logger.LogError(ex.Message, ex.StackTrace);
        throw ex;
      }
    }
    private async Task<IEnumerable<Cupo>> FindCuposCorreByPeriod(DateTime fechaDesde, DateTime fechaHasta)
    {
      try 
      {
        string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string queryLocation = Path.Combine(executableLocation, _configuration["QueryCupposCorre"]);
        StreamReader fileReader = new StreamReader(queryLocation);
        //StreamReader fileReader = new StreamReader(_configuration["QueryCupposCorre"]);
        string query = fileReader.ReadToEnd();
        var dictionary = new Dictionary<string, object>();
        dictionary.Add("@fechaDesde", fechaDesde.ToString("dd/MM/yyyy"));
        dictionary.Add("@fechaHasta", fechaHasta.ToString("dd/MM/yyyy"));
        SqlMapper.AddTypeHandler(new BooleanTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeTypeHandler(logger));
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          await connection.OpenAsync();
          var parameters = new DynamicParameters(dictionary);
          return await connection.QueryAsync<Cupo>(query, parameters);
        }
      }
      catch (Exception e) 
      {
        logger.LogError(e.Message, e.StackTrace);
        throw e; 

      }
    }

    private async Task<IEnumerable<Cupo>> FindCuposStopByPeriod(DateTime fechaDesde, DateTime fechaHasta)
    {
      try 
      {
        string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string queryLocation = Path.Combine(executableLocation, _configuration["QueryCupposStop"]);
        StreamReader fileReader = new StreamReader(queryLocation);
        //StreamReader fileReader = new StreamReader(_configuration["QueryCupposStop"]);
        string query = fileReader.ReadToEnd();
        var dictionary = new Dictionary<string, object>();
        dictionary.Add("@fechaDesde", fechaDesde.ToString("dd/MM/yyyy"));
        dictionary.Add("@fechaHasta", fechaHasta.ToString("dd/MM/yyyy"));
        SqlMapper.AddTypeHandler(new BooleanTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeTypeHandler(logger));
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          await connection.OpenAsync();
          var parameters = new DynamicParameters(dictionary);
          return await connection.QueryAsync<Cupo>(query, parameters);
        }
      } catch (Exception e) 
      {
        logger.LogError(e.Message, e.StackTrace);
        throw e; 
      }
    }
  }
}

using Dapper;
using NuGet.Packaging.Signing;
using Oracle.ManagedDataAccess.Client;
using Shared.StaticShared;
using SILData.Model;

namespace SILData.DataAccess
{
  public class CentroStore
  {
    private string? _connectionString;
    private object logger;

    public CentroStore(IConfiguration configuration, ILogger logger)
    {
      _connectionString = configuration.GetConnectionString("SilConnection");
      this.logger = logger;

    }
    public async Task<IList<CentroSIL>> FindStartsWithLimit(string filtro, int limit)
    {
      try
      {
        string upperFiltro = StaticOperations.IsNumber(filtro) ? filtro : filtro.ToUpper();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          string query = "SELECT centro, centro as codigocentro, nombre FROM CUPOSCENTRO WHERE ";
          query += " upper(nombre) like :filtro1";
          query += " OR upper(centro) like :filtro1";
          query += " Order by nombre desc";

          var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", (upperFiltro + "%").ToString() }
          };
          var parameters = new DynamicParameters(dictionary);
          var cuentas = (IList<CentroSIL>)await connection.QueryAsync<CentroSIL>(query, parameters);
          return cuentas;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public async Task<IList<CentroSIL>> FindContainWithLimit(string filtro, int limit)
    {
      try
      {
        string upperFiltro = StaticOperations.IsNumber(filtro) ? filtro : filtro.ToUpper();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          string query = "SELECT centro, centro as codigocentro, nombre FROM CUPOSCENTRO WHERE ";
          query += " upper(nombre) like :filtro1";
          query += " OR upper(centro) like :filtro1";
          query += " Order by nombre desc";

          var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", ("%" + upperFiltro + "%").ToString() }
          };
          var parameters = new DynamicParameters(dictionary);
          var cuentas = (IList<CentroSIL>)await connection.QueryAsync<CentroSIL>(query, parameters);
          return cuentas;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public async Task<IEnumerable<CentroSIL>> GetAll()
    {
      using (OracleConnection connection = new OracleConnection(_connectionString))
      {
        string query = "SELECT centro as id, centro, centro as codigocentro, nombre FROM CUPOSCENTRO";
        query += " Order by nombre desc";

        var cuentas = await connection.QueryAsync<CentroSIL>(query);
        return cuentas;
      }
    }

    public Task<IList<CentroSIL>> GetById()
    {
      throw new NotImplementedException();
    }

    public async Task<IEnumerable<CentroSIL>> GetByIds(IList<string> ids)
    {
      using (OracleConnection connection = new OracleConnection(_connectionString))
      {
        string query = "SELECT centro as id, centro, centro as codigocentro, nombre FROM CUPOSCENTRO WHERE ";
        query += " centro IN :filtro1";
        query += " Order by nombre desc";

        var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", ids }
          };
        var parameters = new DynamicParameters(dictionary);
        var cuentas = await connection.QueryAsync<CentroSIL>(query, parameters);
        return cuentas;
      }
    }
  }
}

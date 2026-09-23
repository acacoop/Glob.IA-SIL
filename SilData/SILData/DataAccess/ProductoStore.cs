using Dapper;
using Oracle.ManagedDataAccess.Client;
using Shared.StaticShared;
using SILData.Model;

namespace SILData.DataAccess
{
  public class ProductoStore
  {
    private string? _connectionString;
    private object logger;

    public ProductoStore(IConfiguration configuration, ILogger logger)
    {
      _connectionString = configuration.GetConnectionString("SilConnection");
      this.logger = logger;

    }
    public async Task<IList<ProductoSIL>> FindStartsWithLimit(string filtro, int limit)
    {
      try
      {
        string upperFiltro = StaticOperations.IsNumber(filtro) ? filtro : filtro.ToUpper();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          string query = "SELECT grano, nombre FROM CUPOSGRANO";
          query += " WHERE upper(nombre) like :filtro1";
          query += " OR to_char(grano) like :filtro1";
          query += " Order by nombre desc";

          var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", (upperFiltro + "%").ToString() }
          };
          var parameters = new DynamicParameters(dictionary);
          var cuentas = (IList<ProductoSIL>)await connection.QueryAsync<ProductoSIL>(query, parameters);
          return cuentas;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public async Task<IEnumerable<ProductoSIL>> FindContainWithLimit(string filtro, int limit)
    {
      try
      {
        string upperFiltro = StaticOperations.IsNumber(filtro) ? filtro : filtro.ToUpper();
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          string query = "SELECT grano, nombre FROM CUPOSGRANO";
          query += " WHERE upper(nombre) like :filtro1";
          query += " OR to_char(grano) like :filtro1";
          query += " Order by nombre desc";

          var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", ("%" + upperFiltro + "%").ToString() }
          };
          var parameters = new DynamicParameters(dictionary);
          var cuentas = await connection.QueryAsync<ProductoSIL>(query, parameters);
          return cuentas;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public async Task<IEnumerable<ProductoSIL>> GetAll()
    {
      using (OracleConnection connection = new OracleConnection(_connectionString))
      {
        string query = "SELECT grano as id, grano, grano as codigograno, nombre FROM CUPOSGRANO";
        query += " Order by nombre desc";

        var cuentas = await connection.QueryAsync<ProductoSIL>(query);
        return cuentas;
      }
    }

    public async Task<ProductoSIL?> GetById(string id)
    {
      using (OracleConnection connection = new OracleConnection(_connectionString))
      {
        string query = "SELECT grano as id, grano, grano as codigograno, nombre FROM CUPOSGRANO";
        query += " WHERE grano = :filtro1";
        query += " Order by nombre desc";

        var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", id }
          };
        var parameters = new DynamicParameters(dictionary);

        var cuentas = await connection.QueryAsync<ProductoSIL>(query, parameters);
        return cuentas.FirstOrDefault();
      }
    }

    public async Task<IEnumerable<ProductoSIL>> GetByIds(IList<string> ids)
    {
      using (OracleConnection connection = new OracleConnection(_connectionString))
      {
        string query = "SELECT grano as id, grano, grano as codigograno, nombre FROM CUPOSGRANO";
        query += " WHERE grano IN :granos";
        query += " Order by nombre desc";

        var dictionary = new Dictionary<string, object>
          {
            { "@granos", ids }
          };
        var parameters = new DynamicParameters(dictionary);

        var cuentas = await connection.QueryAsync<ProductoSIL>(query, parameters);
        //var cuentas = await connection.QueryAsync<ProductoSIL>(query, new { granos = ids.ToArray() });
        return cuentas;
      }
    }
  }
}

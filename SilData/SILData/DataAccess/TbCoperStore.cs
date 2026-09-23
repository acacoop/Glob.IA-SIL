using Dapper;
using Oracle.ManagedDataAccess.Client;
using SILData.Model;

namespace SILData.DataAccess
{
  public class TbCoperStore
  {
    private string? _connectionString;
    private object logger;

    public TbCoperStore(IConfiguration configuration, ILogger logger)
    {
      _connectionString = configuration.GetConnectionString("SilConnection");
      this.logger = logger;
    }

    public async Task<IEnumerable<TbCoperSIL>> GetAll()
    {
      using (OracleConnection connection = new OracleConnection(_connectionString))
      {
        string query = "SELECT * FROM tb_coper WHERE ";
        query += " codigo IS NOT NULL";
        query += " AND descripcion IS NOT NULL";

        var tbcoper = await connection.QueryAsync<TbCoperSIL>(query);
        return tbcoper;
      }
    }

    public async Task<IEnumerable<TbCoperSIL>> GetByIds(IList<string> ids)
    {
      using (OracleConnection connection = new OracleConnection(_connectionString))
      {
        string query = "SELECT * FROM tb_coper WHERE ";
        query += " codigo IN :filtro1";
        query += " AND descripcion IS NOT NULL";

        var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", ids },
          };
        var parameters = new DynamicParameters(dictionary);
        var tbcoper = await connection.QueryAsync<TbCoperSIL>(query, parameters);
        return tbcoper;
      }
    }
  }
}

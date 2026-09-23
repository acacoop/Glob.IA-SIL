using Dapper;
using Oracle.ManagedDataAccess.Client;
using Shared.StaticShared;
using SILData.Model;

namespace SILData.DataAccess
{
  public class DTablaStore
  {
    private string? _connectionString;
    private object logger;

    public DTablaStore(IConfiguration configuration, ILogger logger)
    {
      _connectionString = configuration.GetConnectionString("SilConnection");
      this.logger = logger;
    }

    public async Task<IEnumerable<DTablaSIL>> GetByEntidadAndOrden(string entidad, string orden)
    {
      using (OracleConnection connection = new OracleConnection(_connectionString))
      {
        string query = "SELECT Entidad, Clave, Orden, Valor, Empresa, Uninego, Zona, null as Nuevo1, Uvalue FROM DTABLA WHERE ";
        query += " entidad = :filtro1";
        query += " AND orden = :filtro2";
        query += " AND TRIM(clave) IS NOT NULL";
        query += " Order by clave desc";

        var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", entidad },
            { "@filtro2", orden }
          };
        var parameters = new DynamicParameters(dictionary);
        var dtablas = await connection.QueryAsync<DTablaSIL>(query, parameters);
        return dtablas;
      }
    }

    public async Task<IEnumerable<DTablaSIL>> GetByEntidadAndOrden(string entidad, string orden, IList<string> ids)
    {
      using (OracleConnection connection = new OracleConnection(_connectionString))
      {
        string query = "SELECT Entidad, Clave, Orden, Valor, Empresa, Uninego, Zona, null as Nuevo1, Uvalue FROM DTABLA WHERE ";
        query += " entidad = :filtro1";
        query += " AND orden = :filtro2";
        query += " AND uvalue IN :filtro3";
        query += " AND TRIM(clave) IS NOT NULL";
        query += " Order by clave desc";

        var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", entidad },
            { "@filtro2", orden },
            { "@filtro3", ids },
          };
        var parameters = new DynamicParameters(dictionary);
        var dtablas = await connection.QueryAsync<DTablaSIL>(query, parameters);
        return dtablas;
      }
    }
  }
}

using Dapper;
using Oracle.ManagedDataAccess.Client;
using SILData.Model;

namespace SILData.DataAccess
{
  public class VendedorStore : IAccountStore
  {
    private string? _connectionString;
    private readonly ILogger<VendedorStore> _logger;

    public VendedorStore(IConfiguration configuration, ILogger<VendedorStore> logger)
    {
      _connectionString = configuration.GetConnectionString("SilConnection");
      _logger = logger;

    }

    public async Task<IList<CuentaSIL>> FindContainWithLimit(string filtro, int limit)
    {
      try
      {
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          string query = "SELECT cuenta, nombre, cuit FROM CUPOSVENDEDOR WHERE";
          query += " upper(nombre) like :filtro1";
          query += " OR upper(cuit) like :filtro1";
          query += " OR to_char(cuenta) like :filtro1";
          query += " Order by nombre desc";

          var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", ("%" + filtro + "%").ToString() }
          };
          var parameters = new DynamicParameters(dictionary);
          var cuentas = (IList<CuentaSIL>)await connection.QueryAsync<CuentaSIL>(query, parameters);
          return cuentas;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public Task<IList<CuentaSIL>> FindLike(string filtro)
    {
      throw new NotImplementedException();
    }

    public async Task<IList<CuentaSIL>> FindStartsWithLimit(string filtro, int limit)
    {
      try
      {
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          string query = "SELECT cuenta, nombre, cuit FROM CUPOSVENDEDOR WHERE";
          query += " upper(nombre) like :filtro1";
          query += " OR upper(cuit) like :filtro1";
          query += " OR to_char(cuenta) like :filtro1";
          query += " Order by nombre desc";

          var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", (filtro + "%").ToString() }
          };
          var parameters = new DynamicParameters(dictionary);
          var cuentas = (IList<CuentaSIL>)await connection.QueryAsync<CuentaSIL>(query, parameters);
          return cuentas;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public async Task<IList<CuentaSIL>> GetAll()
    {
      throw new NotImplementedException();
    }

    public async Task<IList<CuentaSIL>> GetById()
    {
      throw new NotImplementedException();
    }

    public async Task<CuentaSIL> GetVendedorByCuenta(long cuenta)
    {
      try
      {
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          string query = @"
            SELECT cuenta, nombre, cuit
            FROM CUPOSVENDEDOR
            WHERE cuenta = :cuenta
            ORDER BY nombre DESC";

          var parameters = new DynamicParameters();
          parameters.Add("cuenta", cuenta);

          var Cuenta = await connection.QueryFirstOrDefaultAsync<CuentaSIL>(
              query,
              parameters
          );

          return Cuenta;
        }
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    /// <summary>
    /// No aplica para vendedores; la lookup de compradores vive en
    /// <see cref="CompradorStore.GetCompradorByCuenta"/>. Se implementa
    /// sólo para satisfacer el contrato de <see cref="IAccountStore"/>.
    /// </summary>
    public Task<CuentaSIL> GetCompradorByCuenta(long cuenta)
    {
      throw new NotImplementedException();
    }

    public async Task<IList<CuentaSIL>> GetVendedoresByCuentas(IEnumerable<long> cuentas)
    {
      var lista = cuentas?.Distinct().Where(c => c > 0).ToList()
        ?? new List<long>();
      if (lista.Count == 0)
        return new List<CuentaSIL>();

      try
      {
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          // Oracle: hay que expandir el IN a binds nominales. Armamos la
          // lista de parámetros dinámicamente, uno por cuenta. Dapper los
          // pasa como :p0, :p1, … y Oracle los une en el IN.
          var parameters = new DynamicParameters();
          string binds = string.Join(",", lista.Select((_, i) =>
          {
            string name = $"p{i}";
            parameters.Add(name, lista[i]);
            return $":{name}";
          }));

          string query = $@"
            SELECT cuenta, nombre, cuit
            FROM CUPOSVENDEDOR
            WHERE cuenta IN ({binds})";

          var rows = await connection.QueryAsync<CuentaSIL>(query, parameters);
          return rows.ToList();
        }
      }
      catch (Exception ex)
      {
        throw;
      }
    }
  }
}

using Dapper;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using SILData.Model;

namespace SILData.DataAccess
{
  public class CompradorStore : IAccountStore
  {
    private string? _connectionString;
    private object logger;

    public CompradorStore(IConfiguration configuration, ILogger logger)
    {
      _connectionString = configuration.GetConnectionString("SilConnection");
      this.logger = logger;

    }

    public async Task<IList<CuentaSIL>> FindContainWithLimit(string filtro, int limit)
    {
      try
      {
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          string query = "SELECT cuenta, nombre, cuit FROM CUPOSCOMPRADOR WHERE";
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
          string query = "SELECT cuenta, nombre, cuit FROM CUPOSCOMPRADOR WHERE";
          query += " upper(nombre) like :filtro1";
          query += " OR upper(cuit) like :filtro1";
          query += " OR to_char(cuenta) like :filtro1";
          query += " Order by nombre desc";

          var dictionary = new Dictionary<string, object>
          {
            { "@filtro1", (filtro + "%").ToString() }
          };
          var parameters = new DynamicParameters(dictionary);
          var cuentas = (IList<CuentaSIL>) await connection.QueryAsync<CuentaSIL>(query, parameters);
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

    public Task<CuentaSIL> GetVendedorByCuenta(long cuenta)
    {
      throw new NotImplementedException();
    }

    public Task<IList<CuentaSIL>> GetVendedoresByCuentas(IEnumerable<long> cuentas)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Resuelve un comprador por su cuenta numérica, contra CUPOSCOMPRADOR.
    /// Expuesto en <c>GET /api/AccountData/CompradorByCuenta/{Cuenta}</c> para
    /// que la UI pueda resolver un CUIT puntual; no se usa desde el motor
    /// de matching porque el nombre del comprador ya viene en la fila de
    /// cuposcorre (<c>NOMDESTINATARIO</c>).
    /// </summary>
    public async Task<CuentaSIL> GetCompradorByCuenta(long cuenta)
    {
      try
      {
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
          string query = @"
            SELECT cuenta, nombre, cuit
            FROM CUPOSCOMPRADOR
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
  }
}
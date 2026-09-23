using Dapper;
using Domain.Entities.Externo;
using Oracle.ManagedDataAccess.Client;
using SILData.DataAccess.Map_OracleToSql;
using SILData.Model;
using SILData.Model.SolicitudTurno;
using SILData.SilDataExceptions;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace SILData.DataAccess
{
  public class CuposDisponiblesStore : ICuposDisponiblesStore
  {
    private readonly IConfiguration _configuration;
    private readonly ILogger<CuposDisponiblesStore> _logger;

    public CuposDisponiblesStore(IConfiguration configuration, ILogger<CuposDisponiblesStore> logger)
    {
      _configuration = configuration;
      _logger = logger;
    }

    public async Task<IEnumerable<CuposDisponible>> GetDisponibles(long cuentaVendedor, DateTime fecha, long cuentaComprador = 0, int codigoGrano = 0, long zonaGeografica = 0)
    {
      try
      {
        string? queryPath = _configuration["QueryCuposDisponibles"];
        if (string.IsNullOrWhiteSpace(queryPath))
          throw new Exception(
            "No está configurada la ruta del query de cupos disponibles (QueryCuposDisponibles).");

        string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
          ?? throw new Exception(
            "No se pudo determinar la ubicación del ejecutable.");

        string queryLocation = Path.Combine(executableLocation, queryPath);

        if (!File.Exists(queryLocation))
          throw new Exception(
            $"No se encontró el archivo de query en: {queryLocation}");

        string query = await File.ReadAllTextAsync(queryLocation);

        var dictionary = new Dictionary<string, object>
        {
          { "@cuentavendedor", cuentaVendedor },
          { "@fecha",          fecha.Date.ToString("dd/MM/yyyy") },
          { "@cuentacomprador", cuentaComprador },
          { "@producto",       codigoGrano.ToString() },
          { "@zona",           zonaGeografica }
        };

        SqlMapper.AddTypeHandler(new BooleanTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeTypeHandler(_logger));

        using OracleConnection connection = new OracleConnection(
          _configuration.GetConnectionString("SilConnection"));

        await connection.OpenAsync();
        var parameters = new DynamicParameters(dictionary);
        return await connection.QueryAsync<CuposDisponible>(query, parameters);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex,
          "Error de base de datos en GetDisponibles. Vendedor: {Vendedor} Fecha: {Fecha}",
          cuentaVendedor, fecha);
        throw new Exception(
          "Error al consultar los cupos disponibles en la base de datos.", ex);
      }
      catch (IOException ex)
      {
        _logger.LogError(ex, "Error al leer el archivo de query en GetDisponibles.");
        throw new Exception(
          "Error al leer el archivo de consulta de cupos disponibles.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
          "Error inesperado en GetDisponibles. Vendedor: {Vendedor}", cuentaVendedor);
        throw;
      }
    }
    public async Task<IEnumerable<CuposCorreResult>> GetCuposByStatus(CuposCorreFilter cuposCorreFilter)
    {
      try
      {
        string? queryPath = _configuration["QueryCuposCorreByStatus"];
        if (string.IsNullOrWhiteSpace(queryPath))
          throw new Exception(
            "No está configurada la ruta del query de cupos por estado (QueryCuposCorreByStatus).");

        string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
          ?? throw new Exception(
            "No se pudo determinar la ubicación del ejecutable.");

        string queryLocation = Path.Combine(executableLocation, queryPath);

        if (!File.Exists(queryLocation))
          throw new Exception(
            $"No se encontró el archivo de query en: {queryLocation}");

        string query = await File.ReadAllTextAsync(queryLocation);

        var dictionary = new Dictionary<string, object>
        {
          { "@fechaDesde", cuposCorreFilter.FechaDesde.ToString("dd/MM/yyyy") },
          { "@fechaHasta", cuposCorreFilter.FechaHasta.ToString("dd/MM/yyyy") },
          { "@status",     cuposCorreFilter.status },
          { "@grano",      cuposCorreFilter.Grano }
        };

        SqlMapper.AddTypeHandler(new BooleanTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeTypeHandler(_logger));

        using OracleConnection connection = new OracleConnection(
          _configuration.GetConnectionString("SilConnection"));

        await connection.OpenAsync();
        var parameters = new DynamicParameters(dictionary);
        return await connection.QueryAsync<CuposCorreResult>(query, parameters);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex,
          "Error de base de datos en GetCuposByStatus. Grano: {Grano} Estado: {Status}",
          cuposCorreFilter.Grano, cuposCorreFilter.status);
        throw new Exception(
          "Error al consultar los cupos por estado en la base de datos.", ex);
      }
      catch (IOException ex)
      {
        _logger.LogError(ex, "Error al leer el archivo de query en GetCuposByStatus.");
        throw new Exception(
          "Error al leer el archivo de consulta de cupos por estado.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetCuposByStatus. Grano: {Grano}", cuposCorreFilter.Grano);
        throw;
      }
    }
    public async Task<IEnumerable<SolicitudTurnoCuposDisponibles>> GetCuposDisponiblesForShiftRequest(long cuentaVendedor, DateTime fecha, long cuentaComprador = 0, int codigoGrano = 0, long zonaGeografica = 0)
    {
      try
      {
        string? queryPath = _configuration["CuposDisponiblesForShift"];
        if (string.IsNullOrWhiteSpace(queryPath))
          throw new Exception(
            "No está configurada la ruta del query de cupos disponibles (CuposDisponiblesForShift).");

        string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
          ?? throw new Exception(
            "No se pudo determinar la ubicación del ejecutable.");

        string queryLocation = Path.Combine(executableLocation, queryPath);

        if (!File.Exists(queryLocation))
          throw new Exception(
            $"No se encontró el archivo de query en: {queryLocation}");

        string query = await File.ReadAllTextAsync(queryLocation);

        var dictionary = new Dictionary<string, object>
        {
            { "@cuentavendedor",  cuentaVendedor },
            { "@fechaDate", fecha.Date },
            { "@fechaTexto",           fecha.ToString("dd/MM/yyyy") },
            { "@cuentacomprador", cuentaComprador },
            { "@producto",        codigoGrano },
            { "@zona",            zonaGeografica }
        };
        SqlMapper.AddTypeHandler(new BooleanTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeTypeHandler(_logger));
        using OracleConnection connection = new OracleConnection(
            _configuration.GetConnectionString("SilConnection"));
        await connection.OpenAsync();
        var parameters = new DynamicParameters(dictionary);
        return await connection.QueryAsync<SolicitudTurnoCuposDisponibles>(query, parameters);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex,
          "Error de base de datos en GetDisponibles. Vendedor: {Vendedor} Fecha: {Fecha}",
          cuentaVendedor, fecha);
        throw new Exception(
          "Error al consultar los cupos disponibles en la base de datos.", ex);
      }
      catch (IOException ex)
      {
        _logger.LogError(ex, "Error al leer el archivo de query en GetDisponibles.");
        throw new Exception(
          "Error al leer el archivo de consulta de cupos disponibles.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
          "Error inesperado en GetDisponibles. Vendedor: {Vendedor}", cuentaVendedor);
        throw;
      }
    }
  }
}

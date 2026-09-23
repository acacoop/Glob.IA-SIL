
using Dapper;
using Domain.Entities.Externo;
using IdentityModel.Client;
using Oracle.ManagedDataAccess.Client;
using SILData.Model.SolicitudTurno;
using SILData.SilDataExceptions;
using System.Data;
using System.Dynamic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SILData.DataAccess
{
  public class GeographicalAereaStore : IGeographicalAereaStore
  {
    private string? _connectionString;
    private ILogger<GeographicalAereaStore> _logger;

    public GeographicalAereaStore(IConfiguration configuration, ILogger<GeographicalAereaStore> logger)
    {
      _connectionString = configuration.GetConnectionString("SilConnection");
      _logger = logger;
    }

    public async Task AddDestinations(long zonaGeoId, List<Destino> destinos) 
    {
      if (zonaGeoId <= 0 || destinos is null || !destinos.Any())
        throw new SilDataException(
          "Parámetros inválidos: zonaGeoId debe ser positivo y se debe indicar al menos un destino.",
          StatusCodes.Status400BadRequest);

      using OracleConnection connection = new OracleConnection(_connectionString);
      await connection.OpenAsync();
      using OracleTransaction transaction = connection.BeginTransaction();

      try
      {
        string sql = "INSERT INTO PUERTOPORZONA (ZonaGeoId, Cuenta, Cuit) VALUES (:ZonaGeoId, :Cuenta, :Cuit)";

        foreach (var destino in destinos)
        {
          var p = new DynamicParameters();
          p.Add("ZonaGeoId", zonaGeoId);
          p.Add("Cuenta", destino.Cuenta);
          p.Add("Cuit", destino.Cuit);
          await connection.ExecuteAsync(sql, p, transaction);
        }

        await transaction.CommitAsync();
        _logger.LogInformation("AddDestinations: {Count} destinos agregados a ZonaGeoId {Id}.", destinos.Count, zonaGeoId);
      }
      catch (SilDataException)
      {
        await transaction.RollbackAsync();
        throw;
      }
      catch (OracleException ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error de base de datos en AddDestinations. ZonaGeoId: {Id}", zonaGeoId);
        throw new Exception(
          "Error al agregar destinos en la base de datos.", ex);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error inesperado en AddDestinations. ZonaGeoId: {Id}", zonaGeoId);
        throw;
      }
    }

    public async Task<IEnumerable<ZonaGeografica>> GetByIdAsync(long zonaGeoId) 
    {
      try
      {
        using OracleConnection connection = new OracleConnection(_connectionString);

        string strQuery = @"SELECT ZG.ZONAGEOID, ZG.NOMBRE NOMBREZONA, CODIGO CODIGOZONA, DESCRIPCION DESCRIPCIONZONA, CENTROID,
          PZ.ID, PZ.CUENTA CUENTADESTINO, PZ.CUIT CUITDESTINO, CP.NOMBRE NOMBREDESTINO, CP.DOMICILIO DOMICILIODESTINO,
          CP.TIPODECUENTA TIPODECUENTADESTINO, CP.LOCALIDAD LOCALIDADDESTINO, CP.PROVINCIA PROVINCIADESTINO, CP.CPOSTAL CPOSTALDESTINO
          FROM ZONASGEOGRAFICAS ZG
            LEFT JOIN PUERTOPORZONA PZ ON ZG.ZONAGEOID = PZ.ZONAGEOID
            LEFT JOIN CUPOSPUERTO CP ON PZ.CUENTA = CP.CUENTA and PZ.CUIT = CP.CUIT
          WHERE ZG.ZONAGEOID = :ZonaGeoId";

        var parameters = new DynamicParameters();
        parameters.Add("ZonaGeoId", zonaGeoId);

        var raw = await connection.QueryAsync(strQuery, parameters);
        var result = MapToZonaGeografica(raw);

        _logger.LogInformation("GetByIdAsync: {Count} zonas encontradas para Id {Id}.", result.Count(), zonaGeoId);
        return result;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex, "Error de base de datos en GetByIdAsync. ZonaGeoId: {Id}", zonaGeoId);
        throw new Exception(
          "Error al consultar la zona geográfica en la base de datos.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByIdAsync. ZonaGeoId: {Id}", zonaGeoId);
        throw;
      }

    }

    public async Task<ZonaGeografica> CreateAsync(ZonaGeograficaCreate geographicalArea)
    {
      using OracleConnection connection = new OracleConnection(_connectionString);
      await connection.OpenAsync();
      using OracleTransaction transaction = connection.BeginTransaction();

      try
      {
        int existing = await connection.QueryFirstOrDefaultAsync<int>(
          "SELECT COUNT(1) FROM ZONASGEOGRAFICAS WHERE Codigo = :Codigo OR Nombre = :Nombre",
          new { geographicalArea.Codigo, geographicalArea.Nombre },
          transaction);

        if (existing > 0)
          throw new SilDataException(
            "Ya existe una zona geográfica con el mismo Código o Nombre.",
            StatusCodes.Status409Conflict);

        var parameters = new DynamicParameters();
        parameters.Add("Nombre", geographicalArea.Nombre);
        parameters.Add("Codigo", geographicalArea.Codigo);
        parameters.Add("Descripcion", geographicalArea.Descripcion);
        parameters.Add("CentroId", geographicalArea.CentroId);
        parameters.Add("ZonaGeoId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        string sql = @"INSERT INTO ZONASGEOGRAFICAS (Nombre, Codigo, Descripcion, CentroId)
          VALUES (:Nombre, :Codigo, :Descripcion, :CentroId)
          RETURNING Zonageoid INTO :Zonageoid";

        await connection.ExecuteAsync(sql, parameters, transaction);
        int newId = parameters.Get<int>("ZonaGeoId");

        if (geographicalArea.Destinos is not null && geographicalArea.Destinos.Any())
        {
          string sql2 = "INSERT INTO PUERTOPORZONA (ZonaGeoId, Cuenta, Cuit) VALUES (:ZonaGeoId, :Cuenta, :Cuit)";
          foreach (var destino in geographicalArea.Destinos)
          {
            var p2 = new DynamicParameters();
            p2.Add("ZonaGeoId", newId);
            p2.Add("Cuenta", destino.Cuenta);
            p2.Add("Cuit", destino.Cuit);
            await connection.ExecuteAsync(sql2, p2, transaction);
          }
        }

        await transaction.CommitAsync();
        _logger.LogInformation("CreateAsync: Zona geográfica creada con Id {Id}.", newId);

        return new ZonaGeografica
        {
          ZonaGeoId = newId,
          Nombre = geographicalArea.Nombre,
          Codigo = geographicalArea.Codigo,
          Descripcion = geographicalArea.Descripcion,
          CentroId = geographicalArea.CentroId
        };
      }
      catch (SilDataException)
      {
        await transaction.RollbackAsync();
        throw;
      }
      catch (OracleException ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error de base de datos en CreateAsync. Nombre: {Nombre}", geographicalArea.Nombre);
        throw new Exception(
          "Error al crear la zona geográfica en la base de datos.", ex);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error inesperado en CreateAsync. Nombre: {Nombre}", geographicalArea.Nombre);
        throw;
      }
    }
    public async Task<bool> DeleteAsync(int geographicalAreaId)
    {
      using OracleConnection connection = new OracleConnection(_connectionString);
      await connection.OpenAsync();
      using OracleTransaction transaction = connection.BeginTransaction();

      try
      {
        // Primero elimina los destinos asociados (FK)
        await connection.ExecuteAsync(
          "DELETE FROM PUERTOPORZONA WHERE ZONAGEOID = :ZonaGeograficaId",
          new { ZonaGeograficaId = geographicalAreaId },
          transaction);

        int rowsAffected = await connection.ExecuteAsync(
          "DELETE FROM ZONASGEOGRAFICAS WHERE ZONAGEOID = :ZonaGeograficaId",
          new { ZonaGeograficaId = geographicalAreaId },
          transaction);

        if (rowsAffected == 0)
          throw new SilDataException(
            "No se encontró ninguna zona geográfica con el ID especificado.",
            StatusCodes.Status404NotFound);

        await transaction.CommitAsync();
        _logger.LogInformation("DeleteAsync: Zona geográfica {Id} eliminada correctamente.", geographicalAreaId);
        return true;
      }
      catch (SilDataException)
      {
        await transaction.RollbackAsync();
        throw;
      }
      catch (OracleException ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error de base de datos en DeleteAsync. ZonaId: {Id}", geographicalAreaId);
        throw new Exception(
          "Error al eliminar la zona geográfica en la base de datos.", ex);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error inesperado en DeleteAsync. ZonaId: {Id}", geographicalAreaId);
        throw;
      }
    }

    public async Task<IEnumerable<ZonaGeografica>> GetAllAsync()
    {
      try
      {
        using OracleConnection connection = new OracleConnection(_connectionString);

        string strQuery = @"SELECT ZG.ZONAGEOID, ZG.NOMBRE NOMBREZONA, CODIGO CODIGOZONA, DESCRIPCION DESCRIPCIONZONA, CENTROID,
          PZ.ID, PZ.CUENTA CUENTADESTINO, PZ.CUIT CUITDESTINO, CP.NOMBRE NOMBREDESTINO, CP.DOMICILIO DOMICILIODESTINO,
          CP.TIPODECUENTA TIPODECUENTADESTINO, CP.LOCALIDAD LOCALIDADDESTINO, CP.PROVINCIA PROVINCIADESTINO, CP.CPOSTAL CPOSTALDESTINO
          FROM ZONASGEOGRAFICAS ZG
            LEFT JOIN PUERTOPORZONA PZ ON ZG.ZONAGEOID = PZ.ZONAGEOID
            LEFT JOIN CUPOSPUERTO CP ON PZ.CUENTA = CP.CUENTA and PZ.CUIT = CP.CUIT";

        var raw = await connection.QueryAsync(strQuery);
        var result = MapToZonaGeografica(raw);

        _logger.LogInformation("GetAllAsync: {Count} zonas geográficas obtenidas.", result.Count());
        return result;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex, "Error de base de datos en GetAllAsync (GeographicalAereaStore)");
        throw new Exception(
          "Error al consultar las zonas geográficas en la base de datos.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetAllAsync (GeographicalAereaStore)");
        throw;
      }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper privado: mapea el resultado dinámico de Dapper a ZonaGeografica
    // ─────────────────────────────────────────────────────────────────────────
    private IEnumerable<ZonaGeografica> MapToZonaGeografica(IEnumerable<dynamic> raw)
    {
      return raw
        .GroupBy(r => new
        {
          ZonaGeoId = (long)r.ZONAGEOID,
          Nombre = (string)r.NOMBREZONA,
          Codigo = (string)r.CODIGOZONA,
          Descripcion = (string)r.DESCRIPCIONZONA,
          CentroId = (string)r.CENTROID
        })
        .Select(g => new ZonaGeografica
        {
          ZonaGeoId = g.Key.ZonaGeoId,
          Nombre = g.Key.Nombre,
          Codigo = g.Key.Codigo,
          Descripcion = g.Key.Descripcion,
          CentroId = g.Key.CentroId,
          Destinos = g
            .Where(r => r.CUENTADESTINO != null && r.CUITDESTINO != null)
            .Select(r => new Destino
            {
              Id = (r.ID != null) ? (long)r.ID : 0,
              Cuenta = (r.CUENTADESTINO != null) ? (int)r.CUENTADESTINO : 0,
              Cuit = (r.CUITDESTINO != null) ? (string)r.CUITDESTINO : "",
              Nombre = (r.NOMBREDESTINO != null) ? (string)r.NOMBREDESTINO : "",
              Domicilio = (r.DOMICILIODESTINO != null) ? (string)r.DOMICILIODESTINO : "",
              TipodDeCuenta = (r.TIPODECUENTADESTINO != null) ? (string)r.TIPODECUENTADESTINO : "",
              Localidad = (r.LOCALIDADDESTINO != null) ? (string)r.LOCALIDADDESTINO : "",
              Provincia = (r.PROVINCIADESTINO != null) ? (string)r.PROVINCIADESTINO : "",
              CPostal = (r.CPOSTALDESTINO != null) ? (int)r.CPOSTALDESTINO : 0
            })
        });
    }
    private bool HasProperty(dynamic objeto, string propiedad)
    {
      if (objeto is ExpandoObject expandoObjeto)
      {
        IDictionary<string, object>? diccionario = expandoObjeto as IDictionary<string, object>;
        if (diccionario != null)
          return diccionario.ContainsKey(propiedad);
      }
      return false;
    }

    public async Task<IEnumerable<ZonaGeografica>> ResolveByDestinoAsync(long cuentaPuerto)
    {
      try
      {
        using OracleConnection connection = new OracleConnection(_connectionString);

        // JOIN: PUERTOPORZONA.Cuenta = :cuentaPuerto → ZONASGEOGRAFICAS
        // Devuelve cada zona a la que pertenece el puerto (un puerto puede estar
        // en varias zonas, o en ninguna).
        string strQuery = @"SELECT ZG.ZONAGEOID, ZG.NOMBRE NOMBREZONA, ZG.CODIGO CODIGOZONA,
                                   ZG.DESCRIPCION DESCRIPCIONZONA, ZG.CENTROID
                            FROM ZONASGEOGRAFICAS ZG
                            INNER JOIN PUERTOPORZONA PZ ON ZG.ZONAGEOID = PZ.ZONAGEOID
                            WHERE PZ.CUENTA = :cuentaPuerto
                            ORDER BY ZG.NOMBRE";

        var raw = await connection.QueryAsync(strQuery, new { cuentaPuerto });
        var result = MapToZonaGeograficaFromFlat(raw);

        _logger.LogInformation(
          "ResolveByDestinoAsync: cuentaPuerto={Cuenta} zonasEncontradas={Count}",
          cuentaPuerto, result.Count());
        return result;
      }
      catch (OracleException ex)
      {
        _logger.LogError(ex, "Error de base de datos en ResolveByDestinoAsync. CuentaPuerto: {Id}", cuentaPuerto);
        throw new Exception(
          "Error al resolver zonas por destino en la base de datos.", ex);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en ResolveByDestinoAsync. CuentaPuerto: {Id}", cuentaPuerto);
        throw;
      }
    }

    /// <summary>
    /// Variante de <see cref="MapToZonaGeografica"/> para queries que NO traen
    /// los datos de PUERTOPORZONA/CUPOSPUERTO (no hay JOIN a esas tablas).
    /// Sólo hidrata los campos de la zona geográfica.
    /// </summary>
    private static IEnumerable<ZonaGeografica> MapToZonaGeograficaFromFlat(IEnumerable<dynamic> raw)
    {
      return raw.Select(r => new ZonaGeografica
      {
        ZonaGeoId = (long)r.ZONAGEOID,
        Nombre = (string)r.NOMBREZONA,
        Codigo = (string)r.CODIGOZONA,
        Descripcion = (string)r.DESCRIPCIONZONA,
        CentroId = (string)r.CENTROID,
        Destinos = new List<Destino>()
      });
    }
  }
}

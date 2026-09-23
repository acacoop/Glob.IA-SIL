using SILData.Model.SolicitudTurno;

namespace SILData.DataAccess
{
  public interface IGeographicalAereaStore
  {
    Task<IEnumerable<ZonaGeografica>> GetAllAsync();
    Task<IEnumerable<ZonaGeografica>> GetByIdAsync(long zonaGeoId);
    Task<ZonaGeografica> CreateAsync(ZonaGeograficaCreate geographicalArea);
    Task AddDestinations(long zonaGeoId, List<Destino> destinos);
    Task<bool> DeleteAsync(int geographicalAreaId);

    /// <summary>
    /// Resuelve las zonas geográficas a las que pertenece un puerto (Cuenta).
    /// Query: PUERTOPORZONA.Cuenta = :cuentaPuerto → ZONASGEOGRAFICAS.
    /// </summary>
    Task<IEnumerable<ZonaGeografica>> ResolveByDestinoAsync(long cuentaPuerto);
  }
}

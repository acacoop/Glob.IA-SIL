using SILData.Model.SolicitudTurno;

namespace SILData.Services
{
  public interface IGeographicalAereaService
  {
    Task<IEnumerable<ZonaGeograficaView>> GetByIdAsync(long zonaGeoId);
    Task<IEnumerable<ZonaGeograficaView>> GetAllAsync();
    Task<IEnumerable<ZonaGeograficaView>> GetByFilter(FiltroZonaGeografica filter);
    Task<ZonaGeograficaView?> CreateAsync(ZonaGeograficaCreate geographicalAreaCreate);
    Task<IEnumerable<ZonaGeograficaView>?> AddDestinationAsync(long zonaGeoId, List<string> destinos);
    Task<bool> DeleteAsync(int geographicalAreaId);

    /// <summary>
    /// Resuelve las zonas geográficas a las que pertenece un puerto.
    /// Lista vacía si el puerto no está mapeado a ninguna zona.
    /// </summary>
    Task<IEnumerable<ZonaGeograficaView>> ResolveByDestinoAsync(long cuentaPuerto);
  }
}

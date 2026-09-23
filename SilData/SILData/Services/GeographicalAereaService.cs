using AutoMapper;
using Microsoft.Extensions.Logging;
using SILData.DataAccess;
using SILData.Model;
using SILData.Model.SolicitudTurno;
using SILData.SilDataExceptions;

namespace SILData.Services
{
  public class GeographicalAereaService : IGeographicalAereaService
  {
    private readonly IGeographicalAereaStore _geographicalAereaStore;
    private readonly ILogger<GeographicalAereaService> _logger;
    private readonly ICuposDisponiblesService _cuposDisponiblesService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    public GeographicalAereaService(IConfiguration configuration, IGeographicalAereaStore geographicalAereaStore, ILogger<GeographicalAereaService> logger, IMapper mapper, ICuposDisponiblesService cuposDisponiblesService)
    {
      _geographicalAereaStore = geographicalAereaStore;
      _logger = logger;
      _mapper = mapper;
      _configuration = configuration;
      _cuposDisponiblesService = cuposDisponiblesService;
      
    }

    public async Task<ZonaGeograficaView?> CreateAsync(ZonaGeograficaCreate geographicalAreaCreate)
    {
      try
      {
        if (geographicalAreaCreate is null)
          throw new SilDataException(
            "Los datos de la zona geográfica no pueden ser nulos.", StatusCodes.Status400BadRequest);

        ZonaGeografica area = await _geographicalAereaStore.CreateAsync(geographicalAreaCreate);

        if (area is null)
          return null;

        return _mapper.Map<ZonaGeograficaView>(area);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
          "Error inesperado en CreateAsync. Nombre: {Nombre}", geographicalAreaCreate?.Nombre);
        throw new SilDataException(
          "Ocurrió un error inesperado al crear la zona geográfica.", ex,
          StatusCodes.Status500InternalServerError);
      }
    }

    public async Task<IEnumerable<ZonaGeograficaView>?> AddDestinationAsync(long zonaGeoId, List<string> destinations)
    {
      try
      {
        if (zonaGeoId <= 0)
          throw new SilDataException(
            $"La zona geográfica con Id {zonaGeoId} no es válida.",
            StatusCodes.Status400BadRequest);

        if (destinations is null || !destinations.Any())
          throw new SilDataException(
            "Debe indicar al menos un destino para agregar.", StatusCodes.Status400BadRequest);

        IEnumerable<ZonaGeografica> zonasGeograficas =
          await _geographicalAereaStore.GetByIdAsync(zonaGeoId);

        if (zonasGeograficas is null || !zonasGeograficas.Any())
          throw new SilDataException(
            $"La zona geográfica con Id {zonaGeoId} no existe.", StatusCodes.Status404NotFound);

        ZonaGeografica areaToModified = zonasGeograficas.First();

        PuertoStore puertoStore = new PuertoStore(_configuration, _logger);
        List<Destino> destinationToAdd = new List<Destino>();

        foreach (string dest in destinations)
        {
          // Si el destino ya existe en la zona, se omite sin error
          if (areaToModified.Destinos is not null
            && areaToModified.Destinos.Any(x => x.Cuenta.Equals(dest) || x.Cuit.Equals(dest)))
            continue;

          IList<CuentaSIL> cuentas = await puertoStore.FindLike(dest);

          if (cuentas is null || !cuentas.Any())
            throw new SilDataException(
              $"El puerto '{dest}' no existe en el sistema.", StatusCodes.Status404NotFound);

          destinationToAdd.Add(new Destino
          {
            Id = 0,
            Cuenta = cuentas.First().Cuenta,
            Cuit = cuentas.First().Cuit
          });
        }

        if (!destinationToAdd.Any())
          throw new SilDataException(
            "Los puertos indicados ya se encuentran asociados a la zona geográfica especificada.",
            StatusCodes.Status409Conflict);

        await _geographicalAereaStore.AddDestinations(areaToModified.ZonaGeoId, destinationToAdd);

        IEnumerable<ZonaGeografica> updated =
          await _geographicalAereaStore.GetByIdAsync(areaToModified.ZonaGeoId);

        return updated?.Select(x => _mapper.Map<ZonaGeograficaView>(x));
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (InvalidOperationException ex)
      {
        _logger.LogWarning(ex, "Operación inválida en AddDestinationAsync. ZonaGeoId: {Id}", zonaGeoId);
        throw new Exception(ex.Message);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en AddDestinationAsync. ZonaGeoId: {Id}", zonaGeoId);
        throw new Exception(
          "Ocurrió un error inesperado al agregar destinos a la zona geográfica.", ex);
      }
    }

    public async Task<bool> DeleteAsync(int geographicalAreaId)
    {
      try
      {
        if (geographicalAreaId <= 0)
          throw new SilDataException(
            "El identificador de zona geográfica debe ser un número positivo.",
            StatusCodes.Status400BadRequest);

        return await _geographicalAereaStore.DeleteAsync(geographicalAreaId);
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
          "Error inesperado en DeleteAsync. ZonaId: {Id}", geographicalAreaId);
        throw new Exception(
          "Ocurrió un error inesperado al eliminar la zona geográfica.", ex);
      }
    }

    public async Task<IEnumerable<ZonaGeograficaView>> ResolveByDestinoAsync(long cuentaPuerto)
    {
      try
      {
        if (cuentaPuerto <= 0)
          throw new SilDataException(
            "El identificador del destino (CuentaPuerto) debe ser un número positivo.",
            StatusCodes.Status400BadRequest);

        IEnumerable<ZonaGeografica> zonas =
          await _geographicalAereaStore.ResolveByDestinoAsync(cuentaPuerto);

        if (zonas is null || !zonas.Any())
          return Enumerable.Empty<ZonaGeograficaView>();

        return zonas.Select(z => _mapper.Map<ZonaGeograficaView>(z));
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
          "Error inesperado en ResolveByDestinoAsync. CuentaPuerto: {Id}", cuentaPuerto);
        throw new Exception(
          "Ocurrió un error inesperado al resolver las zonas por destino.", ex);
      }
    }

    public async Task<IEnumerable<ZonaGeograficaView>> GetAllAsync()
    {
      try
      {
        IEnumerable<ZonaGeografica> areas = await _geographicalAereaStore.GetAllAsync();

        if (areas is null || !areas.Any())
          return Enumerable.Empty<ZonaGeograficaView>();

        return areas.Select(x => _mapper.Map<ZonaGeograficaView>(x));
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetAllAsync (GeographicalAereaService)");
        throw;
      }
    }

    public async Task<IEnumerable<ZonaGeograficaView>> GetByIdAsync(long zonaGeoId)
    {
      try
      {
        if (zonaGeoId <= 0)
          throw new SilDataException(
            "El identificador de zona geográfica debe ser un número positivo.",
            StatusCodes.Status400BadRequest);

        IEnumerable<ZonaGeografica> areas = await _geographicalAereaStore.GetByIdAsync(zonaGeoId);

        if (areas is null || !areas.Any())
          return Enumerable.Empty<ZonaGeograficaView>();

        return areas.Select(x => _mapper.Map<ZonaGeograficaView>(x));
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByIdAsync. ZonaGeoId: {Id}", zonaGeoId);
        throw new Exception(
          "Ocurrió un error inesperado al obtener la zona geográfica.", ex);
      }
    }
    public async Task<IEnumerable<ZonaGeograficaView>> GetByFilter(FiltroZonaGeografica? filter)
    {
      try
      {
        // Sin filtro: devuelve todo
        if (filter is null)
        {
          IEnumerable<ZonaGeografica> todas = await _geographicalAereaStore.GetAllAsync();
          return todas.Select(x => _mapper.Map<ZonaGeograficaView>(x));
        }

        IEnumerable<CuposDisponible> cupos = await _cuposDisponiblesService.GetDisponibles(
          filter.CuentaVendedora,
          filter.Fecha,
          filter.CuentaCompradora ?? 0,
          filter.Grano ?? 0);

        if (cupos is null || !cupos.Any())
          return Enumerable.Empty<ZonaGeograficaView>();

        return cupos
          .Where(x => x.ZonaGeograficaId != 0)
          .Select(x => _mapper.Map<ZonaGeograficaView>(x));
      }
      catch (SilDataException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
          "Error inesperado en GetByFilter. Vendedor: {Vendedor}", filter?.CuentaVendedora);
        throw new Exception(
          "Ocurrió un error inesperado al filtrar las zonas geográficas.", ex);
      }
    }
  }
}

using Microsoft.Extensions.Caching.Memory;
using SILData.Model.SolicitudTurno;

namespace SILData.Services
{
  /// <summary>
  /// Caché de catálogos de lectura frecuente para el matching (flag
  /// <c>Features:CatalogCache</c>). TTL absoluto configurable con
  /// <c>Features:CatalogCacheMinutos</c> (default 5). Los errores de BD NO
  /// se cachean.
  /// </summary>
  public interface ICatalogCache
  {
    /// <summary>Resolución puerto (CuentaPuerto) → zonas geográficas.</summary>
    Task<IEnumerable<ZonaGeograficaView>> GetZonasPorPuertoAsync(
      long cuentaPuerto,
      Func<long, Task<IEnumerable<ZonaGeograficaView>>> fetch);

    /// <summary>
    /// Nombres de vendedores por cuenta. Sólo consulta al catálogo las cuentas
    /// que no están en caché. <paramref name="fetchMissing"/> debe lanzar
    /// excepción ante un error (para no cachear "desconocidos" falsos).
    /// </summary>
    Task<Dictionary<long, string>> GetNombresVendedorAsync(
      IEnumerable<long> cuentas,
      Func<IReadOnlyCollection<long>, Task<Dictionary<long, string>>> fetchMissing);
  }

  public sealed class CatalogCache : ICatalogCache
  {
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CatalogCache> _logger;

    public CatalogCache(IMemoryCache cache, IConfiguration configuration, ILogger<CatalogCache> logger)
    {
      _cache = cache;
      _configuration = configuration;
      _logger = logger;
    }

    private TimeSpan Ttl =>
      TimeSpan.FromMinutes(Math.Max(1, FeatureFlags.GetInt(_configuration, FeatureFlags.CatalogCacheMinutos, 5)));

    public async Task<IEnumerable<ZonaGeograficaView>> GetZonasPorPuertoAsync(
      long cuentaPuerto,
      Func<long, Task<IEnumerable<ZonaGeograficaView>>> fetch)
    {
      string key = $"catalog:zonasPorPuerto:{cuentaPuerto}";
      if (_cache.TryGetValue(key, out List<ZonaGeograficaView>? cached) && cached is not null)
        return cached;

      var zonas = (await fetch(cuentaPuerto) ?? Enumerable.Empty<ZonaGeograficaView>()).ToList();
      _cache.Set(key, zonas, Ttl);
      return zonas;
    }

    public async Task<Dictionary<long, string>> GetNombresVendedorAsync(
      IEnumerable<long> cuentas,
      Func<IReadOnlyCollection<long>, Task<Dictionary<long, string>>> fetchMissing)
    {
      var resultado = new Dictionary<long, string>();
      var faltantes = new List<long>();

      foreach (var cuenta in (cuentas ?? Enumerable.Empty<long>()).Where(c => c > 0).Distinct())
      {
        // Se cachean también los "no encontrados" (valor null) para no
        // re-consultar cuentas inexistentes en cada matching.
        if (_cache.TryGetValue(VendedorKey(cuenta), out VendedorEntry? entry) && entry is not null)
        {
          if (entry.Nombre is not null) resultado[cuenta] = entry.Nombre;
        }
        else
        {
          faltantes.Add(cuenta);
        }
      }

      if (faltantes.Count == 0) return resultado;

      var encontrados = await fetchMissing(faltantes) ?? new Dictionary<long, string>();
      var ttl = Ttl;
      foreach (var cuenta in faltantes)
      {
        encontrados.TryGetValue(cuenta, out var nombre);
        _cache.Set(VendedorKey(cuenta), new VendedorEntry(nombre), ttl);
        if (nombre is not null) resultado[cuenta] = nombre;
      }

      _logger.LogDebug("CatalogCache vendedores: {Misses} cuentas consultadas al catálogo.", faltantes.Count);
      return resultado;
    }

    private static string VendedorKey(long cuenta) => $"catalog:vendedor:{cuenta}";

    private sealed record VendedorEntry(string? Nombre);
  }
}

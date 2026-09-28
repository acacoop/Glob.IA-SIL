namespace SILData.Services
{
  /// <summary>
  /// Feature flags de optimizaciones de performance. Se leen de la sección
  /// <c>Features</c> de la configuración (appsettings / App Service settings
  /// <c>Features__Xxx=true</c>). Si la clave no existe, el flag está APAGADO
  /// y se ejecuta el camino legacy sin cambios.
  /// </summary>
  public static class FeatureFlags
  {
    public const string AcceptBatchLookup = "AcceptBatchLookup";
    public const string CuposFiltroUnico = "CuposFiltroUnico";
    public const string SqlFileCache = "SqlFileCache";
    public const string CatalogCache = "CatalogCache";
    public const string DistribucionV2Optimizada = "DistribucionV2Optimizada";

    /// <summary>Días máximos hacia atrás (desde hoy) para MatchesDistribucionV2. 0 = sin límite.</summary>
    public const string DistribucionV2MaxDiasAtras = "DistribucionV2MaxDiasAtras";

    /// <summary>Días máximos hacia adelante (desde hoy) para MatchesDistribucionV2. 0 = sin límite.</summary>
    public const string DistribucionV2MaxDiasAdelante = "DistribucionV2MaxDiasAdelante";

    /// <summary>TTL en minutos de la caché de catálogos. Default 5.</summary>
    public const string CatalogCacheMinutos = "CatalogCacheMinutos";

    public static bool IsEnabled(IConfiguration? configuration, string flag)
    {
      // Parse tolerante: un valor mal escrito deja el flag apagado en lugar de tirar excepción.
      return configuration is not null
        && bool.TryParse(configuration[$"Features:{flag}"], out var enabled)
        && enabled;
    }

    public static int GetInt(IConfiguration? configuration, string key, int defaultValue)
    {
      return configuration is not null
        && int.TryParse(configuration[$"Features:{key}"], out var value)
        ? value
        : defaultValue;
    }
  }
}

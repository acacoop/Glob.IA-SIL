using Dapper;
using SILData.DataAccess.Map_OracleToSql;
using System.Collections.Concurrent;

namespace SILData.DataAccess
{
  /// <summary>
  /// Caché en memoria del contenido de los archivos .sql (flag
  /// <c>Features:SqlFileCache</c>). El archivo se lee del disco la primera
  /// vez que se usa y queda en memoria durante la vida del proceso (los .sql
  /// se despliegan junto con el binario, así que un deploy reinicia la app y
  /// refresca la caché). Si la lectura falla, no se cachea el error.
  /// </summary>
  public static class SqlFileCache
  {
    private static readonly ConcurrentDictionary<string, Lazy<string>> _cache =
      new(StringComparer.OrdinalIgnoreCase);

    private static int _typeHandlersRegistered;

    public static string GetOrLoad(string fullPath) => GetOrLoad(fullPath, null);

    /// <param name="missingFileException">
    /// Si se provee y el archivo no existe al cargarlo por primera vez, se
    /// lanza esa excepción (mismo mensaje que el camino legacy con File.Exists).
    /// </param>
    public static string GetOrLoad(string fullPath, Func<Exception>? missingFileException)
    {
      var lazy = _cache.GetOrAdd(fullPath,
        p => new Lazy<string>(() =>
        {
          if (missingFileException is not null && !File.Exists(p))
            throw missingFileException();
          return File.ReadAllText(p);
        }, LazyThreadSafetyMode.ExecutionAndPublication));
      try
      {
        return lazy.Value;
      }
      catch
      {
        _cache.TryRemove(new KeyValuePair<string, Lazy<string>>(fullPath, lazy));
        throw;
      }
    }

    /// <summary>
    /// Registra los TypeHandlers de Dapper UNA sola vez por proceso. El camino
    /// legacy los registra en cada request, y cada <c>SqlMapper.AddTypeHandler</c>
    /// purga la caché de deserializadores de Dapper para ese tipo. El estado
    /// final es el mismo (los handlers son globales), pero sin el costo por request.
    /// </summary>
    public static void EnsureTypeHandlersRegistered(ILogger logger)
    {
      if (Interlocked.CompareExchange(ref _typeHandlersRegistered, 1, 0) != 0) return;
      SqlMapper.AddTypeHandler(new BooleanTypeHandler());
      SqlMapper.AddTypeHandler(new DateTimeTypeHandler(logger));
    }

    internal static void ClearForTests() => _cache.Clear();
  }
}

using System.Diagnostics;

namespace SILData.Services
{
  /// <summary>
  /// Instrumentación liviana por etapas. Uso:
  /// <code>using var perf = new PerfScope(_logger, "BuscarMatchesAsync");
  /// ... perf.Mark("solicitudes"); ...</code>
  /// Al hacer Dispose (fin del método, early return o excepción) emite UNA
  /// línea LogInformation con el total y el tiempo de cada etapa, para poder
  /// comparar en Application Insights (customDimensions: Operation, TotalMs,
  /// Etapas). Sólo loguea: no altera el flujo.
  /// </summary>
  public sealed class PerfScope : IDisposable
  {
    private readonly ILogger _logger;
    private readonly string _operation;
    private readonly Stopwatch _total = Stopwatch.StartNew();
    private readonly Stopwatch _stage = Stopwatch.StartNew();
    private readonly List<KeyValuePair<string, long>> _stages = new();
    private bool _disposed;

    public PerfScope(ILogger logger, string operation)
    {
      _logger = logger;
      _operation = operation;
    }

    /// <summary>Cierra la etapa actual con el nombre dado y arranca la siguiente.</summary>
    public void Mark(string stage)
    {
      _stages.Add(new KeyValuePair<string, long>(stage, _stage.ElapsedMilliseconds));
      _stage.Restart();
    }

    public void Dispose()
    {
      if (_disposed) return;
      _disposed = true;
      try
      {
        _total.Stop();
        string etapas = string.Join(", ", _stages.Select(s => $"{s.Key}={s.Value}ms"));
        _logger.LogInformation(
          "Perf {Operation}: TotalMs={TotalMs} Etapas=[{Etapas}]",
          _operation, _total.ElapsedMilliseconds, etapas);
      }
      catch
      {
        // La instrumentación nunca debe romper el request.
      }
    }
  }
}

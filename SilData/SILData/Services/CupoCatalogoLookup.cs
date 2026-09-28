using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SILData.DataAccess;

namespace SILData.Services
{
  /// <summary>
  /// Lookup de nombres en el catálogo para los IDs que faltan en el query
  /// liviano de <c>CuposStore.FindAvailableCuposByPeriodAsync</c>.
  ///
  /// El query devuelve los cupos con un único JOIN liviano a cupospuerto, así
  /// que las columnas <c>NomDestino</c> y <c>NomVendSIL</c> pueden venir
  /// pobladas; <c>NomVendSIL</c> no está en cuposcorre, así que aún hay que
  /// salir a <c>CUPOSVENDEDOR</c> para resolverla.
  ///
  /// El resto de los nombres ya viene en la fila de cuposcorre o se
  /// resuelve por otros caminos:
  /// <list type="bullet">
  ///   <item><c>NomDestino</c>: viene en la fila (vía LEFT JOIN a cupospuerto).</item>
  ///   <item><c>NomCompSIL</c>: viene en la fila (<c>NOMDESTINATARIO</c>).</item>
  ///   <item>Pertenencia a zonas geográficas: <see cref="IZonaGeograficaResolver"/>.</item>
  /// </list>
  ///
  /// Si el catálogo no tiene el código, el campo del DTO queda null y la
  /// UI lo muestra como "No informado" en cursiva amber. Si el cupo no
  /// trae el código (string vacío o null), tampoco se busca.
  /// </summary>
  public class CupoCatalogoLookup
  {
    private readonly ILogger<CupoCatalogoLookup> _logger;
    private readonly string? _connectionString;

    public CupoCatalogoLookup(
      ILogger<CupoCatalogoLookup> logger,
      string? connectionString)
    {
      _logger = logger;
      _connectionString = connectionString;
    }

    /// <summary>
    /// Resuelve los nombres de los vendedores en batch a partir de las
    /// cuentas del cupo (<c>CodVendSIL</c>). Sólo aparecen las cuentas que
    /// el catálogo conoce; las desconocidas quedan fuera del diccionario.
    ///
    /// Cupos con <c>CodVendSIL</c> vacío o no numérico NO se buscan: el
    /// operador va a leer "No informado" en el card sin gastar una query
    /// en el catálogo.
    /// </summary>
    public async Task<Dictionary<long, string>> ResolverNombresVendedorAsync(
      IEnumerable<string?> codigosVend)
    {
      var cuentas = ParseCuentas(codigosVend);
      if (cuentas.Count == 0) return new Dictionary<long, string>();

      try
      {
        var store = new VendedorStore(MakeConfiguration(), BuildStoreLogger<VendedorStore>());
        var service = new AccountService(store);
        var rows = await service.GetVendedoresByCuentas(cuentas);
        return rows
          .Where(r => r is not null && !string.IsNullOrWhiteSpace(r.Nombre))
          .GroupBy(r => r.Cuenta)
          .ToDictionary(g => g.Key, g => g.First().Nombre);
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "ResolverNombresVendedorAsync: falló el batch de {Count} cuentas.", cuentas.Count);
        return new Dictionary<long, string>();
      }
    }

    /// <summary>
    /// Variante para <see cref="ICatalogCache"/> (flag <c>Features:CatalogCache</c>):
    /// mismo resultado que <see cref="ResolverNombresVendedorAsync"/> pero
    /// PROPAGA la excepción (para no cachear un error como "desconocido") y
    /// consulta en lotes de 900 para no superar el límite de 1000 del IN.
    /// </summary>
    public async Task<Dictionary<long, string>> FetchNombresVendedorAsync(IReadOnlyCollection<long> cuentas)
    {
      var resultado = new Dictionary<long, string>();
      if (cuentas is null || cuentas.Count == 0) return resultado;

      var store = new VendedorStore(MakeConfiguration(), BuildStoreLogger<VendedorStore>());
      var service = new AccountService(store);
      foreach (var lote in cuentas.Chunk(900))
      {
        var rows = await service.GetVendedoresByCuentas(lote);
        foreach (var g in rows
          .Where(r => r is not null && !string.IsNullOrWhiteSpace(r.Nombre))
          .GroupBy(r => r.Cuenta))
        {
          resultado.TryAdd(g.Key, g.First().Nombre);
        }
      }
      return resultado;
    }

    /// <summary>
    /// Helper estático: busca el nombre de un código numérico en el map
    /// devuelto por <see cref="ResolverNombresVendedorAsync"/>. Devuelve
    /// null si el cupo no trae código o si el catálogo no lo conoce.
    /// </summary>
    public static string? TryGetName(Dictionary<long, string> nombres, string? codigo)
    {
      if (string.IsNullOrWhiteSpace(codigo)) return null;
      if (!long.TryParse(codigo.Trim(), out var n)) return null;
      return nombres.TryGetValue(n, out var name) ? name : null;
    }

    /// <summary>
    /// Parsea una colección de strings (que vienen como varchar desde el
    /// query liviano de cuposcorre) a un set de long únicos y positivos.
    /// Los códigos vacíos o no numéricos quedan fuera (la UI los trata
    /// como "No informado" sin gastar una query en el catálogo).
    /// </summary>
    internal static HashSet<long> ParseCuentas(IEnumerable<string?> codigos)
    {
      var cuentas = new HashSet<long>();
      foreach (var s in codigos)
      {
        if (string.IsNullOrWhiteSpace(s)) continue;
        if (long.TryParse(s.Trim(), out var n) && n > 0)
          cuentas.Add(n);
      }
      return cuentas;
    }

    /// <summary>
    /// Construye un <see cref="IConfiguration"/> mínimo con la connection
    /// string que ya tenemos capturada. Sirve para armar las stores de
    /// catálogo (VendedorStore) sin pedirle al motor de DI que conozca
    /// tipos legacy.
    /// </summary>
    private IConfiguration MakeConfiguration()
    {
      var dict = new Dictionary<string, string?>
      {
        ["ConnectionStrings:SilConnection"] = _connectionString
      };
      return new ConfigurationBuilder()
        .AddInMemoryCollection(dict)
        .Build();
    }

    /// <summary>
    /// Las stores de catálogo esperan un <see cref="ILogger{T}"/> propio.
    /// Creamos uno descartable porque los errores de lookup ya se loguean
    /// a nivel del hydrator.
    /// </summary>
    private static ILogger<T> BuildStoreLogger<T>()
      where T : class
    {
      using var factory = LoggerFactory.Create(b => { });
      return factory.CreateLogger<T>();
    }
  }
}
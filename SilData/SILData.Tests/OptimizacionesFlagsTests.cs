using Domain.Entities.Externo;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.ClassShared.Interfaces;
using Shared.ClassShared.Requests;
using SILData.DataAccess;
using SILData.Model.SolicitudTurno;
using SILData.Services;

namespace SILData.Tests
{
  /// <summary>Flag Features:CuposFiltroUnico â€” legacy vs pipeline Ãºnico.</summary>
  public class CuposFiltroUnicoTests
  {
    private static IConfiguration Config(bool flag) =>
      new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["Features:CuposFiltroUnico"] = flag ? "true" : "false" })
        .Build();

    private static List<Cupo> Datos()
    {
      var rnd = new Random(42);
      var vend = new[] { "10", "20", "30", null };
      var dest = new[] { "D1", "D2", "D3" };
      var gran = new[] { "1", "2", "3" };
      var cent = new[] { "C1", "C2" };
      return Enumerable.Range(1, 500).Select(i => new Cupo
      {
        Id = i,
        CodVendSIL = vend[rnd.Next(vend.Length)],
        CodDestino = dest[rnd.Next(dest.Length)],
        CodGrano = gran[rnd.Next(gran.Length)],
        CentroCupo = cent[rnd.Next(cent.Length)],
        EstadoSTOP = rnd.Next(2).ToString(),
        EstaSIL = rnd.Next(2) == 0,
        EstaSTOP = rnd.Next(2) == 0,
        // Fechas repetidas para verificar que el orden estable se conserva.
        Fecha = new DateTime(2026, 10, 1).AddDays(rnd.Next(5))
      }).ToList();
    }

    public static IEnumerable<object[]> Requests()
    {
      CPEReportRequest R(int tipo, int stop = -1, IList<string>? v = null, IList<string>? c = null,
        IList<string>? d = null, IList<string>? p = null, IList<string>? ce = null) =>
        new() { FechaDesde = new DateTime(2026, 10, 1), FechaHasta = new DateTime(2026, 10, 5), TipoDeReporte = tipo,
                EstadoDeCupoEnSTOP = stop, Vendedores = v, Compradores = c, Destinos = d, Productos = p, Centros = ce };

      yield return new object[] { R(2) };
      yield return new object[] { R(0) };
      yield return new object[] { R(1) };
      yield return new object[] { R(2, v: new List<string> { "10", "20" }) };
      yield return new object[] { R(2, v: new List<string> { "10" }, c: new List<string> { "99" }) };
      yield return new object[] { R(0, d: new List<string> { "D1" }, p: new List<string> { "2", "3" }, ce: new List<string> { "C2" }) };
      yield return new object[] { R(1, v: new List<string>(), d: new List<string> { "D2", "D3" }) };
      yield return new object[] { R(2, stop: 0) };
    }

    [Theory]
    [MemberData(nameof(Requests))]
    public async Task Pipeline_devuelve_mismo_resultado_y_orden(CPEReportRequest request)
    {
      var datos = Datos();
      var store = new Mock<ISILCuposStore>();
      // Cada llamada recibe su propia lista (misma instancia de cupos).
      store.Setup(s => s.FindCuposByPeriod(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .ReturnsAsync(() => datos.ToList());

      var legacy = await new CuposServices(store.Object, NullLogger<CuposServices>.Instance, Config(false)).GetCupos(request);
      var nuevo = await new CuposServices(store.Object, NullLogger<CuposServices>.Instance, Config(true)).GetCupos(request);

      Assert.Equal(legacy.Select(c => c.Id), nuevo.Select(c => c.Id));
    }

    [Fact]
    public async Task Pipeline_replica_la_excepcion_legacy_de_Compradores_sin_Vendedores()
    {
      var datos = Datos();
      var store = new Mock<ISILCuposStore>();
      store.Setup(s => s.FindCuposByPeriod(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(() => datos.ToList());
      var req = new CPEReportRequest { TipoDeReporte = 2, EstadoDeCupoEnSTOP = -1, Compradores = new List<string> { "1" } };

      await Assert.ThrowsAsync<NullReferenceException>(() =>
        new CuposServices(store.Object, NullLogger<CuposServices>.Instance, Config(false)).GetCupos(req));
      await Assert.ThrowsAsync<NullReferenceException>(() =>
        new CuposServices(store.Object, NullLogger<CuposServices>.Instance, Config(true)).GetCupos(req));
    }
  }

  public class FeatureFlagsTests
  {
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("false", false)]
    [InlineData("basura", false)]
    [InlineData("true", true)]
    [InlineData("True", true)]
    public void Flag_ausente_o_invalido_queda_apagado(string? valor, bool esperado)
    {
      var dict = new Dictionary<string, string?>();
      if (valor is not null) dict["Features:CatalogCache"] = valor;
      var cfg = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
      Assert.Equal(esperado, FeatureFlags.IsEnabled(cfg, FeatureFlags.CatalogCache));
    }
  }

  public class SqlFileCacheTests
  {
    [Fact]
    public void Lee_una_vez_y_no_cachea_archivos_inexistentes()
    {
      var path = Path.Combine(Path.GetTempPath(), $"sqlcache_{Guid.NewGuid():N}.sql");
      Assert.Throws<InvalidOperationException>(() => SqlFileCache.GetOrLoad(path, () => new InvalidOperationException("no existe")));

      File.WriteAllText(path, "SELECT 1 FROM DUAL");
      try
      {
        Assert.Equal("SELECT 1 FROM DUAL", SqlFileCache.GetOrLoad(path));
        File.WriteAllText(path, "SELECT 2 FROM DUAL");
        Assert.Equal("SELECT 1 FROM DUAL", SqlFileCache.GetOrLoad(path)); // cacheado
      }
      finally
      {
        File.Delete(path);
      }
    }
  }

  public class CatalogCacheTests
  {
    private static CatalogCache NewCache() =>
      new(new MemoryCache(new MemoryCacheOptions()), new ConfigurationBuilder().Build(), NullLogger<CatalogCache>.Instance);

    [Fact]
    public async Task Vendedores_consulta_solo_faltantes_y_cachea_desconocidos()
    {
      var cache = NewCache();
      var consultas = new List<long[]>();
      Task<Dictionary<long, string>> Fetch(IReadOnlyCollection<long> ids)
      {
        consultas.Add(ids.OrderBy(x => x).ToArray());
        return Task.FromResult(ids.Where(i => i != 3).ToDictionary(i => i, i => $"V{i}"));
      }

      var r1 = await cache.GetNombresVendedorAsync(new long[] { 1, 2, 3, 0, -5, 2 }, Fetch);
      var r2 = await cache.GetNombresVendedorAsync(new long[] { 1, 3, 4 }, Fetch);

      Assert.Equal(new Dictionary<long, string> { [1] = "V1", [2] = "V2" }, r1);
      Assert.Equal(new Dictionary<long, string> { [1] = "V1", [4] = "V4" }, r2);
      Assert.Equal(2, consultas.Count);
      Assert.Equal(new long[] { 1, 2, 3 }, consultas[0]);
      Assert.Equal(new long[] { 4 }, consultas[1]);
    }

    [Fact]
    public async Task Vendedores_no_cachea_errores()
    {
      var cache = NewCache();
      await Assert.ThrowsAsync<Exception>(() => cache.GetNombresVendedorAsync(new long[] { 1 }, _ => throw new Exception("db")));
      var r = await cache.GetNombresVendedorAsync(new long[] { 1 }, ids => Task.FromResult(new Dictionary<long, string> { [1] = "V1" }));
      Assert.Equal("V1", r[1]);
    }

    [Fact]
    public async Task Zonas_por_puerto_se_consultan_una_vez()
    {
      var cache = NewCache();
      int llamadas = 0;
      Task<IEnumerable<ZonaGeograficaView>> Fetch(long cp)
      {
        llamadas++;
        return Task.FromResult<IEnumerable<ZonaGeograficaView>>(new[] { new ZonaGeograficaView { zonaGeoId = 7, Nombre = "Z7", CentroId = "C1" } });
      }

      var a = await cache.GetZonasPorPuertoAsync(100, Fetch);
      var b = await cache.GetZonasPorPuertoAsync(100, Fetch);
      Assert.Equal(1, llamadas);
      Assert.Equal(new long[] { 7 }, a.Select(z => z.zonaGeoId));
      Assert.Equal(new long[] { 7 }, b.Select(z => z.zonaGeoId));
    }
  }

  public class DistribucionV2RangoTests
  {
    private static SolicitudTurnoMatchingV2Service Service(Dictionary<string, string?> cfg) =>
      new(Mock.Of<ISolicitudTurnoStore>(), NullLogger<SolicitudTurnoMatchingV2Service>.Instance,
        new ConfigurationBuilder().AddInMemoryCollection(cfg).Build(),
        Mock.Of<ACA.Matching.Engine.IMatchingEngine>(), Mock.Of<IZonaGeograficaResolver>());

    [Fact]
    public void Sin_limites_configurados_no_acota()
    {
      var hoy = new DateTime(2026, 9, 28);
      var (d, h, vacio) = Service(new()).AcotarRangoFechas(new DateTime(2001, 1, 1), new DateTime(2200, 12, 1), hoy);
      Assert.Equal(new DateTime(2001, 1, 1), d);
      Assert.Equal(new DateTime(2200, 12, 1), h);
      Assert.False(vacio);
    }

    [Fact]
    public void Con_limites_acota_al_rango_configurado()
    {
      var hoy = new DateTime(2026, 9, 28);
      var svc = Service(new() { ["Features:DistribucionV2MaxDiasAtras"] = "30", ["Features:DistribucionV2MaxDiasAdelante"] = "90" });
      var (d, h, vacio) = svc.AcotarRangoFechas(new DateTime(2001, 1, 1), new DateTime(2200, 12, 1), hoy);
      Assert.Equal(hoy.AddDays(-30), d);
      Assert.Equal(hoy.AddDays(90), h);
      Assert.False(vacio);

      var (_, _, vacio2) = svc.AcotarRangoFechas(new DateTime(2001, 1, 1), new DateTime(2001, 2, 1), hoy);
      Assert.True(vacio2);
    }
  }
}

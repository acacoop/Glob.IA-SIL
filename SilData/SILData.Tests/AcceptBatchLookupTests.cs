using ACA.Matching.Contexto;
using ACA.Matching.Engine;
using ACA.Matching.Modelos;
using Domain.Entities.Externo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.ClassShared.Interfaces;
using SILData.DataAccess;
using SILData.Model.SolicitudTurno;
using SILData.Services;
using SILData.SilDataExceptions;
using System.Text.Json;

namespace SILData.Tests
{
  /// <summary>
  /// Flag Features:AcceptBatchLookup — compara el camino legacy (GetByIdAsync
  /// por solicitud) contra el batch (GetByIdsAsync) sobre los mismos datos.
  /// </summary>
  public class AcceptBatchLookupTests
  {
    private static IConfiguration Config(bool flag) =>
      new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["Features:AcceptBatchLookup"] = flag ? "true" : "false"
        })
        .Build();

    // Estado "en BD" de las solicitudes: Id → Cantidad. 104 no existe; 105 tiene cantidad 0.
    private static readonly Dictionary<long, int> CantidadesBd = new()
    {
      [101] = 3,
      [102] = 1,
      [103] = 2,
      [105] = 0,
      [106] = 5,
    };

    private sealed class Harness
    {
      public Mock<ISolicitudTurnoStore> Store { get; } = new();
      public Mock<ISILCuposStore> CuposStore { get; } = new();
      public List<AcceptOperation>? OperacionesRecibidas { get; private set; }
      public List<Cupo>? CuposActualizados { get; private set; }
      public SolicitudTurnoService Service { get; }

      public Harness(bool flag, long? incompatibleId = null, bool batchFalla = false, long? storeFallaId = null)
      {
        Store.Setup(s => s.GetByIdAsync(It.IsAny<long>()))
          .ReturnsAsync((long id) =>
          {
            if (id == storeFallaId) throw new Exception("boom");
            return CantidadesBd.TryGetValue(id, out var c)
              ? new SolicitudTurno { Id = id, Cantidad = c, CodigoCentro = "C1" }
              : null;
          });

        Store.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<long>>()))
          .ReturnsAsync((IEnumerable<long> ids) =>
          {
            if (batchFalla) throw new Exception("batch boom");
            return ids.Where(id => id > 0).Distinct()
              .Where(CantidadesBd.ContainsKey)
              .ToDictionary(id => id, id => new SolicitudTurno { Id = id, Cantidad = CantidadesBd[id], CodigoCentro = "C1" });
          });

        Store.Setup(s => s.AcceptRequestsAsync(It.IsAny<IList<AcceptOperation>>()))
          .ReturnsAsync((IList<AcceptOperation> ops) =>
          {
            OperacionesRecibidas = ops.ToList();
            // La operación de la solicitud 103 simula conflicto de concurrencia.
            return ops.Select(o => new AcceptOperationResult
            {
              SolicitudId = o.SolicitudId,
              Exitoso = o.SolicitudId != 103,
              CuposAsignados = o.CuposAsignados,
              MotivoFalla = o.SolicitudId == 103 ? "conflicto" : null
            }).ToList();
          });

        CuposStore.Setup(s => s.UpdateCuposDistributionAsync(It.IsAny<IList<Cupo>>()))
          .Callback((IList<Cupo> cupos) => CuposActualizados = cupos.ToList())
          .Returns(Task.CompletedTask);

        var engine = new Mock<IMatchingEngine>();
        engine.Setup(e => e.Evaluar(It.IsAny<Cupo>(), It.IsAny<SolicitudMatching>(), It.IsAny<IContextoZona?>()))
          .Returns((Cupo c, SolicitudMatching s, IContextoZona? _) =>
            s.Id == incompatibleId ? MatchResult.Incompatible(c, s, "incompatible test") : MatchResult.Directo(c, s));

        var resolver = new Mock<IZonaGeograficaResolver>();
        resolver.Setup(r => r.ResolverAsync(It.IsAny<IEnumerable<long>>()))
          .ReturnsAsync(Mock.Of<IContextoZona>());

        Service = new SolicitudTurnoService(
          Store.Object,
          Mock.Of<ICuposDisponiblesService>(),
          Mock.Of<IGeographicalAereaService>(),
          NullLogger<SolicitudTurnoService>.Instance,
          Config(flag),
          CuposStore.Object,
          engine.Object,
          resolver.Object);
      }
    }

    private static ShiftRequestAcceptData Payload(params (long Id, int Cant)[] reqs)
    {
      long cupoId = 1000;
      var data = new ShiftRequestAcceptData
      {
        ShiftRequest = reqs.Select(r => new SolicitudTurno
        {
          Id = r.Id,
          Cantidad = r.Cant,
          CuentaVendedor = 20,
          CuentaComprador = 30,
          CodigoGrano = 2,
          CuentaDestino = 40,
          CodigoCentro = "C1",
          FechaSolicitado = new DateTime(2026, 10, 1)
        }).ToList(),
        CuposToBeDistributed = new List<Cupo>()
      };
      foreach (var r in reqs)
        for (int i = 0; i < Math.Max(1, r.Cant); i++)
          data.CuposToBeDistributed.Add(new Cupo { Id = cupoId++, Fecha = new DateTime(2026, 10, 1) });
      return data;
    }

    private static string Snapshot(ShiftRequestAcceptResult r, Harness h) =>
      JsonSerializer.Serialize(new
      {
        r,
        ops = h.OperacionesRecibidas,
        cupos = h.CuposActualizados?.Select(c => new { c.Id, c.CodVendSIL, c.CodCompSIL, c.CodGrano, c.CodDestino, c.Fecha, c.CentroCupo, c.EstadoSIL })
      });

    public static IEnumerable<object?[]> Casos()
    {
      // (payload, solicitud incompatible)
      yield return new object?[] { new[] { (101L, 2), (102L, 1), (103L, 2) }, null };
      yield return new object?[] { new[] { (101L, 3), (104L, 1), (105L, 2), (106L, 0) }, null };
      yield return new object?[] { new[] { (101L, 1), (102L, 1), (106L, 4) }, 102L };
      yield return new object?[] { new[] { (0L, 1), (101L, 1), (101L, 2) }, null };
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task Batch_y_legacy_devuelven_lo_mismo((long, int)[] reqs, long? incompatible)
    {
      var legacy = new Harness(flag: false, incompatibleId: incompatible);
      var batch = new Harness(flag: true, incompatibleId: incompatible);

      var rLegacy = await legacy.Service.AcceptRequestsAsync(Payload(reqs));
      var rBatch = await batch.Service.AcceptRequestsAsync(Payload(reqs));

      Assert.Equal(Snapshot(rLegacy, legacy), Snapshot(rBatch, batch));

      // Legacy: N+1. Batch: una sola llamada, sin GetByIdAsync.
      batch.Store.Verify(s => s.GetByIdsAsync(It.IsAny<IEnumerable<long>>()), Times.Once);
      batch.Store.Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Never);
      legacy.Store.Verify(s => s.GetByIdsAsync(It.IsAny<IEnumerable<long>>()), Times.Never);
    }

    [Fact]
    public async Task Cantidad_mayor_a_la_original_falla_igual_en_ambos_caminos()
    {
      // 102 tiene Cantidad=1 en BD y se piden 2.
      var reqs = new[] { (101L, 1), (102L, 2) };
      var exLegacy = await Assert.ThrowsAsync<SilDataException>(() => new Harness(false).Service.AcceptRequestsAsync(Payload(reqs)));
      var exBatch = await Assert.ThrowsAsync<SilDataException>(() => new Harness(true).Service.AcceptRequestsAsync(Payload(reqs)));
      Assert.Equal(exLegacy.Message, exBatch.Message);
      Assert.Equal(exLegacy.StatusCode, exBatch.StatusCode);
    }

    [Fact]
    public async Task Si_el_batch_falla_cae_al_lookup_individual()
    {
      var reqs = new[] { (101L, 2), (106L, 3) };
      var legacy = new Harness(false);
      var batch = new Harness(true, batchFalla: true);

      var rLegacy = await legacy.Service.AcceptRequestsAsync(Payload(reqs));
      var rBatch = await batch.Service.AcceptRequestsAsync(Payload(reqs));

      Assert.Equal(Snapshot(rLegacy, legacy), Snapshot(rBatch, batch));
      batch.Store.Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Exactly(2));
    }
  }
}

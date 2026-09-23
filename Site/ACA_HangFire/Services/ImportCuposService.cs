using Domain.Entities.Externo;
using HangFire.Stores.Own;
using HangFire.Stores.Sil;

namespace HangFire.Services
{
  public class ImportCuposService: ICuposService
  {
    private readonly IOwnCupoStore _ownStore;
    private readonly ISilCupoStore _silStore;
    private readonly ILogger _logger;

    public ImportCuposService(IOwnCupoStore ownStore, ISilCupoStore silStore, ILogger<ImportCuposService> logger)
    {
      _ownStore = ownStore;
      _silStore = silStore;
      _logger = logger;
    }

    /// <summary>
    /// Cargamos los cupos hasta antes de ayer (inclusive)
    /// </summary>
    /// <returns></returns>
    public async Task<int> ImportCupos()
    {
      DateTime LastDayOfLoading = DateTime.Now.Date.AddDays(-2).Date;
      DateTime limiteInferior = (await _ownStore.FindLastDate()).AddDays(1).Date;
      if (limiteInferior.Date.CompareTo(LastDayOfLoading) > 0) return 0; //ya cargué todo
      
      DateTime limiteSuperior = limiteInferior.AddDays(5).Date.CompareTo(LastDayOfLoading) < 0? limiteInferior.AddDays(5).Date : LastDayOfLoading;
      IList<Cupo> cupos = await _silStore.FindByPeriod(GetDate(limiteInferior), GetDate(limiteSuperior));
      return await _ownStore.Insert(cupos);
    }

    public async Task<int> ImportCupos(DateTime desde, DateTime hasta)
    {
      IList<Cupo> cupos = await _silStore.FindByPeriod(GetDate(desde), GetDate(hasta));
      return await _ownStore.Insert(cupos);
    }
    public async Task<IList<Cupo>> GetCupos(DateTime desde, DateTime hasta)
    {
      IList<Cupo> cupos = await _silStore.FindByPeriod(GetDate(desde), GetDate(hasta));
      return cupos;
    }
    private DateTime GetDate(DateTime fecha)
    {
      return new DateTime(fecha.Year, fecha.Month, fecha.Day);
    }
  }
}

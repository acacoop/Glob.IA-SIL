using Domain.Entities.Externo;

namespace HangFire.Stores.Sil
{
  public interface ISilCupoStore
  {
    /// <summary>
    /// Obtiene los cupos dentro del periodo
    /// </summary>
    /// <param name="fechaDesde">Incluye</param>
    /// <param name="fechaHasta">Excluida</param>
    /// <returns></returns>
    Task<IList<Cupo>> FindByPeriod(DateTime fechaDesde, DateTime fechaHasta);

  }
}

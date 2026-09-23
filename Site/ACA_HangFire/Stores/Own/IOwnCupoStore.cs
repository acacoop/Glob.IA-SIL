using Domain.Entities.Externo;

namespace HangFire.Stores.Own
{
  public interface IOwnCupoStore
  {
    Task<int> Insert(IList<Cupo> cupos);
    Task<int> Insert(Cupo cupo);
    Task<DateTime> FindLastDate();
  }
}

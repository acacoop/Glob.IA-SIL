using Domain.Entities.Externo;

namespace HangFire.Services
{
  public interface ICuposService
  {
    Task<int> ImportCupos();
    Task<int> ImportCupos(DateTime desde, DateTime hasta);
  }
}
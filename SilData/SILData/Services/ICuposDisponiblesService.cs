using SILData.Model;
using SILData.Model.SolicitudTurno;

namespace SILData.Services
{
  public interface ICuposDisponiblesService
  {
    Task<long> GetCantidadDisponibles(long cuentaVendedor, DateTime fecha, long cuentaComprador = 0, int codigoGrano = 0, long zonaGeografica = 0);
    Task<IEnumerable<CuposDisponible>> GetDisponibles(long cuentaVendedor, DateTime fecha, long cuentaComprador = 0, int codigoGrano = 0, long zonaGeografica = 0);
    Task<List<CuposCorreResult>> GetCuposPorEstado(CuposFilter cuposFilter);
    Task<IEnumerable<CuposDisponiblesPorGrano>> GetCuposDisponiblesForShiftRequest(long cuentaVendedor, DateTime fecha, long cuentaComprador = 0, int codigoGrano = 0, long zonaGeografica = 0);
  }
}
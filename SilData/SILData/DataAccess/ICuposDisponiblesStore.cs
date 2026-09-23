using SILData.Model;
using SILData.Model.SolicitudTurno;

namespace SILData.DataAccess
{
  public interface ICuposDisponiblesStore
  {
    Task<IEnumerable<CuposDisponible>> GetDisponibles(long cuentaVendedor, DateTime fecha, long cuentaComprador = 0, int codigoGrano = 0, long zonaGeografica = 0);

    Task<IEnumerable<CuposCorreResult>> GetCuposByStatus(CuposCorreFilter cuposCorreFilter);
    Task<IEnumerable<SolicitudTurnoCuposDisponibles>> GetCuposDisponiblesForShiftRequest(long cuentaVendedor, DateTime fecha, long cuentaComprador = 0, int codigoGrano = 0, long zonaGeografica = 0);
  }
}

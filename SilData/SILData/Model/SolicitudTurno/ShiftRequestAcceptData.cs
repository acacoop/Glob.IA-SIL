using Domain.Entities.Externo;

namespace SILData.Model.SolicitudTurno
{
  public class ShiftRequestAcceptData
  {
    public required List<SolicitudTurno> ShiftRequest {  get; set; }
    public required List<Cupo> CuposToBeDistributed { get; set; }

    /// <summary>
    /// Mapa cupoId → cantidad a asignar para este Accept.
    /// Si es null o falta un cupoId, se asume 1.
    /// Hoy todos los cupos son enteros (sin subdivisión), así que este campo
    /// sólo se usa para validar que el frontend no pida más de 1 por cupo.
    /// Queda como hook para cuando se agregue subdivisión intra-cupo.
    /// </summary>
    public Dictionary<long, int>? CantidadPorCupo { get; set; }
  }
}
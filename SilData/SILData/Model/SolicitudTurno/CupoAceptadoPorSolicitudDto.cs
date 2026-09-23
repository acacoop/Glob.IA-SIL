using System.Collections.Generic;

namespace SILData.Model.SolicitudTurno
{
  /// <summary>
  /// Respuesta del endpoint <c>POST /api/ShiftRequest/Cupos/Aceptados/PorSolicitudes</c>.
  /// Para cada solicitud incluida en el request, devuelve los <c>cupo_id</c> que
  /// figuran en <c>SOLTURNOS_DETALLE</c> (cada fila es una aceptación). Permite
  /// que la UI reste los cupos ya otorgados del conteo de matches del motor bulk.
  /// </summary>
  public class CupoAceptadoPorSolicitudDto
  {
    /// <summary>Solicitud a la que pertenecen estos cupos aceptados.</summary>
    public long SolicitudId { get; set; }

    /// <summary>Ids de cupos aceptados para esta solicitud.</summary>
    public List<long> CupoIds { get; set; } = new List<long>();
  }
}

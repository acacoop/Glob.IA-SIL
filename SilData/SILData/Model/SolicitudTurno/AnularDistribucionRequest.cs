using System.Collections.Generic;

namespace SILData.Model.SolicitudTurno
{
  /// <summary>
  /// Body del endpoint <c>POST /api/ShiftRequest/AnularDistribucion</c>.
  /// El cliente envía los ids de los cupos cuyas distribuciones se quieren
  /// anular y el backend procesa cada uno de forma independiente. Por cada
  /// cupo:
  /// <list type="bullet">
  ///   <item><b>Con solicitud asociada</b> (figura en SOLTURNOS_DETALLE):
  ///     se revierte la asociación cupo ↔ solicitud (UPDATE SOLTURNOS:
  ///     CANTIDAD_ACEPTADA -= 1, CANTIDAD += 1; DELETE de la fila de
  ///     detalle). La solicitud vuelve a estado Pendiente.</item>
  ///   <item><b>Sin solicitud asociada</b> (el cupo no figura en
  ///     SOLTURNOS_DETALLE): se reporta como <c>Skipped</c>. NO se hace
  ///     nada en BD. La razón es que en este caso el cupo fue marcado
  ///     como anulado por el flujo legacy de Anular, y no requiere
  ///     reversión lógica.</item>
  /// </list>
  /// </summary>
  public class AnularDistribucionRequest
  {
    /// <summary>
    /// Ids de los cupos (PK de CUPOSCORRE / FK en SOLTURNOS_DETALLE)
    /// cuyas distribuciones se quieren revertir. Sin duplicados ni ceros.
    /// </summary>
    public List<long> CupoIds { get; set; } = new List<long>();
  }
}
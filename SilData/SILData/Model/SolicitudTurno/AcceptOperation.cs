using System.Collections.Generic;

namespace SILData.Model.SolicitudTurno
{
  /// <summary>
  /// Unidad atómica de aceptación. Una solicitud + sus cupos a asignar en esta
  /// corrida de Accept. El store aplica la siguiente secuencia atómica:
  /// <list type="number">
  ///   <item>UPDATE de SOLTURNOS: incrementa <c>CantidadAceptada</c>
  ///     (o <c>CantidadFuturoAceptada</c> si <see cref="SolicitudEsFuturo"/>).</item>
  ///   <item>INSERT en SOLTURNOS_DETALLE: una fila por cupo aceptado.</item>
  /// </list>
  /// Todo se ejecuta bajo la guardia de concurrencia optimista por
  /// acumuladores: <c>CantidadAceptada &lt; Cantidad AND CantidadRechazada = 0
  /// AND CUPO_ID IS NULL</c>. Si la guardia falla (otro operador actuó antes),
  /// ni el UPDATE ni los INSERTs se aplican (se reporta Fallo al servicio).
  /// </summary>
  public class AcceptOperation
  {
    /// <summary>Id de la solicitud a aceptar (clave primaria de SOLTURNOS).</summary>
    public long SolicitudId { get; set; }

    /// <summary>
    /// Cupos a asignar en esta corrida (≥1, sin repetidos). Cada uno
    /// generará una fila en SOLTURNOS_DETALLE.
    /// </summary>
    public List<long> CuposAsignados { get; set; } = new List<long>();

    /// <summary>
    /// Si true, el incremento va a <c>CantidadFuturoAceptada</c>;
    /// si false, va a <c>CantidadAceptada</c>. Se determina por la
    /// solicitud (no por cada cupo).
    /// </summary>
    public bool SolicitudEsFuturo { get; set; }
  }
}

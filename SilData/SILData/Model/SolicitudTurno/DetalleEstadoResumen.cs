namespace SILData.Model.SolicitudTurno
{
  /// <summary>
  /// Resumen del estado de las líneas de detalle de una solicitud, agrupadas
  /// por estado. Se devuelve desde <c>GET /api/ShiftRequest/{id}/Pendientes</c>
  /// para que el frontend (Pantalla 2 y grilla Index) sepa cuántos cupos
  /// fueron asignados, cuántos quedaron pendientes y cuántos rechazados.
  /// </summary>
  public class DetalleEstadoResumen
  {
    /// <summary>Cantidad de líneas en estado Asignado (1).</summary>
    public int Asignados { get; set; }

    /// <summary>Cantidad de líneas en estado Pendiente (0).</summary>
    public int Pendientes { get; set; }

    /// <summary>Cantidad de líneas en estado Rechazado (2).</summary>
    public int Rechazados { get; set; }

    /// <summary>
    /// Total de líneas de detalle de la solicitud (Asignados + Pendientes + Rechazados).
    /// Útil para validar la integridad contra la cantidad pedida original.
    /// </summary>
    public int Total => Asignados + Pendientes + Rechazados;
  }
}
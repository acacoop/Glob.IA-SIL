namespace SILData.Model.SolicitudTurno
{
  /// <summary>
  /// Datos de entrada para el rechazo (manual o automático) de una o varias
  /// solicitudes de turno. El sistema procesa cada ID de forma independiente
  /// aplicando concurrencia optimista: si la solicitud ya no está en estado
  /// Pendiente al momento de persistir, se reporta como falla y se omite.
  /// </summary>
  public class ShiftRequestRejectData
  {
    /// <summary>
    /// IDs de las solicitudes a rechazar. Debe contener al menos un elemento.
    /// </summary>
    public required List<long> SolicitudIds { get; set; }

    /// <summary>
    /// Motivo del rechazo. Opcional para rechazo manual; obligatorio para
    /// rechazo automático (lo completa el job con un texto descriptivo).
    /// </summary>
    public string? Motivo { get; set; }

    /// <summary>
    /// Indica si el rechazo fue disparado por el job automático de las 20:00 hs
    /// (true) o por acción manual del operador (false). Solo afecta el contenido
    /// de la notificación al solicitante.
    /// </summary>
    public bool Automatico { get; set; } = false;
  }
}

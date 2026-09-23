namespace SILData.Model.SolicitudTurno
{
  /// <summary>
  /// Resultado de una operación de rechazo. Discrimina entre los IDs que
  /// pudieron procesarse y los que fallaron (típicamente porque otro
  /// operador ya actuó sobre ellos o porque la solicitud no estaba pendiente).
  /// </summary>
  public class ShiftRequestRejectResult
  {
    /// <summary>Cantidad de IDs recibidos para procesar.</summary>
    public int TotalProcesados { get; set; }

    /// <summary>Cantidad que efectivamente cambió a estado Rechazada.</summary>
    public int TotalRechazados { get; set; }

    /// <summary>Cantidad que no pudo procesarse (ver Failures).</summary>
    public int TotalFallidos { get; set; }

    /// <summary>IDs de solicitudes rechazadas con éxito.</summary>
    public List<long> Rechazados { get; set; } = new();

    /// <summary>Detalle de las solicitudes que no pudieron rechazarse.</summary>
    public List<ShiftRequestRejectFailure> Fallos { get; set; } = new();

    /// <summary>
    /// True si al menos una solicitud fue rechazada. False si todas fallaron.
    /// </summary>
    public bool TieneExitos => Rechazados.Any();

    /// <summary>
    /// True si ninguna solicitud pudo rechazarse (todas fallaron por conflicto).
    /// </summary>
    public bool TodosFallaron => TotalFallidos > 0 && TotalRechazados == 0;
  }

  public class ShiftRequestRejectFailure
  {
    public long SolicitudId { get; set; }
    public string Motivo { get; set; }
  }
}

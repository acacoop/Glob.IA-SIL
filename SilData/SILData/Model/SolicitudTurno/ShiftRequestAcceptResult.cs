namespace SILData.Model.SolicitudTurno
{
  /// <summary>
  /// Resultado de una operación de aceptación masiva. Discrimina entre las
  /// solicitudes que pudieron asignarse (con los IDs de cupos que recibieron)
  /// y las que fallaron por conflicto de concurrencia (otro operador ya
  /// actuó sobre ellas entre la visualización y el commit).
  /// </summary>
  public class ShiftRequestAcceptResult
  {
    /// <summary>Cantidad de operaciones recibidas (parent + splits agrupados).</summary>
    public int TotalOperaciones { get; set; }

    /// <summary>Cantidad de operaciones completadas con éxito.</summary>
    public int TotalAsignadas { get; set; }

    /// <summary>Cantidad de operaciones que fallaron por conflicto.</summary>
    public int TotalConflictos { get; set; }

    /// <summary>
    /// Cantidad total de cupos que el operador PIDIÓ originalmente
    /// (de la fila SOLTURNOS antes del Accept). Se conserva aunque el
    /// Accept haya sido parcial para que el frontend muestre el toast
    /// "Asignaste N de M cupos".
    /// </summary>
    public int CantidadSolicitadaTotal { get; set; }

    /// <summary>
    /// Cantidad de cupos efectivamente asignados en este Accept (suma de
    /// cant asignada por cada (solicitud, cupo) que prosperó).
    /// </summary>
    public int CantidadAsignadaEnEsteAccept { get; set; }

    /// <summary>
    /// Cantidad que quedó Pendiente para asignaciones futuras
    /// (<c>CantidadSolicitadaTotal - CantidadAsignadaEnEsteAccept</c>).
    /// Si es 0, el Accept fue completo. Si es > 0, el operador puede
    /// volver a Pantalla 2 con la misma solicitud y completar.
    /// </summary>
    public int CantidadPendienteRestante { get; set; }

    /// <summary>
    /// Detalle de cada asignación exitosa, con la solicitud original, el ID
    /// del cupo asignado, y los IDs de los cupos asignados a sus splits (si
    /// la solicitud representaba Cantidad > 1).
    /// </summary>
    public List<ShiftRequestAssignedItem> Asignados { get; set; } = new();

    /// <summary>
    /// Detalle de las operaciones que no pudieron completarse por conflicto
    /// de concurrencia (otro operador asignó o rechazó antes).
    /// </summary>
    public List<ShiftRequestAcceptFailure> Fallos { get; set; } = new();

    /// <summary>True si al menos una asignación prosperó.</summary>
    public bool TieneExitos => Asignados.Any();

    /// <summary>True si TODAS las operaciones fallaron por conflicto.</summary>
    public bool TodosFallaron => TotalConflictos > 0 && TotalAsignadas == 0;
  }

  public class ShiftRequestAssignedItem
  {
    public long SolicitudId { get; set; }

    /// <summary>Primer cupo aceptado en esta operación (compatibilidad histórica).</summary>
    public long CupoAsignadoId { get; set; }

    /// <summary>
    /// IDs de todos los cupos efectivamente aceptados en esta operación.
    /// En el modelo "detalle acumulativo" ya no hay splits: cada elemento
    /// de esta lista se persiste como una fila en <c>SOLTURNOS_DETALLE</c>
    /// y se cuenta en <c>CantidadAceptada</c> (o
    /// <c>CantidadFuturoAceptada</c> si la solicitud es <c>EsFuturo=true</c>).
    /// </summary>
    public List<long> CuposAsignados { get; set; } = new();

    /// <summary>
    /// Clasificación del match devuelta por el motor (<c>ACA.Matching.Engine.MatchingEngine</c>).
    /// Permite al cliente saber si debe mostrar el diálogo de confirmación
    /// de la sección 5.2 del doc técnico (cuando es <c>Condicional</c>).
    /// <c>null</c> si el motor no fue invocado (caso degenerado).
    /// </summary>
    public string? TipoMatch { get; set; }
  }

  public class ShiftRequestAcceptFailure
  {
    public long SolicitudId { get; set; }
    public string Motivo { get; set; }
  }
}

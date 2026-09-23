namespace Domain.Entities.Externo
{
  /// <summary>
  /// Línea de asignación en <c>SOLTURNOS_DETALLE</c>. En el modelo "detalle
  /// acumulativo" cada fila representa un cupo ACEPTADO para una solicitud.
  /// </summary>
  /// <remarks>
  /// Una fila por (solicitud, cupo) aceptado. Los rechazados NO se guardan
  /// acá (viven en cuposcorre vía la operación de rechazo). Los pendientes
  /// son implícitos: <c>Cantidad - CantidadAceptada</c> en la fila SOLTURNOS.
  /// <para>
  /// El atributo <see cref="Estado"/> se conserva por compatibilidad y para
  /// futuras evoluciones; en este modelo vale siempre <c>1</c> (Asignado).
  /// </para>
  /// <para>
  /// Esta entidad NO está mapeada como DbSet en el DbContext de ACA_Domain
  /// (la tabla es legacy en Oracle y se accede vía Dapper en SolicitudTurnoStore).
  /// </para>
  /// </remarks>
  public class SolicitudTurnoDetalle
  {
    /// <summary>PK autoincremental vía SEQ (SOLTURNOS_DETALLE_SEQ).</summary>
    public long DetalleId { get; set; }

    /// <summary>FK lógica → <c>SOLTURNOS.solturnos_id</c>.</summary>
    public long SolicitudId { get; set; }

    /// <summary>FK lógica → <c>CUPOSCORRE.Id</c>. Constraint UNIQUE (solicitud_id, cupo_id).</summary>
    public long CupoId { get; set; }

    /// <summary>Fijo en <c>1</c> (Asignado) en este modelo. Reservado para futuras evoluciones.</summary>
    public short Estado { get; set; } = 1;

    /// <summary>Cuándo se aceptó el cupo (default SYSDATE en DB).</summary>
    public DateTime FechaCreacion { get; set; }
  }
}

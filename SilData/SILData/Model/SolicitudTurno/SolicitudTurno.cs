namespace SILData.Model.SolicitudTurno
{
  public class SolicitudTurno
  {
    public virtual long Id { get; set; }
    public virtual long? CuentaComprador { get; set; }
    public virtual long CuentaVendedor { get; set; }
    public virtual long? CuentaDestino { get; set; }
    public virtual TipoDestino? TipoDestino { get; set; }
    public virtual int CodigoGrano { get; set; }
    public virtual DateTime FechaCreacion { get; set; }
    public virtual DateTime FechaSolicitado { get; set; }
    /// <summary>Cupos/turnos solicitados en este registro. FROZEN: nunca se actualiza durante Accept.</summary>
    public virtual int Cantidad { get; set; } = 1;
    /// <summary>Subconjunto de <see cref="Cantidad"/> que se contabiliza como solicitud futura. FROZEN.</summary>
    public virtual int CantidadFuturo { get; set; }
    public virtual bool EsFuturo { get; set; }
    public virtual string CodigoCentro { get; set; }
    public virtual string? Observacion { get; set; }
    public virtual long? CupoId { get; set; }

    /// <summary>
    /// Acumulador de cupos aceptados (SOLTURNOS_DETALLE) sobre una o varias
    /// corridas de Accept. Invariante: <c>CantidadAceptada &lt;= Cantidad</c>.
    /// La solicitud se considera cubierta cuando este valor alcanza
    /// <see cref="Cantidad"/>.
    /// </summary>
    public virtual int CantidadAceptada { get; set; }

    /// <summary>
    /// Acumulador de cupos aceptados con <see cref="EsFuturo"/>=true.
    /// Subconjunto de <see cref="CantidadAceptada"/>. Invariante:
    /// <c>CantidadFuturoAceptada &lt;= CantidadFuturo</c>.
    /// </summary>
    public virtual int CantidadFuturoAceptada { get; set; }

    /// <summary>
    /// Acumulador de cupos rechazados al cierre de la solicitud (mediante
    /// el flujo de rechazo completo). Se setea en el momento del rechazo:
    /// <c>CantidadRechazada = Cantidad - CantidadAceptada</c> en ese
    /// instante. Vale 0 mientras la solicitud sigue pendiente.
    /// Invariante: <c>CantidadAceptada + CantidadRechazada &lt;= Cantidad</c>.
    /// </summary>
    public virtual int CantidadRechazada { get; set; }

    /// <summary>
    /// Acumulador de cupos rechazados con <see cref="EsFuturo"/>=true.
    /// Subconjunto de <see cref="CantidadRechazada"/>. Invariante:
    /// <c>CantidadFuturoAceptada + CantidadFuturoRechazada &lt;= CantidadFuturo</c>.
    /// </summary>
    public virtual int CantidadFuturoRechazada { get; set; }

    /// <summary>
    /// Indica si la solicitud todavía tiene cupos sin aceptar ni rechazar.
    /// Es la fuente de verdad para "pendiente" tras la eliminación de
    /// <c>SOLTURNOS.STATUS</c>.
    /// </summary>
    public bool EsPendiente => CantidadAceptada + CantidadRechazada < Cantidad;

    /// <summary>
    /// Indica si la solicitud fue rechazada completamente (en este modelo,
    /// una solicitud con <c>CantidadRechazada &gt; 0</c> ya no acepta más
    /// cupos).
    /// </summary>
    public bool EsRechazada => CantidadRechazada > 0;
  }
}

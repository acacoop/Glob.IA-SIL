using System.Diagnostics.CodeAnalysis;

namespace SILData.Model.SolicitudTurno
{
  /// <summary>
  /// DTO plano con los campos del cupo y de la solicitud que matchean entre sí,
  /// devuelto por <c>ISolicitudTurnoStore.GetCuposConMatchesAsync</c> al ejecutar
  /// el JOIN nativo de la query unificada. Cada fila representa un par candidato
  /// (cupo, solicitud) que cumple con los filtros básicos del operador
  /// (comprador, vendedor, puerto, grano, rango de fechas) y con las invariantes
  /// obligatorias del modelo:
  /// <list type="bullet">
  ///   <item>cupo.Fecha == solicitud.Fechasolicitada</item>
  ///   <item>cupo.Grano == solicitud.Grano</item>
  ///   <item>cupo.VENDCTA (si está set) == solicitud.CTAVEND</item>
  ///   <item>cupo.COMPCTA (si solicitud.CTACOMP está set) == solicitud.CTACOMP</item>
  ///   <item>cupo.PUERTOCTA pertenece a la zona solicitud.DEST vía PUERTOPORZONA</item>
  /// </list>
  /// El motor ACA.Matching.Engine clasifica cada par como Directo / Parcial /
  /// Condicional a partir de este DTO.
  /// </summary>
  public class CupoConSolicitudMatchDto
  {
    // ── Cupo ────────────────────────────────────────────────────────
    public long CupoId { get; set; }
    public DateTime? CupoFecha { get; set; }
    public string? CupoGrano { get; set; }
    [AllowNull] public long? CupoVendedor { get; set; }
    public long? CupoComprador { get; set; }
    public string? CupoPuerto { get; set; }
    public short? CupoEstado { get; set; }

    // ── Solicitud ───────────────────────────────────────────────────
    public long SolicitudId { get; set; }
    public long SolicitudVendedor { get; set; }
    [AllowNull] public long? SolicitudComprador { get; set; }
    [AllowNull] public long? SolicitudDestino { get; set; }
    [AllowNull] public TipoDestino? SolicitudTipoDestino { get; set; }
    public int SolicitudGrano { get; set; }
    public DateTime SolicitudFecha { get; set; }
    public string? SolicitudCentro { get; set; }
    public bool EsFuturo { get; set; }
    public int Cantidad { get; set; } = 1;
    public int CantidadAceptada { get; set; }
    public int CantidadRechazada { get; set; }
    public int CantidadFuturo { get; set; }
    public int CantidadFuturoAceptada { get; set; }
    public int CantidadFuturoRechazada { get; set; }
    public string? Observacion { get; set; }

    // ── Nombres de catálogos (hidratados vía JOIN en la misma query) ──
    public string? NombreComprador { get; set; }
    public string? NombreVendedor { get; set; }
    public string? NombrePuerto { get; set; }
    public string? NombreGrano { get; set; }
  }
}
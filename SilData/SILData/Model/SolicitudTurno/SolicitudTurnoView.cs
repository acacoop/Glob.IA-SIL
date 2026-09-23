using System.Diagnostics.CodeAnalysis;

namespace SILData.Model.SolicitudTurno
{
  public class SolicitudTurnoView
  {
    public long Id { get; set; }
    [AllowNull] public long? CuentaComprador { get; set; }
    public string NombreComprador { get; set; }
    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }
    public long? CuentaDestino { get; set; }
    public string NombreDestino { get; set; }
    [AllowNull] public TipoDestino? TipoDestino { get; set; }
    public int CodigoGrano { get; set; }
    public string NombreGrano { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaSolicitado { get; set; }
    public int Cantidad { get; set; } = 1;
    public int CantidadFuturo { get; set; }
    public bool EsFuturo { get; set; }
    public string CodigoCentro { get; set; }
    public string NombreCentro { get; set; }
    public string Observacion { get; set; }
    [AllowNull] public short? EstadoCupo { get; set; }
    [AllowNull] public short? CtgCupo { get; set; }

    /// <summary>
    /// Acumulador de cupos aceptados (≤ Cantidad). Se setea en Accept.
    /// </summary>
    public int CantidadAceptada { get; set; }

    /// <summary>
    /// Acumulador de cupos futuros aceptados (≤ CantidadFuturo). Subset
    /// de <see cref="CantidadAceptada"/> cuando la solicitud es EsFuturo=true.
    /// </summary>
    public int CantidadFuturoAceptada { get; set; }

    /// <summary>
    /// Acumulador de cupos rechazados al cierre de la solicitud. Vale 0
    /// mientras la solicitud sigue Pendiente u Otorgada. Invariante:
    /// <c>CantidadAceptada + CantidadRechazada &lt;= Cantidad</c>.
    /// </summary>
    public int CantidadRechazada { get; set; }

    /// <summary>
    /// Acumulador de cupos rechazados con <see cref="EsFuturo"/>=true.
    /// Subconjunto de <see cref="CantidadRechazada"/>. Invariante:
    /// <c>CantidadFuturoAceptada + CantidadFuturoRechazada &lt;= CantidadFuturo</c>.
    /// </summary>
    public int CantidadFuturoRechazada { get; set; }

    /// <summary>
    /// True cuando la solicitud todavía tiene cupos sin aceptar ni rechazar.
    /// Deriva de los acumuladores; reemplaza al antiguo STATUS=0.
    /// </summary>
    public bool EsPendiente => CantidadAceptada + CantidadRechazada < Cantidad;

    /// <summary>
    /// True cuando la solicitud fue rechazada completamente. Deriva de los
    /// acumuladores; reemplaza al antiguo STATUS=1.
    /// </summary>
    public bool EsRechazada => CantidadRechazada > 0;

    /// <summary>
    /// True cuando se cubrió la totalidad de la cantidad pedida
    /// (entre aceptaciones y rechazos). Deriva de los acumuladores.
    /// </summary>
    public bool EsCubierta => CantidadAceptada + CantidadRechazada >= Cantidad;
  }

  public class SolicitudTurnoGrupoView
  {
    public int CodigoGrano { get; set; }
    public string NombreGrano { get; set; }
    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }
    public long? CuentaComprador { get; set; }
    public string NombreComprador { get; set; }
    public long? CuentaDestino { get; set; }
    public string NombreDestino { get; set; }
    public IEnumerable<SolicitudTurnoDetalleGrupoView> CantidadFechas { get; set; }
  }

  public class SolicitudTurnoDetalleGrupoView
  {
    public string Fecha { get; set; }
    public int Cantidad { get; set; }
    public int DiaSemana { get; set; }
  }

  public class SolicitudTurnoGrupoVendedorView
    {
        public int CodigoGrano { get; set; }
        public string NombreGrano { get; set; }
        public IEnumerable<SolicitudTurnoGrupoDetallePendienteDiaView> DetallePendientesDia { get; set; }
        public IEnumerable<SolicitudTurnoGrupoDetalleSolicitadoView> DetallesSolicitados { get; set; }

    }


    public class SolicitudTurnoGrupoDetalleSolicitadoView
    {
        public long? CuentaComprador { get; set; }
        public string NombreComprador { get; set; }
        public long? CuentaDestino { get; set; }
        public string NombreDestino { get; set; }
        public IEnumerable<SolicitudTurnoGrupoDetalleSolicitadoDiaView> DetallesSolicitadosDia { get; set; }
    }

    public class SolicitudTurnoGrupoDetalleSolicitadoDiaView
    {
        public string Fecha { get; set; }
        public int TurnosOtorgados { get; set; }
        public int TurnosActivos { get; set; }
    }

	public class SolicitudTurnoPorGranoView
	{
		public int CodigoGrano { get; set; }
		public string NombreGrano { get; set; }
		public IEnumerable<SolicitudTurnoPendienteGrupoDetalleSolicitadoView> DetallePendientes { get; set; }

	}
	public class SolicitudTurnoPendienteGrupoDetalleSolicitadoView
	{
		public long? CuentaComprador { get; set; }
		public string NombreComprador { get; set; }
		public long? CuentaDestino { get; set; }
		public string NombreDestino { get; set; }
		public IEnumerable<SolicitudTurnoGrupoDetallePendienteDiaView> DetallesSolicitadosDia { get; set; }
	}

  public class SolicitudTurnoGrupoDetallePendienteDiaView
  {
    public string Fecha { get; set; }
    public int Cantidad { get; set; }
  }

}

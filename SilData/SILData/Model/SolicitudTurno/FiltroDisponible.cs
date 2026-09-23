namespace SILData.Model.SolicitudTurno
{
  public class FiltroDisponible
  {
    public required long cuentaVendedor { get;set; }
    public required DateTime fecha { get; set; }
    public long cuentaComprador { get; set; }
    public int codigoGrano { get; set; }
    public int zonaGeografica { get; set; }
}
}

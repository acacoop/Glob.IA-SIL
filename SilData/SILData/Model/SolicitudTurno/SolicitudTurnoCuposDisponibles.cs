namespace SILData.Model.SolicitudTurno
{
  public class SolicitudTurnoCuposDisponibles
  {
    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }
    public string NombreGrano { get; set; }
    public string CodigoGrano { get; set; }
    public long CuentaComprador { get; set; }
    public string NombreComprador { get; set; }
    public int CuposDisponibles { get; set; }
    public int ZonaGeograficaId { get; set; }
    public string ZonaGeografica { get; set; }
    //public string Centro { get; set; }
  }
}
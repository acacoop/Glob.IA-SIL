namespace SILData.Model
{
  public class CuposDisponible
  {
    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }
    public string NombreGrano { get; set; }
    public string CodigoGrano { get; set; }
    public long CuentaComprador { get; set; }
    public string NombreComprador { get; set; }
    public double PendienteEntrega { get; set; }
    public double PendienteAplicar { get; set; }
    public int CuposADistribuir { get; set; }
    public int CuposOtorgados { get; set; }
    public int CuposTotalesADistribuir { get; set; }
    public int ZonaGeograficaId { get; set; }
    public string ZonaGeografica { get; set; }
    public string Centro { get; set; }
  }
}
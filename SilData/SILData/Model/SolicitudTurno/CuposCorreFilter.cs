namespace SILData.Model.SolicitudTurno
{
  public class CuposCorreFilter
  {
    public required long Grano { get; set; }
    public required DateTime FechaDesde { get; set; }
    public required DateTime FechaHasta { get; set; }
    public required int status {  get; set; }
    
    /*si es muy lento especificamos el filtro*/
    //public long CuentaVendedor { get; set; }
    //public long CuentaComprador { get; set; }
    //public List<long> Destinos { get; set; }
  }
}

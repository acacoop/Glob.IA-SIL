namespace SILData.Model.SolicitudTurno
{
  public class CuposFilter
  {
    public required string CuentaVendedor { get; set; }
    public string? CuentaComprador {  get; set; }
    public required string Grano {  get; set; }
    public List<string>? Destinos { get; set; }
    public DateTime? Fecha { get; set; }
    //estado de los cupos que quiero obtener
    public required int status { get; set; } 
  }
}

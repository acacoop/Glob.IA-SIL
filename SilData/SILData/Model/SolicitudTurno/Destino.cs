namespace SILData.Model.SolicitudTurno
{
  public class Destino
  {
    public required long Id {  get; set; }
    public required long Cuenta { get; set; }
    public required string Cuit { get; set; }
    public string? Nombre { get; set; }
    public string? Domicilio { get; set; }
    public string? TipodDeCuenta { get; set; }
    public string? Localidad { get; set; }
    public string? Provincia { get; set; }
    public int? CPostal { get; set; }
  }
}

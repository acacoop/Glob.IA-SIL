namespace SILData.Model.SolicitudTurno
{
  public class DestinoView
  {
    public required long Id { get; set; }
    public required int Cuenta { get; set; }
    public required string Cuit { get; set; }
    public string? Nombre { get; set; }
    public string? Domicilio { get; set; }
    public string? TipodDeCuenta { get; set; }
    public string? Localidad { get; set; }
    public string? Provincia { get; set; }
    public string? CPostal { get; set; }
  }
}

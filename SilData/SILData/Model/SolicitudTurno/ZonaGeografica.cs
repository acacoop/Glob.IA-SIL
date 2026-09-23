namespace SILData.Model.SolicitudTurno
{
  public class ZonaGeografica
  {
    public required virtual long ZonaGeoId { get; set; }
    public required string Nombre {  get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public required string CentroId { get; set; }
    public IEnumerable<Destino>? Destinos { get; set; }
  }
}

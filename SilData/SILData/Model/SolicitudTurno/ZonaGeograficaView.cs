namespace SILData.Model.SolicitudTurno
{
  public class ZonaGeograficaView
  {
    public required virtual long zonaGeoId { get; set; }
    public required string Nombre { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public required string CentroId { get; set; }
    public IEnumerable<DestinoView>? Destinos { get; set; }
    public int? Disponible { get; set; }
    public bool ShowDetails { get; set; } = false;
  }
}

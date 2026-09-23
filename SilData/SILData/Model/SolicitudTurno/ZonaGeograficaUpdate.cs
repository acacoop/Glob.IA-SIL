namespace SILData.Model.SolicitudTurno
{
  public class ZonaGeograficaUpdate
  {
    public required long ZonaGeoId { get; set; }
    public IEnumerable<DestinoUpdate>? Destinos { get; set; }
  }
}


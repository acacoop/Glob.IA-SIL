namespace SILData.Model.SolicitudTurno
{
  public class AvailabilityByProduct
  {
    public int Id { get; set; }
    public string ProductName { get; set; }
    public int Shift { get; set; }

    public List<Comprador> CompradorAvailableList { get; set; }
    public List<GeographicZone> GeographicZoneAvailableList { get; set; }
  }

  public class Comprador
  {
    public long Id { get; set; }
    public string Name { get; set; }
    public int Pendiente { get; set; } = 0;
    public List<GeographicZone> GeographicZoneAvailableList { get; set; } = new List<GeographicZone>();
  }
  public class GeographicZone
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public int Pendiente { get; set; } = 0;
  }
}
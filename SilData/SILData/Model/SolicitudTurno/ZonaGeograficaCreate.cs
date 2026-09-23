namespace SILData.Model.SolicitudTurno
{
  public class ZonaGeograficaCreate
  {
    public required string Nombre { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public required string CentroId { get; set; }
    public IEnumerable<DestinoCreate>? Destinos { get; set; }
  }
}


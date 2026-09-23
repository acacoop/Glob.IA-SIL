namespace Comunicaciones.DataRequest
{
  public class ListaContactosRequest
  {
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public IList<Guid>? ListasHijas { get; set; }
    public IList<Guid>? Personas { get; set; }

    public ListaContactosRequest()
    {
      ListasHijas = new List<Guid>();
      Personas = new List<Guid>();
    }
  }
}

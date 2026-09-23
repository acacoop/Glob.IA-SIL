using Domain.Entities.ListasContactos;

namespace Comunicaciones.DataResponse
{
  public class AutocompleteDataDTO
  {
    public Guid id { get; set; }
    public string nombre { get; set; }
    public IList<Contacto> contactosPersona { get; set; }
    public bool esLista { get; set; }
  }
}

using System.ComponentModel.DataAnnotations;

namespace Comunicaciones.DataRequest
{
  public class ContactoRequest
  {
    public Guid? Id { get; set; }
    public Guid Persona { get; set; }
    public Guid Tipo { get; set; }
    public string Dato { get; set; }
  }
}

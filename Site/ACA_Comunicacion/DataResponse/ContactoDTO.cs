using Comunicaciones.DataResponse.Entity;

namespace Comunicaciones.DataResponse
{
  public class ContactoDTO: EntityDto<Guid>
  {
    public Guid Persona { get; set; }/*ver si va*/
    public TipoContactoDTO Tipo { get; set; }
    public string Dato { get; set; }

  }
}

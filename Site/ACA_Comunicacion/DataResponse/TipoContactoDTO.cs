using Comunicaciones.DataResponse.Entity;

namespace Comunicaciones.DataResponse
{
  public class TipoContactoDTO : EntityDto<Guid>
  {
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
  }
}

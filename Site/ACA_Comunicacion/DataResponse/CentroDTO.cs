using Comunicaciones.DataResponse.Entity;

namespace Comunicaciones.DataResponse
{
  public class CentroDTO : EntityDto<Guid>
  {
    public string Codigo { get; set; }
    public string Nombre { get; set; }
  }
}

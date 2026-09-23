using Comunicaciones.DataResponse.Entity;

namespace Comunicaciones.DataResponse
{
  public class ZonaComercialDTO : EntityDto<Guid>
  {
    public string Nombre { get; set; }
    public string Descripcion { get; set; }

  }
}

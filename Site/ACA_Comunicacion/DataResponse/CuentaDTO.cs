using Comunicaciones.DataResponse.Entity;

namespace Comunicaciones.DataResponse
{
  public class CuentaDTO : EntityDto<Guid>
  {
    public string NroCuenta { get; set; }
    public string Cuit { get; set; }
    public string Nombre { get; set; }
  }
}

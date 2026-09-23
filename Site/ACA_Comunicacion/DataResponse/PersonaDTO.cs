using Comunicaciones.DataResponse.Entity;
using Comunicaciones.Models.ListasContactos;
using Comunicaciones.Models.Personas;

namespace Comunicaciones.DataResponse
{
  public class PersonaDTO: EntityDto<Guid>
  {
		public string Apellido { get; set; }
		public string Nombre { get; set; }
		public CuentaDTO? Cuenta { get; set; }
		public RolDTO? Rol { get; set; }
		public string? Usuario { get; set; }
		public CentroDTO? Centro { get; set; }
		public ZonaComercialDTO? ZonaComercial { get; set; }
		public List<ContactoDTO>? Contactos { get; set; }


	}
}

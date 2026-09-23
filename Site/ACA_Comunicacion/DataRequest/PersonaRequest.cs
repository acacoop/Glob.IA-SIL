using Comunicaciones.Models.ListasContactos;

namespace Comunicaciones.DataRequest
{
  public class PersonaRequest
  {
		public string Apellido { get; set; }
		public string Nombre { get; set; }
		public Guid? Cuenta { get; set; }
		public Guid? Rol { get; set; }
		public string? Usuario { get; set; }
		public Guid? Centro { get; set; }
		public Guid? ZonaComercial { get; set; }
		public ICollection<ContactoRequest>? Contactos { get; set; }
	}
}

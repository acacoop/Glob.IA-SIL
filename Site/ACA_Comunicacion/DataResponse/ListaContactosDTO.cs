using Comunicaciones.DataResponse.Entity;
using Comunicaciones.Models.ListasContactos;

namespace Comunicaciones.DataResponse
{
    public class ListaContactosDTO : EntityDto<Guid>
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public IList<ListaContactosDTO> Contactos { get; set; }
        public bool esLista { get; set; }

    }
}

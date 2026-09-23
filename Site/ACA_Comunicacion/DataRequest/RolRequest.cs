using System.ComponentModel.DataAnnotations;

namespace Comunicaciones.DataRequest
{
  public class RolRequest
  {
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El [Nombre] es requerido.")]
    public string Nombre { get; set; }

    [Display(Name = "Descripcion")]
    [Required(ErrorMessage = "El [Descripcion] es requerido.")]
    public string Descripcion { get; set; }
  }
}

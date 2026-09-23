using System.ComponentModel.DataAnnotations;

namespace Comunicaciones.DataRequest
{
  public class CentroRequest
  {
    [Display(Name = "Codigo")]
    [Required(ErrorMessage = "El [Codigo] es requerido.")]
    public string Codigo { get; set; }

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El [Nombre] es requerido.")]
    public string Nombre { get; set; }
  }
}

using System.ComponentModel.DataAnnotations;

namespace Comunicaciones.DataRequest
{
  public class TipoDeContactoRequest
  {
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El [Nombre] es requerido.")]
    public string Nombre { get; set; }

    [Display(Name = "Descripción")]
    [Required(ErrorMessage = "El [Descripción] es requerido.")]
    public string Descripcion { get; set; }
  }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.PanelControlLogistico
{
  public class ShiftSILBoardRequest
  {
    [Required]
    public required DateTime Desde { get; set; }
    [Required]
    public required DateTime Hasta { get; set; }
    public List<string>? Compradores { get; set; }
    public List<string>? Vendedores { get; set; }
    public List<string>? Productos { get; set; }
    public List<string>? Destinos { get; set; }
    public List<string>? Centros { get; set; }
  }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.PanelControlLogistico
{
  public class ShiftBoardResponse
  {
    public required List<Cuadrante> cuadrantes { get; set; }
  }
}

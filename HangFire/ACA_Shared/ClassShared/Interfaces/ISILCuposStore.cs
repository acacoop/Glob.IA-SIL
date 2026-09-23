using Domain.Entities.Externo;
using Domain.Entities.PanelControlLogistico;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ClassShared.Interfaces
{
  public interface ISILCuposStore
  {
    /// <summary>
    /// Obtiene los cupos dentro del periodo indicadp
    /// Cruza los datos de la cuposstop con la cuposcorre
    /// <param name="fechaDesde">Incluye</param>
    /// <param name="fechaHasta">Excluida</param>
    /// <returns></returns>
    Task<IList<Cupo>> FindCuposByPeriod(DateTime fechaDesde, DateTime fechaHasta);
    Task<IList<Cupo>> FindCuposForDasshboardByPeriodBy(ShiftSILBoardRequest filters);
  }
}

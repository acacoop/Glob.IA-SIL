using Domain.Entities.Externo;
using Domain.Entities.PanelControlLogistico;
using Shared.ClassShared.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ClassShared.Interfaces
{
  public interface ISILCuposServices
  {
    Task<IList<Cupo>> GetCupos(CPEReportRequest Request);
    Task<IList<Cupo>> GetCuposCollectionForDasboard(ShiftSILBoardRequest filters);
  }
}

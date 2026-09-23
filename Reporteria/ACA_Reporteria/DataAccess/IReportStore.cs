using Domain.Entities.Externo;
using Domain.Entities.PanelControlLogistico;
using Shared.ClassShared.Requests;
using System.Data;

namespace Reporteria.DataAccess
{
  public interface IReportStore
  {
    Task<IEnumerable<Cupo>> GetCupos(CPEReportRequest? request = null);
    Task<IEnumerable<Cupo>> GetCuposForDasboard(ShiftBoardRequest? request = null);
    Task<DataTable> GetCuposDT(CPEReportRequest? request = null, string? selectStatement = null);
    Task<long> GetCountCupos(CPEReportRequest? request);
    Task<IEnumerable<Cupo>> GetCuposWithPaging(int pageSize, int pageNumber, string columnForOrder, bool orderDesc, CPEReportRequest? request = null);
    Task<(bool Ok, long Ms, string? Error)> PingSqlAsync();
  }
}

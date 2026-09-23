using Domain.Entities.Externo;
using Domain.Entities.PanelControlLogistico;
using Reporteria.Exceptions;
using Reporteria.Reports;
using Shared.ClassShared.Requests;
using Shared.ClassShared.Types;
using System.Data;

namespace Reporteria.Services
{
  public interface IServicesReport
  {
    Task<IEnumerable<Cupo>> GetCuposCollection(CPEReportRequest reporteCPERequest);
    Task<IEnumerable<Cupo>> GetCuposCollectionForDasboard(ShiftBoardRequest filters);
    Task<DataTable> GetCuposDT(CPEReportRequest reporteCPERequest, string selectStatement);
    Task<long> GetCuposCount(CPEReportRequest reporteCPERequest);
    Task<TablesResponse> GetCuposCollectionWithPaging(TablesRequest filter);
    Task<ShiftBoardResponse> GetTotalCountOfCuposForDasboard(ShiftBoardRequest filter);
    Task<ExceptionsResult<DataTable>> ObtenerDatosSegunFechas(CPEReportRequest request, DateTime cuttingDay, string selectColumn, string silDataUrl);
    Task<ExceptionsResult<DataTable>> ConsultarCuposSIL(CPEReportRequest request, string silDataUrl);
    Task<ExceptionsResult<DataTable>> ObtenerDatosAzureYSIL(CPEReportRequest request, DateTime cuttingDay, string selectColumn, string silDataUrl);
    ExceptionsResult<byte[]> GenerarExcel(DataTable dataTable, Report report);
    Task<(bool Ok, long Ms, string? Error)> PingSqlAsync();
  }
}

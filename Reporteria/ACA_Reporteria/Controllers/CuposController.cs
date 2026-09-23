using ClosedXML.Excel;
using Domain.Entities.Externo;
using Domain.Entities.PanelControlLogistico;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.RulesetToEditorconfig;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Reporteria.Exceptions;
using Reporteria.Reports;
using Reporteria.Services;
using Shared.ClassShared.Requests;
using Shared.ClassShared.Types;
using Shared.ClassShared.WebServiceRequest;
using System.Data;
using System.Dynamic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Reporteria.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  //[Authorize]
  public class CuposController : ControllerBase
  {
    private readonly IServicesReport ReportCuposSvc;
    private readonly ILogger<CuposController> _logger;
    private readonly IConfiguration _configuration;

    public CuposController(IConfiguration configuration, IServicesReport DataAccessService, ILogger<CuposController> logger)
    {
      this.ReportCuposSvc = DataAccessService;
      _logger = logger;
      _configuration = configuration;
    }

    [HttpPost("Cupos")]
    public async Task<IActionResult> Get([FromBody] CPEReportRequest reporteCPERequest)
    {
      try
      {
        Task<IEnumerable<Cupo>> cupos = this.ReportCuposSvc.GetCuposCollection(reporteCPERequest);
        IEnumerable<Cupo> ListCupos = await cupos;
        return Ok(ListCupos);
      }
      catch (Exception e)
      {
        _logger.LogError(e, e.Message);
        throw;
      }
    }

    [HttpPost("Export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetExcel([FromBody] CPEReportRequest reporteCPERequest)
    {
      var result = new ExceptionsResult<byte[]>();
      try
      {
        // Validación básica de fechas
        if (reporteCPERequest.FechaHasta < reporteCPERequest.FechaDesde)
        {
          result.Success = false;
          result.Error = "La fecha 'Hasta' no puede ser anterior a la fecha 'Desde'.";
          return BadRequest(result);
        }

        var report = ReportBuilder.GetReport("Reports/ReportCPE.json");
        if (report == null)
        {
          result.Success = false;
          result.Error = "No se pudo cargar la configuracion del reporte (ReportCPE.json).";
          return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        string selectColumn = report.BuildQuery();
        int day = int.Parse(_configuration["CuttingDays"] ?? "0");
        string SilDataUrl = _configuration["SILDataAPI"] ?? "";
        DateTime CuttingDay = DateTime.Now.AddDays(-day).Date;

        DataTable finalDt = new DataTable();
        List<string> allWarnings = new List<string>();

        reporteCPERequest.FechaDesde = reporteCPERequest.FechaDesde.Date;
        reporteCPERequest.FechaHasta = reporteCPERequest.FechaHasta.Date;

        //Procesar según rangos de fechas
        var dataResult = await ReportCuposSvc.ObtenerDatosSegunFechas(reporteCPERequest, CuttingDay, selectColumn, SilDataUrl);

        if (!dataResult.Success)
        {
          return StatusCode(StatusCodes.Status503ServiceUnavailable, dataResult);
        }

        // Acumular warnings
        if (dataResult.Warnings.Any())
        {
          allWarnings.AddRange(dataResult.Warnings);
        }

        finalDt = dataResult.Data;

        //Validación final
        if (finalDt == null || finalDt.Rows.Count == 0)
        {
          result.Success = false;
          result.Error = "No se encontraron datos para el reporte solicitado.";
          result.Warnings = allWarnings;
          return NotFound(result);
        }

        finalDt.TableName = "Cupos";

        // Generar Excel
        var excelResult = ReportCuposSvc.GenerarExcel(finalDt, report);
        if (!excelResult.Success)
        {
          result.Success = false;
          result.Error = excelResult.Error;
          return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        // Agregar warnings y metadata a los headers
        if (allWarnings.Any())
        {
          Response.Headers.Add("X-Warnings", string.Join(" | ", allWarnings));
        }
        Response.Headers.Add("X-Record-Count", finalDt.Rows.Count.ToString());

        var format = string.IsNullOrWhiteSpace(report.format) ? "xlsx" : report.format;
        var safeName = string.Join("_", report.name.Split(Path.GetInvalidFileNameChars()));
        var fileName = $"{safeName}_{DateTime.Now:ddMMyyyyHHmmss}.{format}";

        _logger.LogInformation("Export finalizado correctamente. Filas exportadas: {Count}", finalDt.Rows.Count);

        return File(excelResult.Data,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",fileName);
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Error al comunicarse con el servicio externo (SILData).");
        result.Success = false;
        result.Error = "Error al comunicarse con el servicio SIL. Intente nuevamente mas tarde.";
        result.Warnings.Add($"Detalle técnico: {ex.Message}");
        return StatusCode(StatusCodes.Status503ServiceUnavailable, result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error interno en la generación del reporte CPE.");
        System.IO.File.AppendAllText("/home/error-log.txt", $"{DateTime.UtcNow}: {ex}\n");
        result.Success = false;
        result.Error = "Ocurrio un error interno al generar el reporte. Contacte a soporte tecnico.";
        result.Warnings.Add($"Detalle técnico: {ex.Message}");
        return StatusCode(StatusCodes.Status500InternalServerError, result);
      }

    }

    [HttpPost("CuposCount")]
    public async Task<IActionResult> GetCuposCount([FromBody] CPEReportRequest reporteCPERequest)
    {
      Task<long> cupos = this.ReportCuposSvc.GetCuposCount(reporteCPERequest);
      return Ok(await cupos);
    }

    [HttpPost("CuposPaginados")]
    public async Task<IActionResult> GetCuposForTable([FromBody] TablesRequest filter)
    {
      Task<TablesResponse> cupos = this.ReportCuposSvc.GetCuposCollectionWithPaging(filter);
      return Ok(await cupos);
    }

    [HttpPost("GetCuposParaInforme", Name = "GetCuposParaInforme")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShiftBoardResponse))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetCuposParaInforme([FromBody] ShiftBoardRequest shiftBoardRequest)
    {
      try
      {
        ShiftBoardResponse cupos = await this.ReportCuposSvc.GetTotalCountOfCuposForDasboard(shiftBoardRequest);
        return Ok(cupos);
      }
      catch (Exception e)
      {
        _logger.LogError(e, e.Message);
        throw;
      }
    }

    /// <summary>
    /// Healthcheck: ejecuta SELECT 1 contra Azure SQL y devuelve el tiempo.
    /// Usar para diagnosticar timeouts de conectividad (503 en /api/cupos/Export).
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
      var ping = await ReportCuposSvc.PingSqlAsync();
      _logger.LogInformation("Healthcheck ejecutado - sqlOk={Ok} ms={Ms}", ping.Ok, ping.Ms);
      if (ping.Ok)
      {
        return Ok(new { status = "ok", sqlMs = ping.Ms, server = "AzureSQL" });
      }
      return StatusCode(StatusCodes.Status503ServiceUnavailable,
        new { status = "error", sqlMs = ping.Ms, error = ping.Error });
    }

    /// <summary>
    /// Test rapido: loguea un trace y devuelve 200. Usar para validar que
    /// ILogger llega a Application Insights.
    /// </summary>
    [HttpGet("trace-test")]
    public IActionResult TraceTest()
    {
      var traceId = Guid.NewGuid().ToString("N")[..8];
      _logger.LogInformation("TRACE-TEST {TraceId} - mensaje de prueba para App Insights", traceId);
      return Ok(new
      {
        ok = true,
        traceId,
        message = $"Buscar 'TRACE-TEST {traceId}' en Application Insights -> Transaction search -> Trace"
      });
    }
  }
}

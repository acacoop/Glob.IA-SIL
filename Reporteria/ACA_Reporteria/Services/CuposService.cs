using Castle.Core.Internal;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2016.Excel;
using Domain.Entities.Externo;
using Domain.Entities.PanelControlLogistico;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Reporteria.Controllers;
using Reporteria.DataAccess;
using Reporteria.Exceptions;
using Reporteria.Reports;
using Shared.ClassShared.Requests;
using Shared.ClassShared.Types;
using Shared.ClassShared.WebServiceRequest;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;

namespace Reporteria.Services
{
  public class CuposService: IServicesReport
  {
    private IReportStore _cuposStore;
    private IConfiguration _configuration;
    private readonly ILogger<CuposController> _logger;

    public CuposService(IReportStore dataAccess, IConfiguration configuration, ILogger<CuposController> logger) 
    {
      _cuposStore = dataAccess;
      _configuration = configuration;
      _logger = logger;
    }

    public async Task<IEnumerable<Cupo>> GetCuposCollection(CPEReportRequest reporteCPERequest)
    {
      Task<IEnumerable<Cupo>> result = _cuposStore.GetCupos(reporteCPERequest);
      return await result;
    }

    /// <summary>
    /// traer los cupos de la BD que cumplan la condicion especificada en los filtros
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Cupo>> GetCuposCollectionForDasboard(ShiftBoardRequest filters)
    {
      /*podriamos validar que los valores del filtro sean valores validos*/
      Task<IEnumerable<Cupo>> result = _cuposStore.GetCuposForDasboard(filters);
      return await result;
    }
    public async Task<DataTable> GetCuposDT(CPEReportRequest reporteCPERequest, string selectStatement)
    {
      Task<DataTable> result = _cuposStore.GetCuposDT(reporteCPERequest, selectStatement);
      return await result;
    }
    // Método auxiliar para obtener datos según fechas
    public async Task<ExceptionsResult<DataTable>> ObtenerDatosSegunFechas(CPEReportRequest request, DateTime cuttingDay, string selectColumn, string silDataUrl)
    {
      var result = new ExceptionsResult<DataTable>();

      try
      {
        if (request.FechaDesde.Date < cuttingDay)
        {
          if (request.FechaHasta.Date < cuttingDay)
          {
            // Solo Azure
            _logger.LogInformation(
              "Consultando solo Azure. CuttingDay: {CuttingDay}, Desde: {Desde}, Hasta: {Hasta}",
              cuttingDay, request.FechaDesde.Date, request.FechaHasta.Date
            );

            var azureDt = await GetCuposDT(request, selectColumn);

            if (azureDt == null || azureDt.Rows.Count == 0)
            {
              result.Warnings.Add($"No se encontraron datos en Azure para el periodo seleccionado. {request.FechaDesde:dd/MM/yyyy} - {request.FechaHasta:dd/MM/yyyy}.");
              result.Data = new DataTable();
            }
            else
            {
              result.Data = azureDt;
            }

            result.Success = true;
          }
          else
          {
            // Azure + SIL
            result = await ObtenerDatosAzureYSIL(request, cuttingDay, selectColumn, silDataUrl);
          }
        }
        else
        {
          // Solo SIL
          _logger.LogInformation(
            "Consultando solo SIL. Fecha desde: {Desde}, CuttingDay: {CuttingDay}",
            request.FechaDesde.Date, cuttingDay
          );

          var silResult = await ConsultarCuposSIL(request, silDataUrl);

          if (!silResult.Success)
          {
            result.Success = false;
            result.Error = silResult.Error;
            return result;
          }

          result.Success = true;
          result.Data = silResult.Data ?? new DataTable();
          result.Warnings.AddRange(silResult.Warnings);
        }

        return result;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error obteniendo datos");
        result.Success = false;
        result.Error = "Error al obtener los datos del reporte.";
        return result;
      }
    }
    public async Task<ExceptionsResult<DataTable>> ConsultarCuposSIL(CPEReportRequest request, string silDataUrl)
    {
      var result = new ExceptionsResult<DataTable>();
      try
      {
        using var ws = new SILData(silDataUrl);

        //var accessToken = Request.Headers[HeaderNames.Authorization];
        var accessToken = "";
        _logger.LogInformation("SILReport: consultando SILData con accessToken {Token}", accessToken);

        IList<Cupo>? cupos = await ws.RequestPostAndDeserializeAsync<IList<Cupo>>("CuposData", "Cupos", request, accessToken);
        _logger.LogInformation("Respuesta de SIL. Cantidad de cupos: {Count}", cupos?.Count ?? 0);

        if (cupos == null || !cupos.Any())
        {
          _logger.LogWarning("SIL devolvió 0 registros para {Desde} - {Hasta}", request.FechaDesde, request.FechaHasta);
          result.Success = true;
          result.Warnings.Add($"SIL no devolvio registros para el periodo {request.FechaDesde:dd/MM/yyyy} - {request.FechaHasta:dd/MM/yyyy}.");
          result.Data = new DataTable();
          return result;
        }
        _logger.LogInformation("Cargando reporte desde: {Path}", Path.GetFullPath("Reports/ReportCPE.json"));

        Report report = ReportBuilder.GetReport("Reports/ReportCPE.json");
        if (report == null)
        {
          result.Success = false;
          result.Error = "No se pudo cargar ReportCPE.json.";
          return result;
        }

        List<string> listaDePropiedades = report.report.properties?.Select(x => x.property).ToList() ?? new List<string>();

        var dynamicList = cupos.Select(i =>
        {
          var expando = new ExpandoObject();
          var expandoAsDict = expando as IDictionary<string, object>;
          var targetProperties = i
            .GetType()
            .GetProperties()
            .Where(p => listaDePropiedades.Contains(p.Name))
            .ToDictionary(p => p.Name, p => p.GetValue(i));
          foreach (var property in targetProperties)
          {
            expandoAsDict.Add(property);
          }
          return (dynamic)expando;
        }).ToList();

        result.Data = DynamicListToDataTableConverter.ToDataTable(dynamicList, report.report);
        result.Success = true;

        return result;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error consultando SIL");
        result.Success = false;
        result.Error = "Error al consultar el servicio SIL. Verifique la conectividad.";
        result.Warnings.Add($"Detalle: {ex.Message}");
        return result;
      }
    }
    // Método auxiliar para consultar Azure y SIL en paralelo
    public async Task<ExceptionsResult<DataTable>> ObtenerDatosAzureYSIL(CPEReportRequest request, DateTime cuttingDay, string selectColumn, string silDataUrl)
    {
      var result = new ExceptionsResult<DataTable>();

      try
      {
        var firstRange = (CPEReportRequest)request.Clone();
        firstRange.FechaHasta = cuttingDay.AddDays(-1).Date;

        var secondRange = (CPEReportRequest)request.Clone();
        secondRange.FechaDesde = cuttingDay;

        _logger.LogInformation(
          "Consultando Azure (desde {Desde} hasta {Hasta}) y SIL (desde {DesdeS} hasta {HastaS})",
          firstRange.FechaDesde, firstRange.FechaHasta,
          secondRange.FechaDesde, secondRange.FechaHasta
        );

        Task<DataTable> azureTask = GetCuposDT(firstRange, selectColumn);
        Task<ExceptionsResult<DataTable>> silTask = ConsultarCuposSIL(secondRange, silDataUrl);

        await Task.WhenAll(azureTask, silTask);

        var azureDt = azureTask.Result ?? new DataTable();
        var silResult = silTask.Result;

        // Verificar error en SIL
        if (!silResult.Success)
        {
          result.Success = false;
          result.Error = silResult.Error;
          return result;
        }

        // Acumular warnings
        result.Warnings.AddRange(silResult.Warnings);

        var silDt = silResult.Data ?? new DataTable();

        // Validar si hay datos
        if (azureDt.Rows.Count == 0 && silDt.Rows.Count == 0)
        {
          result.Warnings.Add("No se encontraron datos ni en Azure ni en SIL para el período seleccionado.");
          result.Data = new DataTable();
          result.Success = true;
          return result;
        }

        // Combinar datos
        if (azureDt.Rows.Count == 0 && silDt.Rows.Count > 0)
        {
          result.Warnings.Add("Solo se encontraron datos en SIL. Azure no devolvio registros.");
          result.Data = silDt;
        }
        else if (azureDt.Rows.Count > 0 && silDt.Rows.Count == 0)
        {
          result.Warnings.Add("Solo se encontraron datos en Azure. SIL no devolvio registros.");
          result.Data = azureDt;
        }
        else
        {
          foreach (DataRow row in silDt.Rows)
            azureDt.ImportRow(row);
          result.Data = azureDt;
        }

        result.Success = true;
        return result;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error consultando Azure y SIL");
        result.Success = false;
        result.Error = "Error al consultar los servicios de datos.";
        return result;
      }
    }
    //Método auxiliar para generar Excel
    public ExceptionsResult<byte[]> GenerarExcel(DataTable dataTable, Report report)
    {
      var result = new ExceptionsResult<byte[]>();

      try
      {
        using (XLWorkbook wb = new XLWorkbook())
        {
          var ajustedDt = dataTable.AdjustDataTableFromReport(report);

          if (ajustedDt == null)
          {
            result.Success = false;
            result.Error = "Error interno al ajustar los datos del reporte.";
            return result;
          }

          wb.Worksheets.Add(ajustedDt);

          using (MemoryStream stream = new MemoryStream())
          {
            wb.SaveAs(stream);
            stream.Position = 0;
            result.Success = true;
            result.Data = stream.ToArray();
            return result;
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error generando Excel");
        result.Success = false;
        result.Error = "Error al generar el archivo Excel.";
        return result;
      }
    }

    public Task<(bool Ok, long Ms, string? Error)> PingSqlAsync()
    {
      return _cuposStore.PingSqlAsync();
    }
    public async Task<long> GetCuposCount(CPEReportRequest reporteCPERequest)
    {
      Task<long> result = _cuposStore.GetCountCupos(reporteCPERequest);
      return await result;
    }
    public async Task<TablesResponse> GetCuposCollectionWithPaging(TablesRequest filter)
    {
      try
      {
        TablesResponse result = new TablesResponse();
        if (filter != null && filter.ParameterClass != null)
        {

          string? value = filter.ParameterClass.ToString();
          CPEReportRequest param = JsonConvert.DeserializeObject<CPEReportRequest>(value: value);
          result.Values = await _cuposStore.GetCuposWithPaging(filter.PageSize, filter.PageNumber, filter.OrderColumn, filter.OrderDesc, param);
          result.RecordTotal = await _cuposStore.GetCountCupos(param);
        }

        return result;
      }
      catch (Exception ex) 
      { 
        throw; 
      }
    }
    
    
    /// <summary>
    /// Agrupar y obtener los totales por grano, por dia segun su estado
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<ShiftBoardResponse> GetTotalCountOfCuposForDasboardOLD(ShiftBoardRequest filter) 
    {
      try
      {
        ShiftSILBoardRequest shiftSILBoardRequest = new ShiftSILBoardRequest
        {
          Desde = DateTime.Now.AddDays(-1).Date,
          Hasta = DateTime.Now.AddDays(30).Date,
          Compradores = filter.Compradores,
          Vendedores = filter.Vendedores,
          Productos = filter.Productos,
          Destinos = filter.Destinos,
          Centros = filter.Centros
        };
        IList<Cupo> SILcupos = await GetSILCuposForDasboard(shiftSILBoardRequest);
        IEnumerable<Cupo> result = await GetCuposCollectionForDasboard(filter);
        List<Cupo> AzureCupos = result.ToList();
        List<Cupo> cupos = new List<Cupo>();
        cupos.AddRange(SILcupos);
        cupos.AddRange(AzureCupos);
        if (cupos is not null && cupos.Any())
        {
          ShiftBoardResponse resultado = new ShiftBoardResponse { 
            cuadrantes = cupos
            .GroupBy(x => new { x.CodGrano, x.NomGrano, x.Fecha })
            .Select(gr => new Cuadrante
            {
              CodGrano = !string.IsNullOrEmpty(gr.Key.CodGrano) ? gr.Key.CodGrano : "No identificado",
              NomGrano = !string.IsNullOrEmpty(gr.Key.NomGrano) ? gr.Key.NomGrano : "No identificado",
              Fecha = gr.Key.Fecha.HasValue ? gr.Key.Fecha.Value : DateTime.Now.Date,
              CuposCount = cupos.Count(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha),
              CuposDetail = cupos.Where(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha).ToList(),

              NoSTOPCount = cupos.Count(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !x.EstaSTOP),
              NoSTOPDetail = cupos.Where(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !x.EstaSTOP).ToList(),

              NoSILCount = cupos.Count(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !x.EstaSIL),
              NoSILDetail = cupos.Where(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !x.EstaSIL).ToList(),

              SinCTGCount = cupos.Count(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !string.IsNullOrEmpty(x.EstadoSTOP) && x.EstadoSTOP == "0"),
              SinCTGDetail = cupos.Where(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !string.IsNullOrEmpty(x.EstadoSTOP) && x.EstadoSTOP == "0").ToList(),

              ActivadosCount = cupos.Count(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !string.IsNullOrEmpty(x.EstadoSTOP) && x.EstadoSTOP == "1"),
              ActivadosDetail = cupos.Where(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !string.IsNullOrEmpty(x.EstadoSTOP) && x.EstadoSTOP == "1").ToList(),

              ArribadosCount = cupos.Count(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !string.IsNullOrEmpty(x.EstadoSTOP) && x.EstadoSTOP == "2"),
              ArribadosDetail = cupos.Where(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !string.IsNullOrEmpty(x.EstadoSTOP) && x.EstadoSTOP == "2").ToList(),

              DescargadosCount = cupos.Count(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !string.IsNullOrEmpty(x.EstadoSTOP) && x.EstadoSTOP == "3"),
              DescargadosDetail = cupos.Where(x => x.CodGrano == gr.Key.CodGrano && x.NomGrano == gr.Key.NomGrano && x.Fecha == gr.Key.Fecha && !string.IsNullOrEmpty(x.EstadoSTOP) && x.EstadoSTOP == "3").ToList(),
            })
            .ToList()
          };
          return resultado;
        }
        else 
        {
          return new ShiftBoardResponse { cuadrantes = new List<Cuadrante>()};
        }
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    public async Task<ShiftBoardResponse> GetTotalCountOfCuposForDasboard(ShiftBoardRequest filter)
    {
      try
      {
        DateTime fechaFiltro = filter.Fecha.Date;
        DateTime hoy = DateTime.Now.Date;
        int diff = (fechaFiltro - hoy).Days;

        DateTime desdeSIL = fechaFiltro;
        DateTime hastaSIL = fechaFiltro;
        bool llamarSIL = true;
        bool llamarAzure = diff < 0;

        if (diff >= 0) // Hoy o futuro
        {
          desdeSIL = fechaFiltro.AddDays(-1);
          hastaSIL = fechaFiltro.AddDays(3);
        }
        else if (diff == -1) // Ayer
        {
          desdeSIL = fechaFiltro;
          hastaSIL = fechaFiltro.AddDays(3);
        }
        else if (diff == -2) // Antes de ayer
        {
          desdeSIL = fechaFiltro.AddDays(1);
          hastaSIL = fechaFiltro.AddDays(3);
        }
        else if (diff == -3) // Hace 3 días
        {
          desdeSIL = fechaFiltro.AddDays(2);
          hastaSIL = fechaFiltro.AddDays(3);
        }
        else if (diff <= -4)
        {
          desdeSIL = fechaFiltro.AddDays(3);
          hastaSIL = fechaFiltro.AddDays(3);
        }
        else if (diff <= -5)
        {
          llamarSIL = false; // demasiado viejo, solo Azure
        }

        Task<IList<Cupo>> silTask = Task.FromResult<IList<Cupo>>(new List<Cupo>());
        Task<IEnumerable<Cupo>> azureTask = Task.FromResult<IEnumerable<Cupo>>(Enumerable.Empty<Cupo>());

        if (llamarSIL)
        {
          var shiftSILBoardRequest = new ShiftSILBoardRequest
          {
            Desde = desdeSIL,
            Hasta = hastaSIL,
            Compradores = filter.Compradores,
            Vendedores = filter.Vendedores,
            Productos = filter.Productos,
            Destinos = filter.Destinos,
            Centros = filter.Centros
          };

          silTask = GetSILCuposForDasboard(shiftSILBoardRequest);
        }

        if (false)
        {
          azureTask = GetCuposCollectionForDasboard(filter);
        }

        await Task.WhenAll(silTask, azureTask);

        List<Cupo> cupos = new List<Cupo>();
        if (silTask.Result?.Any() == true)
          cupos.AddRange(silTask.Result);

        if (azureTask.Result?.Any() == true)
          cupos.AddRange(azureTask.Result);

        if (!cupos.Any())
          return new ShiftBoardResponse { cuadrantes = new List<Cuadrante>() };

        var resultado = new ShiftBoardResponse
        {
          cuadrantes = cupos
            .GroupBy(x => new { x.CodGrano, x.NomGrano, x.Fecha })
            .Select(gr => new Cuadrante
            {
              CodGrano = !string.IsNullOrEmpty(gr.Key.CodGrano) ? gr.Key.CodGrano : "No identificado",
              NomGrano = !string.IsNullOrEmpty(gr.Key.NomGrano) ? gr.Key.NomGrano : "No identificado",
              Fecha = gr.Key.Fecha ?? hoy,
              //actualmente no mostramos el siguiente valor
              CuposCount = gr.Count(),
              CuposDetail = gr.ToList(),

              //es decir, lo que no tiene un puerto asociado con los puertos de stop
              NoSTOPCount = gr.Count(x => !x.Turneable), 
              NoSTOPDetail = gr.Where(x => !x.Turneable).ToList(),

              //actualmente no mostramos el siguiente valor. Traemos solamente lo que esta en SIL
              NoSILCount = gr.Count(x => !x.EstaSIL), 
              NoSILDetail = gr.Where(x => !x.EstaSIL).ToList(),

              SinCTGCount = gr.Count(x => x.EstadoSTOP == "0" && x.Turneable),
              SinCTGDetail = gr.Where(x => x.EstadoSTOP == "0" && x.Turneable).ToList(),

              ActivadosCount = gr.Count(x => x.EstadoSTOP == "1" && x.Turneable),
              ActivadosDetail = gr.Where(x => x.EstadoSTOP == "1" && x.Turneable).ToList(),

              ArribadosCount = gr.Count(x => x.EstadoSTOP == "2" && x.Turneable),
              ArribadosDetail = gr.Where(x => x.EstadoSTOP == "2" && x.Turneable).ToList(),

              DescargadosCount = gr.Count(x => x.EstadoSTOP == "3" && x.Turneable),
              DescargadosDetail = gr.Where(x => x.EstadoSTOP == "3" && x.Turneable).ToList(),
            })
            .ToList()
        };

        return resultado;
      }
      catch
      {
        throw;
      }
    }

    public async Task<IList<Cupo>> GetSILCuposForDasboard(ShiftSILBoardRequest shiftSILBoardRequest) 
    {
      string SilDataUrl = _configuration["SILDataAPI"] ?? "";
      using (SILData ws = new SILData(SilDataUrl))
      {
        IList<Cupo> cupos = await ws.RequestPostAndDeserializeAsync<IList<Cupo>>("CuposData", "GetCuposParaInforme", shiftSILBoardRequest, "", 5);
        return cupos?? new List<Cupo>();
      }
    }
  }
}

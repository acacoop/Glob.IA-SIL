using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Domain.Entities.Externo;
using Domain.Entities.PanelControlLogistico;
using Domain.Entities.Personas;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Shared.ClassShared.Interfaces;
using Shared.ClassShared.Requests;
using Shared.StaticShared;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Expression = System.Linq.Expressions.Expression;

namespace SILData.Services
{
  public class CuposServices : ISILCuposServices
  {
    private readonly ISILCuposStore _silStore;
    private readonly ILogger _logger;

    public CuposServices(ISILCuposStore silStore, ILogger<CuposServices> logger)
    {
      _silStore = silStore;
      _logger = logger;


    }

    public async Task<IList<Cupo>> GetCupos(CPEReportRequest request)
    {
      IList<Cupo> cuposList = new List<Cupo>();
      cuposList = await _silStore.FindCuposByPeriod(request.FechaDesde.Date, request.FechaHasta.Date);

      if (request.Vendedores != null && request.Vendedores.Count > 0)
        cuposList = cuposList.Where(x => request.Vendedores.Contains(x.CodVendSIL)).ToList();
      if (request.Compradores != null && request.Compradores.Count > 0)
        cuposList = cuposList.Where(x => request.Vendedores.Contains(x.CodVendSIL)).ToList();
      if (request.Destinos != null && request.Destinos.Count > 0)
        cuposList = cuposList.Where(x => request.Destinos.Contains(x.CodDestino)).ToList();
      if (request.Productos != null && request.Productos.Count > 0)
        cuposList = cuposList.Where(x => request.Productos.Contains(x.CodGrano) == true).ToList();
      if (request.Centros != null && request.Centros.Count > 0)
        cuposList = cuposList.Where(x => request.Centros.Contains(x.CentroCupo)).ToList();
      if (request.EstadoDeCupoEnSTOP > -1)
        cuposList = cuposList.Where(x => x.EstadoSTOP.Equals(request.EstadoDeCupoEnSTOP)).ToList();
      if (request.TipoDeReporte == 0)
        cuposList = cuposList.Where(x => x.EstaSIL && !x.EstaSTOP).ToList();
      if (request.TipoDeReporte == 1)
        cuposList = cuposList.Where(x => !x.EstaSIL && x.EstaSTOP).ToList();
      /*  Si tipo de reporte = 2 -> traigo todo */
      var cupos = cuposList.OrderBy(x => x.Fecha).ToList();
      _logger.LogInformation("SILData: filtro y obtengo " + cupos.Count() + " registros de SIL");
      return cupos;
    }

    /// <summary>
    /// traer los cupos de la BD de SIL que cumplan la condicion especificada en los filtros
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<IList<Cupo>> GetCuposCollectionForDasboard(ShiftSILBoardRequest filters)
    {
      IList<Cupo> cuposList = await _silStore.FindCuposForDasshboardByPeriodBy(filters);

      return cuposList;
    }
  }
}

using Microsoft.Extensions.Logging;
using NuGet.Packaging.Signing;
using SILData.DataAccess;
using SILData.Model;

namespace SILData.Services
{
  public class DataService : IDataService
  {
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataService> _logger;

    public DataService(IConfiguration configuration, ILogger<DataService> logger) { 
      _configuration = configuration;
      _logger = logger;
    }

    public Task<IEnumerable<ProductoSIL>> FindProductoStartsWithLimit(string filtro, int limit)
    {
      ProductoStore servicio = new ProductoStore(_configuration, _logger);
      Task<IEnumerable<ProductoSIL>> res = servicio.FindContainWithLimit(filtro, 10);
      return res;
    }

    public Task<IEnumerable<CentroSIL>> GetCentros()
    {
      CentroStore servicio = new CentroStore(_configuration, _logger);
      Task<IEnumerable<CentroSIL>> res = servicio.GetAll();
      return res;
    }

    public Task<IEnumerable<CentroSIL>> GetCentros(IList<string> ids)
    {
      CentroStore servicio = new CentroStore(_configuration, _logger);
      Task<IEnumerable<CentroSIL>> res = servicio.GetByIds(ids);
      return res;
    }

    public Task<IEnumerable<DTablaSIL>> GetCondicionesMercaderia()
    {
      DTablaStore servicio = new DTablaStore(_configuration, _logger);
      Task<IEnumerable<DTablaSIL>> res = servicio.GetByEntidadAndOrden("CALIDAD", "CA2");
      return res;
    }

    public Task<IEnumerable<DTablaSIL>> GetCondicionesMercaderia(IList<string> ids)
    {
      DTablaStore servicio = new DTablaStore(_configuration, _logger);
      Task<IEnumerable<DTablaSIL>> res = servicio.GetByEntidadAndOrden("CALIDAD", "CA2", ids);
      return res;
    }

    public Task<IEnumerable<DTablaSIL>> GetMonedas()
    {
      DTablaStore servicio = new DTablaStore(_configuration, _logger);
      Task<IEnumerable<DTablaSIL>> res = servicio.GetByEntidadAndOrden("MONEDA", "MN2");
      return res;
    }

    public Task<IEnumerable<DTablaSIL>> GetMonedas(IList<string> ids)
    {
      DTablaStore servicio = new DTablaStore(_configuration, _logger);
      Task<IEnumerable<DTablaSIL>> res = servicio.GetByEntidadAndOrden("MONEDA", "MN2", ids);
      return res;
    }

    public Task<ProductoSIL?> GetProducto(string id)
    {
      ProductoStore servicio = new ProductoStore(_configuration, _logger);
      Task<ProductoSIL?> res = servicio.GetById(id);
      return res;
    }

    public Task<IEnumerable<ProductoSIL>> GetProductos()
    {
      ProductoStore servicio = new ProductoStore(_configuration, _logger);
      Task<IEnumerable<ProductoSIL>> res = servicio.GetAll();
      return res;
    }

    public Task<IEnumerable<ProductoSIL>> GetProductos(IList<string> ids)
    {
      ProductoStore servicio = new ProductoStore(_configuration, _logger);
      Task<IEnumerable<ProductoSIL>> res = servicio.GetByIds(ids);
      return res;
    }

    public Task<IEnumerable<DTablaSIL>> GetTiposCuenta()
    {
      DTablaStore servicio = new DTablaStore(_configuration, _logger);
      Task<IEnumerable<DTablaSIL>> res = servicio.GetByEntidadAndOrden("STPROVEE", "02FA");
      return res;
    }

    public Task<IEnumerable<DTablaSIL>> GetTiposCuenta(IList<string> ids)
    {
      DTablaStore servicio = new DTablaStore(_configuration, _logger);
      Task<IEnumerable<DTablaSIL>> res = servicio.GetByEntidadAndOrden("STPROVEE", "02FA", ids);
      return res;
    }

    public Task<IEnumerable<DTablaSIL>> GetTiposOperacion()
    {
      DTablaStore servicio = new DTablaStore(_configuration, _logger);
      Task<IEnumerable<DTablaSIL>> res = servicio.GetByEntidadAndOrden("TIP_OPER", "OP2");
      return res;
    }

    public Task<IEnumerable<DTablaSIL>> GetTiposOperacion(IList<string> ids)
    {
      DTablaStore servicio = new DTablaStore(_configuration, _logger);
      Task<IEnumerable<DTablaSIL>> res = servicio.GetByEntidadAndOrden("TIP_OPER", "OP2", ids);
      return res;
    }

    public Task<IEnumerable<TbCoperSIL>> GetZonaComercial()
    {
      TbCoperStore servicio = new TbCoperStore(_configuration, _logger);
      Task<IEnumerable<TbCoperSIL>> res = servicio.GetAll();
      return res;
    }

    public Task<IEnumerable<TbCoperSIL>> GetZonaComercial(IList<string> ids)
    {
      TbCoperStore servicio = new TbCoperStore(_configuration, _logger);
      Task<IEnumerable<TbCoperSIL>> res = servicio.GetByIds(ids);
      return res;
    }
  }
}

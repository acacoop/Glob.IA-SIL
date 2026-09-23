using Domain.Entities.Personas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NuGet.Packaging.Signing;
using SILData.DataAccess;
using SILData.Model;
using SILData.Services;
using System.Net;

namespace SILData.Controllers
{
  [Route("api/Datos")]
  [ApiController]
  public class DataController : ControllerBase
  {
    private readonly IDataService _dataService;

    public DataController(IDataService dataService)
    {
      _dataService = dataService;
    }

    [HttpGet("Productos")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetProductos()
    {
      IEnumerable<ProductoSIL> productos = await _dataService.GetProductos();
      return Ok(productos != null ? productos.Select(x => new CodigoNombre { Codigo = x.CodigoGrano, Nombre = x.Nombre }) : null);
    }
    /// <summary>
    /// Filtro por nombre o codigo de grano
    /// </summary>
    /// <param name="filtro"></param>
    /// <returns></returns>
    [HttpGet("ProductosByFilter/{filtro}")]
    public async Task<ActionResult<IEnumerable<ProductoSIL>>> GetProductosByFilter(string filtro)
    {
      IEnumerable<ProductoSIL> productos = await _dataService.FindProductoStartsWithLimit(filtro,10);
      return Ok(productos != null ? productos.Select(x => new CodigoNombre { Codigo = x.Grano, Nombre = x.Nombre}) : null);
    }

    [HttpGet("Producto/{id}")]
    public async Task<ActionResult<CodigoNombre>> GetProducto(string id)
    {
      ProductoSIL producto = await _dataService.GetProducto(id);
      return Ok(producto != null ? new CodigoNombre { Codigo = producto.CodigoGrano, Nombre = producto.Nombre } : null);
    }

    [HttpPost("GetProductos")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetProductos(IList<string> ids)
    {
      IEnumerable<ProductoSIL> productos = await _dataService.GetProductos(ids);
      return Ok(productos != null ? productos.Select(x => new CodigoNombre { Codigo = x.CodigoGrano, Nombre = x.Nombre }) : null);
    }

    [HttpGet("Centros")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetCentros()
    {
      IEnumerable<CentroSIL> centros = await _dataService.GetCentros();
      return Ok(centros != null ? centros.Select(x => new CodigoNombre { Codigo = x.CodigoCentro, Nombre = x.Nombre }) : null);
    }

    [HttpPost("GetCentros")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetCentros(IList<string> ids)
    {
      IEnumerable<CentroSIL> centros = await _dataService.GetCentros(ids);
      return Ok(centros != null ? centros.Select(x => new CodigoNombre { Codigo = x.CodigoCentro, Nombre = x.Nombre }) : null);
    }

    [HttpGet("Monedas")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetMonedas()
    {
      IEnumerable<DTablaSIL> dTablas = await _dataService.GetMonedas();
      return Ok(dTablas != null ? dTablas.Select(x => new CodigoNombre { Codigo = x.Clave, Nombre = x.Valor }) : null);
    }

    [HttpPost("GetMonedas")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetMonedas(IList<string> ids)
    {
      IEnumerable<DTablaSIL> dTablas = await _dataService.GetMonedas(ids);
      return Ok(dTablas != null ? dTablas.Select(x => new CodigoNombre { Codigo = x.Clave, Nombre = x.Valor }) : null);
    }

    [HttpGet("CondicionesMercaderia")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetCondicionesMercaderia()
    {
      IEnumerable<DTablaSIL> dTablas = await _dataService.GetCondicionesMercaderia();
      return Ok(dTablas != null ? dTablas.Select(x => new CodigoNombre { Codigo = x.Clave, Nombre = x.Valor }) : null);
    }

    [HttpPost("GetCondicionesMercaderia")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetCondicionesMercaderia(IList<string> ids)
    {
      IEnumerable<DTablaSIL> dTablas = await _dataService.GetCondicionesMercaderia(ids);
      return Ok(dTablas != null ? dTablas.Select(x => new CodigoNombre { Codigo = x.Clave, Nombre = x.Valor }) : null);
    }

    [HttpGet("TiposCuenta")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetTiposCuenta()
    {
      IEnumerable<DTablaSIL> dTablas = await _dataService.GetTiposCuenta();
      return Ok(dTablas != null ? dTablas.Select(x => new CodigoNombre { Codigo = x.Clave, Nombre = x.Valor }) : null);
    }

    [HttpPost("GetTiposCuenta")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetTiposCuenta(IList<string> ids)
    {
      IEnumerable<DTablaSIL> dTablas = await _dataService.GetTiposCuenta(ids);
      return Ok(dTablas != null ? dTablas.Select(x => new CodigoNombre { Codigo = x.Clave, Nombre = x.Valor }) : null);
    }

    [HttpGet("TiposOperacion")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetTiposOperacion()
    {
      IEnumerable<DTablaSIL> dTablas = await _dataService.GetTiposOperacion();
      return Ok(dTablas != null ? dTablas.Select(x => new CodigoNombre { Codigo = x.Clave, Nombre = x.Valor }) : null);
    }

    [HttpPost("GetTiposOperacion")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetTiposOperacion(IList<string> ids)
    {
      IEnumerable<DTablaSIL> dTablas = await _dataService.GetTiposOperacion(ids);
      return Ok(dTablas != null ? dTablas.Select(x => new CodigoNombre { Codigo = x.Clave, Nombre = x.Valor }) : null);
    }

    [HttpGet("ZonaComercial")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetZonaComercial()
    {
      IEnumerable<TbCoperSIL> tbCopers = await _dataService.GetZonaComercial();
      return Ok(tbCopers != null ? tbCopers.Select(x => new CodigoNombre { Codigo = x.Codigo, Nombre = x.Descripcion }) : null);
    }

    [HttpPost("GetZonaComercial")]
    public async Task<ActionResult<IEnumerable<CodigoNombre>>> GetZonaComercial(IList<string> ids)
    {
      IEnumerable<TbCoperSIL> tbCopers = await _dataService.GetZonaComercial(ids);
      return Ok(tbCopers != null ? tbCopers.Select(x => new CodigoNombre { Codigo = x.Codigo, Nombre = x.Descripcion }) : null);
    }
  }
}

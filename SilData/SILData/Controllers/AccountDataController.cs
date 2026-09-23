using Domain.Entities.Personas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SILData.DataAccess;
using SILData.Model;
using SILData.Services;

namespace SILData.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  //[Authorize]
  public class AccountDataController : ControllerBase
  {
    private IConfiguration _configuration;
    private ILogger<AccountDataController> logger;
    private readonly AccountService _service;

    public AccountDataController(IConfiguration configuration, ILogger<AccountDataController> logger, AccountService service)
    {
      _configuration = configuration;
      this.logger = logger;
      _service = service; 
    }

    /// <summary>
    /// From cuenta-nombre
    /// </summary>
    /// <param name="Cuenta"></param>
    /// <returns></returns>
    [HttpGet("Comprador/{Cuenta}")]
    public async Task<ActionResult<List<CuentaSIL>>> GetCuentaComprador(string Cuenta)
    {
      var servicio = new AccountService(new CompradorStore(_configuration, logger));
      var res = await servicio.GetContain(Cuenta);
      return Ok((List<CuentaSIL>)res);
       
    }

    [HttpGet("Vendedor/{Cuenta}")]
    public async Task<ActionResult<IList<CuentaSIL>>> GetCuentaVendedor(string Cuenta)
    {
      var res = await _service.GetContain(Cuenta);
      return Ok((List<CuentaSIL>)res);
    }

    [HttpGet("VendedorByCuenta/{Cuenta}")]
    public async Task<ActionResult<CuentaSIL>> GetCuentaVendedorByCuenta(long Cuenta)
    {
      var res = await _service.GetVendedorByCuenta(Cuenta);
      return Ok((CuentaSIL)res);
    }

    /// <summary>
    /// Espejo de <see cref="GetCuentaVendedorByCuenta"/> pero para compradores
    /// (tabla CUPOSCOMPRADOR). Devuelve la <see cref="CuentaSIL"/> con su nombre
    /// y cuit. Se usa desde el motor de matching para hidratar el nombre del
    /// comprador de un cupo en el payload que devuelve <c>POST /Matches</c>.
    /// </summary>
    [HttpGet("CompradorByCuenta/{Cuenta}")]
    public async Task<ActionResult<CuentaSIL>> GetCuentaCompradorByCuenta(long Cuenta)
    {
      var servicio = new AccountService(new CompradorStore(_configuration, logger));
      var res = await servicio.GetCompradorByCuenta(Cuenta);
      return Ok((CuentaSIL)res);
    }


    [HttpGet("Cuit/{Cuenta}")]
    public async Task<ActionResult<IList<CuentaSIL>>> GetCuit(string Cuenta)
    {
      var servicio = new AccountService(new CuitStore(_configuration, logger));
      var res = await servicio.GetContain(Cuenta);
      return Ok((List<CuentaSIL>)res);
    }

    [HttpGet("Puerto/{Cuenta}")]
    public async Task<ActionResult<IList<CuentaSIL>>> GetPuerto(string Cuenta)
    {
      var servicio = new AccountService(new PuertoStore(_configuration, logger));
      var res = await servicio.GetContain(Cuenta);
      return Ok((List<CuentaSIL>)res);
    }

    [HttpGet("Centro/{Cuenta}")]
    public async Task<ActionResult<IList<CentroSIL>>> GetCentro(string Cuenta)
    {
      var servicio = new CentroStore(_configuration, logger);
      var res = await servicio.FindContainWithLimit(Cuenta.ToUpper(), 10);
      return base.Ok((List<Model.CentroSIL>)res);
    }
  }
}

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
  public class ProductDataController : ControllerBase
  {
    private IConfiguration _configuration;
    private ILogger<AccountDataController> logger;

    public ProductDataController(IConfiguration configuration, ILogger<AccountDataController> logger)
    {
      _configuration = configuration;
      this.logger = logger;

    }

    /// <summary>
    /// From 
    /// </summary>
    /// <param name="Cuenta"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoSIL>>> Get(string filtro)
    {
      var servicio = new ProductoStore(_configuration, logger);
      var res = await servicio.FindContainWithLimit(filtro, 10);
      return Ok(res);

    }
  }
}

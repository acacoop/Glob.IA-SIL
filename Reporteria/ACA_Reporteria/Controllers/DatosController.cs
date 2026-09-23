using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reporteria.Model;

namespace Reporteria.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  //[Authorize]
  public class DatosController : ControllerBase
  {
    [HttpGet("EstadoDeCupoEnStop")]
    public ActionResult<IEnumerable<Dictionary<int, string>>> EstadoDeCupoEnStop()
    {
      return Ok(DictionaryOfValues.EstadoDeCupoEnStop);
    }

    [HttpGet("OpcionesCupos")]
    public ActionResult<IEnumerable<Dictionary<int, string>>> OpcionesCupos()
    {
      return Ok(DictionaryOfValues.OpcionesCupos);
    }
  }
}

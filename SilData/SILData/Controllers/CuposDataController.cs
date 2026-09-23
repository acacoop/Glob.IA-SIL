using Domain.Entities.Externo;
using Domain.Entities.PanelControlLogistico;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.ClassShared.Interfaces;
using Shared.ClassShared.Requests;

namespace SILData.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  //[Authorize]
  public class CuposDataController : ControllerBase
  {
    private readonly ISILCuposServices _cuposServices;
    private readonly ILogger<CuposDataController> _logger;

    public CuposDataController(ISILCuposServices cuposServices, ILogger<CuposDataController> logger)
    {
      _cuposServices = cuposServices;
      _logger = logger;
    }

    [HttpPost("Cupos")]
    public async Task<IList<Cupo>> GetCupos([FromBody] CPEReportRequest Request)
    {      
      var b = _cuposServices.GetCupos(Request);
      return await b;
    }

    [HttpPost("GetCuposParaInforme", Name = "GetCuposParaInforme")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Cupo>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetCuposParaInforme([FromBody] ShiftSILBoardRequest shiftBoardRequest)
    {
      try
      {
        IList<Cupo> cupos = await this._cuposServices.GetCuposCollectionForDasboard(shiftBoardRequest);
        return Ok(cupos);
      }
      catch (Exception e)
      {
        _logger.LogError(e, e.Message);
        throw;
      }
    }
  }
}

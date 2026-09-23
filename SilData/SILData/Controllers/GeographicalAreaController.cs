using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration;
using SILData.DataAccess;
using SILData.Model.SolicitudTurno;
using SILData.Services;
using SILData.SilDataExceptions;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace SILData.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  //[Authorize]
  public class GeographicalAreaController : ControllerBase
  {
    private IConfiguration _configuration { get; set; }
    private ILogger<GeographicalAreaController> _logger { get; set; }
    private IGeographicalAereaService _geographicalAereaService { get; set; }

    public GeographicalAreaController(ILogger<GeographicalAreaController> logger, IGeographicalAereaService geographicalAereaService, IConfiguration configuration)
    {
      _logger = logger;
      _configuration = configuration;
      _geographicalAereaService = geographicalAereaService;
    }

    [HttpGet("GetAllAsync", Name = "GeographicalAreaGetAllAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ZonaGeograficaView>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IEnumerable<ZonaGeograficaView>>> GetAllAsync()
    {
      try
      {
        IEnumerable<ZonaGeograficaView> areas = await _geographicalAereaService.GetAllAsync();

        if (areas is null || !areas.Any())
          return NoContent();

        return Ok(areas);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en GetAllAsync (GeographicalArea)");
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetAllAsync (GeographicalArea)");
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    [HttpPost("GetByFilterAsync", Name = "GeographicalAreaGetByFilterAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SolicitudTurnoView>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IEnumerable<SolicitudTurnoView>>> GetByFilterAsync([FromBody][SwaggerParameter("Imput data fot Filter", Required = false)] FiltroZonaGeografica filter)
    {
      if (filter is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      if (filter.CuentaVendedora <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "CuentaVendedora debe ser un número positivo.",
          Instance = HttpContext.Request.Path
        });

      if (filter.Fecha == default)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Fecha inválida",
          Detail = "Debe indicar una fecha válida. Formato esperado: 'YYYY-MM-DD' o 'YYYY-MM-DDTHH:mm:ss'.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        IEnumerable<ZonaGeograficaView> areas = await _geographicalAereaService.GetByFilter(filter);

        if (areas is null || !areas.Any())
          return NoContent();

        return Ok(areas);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en GetByFilterAsync. Vendedor: {Vendedor}", filter.CuentaVendedora);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByFilterAsync. Vendedor: {Vendedor}", filter.CuentaVendedora);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }

    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /ResolveByDestino
    // Body: { "cuentaPuerto": 3050 }
    // Devuelve las zonas geográficas (ZONASGEOGRAFICAS) a las que pertenece
    // el puerto (vía PUERTOPORZONA). Un puerto puede estar en varias zonas;
    // 200 con [] si no pertenece a ninguna.
    //
    // Usado por el flujo de matching desde distribución: el operador tiene
    // el destino del cupo pero no la zona; este endpoint resuelve
    // destino → ZonaGeograficaId, que después se pasa como filtro a
    // POST /api/ShiftRequest/Matches.
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("ResolveByDestino", Name = "GeographicalAreaResolveByDestino")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ZonaGeograficaView>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IEnumerable<ZonaGeograficaView>>> ResolveByDestino([FromBody] ResolveByDestinoRequest req)
    {
      if (req is null || req.CuentaPuerto <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "Debe indicar cuentaPuerto (número positivo) en el cuerpo de la solicitud.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        IEnumerable<ZonaGeograficaView> zonas =
          await _geographicalAereaService.ResolveByDestinoAsync(req.CuentaPuerto);

        if (zonas is null || !zonas.Any())
          return NoContent();

        return Ok(zonas);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en ResolveByDestino. CuentaPuerto: {Id}", req.CuentaPuerto);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en ResolveByDestino. CuentaPuerto: {Id}", req.CuentaPuerto);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ZonaGeograficaView))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult RecibirFiltro([FromBody] FiltroZonaGeografica filter)
    {
      try
      {
        if (filter.Fecha == default)
        {
          throw new SilDataException("Formato de fecha inválido. Usa 'YYYY-MM-DD' o 'YYYY-MM-DDTHH:mm:ss'.");
        }

        return Ok(new { success = true, mensaje = "Fecha recibida correctamente", filter.Fecha });
      }
      catch (SilDataException ex)
      {
        var problemDetails = new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        };
        return Conflict(problemDetails);
      }
      catch (Exception ex)
      {
        var problemDetails = new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        };
        return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
      }
    }


    [HttpPost("CreateAsync", Name = "GeographicalAreaCreateAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ZonaGeograficaView))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ZonaGeograficaView>> CreateAsync([FromBody, Required] ZonaGeograficaCreate geographicalAreaCreate)
    {
      if (geographicalAreaCreate is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      if (string.IsNullOrWhiteSpace(geographicalAreaCreate.Nombre))
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "El campo Nombre es requerido.",
          Instance = HttpContext.Request.Path
        });

      if (string.IsNullOrWhiteSpace(geographicalAreaCreate.Codigo))
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "El campo Codigo es requerido.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        ZonaGeograficaView? result = await _geographicalAereaService.CreateAsync(geographicalAreaCreate);

        if (result is null)
          throw new SilDataException(
            "No fue posible crear la zona geográfica. Consulte al área de soporte.",
            StatusCodes.Status409Conflict);

        return Ok(result);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en CreateAsync. Nombre: {Nombre}", geographicalAreaCreate.Nombre);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en CreateAsync. Nombre: {Nombre}", geographicalAreaCreate.Nombre);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    [HttpPost("AddDestinationsAsync", Name = "GeographicalAreaAddDestinationsAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ZonaGeograficaView))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ZonaGeograficaView>> AddDestinationsAsync([FromBody, Required] ZonaGeograficaUpdate zonaGeograficaUpdate)
    {
      if (zonaGeograficaUpdate is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      if (zonaGeograficaUpdate.ZonaGeoId <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "ZonaGeoId debe ser un número positivo.",
          Instance = HttpContext.Request.Path
        });

      if (zonaGeograficaUpdate.Destinos is null || !zonaGeograficaUpdate.Destinos.Any())
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Sin destinos",
          Detail = "Debe indicar al menos un destino para agregar a la zona geográfica.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        IEnumerable<ZonaGeograficaView>? result = await _geographicalAereaService.AddDestinationAsync(
          zonaGeograficaUpdate.ZonaGeoId,
          zonaGeograficaUpdate.Destinos.Select(x => x.CuentaCuit).ToList());

        if (result is null)
          throw new SilDataException(
            "No es posible agregar el destino para la zona requerida.",
            StatusCodes.Status409Conflict);

        return Ok(result);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en AddDestinationsAsync. ZonaGeoId: {ZonaGeoId}", zonaGeograficaUpdate.ZonaGeoId);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en AddDestinationsAsync. ZonaGeoId: {ZonaGeoId}", zonaGeograficaUpdate.ZonaGeoId);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    [HttpDelete("Delete", Name = "GeographicalAreaDelete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromQuery, Required] int geographicalAreaId)
    {
      if (geographicalAreaId <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "geographicalAreaId debe ser un número positivo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        bool result = await _geographicalAereaService.DeleteAsync(geographicalAreaId);

        if (!result)
          throw new SilDataException(
            "Hubo un problema al eliminar la zona geográfica requerida.",
            StatusCodes.Status409Conflict);

        return Ok(true);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en Delete. ZonaId: {Id}", geographicalAreaId);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en Delete. ZonaId: {Id}", geographicalAreaId);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }
  }
}

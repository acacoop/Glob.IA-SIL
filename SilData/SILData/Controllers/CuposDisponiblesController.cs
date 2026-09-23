using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SILData.Model;
using SILData.Model.SolicitudTurno;
using SILData.Services;
using SILData.SilDataExceptions;
using System.Collections.Generic;

namespace SILData.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class CuposDisponiblesController : ControllerBase
  {
    private readonly ICuposDisponiblesService _cuposDisponiblesService;
    private readonly ILogger<CuposDisponiblesController> _logger;

    public CuposDisponiblesController(ICuposDisponiblesService cuposDisponiblesService, ILogger<CuposDisponiblesController> logger)
    {
      _cuposDisponiblesService = cuposDisponiblesService;
      _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /CantidadDisponibles
    // Devuelve la cantidad total de cupos disponibles para un vendedor/fecha.
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("CantidadDisponibles", Name = "CuposDisponiblesCantidadDisponibles")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(long))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<long>> CantidadDisponibles([FromBody] FiltroDisponible filter)
    {
      if (filter is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      if (filter.cuentaVendedor <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "cuentaVendedor debe ser un número positivo.",
          Instance = HttpContext.Request.Path
        });

      if (filter.fecha == default)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Fecha inválida",
          Detail = "Debe indicar una fecha válida para consultar los cupos disponibles.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        long result = await _cuposDisponiblesService.GetCantidadDisponibles(
          filter.cuentaVendedor,
          filter.fecha,
          filter.cuentaComprador,
          filter.codigoGrano,
          filter.zonaGeografica);

        return Ok(result);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en CantidadDisponibles. Vendedor: {Vendedor}", filter.cuentaVendedor);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en CantidadDisponibles. Vendedor: {Vendedor}", filter.cuentaVendedor);
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
    // POST /Disponibles
    // Devuelve el detalle de cupos disponibles para un vendedor/fecha.
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("Disponibles", Name = "CuposDisponibles")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CuposDisponible>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IList<CuposDisponible>>> Disponibles([FromBody] FiltroDisponible filter)
    {
      if (filter is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      if (filter.cuentaVendedor <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "cuentaVendedor debe ser un número positivo.",
          Instance = HttpContext.Request.Path
        });

      if (filter.fecha == default)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Fecha inválida",
          Detail = "Debe indicar una fecha válida para consultar los cupos disponibles.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        IEnumerable<CuposDisponible> result = await _cuposDisponiblesService.GetDisponibles(
          filter.cuentaVendedor,
          filter.fecha,
          filter.cuentaComprador,
          filter.codigoGrano,
          filter.zonaGeografica);

        if (result is null || !result.Any())
          return NoContent();

        return Ok(result);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en Disponibles. Vendedor: {Vendedor}", filter.cuentaVendedor);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en Disponibles. Vendedor: {Vendedor}", filter.cuentaVendedor);
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
    // POST /PorEstado
    // Devuelve cupos filtrados por estado. Utilizado por la pantalla de
    // aceptación/rechazo de solicitudes de turnos.
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("PorEstado", Name = "CuposPorEstado")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CuposCorreResult>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IList<CuposCorreResult>>> PorEstado([FromBody] CuposFilter filter)
    {
      if (filter is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      // CuentaVendedor es required en el modelo pero string, validamos que no sea vacío
      if (string.IsNullOrWhiteSpace(filter.CuentaVendedor))
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "CuentaVendedor es requerido.",
          Instance = HttpContext.Request.Path
        });

      if (string.IsNullOrWhiteSpace(filter.Grano))
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "Grano es requerido.",
          Instance = HttpContext.Request.Path
        });

      if (!long.TryParse(filter.Grano, out _))
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Formato inválido",
          Detail = "El campo Grano debe ser un valor numérico.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        List<CuposCorreResult> result = await _cuposDisponiblesService.GetCuposPorEstado(filter);

        if (result is null || !result.Any())
          return NoContent();

        return Ok(result);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en PorEstado. Vendedor: {Vendedor} Grano: {Grano}", filter.CuentaVendedor, filter.Grano);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en PorEstado. Vendedor: {Vendedor} Grano: {Grano}", filter.CuentaVendedor, filter.Grano);
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

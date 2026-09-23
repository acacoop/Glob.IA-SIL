using Domain.Entities.Externo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.ClassShared.Interfaces;
using SILData.DataAccess;
using SILData.Model;
using SILData.Model.SolicitudTurno;
using SILData.Services;
using SILData.SilDataExceptions;

namespace SILData.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  //[Authorize]
  public class ShiftRequestController : ControllerBase
  {
    private IConfiguration _configuration { get; set; }
    private ILogger<ShiftRequestController> _logger { get; set; }
    private readonly ISolicitudTurnoService _solicitudTurnoService;
    private readonly SolicitudTurnoMatchingV2Service _solicitudTurnoMatchingV2Service;
    private readonly AccountService _accountService;
    private readonly ICuposDisponiblesService _cuposDisponiblesService;
    private readonly ISILCuposStore _silCuposStore;
    private readonly IZonaGeograficaResolver _zonaGeograficaResolver;
    public ShiftRequestController(
      ILogger<ShiftRequestController> logger,
      ISolicitudTurnoService solicitudTurnoService,
      SolicitudTurnoMatchingV2Service solicitudTurnoMatchingV2Service,
      IConfiguration configuration,
      AccountService accountService,
      ICuposDisponiblesService cuposDisponiblesService,
      ISILCuposStore silCuposStore,
      IZonaGeograficaResolver zonaGeograficaResolver)
    {
      _logger = logger;
      _configuration = configuration;
      _solicitudTurnoService = solicitudTurnoService;
      _solicitudTurnoMatchingV2Service = solicitudTurnoMatchingV2Service;
      _accountService = accountService;
      _cuposDisponiblesService = cuposDisponiblesService;
      _silCuposStore = silCuposStore;
      _zonaGeograficaResolver = zonaGeograficaResolver;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /GetAllAsync
    // Devuelve todas las solicitudes de los próximos 7 días.
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("GetAllAsync", Name = "ShiftRequestGetAllAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SolicitudTurnoView>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IEnumerable<SolicitudTurnoView>>> GetAllAsync()
    {
      try
      {
        IEnumerable<SolicitudTurnoView> solicitudes = await _solicitudTurnoService.GetAllAsync(DateTime.Now.Date, DateTime.Now.Date.AddDays(7));
        if (solicitudes is null || !solicitudes.Any())
          return NoContent();

        return Ok(solicitudes);
      }
      catch (SilDataException SilEx)
      {
        _logger.LogWarning(SilEx, "Error de negocio en GetAllAsync");
        return Conflict(new ProblemDetails
        {
          Status = SilEx.StatusCode,
          Detail = SilEx.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetAllAsync");
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    /// <summary>
    /// Este servicio es utilizado por la Index de la app de solicitudes de turnos.
    /// Pagina que muestra todas las solicitudes pendientes para el solicitante.
    /// </summary>
    /// <param name="cuentaVendedor"></param>
    /// <returns></returns>
    [HttpGet("GetByVendedorAsync", Name = "ShiftRequestGetByVendedor")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SolicitudTurnoView>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IEnumerable<SolicitudTurnoView>>> GetByVendedorAsync(long cuentaVendedor)
    {
      if (cuentaVendedor <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "El parámetro cuentaVendedor debe ser un número positivo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        var existVendedor = await _accountService.GetVendedorByCuenta(cuentaVendedor);
        if (existVendedor == null)
          return BadRequest(new ProblemDetails
          {
            Status = StatusCodes.Status400BadRequest,
            Title = "Parámetro inválido",
            Detail = "La cuenta del vendedor no existe.",
            Instance = HttpContext.Request.Path
          });

        IEnumerable<SolicitudTurnoView> solicitudes = await _solicitudTurnoService.GetByVendedorAsync(cuentaVendedor);
        if (solicitudes == null || !solicitudes.Any())
          return NoContent();

        return Ok(solicitudes);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en GetByVendedorAsync para vendedor {CuentaVendedor}", cuentaVendedor);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetByVendedorAsync para vendedor {CuentaVendedor}", cuentaVendedor);
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
    // GET /GetGroupAsync
    // Devuelve las solicitudes de los próximos 7 días agrupadas por grano/partes.
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("GetGroupAsync", Name = "ShiftRequestGetGroupAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SolicitudTurnoGrupoView>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SolicitudTurnoGrupoView>>> GetGroupAsync()
    {
      try
      {
        IEnumerable<SolicitudTurnoView> todasSolicitudes = await _solicitudTurnoService
          .GetAllAsync(DateTime.Now.Date, DateTime.Now.Date.AddDays(7));

        if (todasSolicitudes is null || !todasSolicitudes.Any())
          return NoContent();

        IEnumerable<SolicitudTurnoGrupoView> solicitudes = todasSolicitudes
          .GroupBy(s => new
          {
            s.CodigoGrano,
            s.NombreGrano,
            s.CuentaComprador,
            s.NombreComprador,
            s.CuentaVendedor,
            s.NombreVendedor,
            s.CuentaDestino,
            s.NombreDestino
          })
          .Select(solPorGrano => new SolicitudTurnoGrupoView
          {
            CodigoGrano = solPorGrano.Key.CodigoGrano,
            NombreGrano = solPorGrano.Key.NombreGrano,
            CuentaComprador = solPorGrano.Key.CuentaComprador,
            NombreComprador = solPorGrano.Key.NombreComprador,
            CuentaVendedor = solPorGrano.Key.CuentaVendedor,
            NombreVendedor = solPorGrano.Key.NombreVendedor,
            CuentaDestino = solPorGrano.Key.CuentaDestino,
            NombreDestino = solPorGrano.Key.NombreDestino,
            CantidadFechas = solPorGrano
              .GroupBy(x => x.FechaSolicitado)
              .Select(x => new SolicitudTurnoDetalleGrupoView
              {
                Fecha = x.Key.ToString("dd/MM/yyyy"),
                Cantidad = x.Sum(r => r.Cantidad > 0 ? r.Cantidad : 1),
                DiaSemana = (x.Key.Date - DateTime.Now.Date).Days
              })
          });

        return Ok(solicitudes);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en GetGroupAsync");
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetGroupAsync");
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    /// <summary>
    /// Identificar donde se utliza este servicio.....
    /// </summary>
    /// <param name="cuentaVendedor"></param>
    /// <returns></returns>
    [HttpGet("GetGroupByVendedorAsync/{cuentaVendedor}", Name = "ShiftRequestGetGroupByVendedorAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SolicitudTurnoGrupoVendedorView>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IEnumerable<SolicitudTurnoGrupoVendedorView>>> GetGroupByVendedor(long cuentaVendedor)
    {
      if (cuentaVendedor <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "El parámetro cuentaVendedor debe ser un número positivo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        IEnumerable<SolicitudTurnoView> todasSolicitudes =
          await _solicitudTurnoService.GetByVendedorAsync(cuentaVendedor);

        if (todasSolicitudes is null || !todasSolicitudes.Any())
          return NoContent();

        IEnumerable<SolicitudTurnoGrupoVendedorView> solicitudes = todasSolicitudes
          .GroupBy(s => new { s.CodigoGrano, s.NombreGrano })
          .Select(solPorGrano => new SolicitudTurnoGrupoVendedorView
          {
            CodigoGrano = solPorGrano.Key.CodigoGrano,
            NombreGrano = solPorGrano.Key.NombreGrano,
            DetallePendientesDia = solPorGrano
              .Where(sol => !sol.EsRechazada)
              .GroupBy(sol => sol.FechaSolicitado)
              .Select(sol => new SolicitudTurnoGrupoDetallePendienteDiaView
              {
                Fecha = sol.Key.ToString("dd/MM/yyyy"),
                Cantidad = sol.Sum(r => r.Cantidad > 0 ? r.Cantidad : 1)
              }),
            DetallesSolicitados = solPorGrano
              .GroupBy(sol => new
              {
                sol.CuentaComprador,
                sol.NombreComprador,
                sol.NombreDestino,
                sol.CuentaDestino
              })
              .Select(solPorGranoCompradorDestino => new SolicitudTurnoGrupoDetalleSolicitadoView
              {
                CuentaComprador = solPorGranoCompradorDestino.Key.CuentaComprador,
                NombreComprador = solPorGranoCompradorDestino.Key.NombreComprador,
                CuentaDestino = solPorGranoCompradorDestino.Key.CuentaDestino,
                NombreDestino = solPorGranoCompradorDestino.Key.NombreDestino,
                DetallesSolicitadosDia = solPorGranoCompradorDestino
                  .GroupBy(sol => sol.FechaSolicitado)
                  .Select(solPorGranoCompradorDestinoFecha => new SolicitudTurnoGrupoDetalleSolicitadoDiaView
                  {
                    Fecha = solPorGranoCompradorDestinoFecha.Key.ToString("dd/MM/yyyy"),
                    TurnosActivos = solPorGranoCompradorDestinoFecha.Count(x => x.CtgCupo.HasValue),
                    TurnosOtorgados = solPorGranoCompradorDestinoFecha.Count(x => x.EstadoCupo.HasValue)
                  })
              })
          });

        return Ok(solicitudes);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en GetGroupByVendedor para vendedor {CuentaVendedor}", cuentaVendedor);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetGroupByVendedor para vendedor {CuentaVendedor}", cuentaVendedor);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }
    
    /// <summary>
    /// Este servicio es utilizado por la pantalla de alta de solicitudes de la solucion 
    /// SOLICITUD DE TURNOS
    /// </summary>
    /// <param name="solicitudTurnosFilter"></param>
    /// <returns></returns>
    [HttpPost("GetGroupByFilterAsync", Name = "ShiftRequestGetGroupByFilterAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SolicitudTurnoPorGranoView>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IEnumerable<SolicitudTurnoPorGranoView>>> GetGroupByFilterAsync([FromBody] SolicitudTurnosFilter solicitudTurnosFilter)
    {
      if (solicitudTurnosFilter is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      if (solicitudTurnosFilter.CuentaVendedor <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "CuentaVendedor debe ser un número positivo.",
          Instance = HttpContext.Request.Path
        });

      if (solicitudTurnosFilter.Desde > solicitudTurnosFilter.Hasta)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Rango de fechas inválido",
          Detail = "La fecha Desde no puede ser posterior a la fecha Hasta.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        IEnumerable<SolicitudTurnoView> todasSolicitudes =
          await _solicitudTurnoService.GetByFilterAsync(solicitudTurnosFilter);

        if (todasSolicitudes is null || !todasSolicitudes.Any())
          return NoContent();

        IEnumerable<SolicitudTurnoPorGranoView> solicitudes = todasSolicitudes
          .GroupBy(s => new { s.CodigoGrano, s.NombreGrano })
          .Select(solPorGrano => new SolicitudTurnoPorGranoView
          {
            CodigoGrano = solPorGrano.Key.CodigoGrano,
            NombreGrano = string.IsNullOrEmpty(solPorGrano.Key.NombreGrano)
              ? ""
              : solPorGrano.Key.NombreGrano,
            DetallePendientes = solPorGrano
              .Where(sol => sol.EsPendiente)
              .GroupBy(sol => new
              {
                CuentaComprador = sol.CuentaComprador == null ? 0 : sol.CuentaComprador,
                sol.NombreComprador,
                sol.NombreDestino,
                CuentaDestino = sol.CuentaDestino == null ? 0 : sol.CuentaDestino
              })
              .Select(sol => new SolicitudTurnoPendienteGrupoDetalleSolicitadoView
              {
                CuentaComprador = sol.Key.CuentaComprador == 0 ? null : sol.Key.CuentaComprador,
                NombreComprador = string.IsNullOrEmpty(sol.Key.NombreComprador) ? "" : sol.Key.NombreComprador,
                CuentaDestino = sol.Key.CuentaDestino == null ? 0 : sol.Key.CuentaDestino,
                NombreDestino = string.IsNullOrEmpty(sol.Key.NombreDestino) ? "" : sol.Key.NombreDestino,
                DetallesSolicitadosDia = sol
                  .GroupBy(s => s.FechaSolicitado)
                  .Select(s => new SolicitudTurnoGrupoDetallePendienteDiaView
                  {
                    Fecha = s.Key.ToString("dd/MM/yyyy"),
                    Cantidad = s.Sum(r => r.Cantidad > 0 ? r.Cantidad : 1)
                  })
              })
          });

        return Ok(solicitudes);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en GetGroupByFilterAsync");
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetGroupByFilterAsync");
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }
    /// <summary>
    /// Este servicio es utilizado por SIL pantalla de solicitudes de turnos pendientes
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("GetAllPendingShiftRequestAsync", Name = "ShiftRequestGetAllPendingShiftRequestAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SolicitudTurnoView>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IEnumerable<SolicitudTurnoView>>> GetAllPendingShiftRequestAsync(SILSolicitudDeTurnosFilter filter)
    {
      if (filter is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      if (filter.Dias <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "El campo Dias debe ser un número positivo mayor a cero.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        IEnumerable<SolicitudTurnoView> solicitudes =
          await _solicitudTurnoService.GetByFilterAsync(filter);

        if (solicitudes is null || !solicitudes.Any())
          return NoContent();

        return Ok(solicitudes);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en GetAllPendingShiftRequestAsync");
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetAllPendingShiftRequestAsync");
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    [HttpPost("Create", Name = "ShiftRequestCreate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResult))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Create(SolicitudTurnoCreate solicitudTurnoCreate)
    {
      if (solicitudTurnoCreate is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      // Aprovecha la validación ya implementada en el modelo
      string? modelError = solicitudTurnoCreate.GetError();
      if (!string.IsNullOrWhiteSpace(modelError))
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Datos inválidos",
          Detail = modelError,
          Instance = HttpContext.Request.Path
        });

      try
      {
        await _solicitudTurnoService.AddRequest(solicitudTurnoCreate);
        return Ok(new { success = true, message = "Solicitud creada con éxito." });
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en Create para vendedor {CuentaVendedor}", solicitudTurnoCreate.CuentaVendedor);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en Create para vendedor {CuentaVendedor}", solicitudTurnoCreate.CuentaVendedor);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    [HttpPost("Update", Name = "ShiftRequestUpdate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResult))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(SolicitudTurnoCreate solicitudTurnoCreate)
    {
      if (solicitudTurnoCreate is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      string? modelError = solicitudTurnoCreate.GetError();
      if (!string.IsNullOrWhiteSpace(modelError))
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Datos inválidos",
          Detail = modelError,
          Instance = HttpContext.Request.Path
        });

      try
      {
        await _solicitudTurnoService.UpdateRequest(solicitudTurnoCreate);
        return Ok(new { success = true, message = "Solicitud procesada con éxito." });
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en Update para vendedor {CuentaVendedor}", solicitudTurnoCreate.CuentaVendedor);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en Update para vendedor {CuentaVendedor}", solicitudTurnoCreate.CuentaVendedor);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    [HttpPost("Delete", Name = "ShiftRequestDelete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResult))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(SolicitudTurnoCreate solicitudTurnoCreate)
    {
      if (solicitudTurnoCreate is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      string? modelError = solicitudTurnoCreate.GetError();
      if (!string.IsNullOrWhiteSpace(modelError))
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Datos inválidos",
          Detail = modelError,
          Instance = HttpContext.Request.Path
        });

      try
      {
        await _solicitudTurnoService.DeleteRequest(solicitudTurnoCreate);
        return Ok(new { success = true, message = "Solicitud eliminada con éxito." });
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en Delete para vendedor {CuentaVendedor}", solicitudTurnoCreate.CuentaVendedor);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en Delete para vendedor {CuentaVendedor}", solicitudTurnoCreate.CuentaVendedor);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    /// <summary>
    /// Acepta una o varias solicitudes de turno distribuyendo los cupos provistos.
    /// Cada solicitud se procesa con concurrencia optimista: si ya no está
    /// pendiente, se reporta como conflicto individual sin afectar al resto.
    /// Devuelve un resultado discriminado con Asignados y Fallos.
    /// Si TODAS las operaciones fallan, retorna 409 Conflict.
    /// Si al menos una prospera, retorna 200 con el detalle discriminado.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Accept", Name = "ShiftRequestAccept")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShiftRequestAcceptResult))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ShiftRequestAcceptResult>> Accept([FromBodyAttribute] ShiftRequestAcceptData shiftRequestAcceptData)
    {
      if (shiftRequestAcceptData is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      if (shiftRequestAcceptData.ShiftRequest is null || !shiftRequestAcceptData.ShiftRequest.Any())
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Sin solicitudes",
          Detail = "La lista ShiftRequest no puede estar vacía.",
          Instance = HttpContext.Request.Path
        });

      if (shiftRequestAcceptData.CuposToBeDistributed is null || !shiftRequestAcceptData.CuposToBeDistributed.Any())
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Sin cupos",
          Detail = "La lista CuposToBeDistributed no puede estar vacía.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        ShiftRequestAcceptResult resultado = await _solicitudTurnoService.AcceptRequestsAsync(shiftRequestAcceptData);

        if (!resultado.TieneExitos)
          return NoContent();

        return Ok(resultado);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en Accept");
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en Accept");
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    /// <summary>
    /// Rechaza una o varias solicitudes de turno en estado Pendiente.
    /// Implementa rechazo manual (operador) y rechazo automático (job 20:00 hs).
    /// Cada ID se procesa con concurrencia optimista: si la solicitud ya no
    /// está pendiente, se reporta como fallo individual sin afectar al resto.
    /// Si TODAS las solicitudes del lote fallan, retorna 409 Conflict.
    /// Si al menos una se rechaza, retorna 200 con el detalle discriminado.
    /// </summary>
    /// <param name="shiftRequestRejectData">Lista de IDs a rechazar y metadatos.</param>
    /// <returns>Resultado con Rechazados y Fallos discriminados.</returns>
    [HttpPost("Reject", Name = "ShiftRequestReject")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShiftRequestRejectResult))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ShiftRequestRejectResult>> Reject([FromBody] ShiftRequestRejectData shiftRequestRejectData)
    {
      if (shiftRequestRejectData is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      if (shiftRequestRejectData.SolicitudIds is null || !shiftRequestRejectData.SolicitudIds.Any())
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Sin solicitudes",
          Detail = "La lista SolicitudIds no puede estar vacía.",
          Instance = HttpContext.Request.Path
        });

      if (shiftRequestRejectData.SolicitudIds.Any(id => id <= 0))
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "IDs inválidos",
          Detail = "Todos los IDs de solicitudes deben ser números positivos.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        ShiftRequestRejectResult resultado = await _solicitudTurnoService.RejectRequestsAsync(shiftRequestRejectData);

        if (!resultado.TieneExitos)
          return NoContent();

        return Ok(resultado);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en Reject");
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en Reject");
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    // ====================================================================
    // Endpoints auxiliares para Pantalla 2 (MVC) — soporte del flujo de Accept
    // ====================================================================

    /// <summary>
    /// Devuelve la entidad <see cref="SolicitudTurno"/> por id. Usado por el MVC
    /// (<c>AcceptPayloadBuilder</c>) para armar el payload de Accept sin
    /// duplicar estado en el cliente.
    /// </summary>
    /// <param name="id">Id de la solicitud.</param>
    /// <response code="200">Solicitud encontrada.</response>
    /// <response code="404">No existe la solicitud con ese id.</response>
    [HttpGet("{id:long}", Name = "ShiftRequestGetById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SolicitudTurno))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudTurno>> GetById(long id)
    {
      if (id <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Id inválido",
          Detail = "El id de la solicitud debe ser positivo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        var solicitud = await _solicitudTurnoService.GetByIdAsync(id);
        return solicitud == null
          ? NotFound(new ProblemDetails
          {
            Status = StatusCodes.Status404NotFound,
            Title = "Solicitud no encontrada",
            Detail = $"No existe la solicitud con id {id}.",
            Instance = HttpContext.Request.Path
          })
          : Ok(solicitud);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetById({Id})", id);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    /// <summary>
    /// Devuelve los cupos completos (entidad <see cref="Cupo"/>) por una lista de ids.
    /// Usado por el MVC para reconstruir los cupos seleccionados y armar el
    /// payload de Accept.
    /// </summary>
    /// <param name="ids">Lista de ids (positivos). Vacía/nula → 200 con lista vacía.</param>
    /// <response code="200">Cupos encontrados (puede ser menos que los solicitados si algunos ids no existen).</response>
    [HttpPost("Cupos/ByIds", Name = "CuposByIds")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Cupo>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<Cupo>>> GetCuposByIds([FromBody] List<long> ids)
    {
      if (ids == null)
        return Ok(new List<Cupo>());

      // Filtrar ids no positivos en defensa.
      var idsValidos = ids.Where(id => id > 0).Distinct().ToList();

      try
      {
        var cupos = await _solicitudTurnoService.GetCuposByIdsAsync(idsValidos);
        return Ok(cupos);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetCuposByIds. Solicitados: {Cantidad}", ids.Count);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    /// <summary>
    /// Devuelve un mapa <c>solicitudId → List&lt;cupoId&gt;</c> con todos los
    /// cupos ACEPTADOS para las solicitudes dadas. Lo consume la grilla
    /// (Pantalla 1) para descontar del conteo de matches del motor los cupos
    /// que ya fueron otorgados.
    /// </summary>
    /// <param name="solicitudIds">Ids de solicitudes a consultar.</param>
    /// <response code="200">Lista siempre (vacía si la entrada es nula).</response>
    [HttpPost("Cupos/Aceptados/PorSolicitudes", Name = "ShiftRequestCuposAceptadosPorSolicitudes")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CupoAceptadoPorSolicitudDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CupoAceptadoPorSolicitudDto>>> GetCuposAceptadosPorSolicitudesAsync(
      [FromBody] List<long> solicitudIds)
    {
      if (solicitudIds == null)
        return Ok(Enumerable.Empty<CupoAceptadoPorSolicitudDto>());

      try
      {
        var resultado = await _solicitudTurnoService.GetCuposAceptadosPorSolicitudesAsync(solicitudIds)
                        ?? new Dictionary<long, List<long>>();
        var respuesta = resultado
          .Select(kvp => new CupoAceptadoPorSolicitudDto
          {
            SolicitudId = kvp.Key,
            CupoIds = kvp.Value ?? new List<long>()
          })
          .ToList();
        return Ok(respuesta);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
          "Error inesperado en GetCuposAceptadosPorSolicitudesAsync. CantIds: {Cantidad}",
          solicitudIds?.Count ?? 0);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    /// <summary>
    /// Devuelve el resumen de aceptación para una solicitud, calculado a
    /// partir de los acumuladores de <c>SOLTURNOS</c>. Útil para que la UI
    /// (Pantalla 2 y la grilla Index) sepa cuántos cupos fueron asignados,
    /// cuántos quedaron pendientes y cuántos rechazados.
    /// </summary>
    /// <param name="id">Id de la solicitud.</param>
    /// <response code="200">Resumen siempre (0 en cada contador si no hay detalle).</response>
    [HttpGet("{id:long}/Pendientes", Name = "ShiftRequestPendientes")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DetalleEstadoResumen))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DetalleEstadoResumen>> GetPendientes(long id)
    {
      if (id <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Id inválido",
          Detail = "El id de la solicitud debe ser positivo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        var resumen = await _solicitudTurnoService.GetDetalleResumenAsync(id);
        return Ok(resumen);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en GetPendientes({Id})", id);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "Error inesperado",
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
    }

    [HttpPost("CuposDisponibles", Name = "ShiftRequestCuposDisponibles")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CuposDisponiblesPorGrano>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IList<CuposDisponiblesPorGrano>>> ShiftRequestCuposDisponibles([FromBody] FiltroDisponible filter)
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
        IEnumerable<CuposDisponiblesPorGrano> result = await _cuposDisponiblesService.GetCuposDisponiblesForShiftRequest(
          filter.cuentaVendedor,
          filter.fecha,
          filter.cuentaComprador,
          filter.codigoGrano,
          filter.zonaGeografica);

        if (result is null || !result.Any())
          return NoContent();

        return Ok(result.ToList());
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
    // POST /Matches
    // Bulk match: devuelve todos los pares (solicitud, cupo) compatibles
    // para los filtros dados, ya clasificados por el motor.
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("Matches", Name = "ShiftRequestMatches")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatchesResultDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MatchesResultDto>> Matches([FromBody] MatchesFilterDto filter)
    {
      if (filter is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        MatchesResultDto resultado = await _solicitudTurnoService.BuscarMatchesAsync(filter);

        if (resultado.Items is null || !resultado.Items.Any())
          return NoContent();

        return Ok(resultado);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en Matches");
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en Matches");
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
    // POST /MatchesVentana
    // Matching bulk por VENTANA. Devuelve los pares (solicitud, cupo)
    // compatibles de todas las solicitudes pendientes del rango en una sola
    // llamada, con un item "flaco" (sin solicitud ni cupo hidratados).
    //
    // Existe para la grilla de Pantalla 1, que antes hacía una llamada a
    // /Matches por cada fila: con R filas eso eran R requests HTTP y ~4R
    // queries a Oracle, repitiendo el mismo scan de cuposcorre. Acá es
    // 1 request y 2+G queries (G = granos distintos de la ventana).
    //
    // NO reemplaza ni modifica /Matches, que sigue sirviendo al detalle de
    // Pantalla 2 (necesita los datos hidratados de cada par).
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("MatchesVentana", Name = "ShiftRequestMatchesVentana")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatchesVentanaResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MatchesVentanaResultDto>> MatchesVentana([FromBody] MatchesVentanaFilterDto filter)
    {
      if (filter is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        MatchesVentanaResultDto resultado = await _solicitudTurnoService.BuscarMatchesVentanaAsync(filter);

        // A diferencia de /Matches, una ventana sin matches NO devuelve 204:
        // el consumidor necesita distinguir "no hay coincidencias" (que se
        // pinta como "Sin coincidencia" en cada fila) de "no hubo respuesta".
        // Devolver siempre 200 con Items vacío evita que el MVC tenga que
        // tratar un null como error.
        return Ok(resultado);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en MatchesVentana");
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en MatchesVentana");
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
    // POST /MatchesDistribucion
    // Variante de /Matches específica para la pantalla de Distribución
    // (Pantalla 3). Acepta también solicitudes cuyos campos opcionales
    // (comprador, destino) son NULL cuando el cupo los tiene poblados:
    // el motor los marca como Parcial.
    // NO reemplaza ni modifica /Matches, que sigue usándose desde Pantalla 2.
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("MatchesDistribucion", Name = "ShiftRequestMatchesDistribucion")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatchesResultDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MatchesResultDto>> MatchesDistribucion([FromBody] MatchesFilterDto filter)
    {
      if (filter is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        MatchesResultDto resultado = await _solicitudTurnoService.BuscarMatchesParaDistribucionAsync(filter);

        if (resultado.Items is null || !resultado.Items.Any())
          return NoContent();

        return Ok(resultado);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en MatchesDistribucion");
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en MatchesDistribucion");
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
    // POST /MatchesDistribucionV2
    // Variante V2 del matching de Distribución. Usa un INNER JOIN nativo
    // entre cuposcorre y SOLTURNOS con todos los filtros obligatorios
    // aplicados en SQL, y delega al motor ACA.Matching.Engine sólo la
    // clasificación Directo/Parcial/Condicional.
    //
    // NO reemplaza /MatchesDistribucion: ambos coexisten. Si /MatchesDistribucionV2
    // devuelve 4xx/5xx, el cliente (CuposMatchingController) puede caer al
    // endpoint legacy como defensa.
    //
    // Filtros obligatorios para este endpoint:
    //   • CodigoGrano (> 0)
    //   • CuentaComprador (> 0) — siempre obligatorio en Distribución
    //   • CuentaPuerto (> 0) — siempre obligatorio en Distribución
    //   • FechaDesde..FechaHasta (defaults: hoy..hoy+7)
    //   • CuentaVendedor: si > 0 match exacto; si 0, sólo cupos sin vendedor
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("MatchesDistribucionV2", Name = "ShiftRequestMatchesDistribucionV2")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatchesResultDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MatchesResultDto>> MatchesDistribucionV2([FromBody] MatchesFilterDto filter)
    {
      if (filter is null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Body requerido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        MatchesResultDto resultado = await _solicitudTurnoMatchingV2Service.BuscarMatchesV2Async(filter);

        if (resultado.Items is null || !resultado.Items.Any())
          return NoContent();

        return Ok(resultado);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en MatchesDistribucionV2");
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en MatchesDistribucionV2");
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
    // GET /MatchesPorCupo/{cupoId}
    // Devuelve las solicitudes compatibles con un cupo dado.
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("MatchesPorCupo/{cupoId:long}", Name = "ShiftRequestMatchesPorCupo")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MatchResultDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MatchResultDto>>> MatchesPorCupo(long cupoId)
    {
      if (cupoId <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "cupoId debe ser un número positivo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        // Cargar el cupo desde el store de cupos.
        // ISILCuposStore no expone FindById: usamos FindCuposByPeriod con un
        // rango amplio y filtramos en memoria. Es ineficiente pero evita
        // modificar ACA_Shared.
        var cupos = await _silCuposStore.FindCuposByPeriod(
          DateTime.Now.Date.AddYears(-1), DateTime.Now.Date.AddYears(1));
        var cupo = cupos.FirstOrDefault(c => c.Id == cupoId);

        if (cupo is null)
          return NotFound(new ProblemDetails
          {
            Status = StatusCodes.Status404NotFound,
            Detail = $"No se encontró el cupo con id {cupoId}.",
            Instance = HttpContext.Request.Path
          });

        // La fecha del match es obligatoria (grano + fecha). Un cupo sin fecha
        // no puede matchear contra ninguna solicitud: cortamos acá antes de
        // gastar la query de solicitudes.
        if (cupo.Fecha is null)
          return NoContent();

        // Cargar solicitudes pendientes compatibles con el cupo (filtro heurístico:
        // mismo grano, mismo vendedor; el motor hace el resto).
        var filterSolicitudes = new SolicitudTurnosFilter
        {
          CuentaVendedor = long.TryParse(cupo.CodVendSIL, out var vend) ? vend : 0,
          CuentaComprador = 0,
          CuentaDestino = 0,
          Desde = cupo.Fecha.Value.Date,
          Hasta = cupo.Fecha.Value.Date
        };

        var solicitudes = (await _solicitudTurnoService.GetByFilterAsync(filterSolicitudes))
          .Where(s => s.EsPendiente
                   && !s.EstadoCupo.HasValue
                   && s.CodigoGrano.ToString() == (cupo.CodGrano ?? string.Empty)
                   && s.FechaSolicitado.Date == cupo.Fecha.Value.Date)
          .ToList();

        var ctxZona = await _zonaGeograficaResolver.ResolverAsync(new[] { cupo.Id });
        var engine = HttpContext.RequestServices.GetService<ACA.Matching.Engine.IMatchingEngine>();
        if (engine is null)
          return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
          {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Motor de matching no registrado",
            Detail = "IMatchingEngine no está registrado en el contenedor de DI.",
            Instance = HttpContext.Request.Path
          });

        var results = new List<MatchResultDto>();
        foreach (var s in solicitudes)
        {
          var match = engine.Evaluar(cupo, SolicitudMatchingAdapter.From(s), ctxZona);
          if (match.Compatible)
            results.Add(MatchResultDto.From(cupo, SolicitudMatchingAdapter.From(s), match));
        }

        if (results.Count == 0)
          return NoContent();

        return Ok(results);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en MatchesPorCupo {CupoId}", cupoId);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en MatchesPorCupo {CupoId}", cupoId);
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
    // GET /MatchesPorSolicitud/{solicitudId}
    // Devuelve los cupos compatibles con una solicitud dada.
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("MatchesPorSolicitud/{solicitudId:long}", Name = "ShiftRequestMatchesPorSolicitud")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MatchResultDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MatchResultDto>>> MatchesPorSolicitud(long solicitudId)
    {
      if (solicitudId <= 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Parámetro inválido",
          Detail = "solicitudId debe ser un número positivo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        // Cargar la solicitud pendiente.
        var solicitudes = (await _solicitudTurnoService.GetAllAsync(
          DateTime.Now.Date.AddYears(-1), DateTime.Now.Date.AddYears(1)))
          .Where(s => s.Id == solicitudId
                   && s.EsPendiente
                   && !s.EstadoCupo.HasValue)
          .ToList();

        if (solicitudes.Count == 0)
          return NotFound(new ProblemDetails
          {
            Status = StatusCodes.Status404NotFound,
            Detail = $"No se encontró la solicitud pendiente con id {solicitudId}.",
            Instance = HttpContext.Request.Path
          });

        var solicitudView = solicitudes.First();
        var solicitudMatching = SolicitudMatchingAdapter.From(solicitudView);

        // Cargar cupos disponibles compatibles (filtro heurístico: mismo grano, mismo vendedor).
        // Usamos FindCuposByPeriod + filtro en memoria porque GetCuposDisponiblesForShiftRequest
        // devuelve agregados, no cupos individuales (el motor necesita Cupo entities).
        var cuposDisponibles = (await _silCuposStore.FindCuposByPeriod(
          solicitudView.FechaSolicitado.Date,
          solicitudView.FechaSolicitado.Date.AddDays(1)))
          .Where(c => c.CodGrano == solicitudView.CodigoGrano.ToString()
                   && c.CodVendSIL == solicitudView.CuentaVendedor.ToString())
          .ToList();

        if (cuposDisponibles.Count == 0)
          return NoContent();

        var ctxZona = await _zonaGeograficaResolver.ResolverAsync(cuposDisponibles.Select(c => c.Id));
        var engine = HttpContext.RequestServices.GetService<ACA.Matching.Engine.IMatchingEngine>();
        if (engine is null)
          return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
          {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Motor de matching no registrado",
            Detail = "IMatchingEngine no está registrado en el contenedor de DI.",
            Instance = HttpContext.Request.Path
          });

        var results = new List<MatchResultDto>();
        foreach (var c in cuposDisponibles)
        {
          var match = engine.Evaluar(c, solicitudMatching, ctxZona);
          if (match.Compatible)
            results.Add(MatchResultDto.From(c, solicitudMatching, match));
        }

        if (results.Count == 0)
          return NoContent();

        return Ok(results);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en MatchesPorSolicitud {SolicitudId}", solicitudId);
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en MatchesPorSolicitud {SolicitudId}", solicitudId);
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
    // POST /AnularDistribucion
    // Revierte la asociación cupo ↔ solicitud cuando el operador anula la
    // distribución de uno o más cupos desde Pantalla 2
    // (Views/Cupos/Editar.cshtml). Es complementario al Anular legacy de
    // CuposDataController:
    //   · Anular (legacy): marca los cupos en CUPOSCORRE como anulados.
    //   · AnularDistribucion (este): devuelve las solicitudes originales
    //     al estado Pendiente (decrementa CANTIDAD_ACEPTADA, incrementa
    //     CANTIDAD, borra la fila de SOLTURNOS_DETALLE).
    //
    // Los cupos del request que NO tengan solicitud asociada en
    // SOLTURNOS_DETALLE se reportan como Skipped (no se toca la BD): el
    // flujo legacy ya los cubrió en CUPOSCORRE y no requieren reversión
    // lógica.
    //
    // Body: { "cupoIds": [long, long, ...] }
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("AnularDistribucion", Name = "ShiftRequestAnularDistribucion")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AnularDistribucionResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AnularDistribucionResult>> AnularDistribucion([FromBody] AnularDistribucionRequest request)
    {
      if (request == null)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Cuerpo inválido",
          Detail = "El cuerpo de la solicitud no puede ser nulo.",
          Instance = HttpContext.Request.Path
        });

      if (request.CupoIds == null || request.CupoIds.Count == 0)
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Lista vacía",
          Detail = "cupoIds debe contener al menos un id de cupo.",
          Instance = HttpContext.Request.Path
        });

      try
      {
        var resultado = await _solicitudTurnoService.AnularDistribucionAsync(request.CupoIds);

        // 200 OK aunque todos los cupos hayan sido Skipped: el cuerpo trae
        // el desglose por item y la cantidad de cada estado. 409 sólo si
        // hay items Fallo (estado inconsistente de la solicitud).
        if (resultado.CantidadFallos > 0)
        {
          return Conflict(new ProblemDetails
          {
            Status = StatusCodes.Status409Conflict,
            Title = "Algunas distribuciones no pudieron revertirse",
            Detail = $"{resultado.CantidadFallos} cupo(s) no se pudieron revertir. " +
                     $"{resultado.CantidadExitosos} revertidos, {resultado.CantidadSkipped} sin distribución activa.",
            Instance = HttpContext.Request.Path
          });
        }

        return Ok(resultado);
      }
      catch (SilDataException ex)
      {
        _logger.LogWarning(ex, "Error de negocio en AnularDistribucion. CupoIds: {CupoIds}",
          string.Join(",", request.CupoIds));
        return Conflict(new ProblemDetails
        {
          Status = ex.StatusCode,
          Detail = ex.Message,
          Instance = HttpContext.Request.Path
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error inesperado en AnularDistribucion. CupoIds: {CupoIds}",
          string.Join(",", request.CupoIds));
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

using Comunicaciones.BusinessExceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shared.ClassShared.BusinessExceptions;

namespace Comunicaciones.Controllers
{
  public class ErrorController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }

    [HttpGet("Throw")]
    public IActionResult Throw() =>
    //throw new Exception("Sample exception.");
    throw new BusinessException(BusinessRulesCode.PersonAlreadyExists);

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/error-development")]
    public IActionResult HandleErrorDevelopment(
    [FromServices] IHostEnvironment hostEnvironment)
    {
      if (!hostEnvironment.IsDevelopment())
      {
        return NotFound();
      }

      var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

      return Problem(
          detail: exceptionHandlerFeature.Error.StackTrace,
          title: exceptionHandlerFeature.Error.Message);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/error")]
    public IActionResult HandleError() =>
    Problem();



  }
}

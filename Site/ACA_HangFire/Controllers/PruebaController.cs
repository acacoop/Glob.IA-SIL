using HangFire.Services;
using HangFire.Stores.Own;
using HangFire.Stores.Sil;
using Microsoft.AspNetCore.Mvc;

namespace HangFire.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class PruebaController : ControllerBase
  {
    ImportCuposService servicio;
    ISilCupoStore cupoSil_;
    private readonly ILogger<ImportCuposService> logger;

    public PruebaController(IOwnCupoStore cupoSql, ISilCupoStore cupoSil, ILogger<ImportCuposService> logger)
    {
      servicio = new ImportCuposService(cupoSql, cupoSil, logger);
      cupoSil_ = cupoSil;
      this.logger = logger;
    }

    [HttpGet("Cupos")]
    public async Task<int> GetCupos() 
    {
      DateTime desde = DateTime.ParseExact("2023-03-13 00:00:00,531", "yyyy-MM-dd HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture);
      DateTime hasta = DateTime.ParseExact("2023-03-13 00:00:00,531", "yyyy-MM-dd HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture);
      var b = servicio.ImportCupos(desde,hasta);
      return await b;
    }
    
  }
}

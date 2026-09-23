using Comunicaciones.DataRequest;
using Comunicaciones.Models.ListasContactos;
using Comunicaciones.Models.Personas;
using Comunicaciones.Services;
using Domain.Context;
using Domain.Entities.ListasContactos;
using Domain.Entities.Personas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.ClassShared.Types;
using Shared.StaticShared.Enumerators;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Comunicaciones.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize]
  public class ConfigurationController : ControllerBase
  {
    private readonly SQLDBContext _context;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(SQLDBContext context, ILogger<ConfigurationController> logger)
    {
      _context = context;
      _logger = logger;
    }
    
    #region Service
    internal ServicioConfiguracion<Rol> RolSvc
    {
      get
      {
        return new ServicioConfiguracion<Rol>(new RolStore(this._context));
      }
    }
    internal ServicioConfiguracion<Centro> CentroSvc
    {
      get
      {
        return new ServicioConfiguracion<Centro>(new CentroStore(this._context));
      }
    }
    internal ServicioConfiguracion<ZonaComercial> ZonaComercialSvc
    {
      get
      {
        return new ServicioConfiguracion<ZonaComercial>(new ZonaComercialStore(this._context));
      }
    }
    internal ServicioConfiguracion<TipoContacto> TipoContactolSvc
    {
      get
      {
        return new ServicioConfiguracion<TipoContacto>(new TipoContactoStore(this._context));
      }
    }
    #endregion

    #region GET
    // GET: api/<ConfigurationController>
    [HttpGet]
    public IEnumerable<string> Get()
    {
      return new string[] { "value1", "value2" };
    }

    [HttpGet("Rol")]
    public ActionResult<IEnumerable<Rol>> GetRol([FromQuery] QueryParameter? queryParameter = null)
    {
      if (string.IsNullOrEmpty(queryParameter?.q))
      {
        return Ok(this.RolSvc.GetObjectCollection());
      }
      else 
      {
        IEnumerable<Rol> roles = this.RolSvc.GetObjectCollection(new List<FilterRequest>(){
        new FilterRequest {
          Property = "Nombre",
          Value = queryParameter.q,
          OperatorOfComparison = TypesOfComparison.Contain
        },
        new FilterRequest {
          Property = "Descripcion",
          Value = queryParameter.q,
          OperatorOfComparison = TypesOfComparison.Contain
        }
        });
        return Ok(roles);
      }
    }

    [HttpGet("Centro")]
    public ActionResult<IEnumerable<Centro>> GetCentro([FromQuery] QueryParameter? queryParameter = null)
    {
      try
      {
        _logger.LogInformation(queryParameter?.q);
        if (string.IsNullOrEmpty(queryParameter?.q))
        {
          return Ok(this.CentroSvc.GetObjectCollection());
        }
        else
        {
          IEnumerable<Centro> centros = this.CentroSvc.GetObjectCollection(new List<FilterRequest>(){
        new FilterRequest {
          Property = "Nombre",
          Value = queryParameter.q,
          OperatorOfComparison = TypesOfComparison.Contain
        },
        new FilterRequest {
          Property = "Codigo",
          Value = queryParameter.q,
          OperatorOfComparison = TypesOfComparison.Contain
        }
        });
          return Ok(centros);
        }
      } 
      catch (Exception e)
      {
        _logger.LogError(e, e.Message);
        throw;
      }
    }

    [HttpGet("ZonaComercial")]
    public ActionResult<IEnumerable<ZonaComercial>> GetZonaComercial([FromQuery] QueryParameter? queryParameter = null)
    {
      if (string.IsNullOrEmpty(queryParameter?.q))
      {
        return Ok(this.ZonaComercialSvc.GetObjectCollection());
      }
      else
      {
        IEnumerable<ZonaComercial> zonas = this.ZonaComercialSvc.GetObjectCollection(new List<FilterRequest>(){
        new FilterRequest {
          Property = "Nombre",
          Value = queryParameter.q,
          OperatorOfComparison = TypesOfComparison.Contain
        },
        new FilterRequest {
          Property = "Descripcion",
          Value = queryParameter.q,
          OperatorOfComparison = TypesOfComparison.Contain
        }
        });
        return Ok(zonas);
      }
    }

    [HttpGet("TipoContacto")]
    public ActionResult<IEnumerable<TipoContacto>> GetTipoContacto([FromQuery] QueryParameter? queryParameter = null)
    {
      if (string.IsNullOrEmpty(queryParameter?.q))
      {
        return Ok(this.TipoContactolSvc.GetObjectCollection());
      }
      else
      {
        IEnumerable<TipoContacto> tipos = this.TipoContactolSvc.GetObjectCollection(new List<FilterRequest>(){
        new FilterRequest {
          Property = "Nombre",
          Value = queryParameter.q,
          OperatorOfComparison = TypesOfComparison.Contain
        },
        new FilterRequest {
          Property = "Descripcion",
          Value = queryParameter.q,
          OperatorOfComparison = TypesOfComparison.Contain
        }
        });
        return Ok(tipos);
      }
    }

    // GET api/<ConfigurationController>/5
    [HttpGet("Rol/{id:Guid}")]
    public ActionResult<Rol> GetRol(Guid id)
    {
      if (_context.Rol == null)
      {
        return Problem("El set de Roles es null.");
      }
      var myRol = this.RolSvc.GetObject(id);
     
      return Ok(myRol);
    }

    [HttpGet("Centro/{id:Guid}")]
    public ActionResult<Centro> GetCentro(Guid id)
    {
      if (_context.Centro == null)
      {
        return Problem("El set de Centros es null.");
      }
      var myentity = this.CentroSvc.GetObject(id);
      return Ok(myentity);
    }

    [HttpGet("ZonaComercial/{id:Guid}")]
    public ActionResult<ZonaComercial> GetZonaComercial(Guid id)
    {
      if (_context.ZonaComercial == null)
      {
        return Problem("El set de Zonas Comerciales es null.");
      }
      var myentity = this.ZonaComercialSvc.GetObject(id);

      return Ok(myentity);
    }

    [HttpGet("TipoContacto/{id:Guid}")]
    public ActionResult<TipoContacto> GetTipoContacto(Guid id)
    {
      if (_context.TipoContacto== null)
      {
        return Problem("El set de Tipos de Contactos es null.");
      }
      var myentity = this.TipoContactolSvc.GetObject(id);

      return Ok(myentity);
    }
    #endregion

    #region POST
    // POST api/<ConfigurationController>
    [HttpPost("Rol")]
    public ActionResult AddRol(Rol value)
    {
      if (_context.Rol == null)
      {
        return Problem("El set de Roles es null.");
      }
      Rol newEntity = this.RolSvc.AddObject(value);

      return CreatedAtAction("GetRol", new { id = newEntity.Id }, newEntity);
    }

    [HttpPost("Centro")]
    public ActionResult AddCentro(Centro value)
    {
      if (_context.Centro == null)
      {
        return Problem("El set de Centros es null.");
      }
      Centro newEntity = this.CentroSvc.AddObject(value);

      return CreatedAtAction("GetCentro", new { id = newEntity.Id }, newEntity);
    }

    [HttpPost("ZonaComercial")]
    public ActionResult AddZonaComercial(ZonaComercial value)
    {
      if (_context.ZonaComercial == null)
      {
        return Problem("El set de Zonas Comerciales es null.");
      }
      ZonaComercial newEntity = this.ZonaComercialSvc.AddObject(value);

      return CreatedAtAction("GetZonaComercial", new { id = newEntity.Id }, newEntity);
    }

    [HttpPost("TipoContacto")]
    public ActionResult AddTipoContacto(TipoContacto value)
    {
      if (_context.TipoContacto == null)
      {
        return Problem("El set de Tipos de Contactos es null.");
      }
      TipoContacto newEntity = this.TipoContactolSvc.AddObject(value);

      return CreatedAtAction("GetTipoContacto", new { id = newEntity.Id }, newEntity);
    }
    #endregion

    #region PUT
    // PUT api/<ConfigurationController>/5
    [HttpPut("Rol/{id}")]
    public ActionResult PutRol(Guid id, Rol value)
    {
      if (id != value.Id)
      {
        return BadRequest();
      }

      _context.Entry(value).State = EntityState.Modified;

      try
      {
        return Ok(this.RolSvc.ModifyObject(value));
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!RolExists(id))
        {
          return NoContent();
        }
        else
        {
          throw;
        }
      }
    }

    [HttpPut("Centro/{id}")]
    public ActionResult PutCentro(Guid id, Centro value)
    {
      if (id != value.Id)
      {
        return BadRequest();
      }

      _context.Entry(value).State = EntityState.Modified;

      try
      {
        return Ok(this.CentroSvc.ModifyObject(value));
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!CentroExists(id))
        {
          return NoContent();
        }
        else
        {
          throw;
        }
      }
    }

    [HttpPut("ZonaComercial/{id}")]
    public ActionResult PutZonaComercial(Guid id, ZonaComercial value)
    {
      if (id != value.Id)
      {
        return BadRequest();
      }

      _context.Entry(value).State = EntityState.Modified;

      try
      {
        return Ok(this.ZonaComercialSvc.ModifyObject(value));
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!ZonaComercialExists(id))
        {
          return NoContent();
        }
        else
        {
          throw;
        }
      }
    }

    [HttpPut("TipoContacto/{id}")]
    public ActionResult PutTipoContacto( Guid id, TipoContacto value)
    {
      if (id != value.Id)
      {
        return BadRequest();
      }

      _context.Entry(value).State = EntityState.Modified;

      try
      {
         return Ok(this.TipoContactolSvc.ModifyObject(value));
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!TipoContactoExists(id))
        {
          return NoContent();
        }
        else
        {
          throw;
        }
      }
    }
    #endregion  

    #region DELETE
    // DELETE api/<ConfigurationController>/5
    [HttpDelete("Rol/{id}")]
    public ActionResult DeleteRol(Guid id)
    {
      if (_context.Rol == null)
      {
        return Problem("El set de Roles es null.");
      }
      var entity = this.RolSvc.GetObject(id);
      if (entity == null)
      {
        return NoContent();
      }

      return Ok(this.RolSvc.DeleteObject(entity));
    }

    [HttpDelete("Centro/{id}")]
    public ActionResult DeleteCentro(Guid id)
    {
      if (_context.Centro == null)
      {
        return Problem("El set de Centros  es null.");
      }
      var entity = this.CentroSvc.GetObject(id);
      if (entity == null)
      {
        return NoContent();
      }

      return Ok(this.CentroSvc.DeleteObject(entity));      
    }

    [HttpDelete("ZonaComercial/{id}")]
    public ActionResult DeleteZonaComercial(Guid id)
    {
      if (_context.ZonaComercial == null)
      {
        return Problem("El set de Zonas Comercial es null.");
      }
      var entity = this.ZonaComercialSvc.GetObject(id);
      if (entity == null)
      {
        return NoContent();
      }

      return Ok(this.ZonaComercialSvc.DeleteObject(entity));
    }

    [HttpDelete("TipoContacto/{id}")]
    public ActionResult DeleteTipoContacto(Guid id)
    {
      if (_context.TipoContacto == null)
      {
        return Problem("El set de Tipo de Contacto es null.");
      }
      var entity = this.TipoContactolSvc.GetObject(id);
      if (entity == null)
      {
        return NoContent();
      }

      return Ok(this.TipoContactolSvc.DeleteObject(entity));
    }
    #endregion

    #region Private Support
    private bool RolExists(Guid id)
    {
      return (_context.Rol?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    private bool CentroExists(Guid id)
    {
      return (_context.Centro?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    private bool ZonaComercialExists(Guid id)
    {
      return (_context.ZonaComercial?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    private bool TipoContactoExists(Guid id)
    {
      return (_context.Contacto?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    #endregion
  }
}

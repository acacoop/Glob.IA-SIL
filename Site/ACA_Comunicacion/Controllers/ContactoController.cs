using AutoMapper;
using Comunicaciones.DataRequest;
using Comunicaciones.DataResponse;
using Comunicaciones.Services;
using Domain.Context;
using Domain.Entities.ListasContactos;
using Domain.Entities.Personas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.ClassShared.Types;
using Shared.StaticShared.Enumerators;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Comunicaciones.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize]
  public class ContactoController : ControllerBase
  {
    private readonly SQLDBContext _context;
    public readonly IMapper _mapper;

    public ContactoController(IMapper mapper, SQLDBContext context)
    {
      _mapper = mapper;
      _context = context;
    }

    #region Service
    internal ServicioContacto ContactoSvc
    {
      get
      {
        return new ServicioContacto(this._context);
      }
    }
    #endregion

    #region Get

    [HttpGet("Personas")]
    public ActionResult<IEnumerable<PersonaDTO>> GetPersonas([FromQuery] QueryParameter? queryParameter = null)
    {
      if (_context.Persona == null) return Problem("El set de Personas es null.");

      if (string.IsNullOrEmpty(queryParameter?.q))
      {
        List<PersonaDTO> personaOut = _mapper.Map<List<PersonaDTO>>(this.ContactoSvc.GetPersonaCollection());
        return Ok(personaOut);
      }
      else
      {
        ICollection<Persona> personas = this.ContactoSvc.GetPersonaCollection(queryParameter?.q);
        return Ok(_mapper.Map<ICollection<PersonaDTO>>(personas));
      }
    }

    [HttpGet("Persona/{id:Guid}")]
    public ActionResult<PersonaDTO> GetPersona(Guid id)
    {
      if (_context.Persona == null) return Problem("El set de Personas es null.");

      Persona persona = this.ContactoSvc.GetPersona(id);
      PersonaDTO respuesta = _mapper.Map<PersonaDTO>(persona);
      return Ok(respuesta);
    }

    [HttpGet("Listas")]
    public ActionResult<IEnumerable<ListaContactosDTO>> GetListas([FromQuery] QueryParameter? queryParameter = null)
    {
      ICollection<ListaContactos> contactos;
      if (queryParameter == null || string.IsNullOrEmpty(queryParameter.q))
      {
        contactos = this.ContactoSvc.GetListaContactosCollection();
      }
      else
      {
        contactos = this.ContactoSvc.GetListaContactosCollection(new List<FilterRequest>() { new FilterRequest { Property = "Nombre", Value = queryParameter?.q,
          OperatorOfComparison = TypesOfComparison.Contain } });
      }
      return Ok(_mapper.Map<ICollection<ListaContactosDTO>>(contactos));
    }

    [HttpGet("Lista/{id:Guid}")]
    public ActionResult<ListaContactosDTO> GetLista(Guid id)
    {
      if (_context.ListaContactos == null) return Problem("El set de Listas es null.");

      ListaContactos lista = this.ContactoSvc.GetListaContactos(id);
      ListaContactosDTO respuesta = _mapper.Map<ListaContactosDTO>(lista);
      return Ok(respuesta);
    }

    [HttpGet("Contactos")]
    public ActionResult<IEnumerable<AutocompleteDataDTO>> GetContactos([FromQuery] QueryParameter? queryParameter = null)
    {
      if (string.IsNullOrEmpty(queryParameter?.q)) return BadRequest();

      if (_context.ListaContactos == null) return Problem("El set de Listas es null.");
      if (_context.Contacto == null) return Problem("El set de Contactos es null.");

      ICollection<AutocompleteDataDTO> contactos = this.ContactoSvc.GetContactos(new List<FilterRequest>(){ 
         new FilterRequest { 
           Property = "Nombre", 
           Value = queryParameter.q, 
           OperatorOfComparison = TypesOfComparison.Contain
         },
        new FilterRequest { 
          Property = "Apellido", 
          Value = queryParameter.q, 
          OperatorOfComparison = TypesOfComparison.Contain
        }
      });
      return Ok(contactos);
    }

    [HttpGet("TiposDeComparacion")]
    public ActionResult<TypesOfComparison> GetTiposDeComparacion()
    {
      TypesOfComparison[] operaciones = (TypesOfComparison[])Enum.GetValues(typeof(TypesOfComparison));
      List<string> tipos = new List<string>();
      for (int index = 0; index < operaciones.Length; index++)
      {
        tipos.Add(Enum.GetName(operaciones[index]));
      }
      return Ok(tipos);
    }
    #endregion

    #region POST

    [HttpPost("Persona")]
    public ActionResult AddPersona(PersonaRequest value)
    {
      if (_context.Persona == null) return Problem("El set de Personas es null.");

      PersonaDTO newEntity = _mapper.Map<PersonaDTO>(this.ContactoSvc.AddPersona(value));
      return CreatedAtAction("GetPersona", new { id = newEntity.Id }, newEntity);
    }

    [HttpPost("Contacto/{id:Guid}")]
    public ActionResult AddContacto(Guid id, ContactoRequest value)
    {
      if (_context.Persona == null) return Problem("El set de Personas es null.");
      if (_context.Contacto == null) return Problem("El set de Contacto es null.");
      if (value == null) return NoContent();

      Contacto contactoOut = this.ContactoSvc.AddContacto(value);
      return CreatedAtAction("GetContacto", new { id = contactoOut.Id }, contactoOut);
    }

    [HttpPost("Lista")]
    public ActionResult AddLista(ListaContactosRequest value)
    {
      if (_context.ListaContactos == null) return Problem("El set de Listas es null.");
      if (value == null) return NoContent();

      ListaContactos myLista = this.ContactoSvc.AddListaContactos(value);
      if (myLista == null) return NoContent();

      ListaContactosDTO listaOut = _mapper.Map<ListaContactosDTO>(myLista);
      return CreatedAtAction("GetLista", new { id = listaOut.Id }, listaOut);
    }

    #endregion

    #region PUT
    [HttpPut("Persona/{id}")]
    public ActionResult PutPersona(Guid id, PersonaRequest value)
    {
      try
      {
        if (_context.Persona == null) return Problem("El set de Personas es null.");

        Persona personaOut = this.ContactoSvc.ModifyPersona(id, value);
        return Ok(_mapper.Map<PersonaDTO>(personaOut));
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!this.PersonaExist(id))
        {
          return NoContent();
        }
        else
        {
          throw;
        }
      }
    }

    [HttpPut("Lista/{id}")]
    public ActionResult PutLista(Guid id, ListaContactosRequest value)
    {
      try
      {
        if (_context.ListaContactos == null) return Problem("El set de Listas es null.");
        ListaContactos listaOut = this.ContactoSvc.ModifyListaContactos(id, value);
        return Ok(_mapper.Map<ListaContactosDTO>(listaOut));
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!this.ListaExist(id))
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

    // DELETE api/<ContactoController>/5

    [HttpDelete("Persona/{id}")]
    public ActionResult DeletePersona(Guid id)
    {
      if (_context.Persona == null) return Problem("El set de Persona es null.");
      if (!this.PersonaExist(id)) return NoContent();

      Persona entity = new Persona() { Id = id };
      return Ok(this.ContactoSvc.DeletePersona(entity));
    }

    [HttpDelete("Contacto/{id}")]
    public ActionResult DeleteContacto(Guid id)
    {
      if (_context.Contacto == null) return Problem("El set de Contactos es null.");
      if (!this.ContactoExist(id)) return NoContent();
      Contacto entity = new Contacto() { Id = id };
      return Ok(this.ContactoSvc.DeleteContacto(entity));
    }

    [HttpDelete("Listas/{id}")]
    public ActionResult DeleteLista(Guid id)
    {
      if (_context.ListaContactos == null) return Problem("El set de Listas es null.");
      if (!this.ListaExist(id)) return NoContent();
      ListaContactos entity = new ListaContactos() { Id = id };
      return Ok(this.ContactoSvc.DeleteListaContactos(entity));
    }
    #endregion

    #region Private Support

    private bool PersonaExist(Guid id)
    {
      return (_context.Persona?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    private bool ListaExist(Guid id)
    {
      return (_context.ListaContactos?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    private bool ContactoExist(Guid id)
    {
      return (_context.Contacto?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    #endregion
  }
}
using Comunicaciones.BusinessExceptions;
using Domain.Context;
using Domain.Entities.Personas;
using Shared.AccessShared;
using Shared.ClassShared.BusinessExceptions;
using Shared.ClassShared.Types;
using Shared.StaticShared;
using System.Reflection;

namespace Comunicaciones.Models.Personas
{
  public class PersonaStore : DomainStore, IEntityStore<Persona>
  {
    public PersonaStore(SQLDBContext context)
      : base(context)
    {
    }
    private Persona NewPersona(Persona personaIn)
    {
      Persona pers = new()
      {
        Apellido = personaIn.Apellido,
        Nombre = personaIn.Nombre,
        Usuario = personaIn.Usuario,
        Rol = personaIn.Rol,
        ZonaComercial = personaIn.ZonaComercial,
        Centro = personaIn.Centro,
        Cuenta = personaIn.Cuenta,
      };
      pers.GenerateNewIdentity();
      return pers;
    }

    public Persona AddEntity(Persona entityToAdd)
    {
      entityToAdd.GenerateNewIdentity();
      if (this.GetEntity(entityToAdd.Id) != null)
      {
        throw new BusinessException(BusinessRulesCode.PersonAlreadyExists);
      }
      this.Context.Persona.Add(entityToAdd);
      this.Context.SaveChanges();
      return entityToAdd;
    }

    public Persona GetEntity(Guid Id)
    {
      if ((Guid)Id != Guid.Empty)
      {
        IQueryable<Persona> a = Context.Persona.Where(p => p.Id == Id);
        if (!a.Any())
        {
          //throw new BusinessException(BusinessRulesCode.PersonNotExists);
          return null;
        }
        else 
        {
          return a.FirstOrDefault();
        }
      }
      throw new ArgumentException("Identificador Invalido");
    }

    /// <summary>
    /// Por defecto compara string
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Persona GetEntity(string property, string value)
    {
      PropertyInfo? columna = typeof(Persona).GetProperty(property);
      if (columna != null)
      {
        string value1 = value;
        return Context.Persona.Where(predicate: StaticOperations.PropertyEquals<Persona, string>(property: columna, value1)).FirstOrDefault();
      }

      throw new ArgumentException("La columna indicada no corresponde a la entidad Persona");
    }

    public ICollection<Persona> GetEntity()
    {
      return this.Context.Persona.ToList();
    }

    public ICollection<Persona> GetEntityContainsName(string personName)
    {
      if (string.IsNullOrEmpty(personName)) throw new ArgumentNullException("Invalid Name");
      return this.Context.Persona.Where(x => x.Nombre.Contains(personName)).ToList();
    }

    public Persona ModifyEntity(Persona entityToModify)
    {
      Persona entidad;
      if ((Guid)entityToModify.Id != Guid.Empty)
      {
        entidad = this.GetEntity(entityToModify.Id);
        if (entidad != null)
        {
          entidad = entityToModify;
          this.Context.SaveChanges();
          return entityToModify;
        }
      }
      throw new BusinessException(BusinessRulesCode.PersonNotExists);
    }

    public bool RemoveEntity(Persona entityToRemove)
    {
      if (entityToRemove != null && entityToRemove.Id != Guid.Empty)
      {
        Persona personToDelete = this.GetEntity(entityToRemove.Id);
        this.Context.Persona.Remove(personToDelete);
        this.Context.SaveChanges();
        return true;
      }
      throw new BusinessException(BusinessRulesCode.PersonNotExists);

    }

    public IQueryable<Persona> GetEntity(FilterRequest filter)
    {
      if (string.IsNullOrEmpty(filter.Property)) throw new ArgumentException("La columna indicada no corresponde a la entidad Persona");
      PropertyInfo? column = typeof(Persona).GetProperty(filter.Property);
      if (column == null) throw new ArgumentException("La columna indicada no corresponde a la entidad Persona");
      if (string.IsNullOrEmpty(filter.Value)) throw new ArgumentException("El valor indicado debe ser distinto de null");
        
      return Context.Persona.Where(StaticOperations.QueryableOfComparison<Persona, string>(column, filter.Value, filter.OperatorOfComparison));
    }

    public IList<Persona> GetEntitys(IList<Guid> Ids)
    {
      return this.Context.Persona.Where(persona => Ids.Any(id => id.Equals(persona.Id))).ToList();
    }
  }
}

using Comunicaciones.BusinessExceptions;
using Domain.Context;
using Domain.Entities.ListasContactos;
using Shared.AccessShared;
using Shared.ClassShared.BusinessExceptions;
using Shared.ClassShared.Types;
using Shared.StaticShared;
using System.Reflection;

namespace Comunicaciones.Models.ListasContactos
{
  public class ContactoStore : DomainStore, IEntityStore<Contacto>
  {
    public ContactoStore(SQLDBContext context) : base(context)
    {
    }
    public Contacto AddEntity(Contacto entityToAdd)
    {
      entityToAdd.GenerateNewIdentity();
      if (this.GetEntity(entityToAdd.Id) != null)
      {
        throw new BusinessException(BusinessRulesCode.ContactoAlreadyExists);
      }
      this.Context.Contacto.Add(entityToAdd);
      this.Context.SaveChanges();
      return entityToAdd;
    }

    public Contacto GetEntity(Guid Id)
    {
      if ((Guid)Id != Guid.Empty)
      {
        IQueryable<Contacto> a = Context.Contacto.Where(p => p.Id == Id);
        if (!a.Any())
        {
          //throw new BusinessException(BusinessRulesCode.ContactoNotExists);
          return null;
        }
        else
        {
          return a.FirstOrDefault();
        }
      }
      throw new ArgumentException("Identificador Invalido");
    }

    public Contacto GetEntity(string property, string value)
    {
      PropertyInfo? columna = typeof(Contacto).GetProperty(property);
      if (columna != null)
      {
        string value1 = value;
        return Context.Contacto.Where(predicate: StaticOperations.PropertyEquals<Contacto, string>(property: columna, value1)).FirstOrDefault();
      }

      throw new ArgumentException("La columna indicada no corresponde a la entidad Contacto");
    }

    public IList<Contacto> GetEntityContainsPersonName(string personName)
    {
      if (string.IsNullOrEmpty(personName)) throw new ArgumentNullException("Invalid Name");
      return Context.Contacto.Where(x => x.Persona.Nombre.Contains(personName)).ToList();
    }

    public ICollection<Contacto> GetEntityByPersonId(Guid value)
    {
      if ((Guid)value != Guid.Empty)
      {
        return Context.Contacto.Where(p => p.Persona.Id == value).ToList<Contacto>();
      }
      throw new ArgumentException("Identificador Invalido");
    }

    public ICollection<Contacto> GetEntitys(ICollection<Guid> values)
    {
      if (values!=null && values.Count>0)
      {
        return Context.Contacto.Where(p => values.Contains(p.Id)).ToList<Contacto>();
      }
      return new List<Contacto>();
    }

    public ICollection<Contacto> GetEntity()
    {
      return this.Context.Contacto.ToList();
    }

    public Contacto ModifyEntity(Contacto entityToModify)
    {
      Contacto entidad;
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
      throw new BusinessException(BusinessRulesCode.ContactoNotExists);
    }

    public bool RemoveEntity(Contacto entityToRemove)
    {
      if (entityToRemove != null && entityToRemove.Id != Guid.Empty)
      {
        Contacto contactoToDelete = this.GetEntity(entityToRemove.Id);
        this.Context.Contacto.Remove(contactoToDelete);
        this.Context.SaveChanges();
        return true;
      }
      throw new BusinessException(BusinessRulesCode.ContactoNotExists);
    }

    public IQueryable<Contacto> GetEntity(FilterRequest filter)
    {
      if (string.IsNullOrEmpty(filter.Property)) throw new ArgumentException("La columna indicada no corresponde a la entidad Contacto");
      PropertyInfo? column = typeof(Contacto).GetProperty(filter.Property);
      if (column == null) throw new ArgumentException("La columna indicada no corresponde a la entidad Contacto");
      if (string.IsNullOrEmpty(filter.Value)) throw new ArgumentException("El valor indicado debe ser distinto de null");

      return Context.Contacto.Where(StaticOperations.QueryableOfComparison<Contacto, string>(column, filter.Value, filter.OperatorOfComparison));
    }
  }
}

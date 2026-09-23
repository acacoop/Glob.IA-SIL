
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
  public class RolStore : DomainStore, IEntityStore<Rol>
  {
    public RolStore(SQLDBContext context) : base(context){}

    public Rol AddEntity(Rol entityToAdd)
    {
      entityToAdd.GenerateNewIdentity();
      if (this.GetEntity(entityToAdd.Id) != null)
      {
        throw new BusinessException(BusinessRulesCode.RolAlreadyExists);
      }
      this.Context.Rol.Add(entityToAdd);
      this.Context.SaveChanges();
      return entityToAdd;
    }

    public Rol GetEntity(Guid Id)
    {
      if ((Guid)Id != Guid.Empty) 
      {
        IQueryable<Rol> a = Context.Rol.Where(p => p.Id == Id);
        if (!a.Any())
        {
          //throw new BusinessException(BusinessRulesCode.RolNotExists);
          return null;
        }
        else
        {
          return a.FirstOrDefault();
        }
      }
      throw new ArgumentException("Identificador Invalido");
    }

    public Rol GetEntity(string property, string value)
    {
      PropertyInfo? columna = typeof(Persona).GetProperty(property);
      if (columna != null)
      {
        string value1 = value;
        return Context.Rol.Where(StaticOperations.PropertyEquals<Rol, string>(columna, value1)).FirstOrDefault();
      }

      throw new ArgumentException("La columna indicada no corresponde a la entidad Rol");
    }

    public ICollection<Rol> GetEntity()
    {
      return this.Context.Rol.ToList();
    }

    public Rol ModifyEntity(Rol entityToModify)
    {
      if (this.GetEntity(entityToModify.Id)==null) throw new BusinessException(BusinessRulesCode.RolNotExists);
      this.Context.Update(entityToModify);
      this.Context.SaveChanges();
      return entityToModify;
    }

    public bool RemoveEntity(Rol entityToRemove)
    {
      if (entityToRemove != null && entityToRemove.Id != Guid.Empty)
      {
        Rol rolToDelete = this.GetEntity(entityToRemove.Id);
        this.Context.Rol.Remove(rolToDelete);
        this.Context.SaveChanges();
        return true;
      }
      throw new BusinessException(BusinessRulesCode.RolNotExists); 
    }

    public IQueryable<Rol> GetEntity(FilterRequest filter)
    {
      if (string.IsNullOrEmpty(filter.Property)) throw new ArgumentException("La columna indicada no corresponde a la entidad Rol");
      PropertyInfo? column = typeof(Rol).GetProperty(filter.Property);
      if (column == null) throw new ArgumentException("La columna indicada no corresponde a la entidad Rol");
      if (string.IsNullOrEmpty(filter.Value)) throw new ArgumentException("El valor indicado debe ser distinto de null");

      return Context.Rol.Where(StaticOperations.QueryableOfComparison<Rol, string>(column, filter.Value, filter.OperatorOfComparison));
    }
  }
}

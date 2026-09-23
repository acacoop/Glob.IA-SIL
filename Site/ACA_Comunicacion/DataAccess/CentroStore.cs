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
  public class CentroStore : DomainStore, IEntityStore<Centro>
  {
    public CentroStore(SQLDBContext context) : base(context) {   }

    public Centro AddEntity(Centro entityToAdd)
    {
      entityToAdd.GenerateNewIdentity();
      if (this.GetEntity(entityToAdd.Id) != null)
      {
        throw new BusinessException(BusinessRulesCode.CentroAlreadyExists);
      }
      this.Context.Centro.Add(entityToAdd);
      this.Context.SaveChanges();
      return entityToAdd;
    }

    public Centro GetEntity(Guid Id)
    {
      if ((Guid)Id != Guid.Empty)
      {
        IQueryable<Centro> a = Context.Centro.Where(p => p.Id == Id);
        if (!a.Any())
        {
          //throw new BusinessException(BusinessRulesCode.CentroNotExists);
          return null;  
        }
        else
        {
          return a.FirstOrDefault();
        }
      }
      throw new ArgumentException("Identificador Invalido");
    }

    public Centro GetEntity(string property, string value)
    {
      PropertyInfo? columna = typeof(Centro).GetProperty(property);
      if (columna != null)
      {
        string value1 = value;
        return Context.Centro.Where(predicate: StaticOperations.PropertyEquals<Centro, string>(property: columna, value1)).FirstOrDefault();
      }

      throw new ArgumentException("La columna indicada no corresponde a la entidad Centro");  
    }

    public ICollection<Centro> GetEntity()
    {
      return this.Context.Centro.ToList();
    }

    public Centro ModifyEntity(Centro entityToModify)
    {
      Centro entidad;
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
      throw new BusinessException(BusinessRulesCode.CentroNotExists);
    }

    public bool RemoveEntity(Centro entityToRemove)
    {
      if (entityToRemove != null && entityToRemove.Id != Guid.Empty)
      {
        Centro centroToDelete = this.GetEntity(entityToRemove.Id);
        this.Context.Centro.Remove(centroToDelete);
        this.Context.SaveChanges();
        return true;
      }
      throw new BusinessException(BusinessRulesCode.CentroNotExists);
    }

    public IQueryable<Centro> GetEntity(FilterRequest filter)
    {
      if (string.IsNullOrEmpty(filter.Property)) throw new ArgumentException("La columna indicada no corresponde a la entidad Centro");
      PropertyInfo? column = typeof(Centro).GetProperty(filter.Property);
      if (column == null) throw new ArgumentException("La columna indicada no corresponde a la entidad Centro");
      if (string.IsNullOrEmpty(filter.Value)) throw new ArgumentException("El valor indicado debe ser distinto de null");

      return Context.Centro.Where(StaticOperations.QueryableOfComparison<Centro, string>(column, filter.Value, filter.OperatorOfComparison));
    }
  }
}

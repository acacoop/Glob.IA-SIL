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
  public class CuentaStore : DomainStore, IEntityStore<Cuenta>
  {
    public CuentaStore(SQLDBContext context) : base(context) { }

    public Cuenta AddEntity(Cuenta entityToAdd)
    {
      throw new NotImplementedException();
    }

    public Cuenta GetEntity(Guid Id)
    {
      throw new NotImplementedException();
    }
    public Cuenta GetEntity(string property, string value)
    {
      PropertyInfo? columna = typeof(Cuenta).GetProperty(property);
      if (columna != null)
      {
        string value1 = value;
        return Context.Cuenta.Where(predicate: StaticOperations.PropertyEquals<Cuenta, string>(property: columna, value1)).FirstOrDefault();
      }

      throw new ArgumentException("La columna indicada no corresponde a la entidad Cuenta");
    }

    public ICollection<Cuenta> GetEntity()
    {
      return this.Context.Cuenta.ToList();
    }

    public Cuenta ModifyEntity(Cuenta entityToModify)
    {
      Cuenta entidad;
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
      throw new BusinessException(BusinessRulesCode.CuentaNotExists);
    }

    public bool RemoveEntity(Cuenta entityToRemove)
    {
      if (entityToRemove != null && entityToRemove.Id != Guid.Empty)
      {
        Cuenta cuentaToDelete = this.GetEntity(entityToRemove.Id);
        this.Context.Cuenta.Remove(cuentaToDelete);
        this.Context.SaveChanges();
        return true;
      }
      throw new BusinessException(BusinessRulesCode.CuentaNotExists);
    }

    public IQueryable<Cuenta> GetEntity(FilterRequest filter)
    {
      if (string.IsNullOrEmpty(filter.Property)) throw new ArgumentException("La columna indicada no corresponde a la entidad Cuenta");
      PropertyInfo? column = typeof(Cuenta).GetProperty(filter.Property);
      if (column == null) throw new ArgumentException("La columna indicada no corresponde a la entidad Cuenta");
      if (string.IsNullOrEmpty(filter.Value)) throw new ArgumentException("El valor indicado debe ser distinto de null");

      return Context.Cuenta.Where(StaticOperations.QueryableOfComparison<Cuenta, string>(column, filter.Value, filter.OperatorOfComparison));
    }
  }
}

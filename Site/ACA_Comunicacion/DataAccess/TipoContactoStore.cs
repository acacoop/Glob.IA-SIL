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
  public class TipoContactoStore : DomainStore, IEntityStore<TipoContacto>
  {
    public TipoContactoStore(SQLDBContext context) : base(context) { }

    private TipoContacto NewTipoContacto(TipoContacto tipoContactoIn)
    {
      TipoContacto tipoContactoOut = new()
      {
        Id = tipoContactoIn.Id,
        Nombre = tipoContactoIn.Nombre,
        Descripcion= tipoContactoIn.Descripcion,
      };
      tipoContactoOut.GenerateNewIdentity();
      return tipoContactoOut;
    }

    public TipoContacto AddEntity(TipoContacto entityToAdd)
    {
      entityToAdd.GenerateNewIdentity();
      if (this.GetEntity(entityToAdd.Id) != null)
      {
        throw new BusinessException(BusinessRulesCode.TipoContactoAlreadyExists);
      }
      this.Context.TipoContacto.Add(entityToAdd);
      this.Context.SaveChanges();
      return entityToAdd;
    }

    public TipoContacto GetEntity(Guid Id)
    {
      if ((Guid)Id != Guid.Empty)
      {
        IQueryable<TipoContacto> a = Context.TipoContacto.Where(p => p.Id == Id);
        if (!a.Any())
        {
          //throw new BusinessException(BusinessRulesCode.TipoContactoNotExists);
          return null;
        }
        else
        {
          return a.FirstOrDefault();
        }
      }
      throw new ArgumentException("Identificador Invalido");
    }

    public TipoContacto GetEntity(string property, string value)
    {
      PropertyInfo? columna = typeof(TipoContacto).GetProperty(property);
      if (columna != null)
      {
        string value1 = value;
        return Context.TipoContacto.Where(predicate: StaticOperations.PropertyEquals<TipoContacto, string>(property: columna, value1)).FirstOrDefault();
      }

      throw new ArgumentException("La columna indicada no corresponde a la entidad Tipo Contacto");
    }

    public ICollection<TipoContacto> GetEntity()
    {
      return this.Context.TipoContacto.ToList();
    }

    public TipoContacto ModifyEntity(TipoContacto entityToModify)
    {
      TipoContacto entidad;
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
      throw new BusinessException(BusinessRulesCode.TipoContactoNotExists);
    }

    public bool RemoveEntity(TipoContacto entityToRemove)
    {

      if (entityToRemove != null && entityToRemove.Id != Guid.Empty)
      {
        TipoContacto tipoToDelete = this.GetEntity(entityToRemove.Id);
        this.Context.TipoContacto.Remove(tipoToDelete);
        this.Context.SaveChanges();
        return true;
      }
      throw new BusinessException(BusinessRulesCode.TipoContactoNotExists);
    }

    public IQueryable<TipoContacto> GetEntity(FilterRequest filter)
    {
      if (string.IsNullOrEmpty(filter.Property)) throw new ArgumentException("La columna indicada no corresponde a la entidad TipoContacto");
      PropertyInfo? column = typeof(TipoContacto).GetProperty(filter.Property);
      if (column == null) throw new ArgumentException("La columna indicada no corresponde a la entidad TipoContacto");
      if (string.IsNullOrEmpty(filter.Value)) throw new ArgumentException("El valor indicado debe ser distinto de null");

      return Context.TipoContacto.Where(StaticOperations.QueryableOfComparison<TipoContacto, string>(column, filter.Value, filter.OperatorOfComparison));
    }
  }
}


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
  public class ListaContactosStore : DomainStore, IEntityStore<ListaContactos>
  {
    public ListaContactosStore(SQLDBContext context) : base(context)
    {
    }

    public ListaContactos AddEntity(ListaContactos entityToAdd)
    {
      entityToAdd.GenerateNewIdentity();
      if (this.GetEntity(entityToAdd.Id) != null)
      {
        throw new BusinessException(BusinessRulesCode.ListaContactoAlreadyExists);
      }
      this.Context.ListaContactos.Add(entityToAdd);
      this.Context.SaveChanges();
      return entityToAdd;
    }

    public ListaContactos GetEntity(Guid Id)
    {
      if ((Guid)Id != Guid.Empty)
      {
        IQueryable<ListaContactos> a = Context.ListaContactos.Where(p => p.Id == Id);
        if (!a.Any())
        {
          //throw new BusinessException(BusinessRulesCode.ListaContactoNotExists);
          return null;
        }
        else
        {
          return a.FirstOrDefault();
        }
      }
      throw new ArgumentException("Identificador Invalido");
    }

    public ListaContactos GetEntity(string property, string value)
    {
      PropertyInfo? columna = typeof(ListaContactos).GetProperty(property);
      if (columna != null)
      {
        string value1 = value;
        return Context.ListaContactos.Where(predicate: StaticOperations.PropertyEquals<ListaContactos, string>(property: columna, value1)).FirstOrDefault();
      }

      throw new ArgumentException("La columna indicada no corresponde a la entidad ListaContactos");
    }

    public ICollection<ListaContactos> GetEntity()
    {
      return this.Context.ListaContactos.ToList();
    }

    public IQueryable<ListaContactos> GetEntity(FilterRequest filter)
    {
      if (string.IsNullOrEmpty(filter.Property)) throw new ArgumentException("La columna indicada no corresponde a la entidad ListaContactos");
      PropertyInfo? column = typeof(ListaContactos).GetProperty(filter.Property);
      if (column == null) throw new ArgumentException("La columna indicada no corresponde a la entidad ListaContactos");
      if (string.IsNullOrEmpty(filter.Value)) throw new ArgumentException("El valor indicado debe ser distinto de null");

      return Context.ListaContactos.Where(StaticOperations.QueryableOfComparison<ListaContactos, string>(column, filter.Value, filter.OperatorOfComparison));      
    }

    public ICollection<ListaContactos> GetEntitys(ICollection<Guid> values)
    {
      if (values != null && values.Count > 0)
      {
        return Context.ListaContactos.Where(p => values.Contains(p.Id)).ToList<ListaContactos>();
      }
      return new List<ListaContactos>();

    }
    public ListaContactos ModifyEntity(ListaContactos entityToModify)
    {
      ListaContactos entidad;
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
      throw new BusinessException(BusinessRulesCode.ListaContactoNotExists);
    }

    public bool RemoveEntity(ListaContactos entityToRemove)
    {
      if (entityToRemove != null && entityToRemove.Id != Guid.Empty)
      {
        ListaContactos listaToDelete = this.GetEntity(entityToRemove.Id);
        this.Context.ListaContactos.Remove(listaToDelete);
        this.Context.SaveChanges();
        return true;
      }
      throw new BusinessException(BusinessRulesCode.ListaContactoNotExists);
    }
  }
}

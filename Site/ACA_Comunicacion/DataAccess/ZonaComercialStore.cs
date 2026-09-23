
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
  public class ZonaComercialStore : DomainStore, IEntityStore<ZonaComercial>
  {
    public ZonaComercialStore(SQLDBContext context) : base(context)  {  }


    private ZonaComercial NewZonaComercial(ZonaComercial zonaComercialIn)
    {
      ZonaComercial zonaComercialOut = new()
      {
        Nombre = zonaComercialIn.Nombre,
        Descripcion = zonaComercialIn.Descripcion,
      };
      zonaComercialOut.GenerateNewIdentity();
      return zonaComercialOut;
    }

    public ZonaComercial AddEntity(ZonaComercial entityToAdd)
    {
      entityToAdd.GenerateNewIdentity();
      if (this.GetEntity(entityToAdd.Id) != null)
      {
        throw new BusinessException(BusinessRulesCode.ZonaComercialAlreadyExists);
      }
      this.Context.ZonaComercial.Add(entityToAdd);
      this.Context.SaveChanges();
      return entityToAdd;
    }

    public ZonaComercial GetEntity(Guid Id)
    {
      if ((Guid)Id != Guid.Empty)
      {
        IQueryable<ZonaComercial> a = Context.ZonaComercial.Where(p => p.Id == Id);
        if (!a.Any())
        {
          //throw new BusinessException(BusinessRulesCode.ZonaComercialNotExists);
          return null;  
        }
        else
        {
          return a.FirstOrDefault();
        }

      }
      throw new ArgumentException("Identificador Invalido"); 
    }

    public ZonaComercial GetEntity(string property, string value)
    {
      PropertyInfo? columna = typeof(ZonaComercial).GetProperty(property);
      if (columna != null) 
      {
        string value1 = value;
        return Context.ZonaComercial.Where(predicate: StaticOperations.PropertyEquals<ZonaComercial, string>(property: columna, value1)).FirstOrDefault();
      }
      throw new ArgumentException("La columna indicada no corresponde a la entidad Zona Comercial");
    }

    public ICollection<ZonaComercial> GetEntity()
    {
      return Context.ZonaComercial.ToList();
    }

    public ZonaComercial ModifyEntity(ZonaComercial entityToModify)
    {
      ZonaComercial zona;
      if ((Guid)entityToModify.Id != Guid.Empty) 
      {
        zona = this.GetEntity(entityToModify.Id);
        if (zona != null) 
        {
          this.Context.Update<ZonaComercial>(entityToModify);
          this.Context.SaveChanges();
          return entityToModify;
        }
      }
      throw new BusinessException(BusinessRulesCode.ZonaComercialNotExists);
    }

    public bool RemoveEntity(ZonaComercial entityToRemove)
    {
      if (entityToRemove != null && entityToRemove.Id != Guid.Empty)
      {
        ZonaComercial zonaToDelete = this.GetEntity(entityToRemove.Id);
        this.Context.ZonaComercial.Remove(zonaToDelete);
        this.Context.SaveChanges();
        return true;
      }
      throw new BusinessException(BusinessRulesCode.ZonaComercialNotExists);
    }

    public IQueryable<ZonaComercial> GetEntity(FilterRequest filter)
    {
      if (string.IsNullOrEmpty(filter.Property)) throw new ArgumentException("La columna indicada no corresponde a la entidad Zona Comercial");
      PropertyInfo? column = typeof(ZonaComercial).GetProperty(filter.Property);
      if (column == null) throw new ArgumentException("La columna indicada no corresponde a la entidad Zona Comercial");
      if (string.IsNullOrEmpty(filter.Value)) throw new ArgumentException("El valor indicado debe ser distinto de null");

      return Context.ZonaComercial.Where(StaticOperations.QueryableOfComparison<ZonaComercial, string>(column, filter.Value, filter.OperatorOfComparison));
    }
  }
}

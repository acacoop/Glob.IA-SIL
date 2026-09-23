using Shared.AccessShared;
using Shared.ClassShared.Types;
using Shared.StaticShared.Enumerators;

namespace Comunicaciones.Services
{
  public class ServicioConfiguracion<T> where T : class
  {
    private IEntityStore<T> storeSvc;

    public ServicioConfiguracion(IEntityStore<T> entityStore)
    {
      this.storeSvc = entityStore;
    }

    public void SetEntityStore(IEntityStore<T> entityStore)
    {
      this.storeSvc = entityStore;
    }

    public T GetObject(Guid Id)
    {
      return this.storeSvc.GetEntity(Id);
    }

    public ICollection<T> GetObjectCollection(List<FilterRequest>? filters = null)
    {
      if (filters != null) 
      { 
        HashSet<T> collection = new HashSet<T>();
        filters.ForEach(x => collection.UnionWith(this.storeSvc.GetEntity(x).ToHashSet()));        
        //foreach (FilterRequest filter in filters) 
        //{
        //  collection.UnionWith(this.storeSvc.GetEntity(filter).ToHashSet());
        //}
        return collection;
      } 
      else 
      {
        return this.storeSvc.GetEntity();
      }
    }

    public T AddObject(T entityToAdd) 
    {
      return this.storeSvc.AddEntity(entityToAdd);
    }

    public bool DeleteObject(T entityToRemove) 
    {
      return (entityToRemove != null) ? this.storeSvc.RemoveEntity(entityToRemove) : false;
    }

    public T ModifyObject(T entityToModify) 
    {
      return this.storeSvc.ModifyEntity(entityToModify);
    }

  }
}

using Shared.ClassShared.Types;

namespace Shared.AccessShared
{           /*Respository*/
  public interface IEntityStore<T> where T : class
  {
    T GetEntity(Guid Id);
    T GetEntity(string property, string value);
    IQueryable<T> GetEntity(FilterRequest filter);
    ICollection<T> GetEntity();
    T AddEntity(T entityToAdd);
    bool RemoveEntity(T entityToRemove);
    T ModifyEntity(T entityToModify);

  }
}

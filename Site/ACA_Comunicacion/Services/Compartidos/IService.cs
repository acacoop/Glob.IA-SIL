namespace Comunicaciones.Services.Compartidos
{
  public interface IService<T> where T : class
  {
    T GetObject(Guid Id);
    ICollection<T> GetObjectCollection();
    T AddObject(T entityToAdd);
    bool DeleteObject(T entityToRemove);
    T ModifyObject(T entityToModify);

  }
}

namespace Comunicaciones.DataResponse.Entity
{
  public class EntityDto<T>: EntityDtoBase
  {
    public virtual T Id
    {
      get;
      set;
    }
  }
}

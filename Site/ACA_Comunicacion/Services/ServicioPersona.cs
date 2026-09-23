using Comunicaciones.Models.Personas;
using Comunicaciones.Services.Compartidos;
using Domain.Entities.Personas;
using Shared.AccessShared;

namespace Comunicaciones.Services
{
  public class ServicioPersona: Shared.AccessShared.IService<Persona>
  {
    private IEntityStore<Persona> personaSvc;

    public ServicioPersona(IEntityStore<Persona> entityStore) 
    {
      this.personaSvc = entityStore;
    }

    public Persona AddObject(Persona entityToAdd)
    {
     return this.personaSvc.AddEntity(entityToAdd);
    }

    public bool DeleteObject(Persona entityToRemove)
    {
      throw new NotImplementedException();
    }

    public Persona GetObject(Guid Id)
    {
      return this.personaSvc.GetEntity(Id);
    }

    public ICollection<Persona> GetObjectCollection()
    {
      return this.personaSvc.GetEntity();
    }

    public Persona ModifyObject(Persona entityToModify)
    {
      throw new NotImplementedException();
    }
  }
}

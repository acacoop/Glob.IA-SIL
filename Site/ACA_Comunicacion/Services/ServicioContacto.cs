using Comunicaciones.DataRequest;
using Comunicaciones.DataResponse;
using Comunicaciones.Models.ListasContactos;
using Comunicaciones.Models.Personas;
using Domain.Context;
using Domain.Entities.ListasContactos;
using Domain.Entities.Personas;
using Shared.AccessShared;
using Shared.ClassShared.Types;

namespace Comunicaciones.Services
{
  public class ServicioContacto
  {
    private SQLDBContext Context;

    public ServicioContacto(SQLDBContext context)
    {
      this.Context = context;
    }
    #region Persona

    public Persona AddPersona(PersonaRequest entityToAdd)
    {
      try 
      { 
        Persona newPersona = new Persona();
        newPersona.Nombre = entityToAdd.Nombre;
        newPersona.Apellido = entityToAdd.Apellido;
        newPersona.Usuario = entityToAdd.Usuario;

        newPersona.Rol = (entityToAdd.Rol.HasValue && entityToAdd.Rol != Guid.Empty) ? new RolStore(this.Context).GetEntity(entityToAdd.Rol ?? Guid.Empty) : null;
        newPersona.Centro = (entityToAdd.Centro.HasValue && entityToAdd.Centro != Guid.Empty) ? new CentroStore(this.Context).GetEntity(entityToAdd.Centro ?? Guid.Empty) : null;
        newPersona.ZonaComercial = (entityToAdd.ZonaComercial.HasValue && entityToAdd.ZonaComercial != Guid.Empty) ? new ZonaComercialStore(this.Context).GetEntity(entityToAdd.ZonaComercial ?? Guid.Empty) : null;
        newPersona.Cuenta = (entityToAdd.Cuenta.HasValue && entityToAdd.Cuenta != Guid.Empty) ? new CuentaStore(this.Context).GetEntity(entityToAdd.Cuenta ?? Guid.Empty) : null;
      
      
        IEntityStore<Persona> personaStore = new PersonaStore(this.Context);
        newPersona = personaStore.AddEntity(newPersona);
      
        if (entityToAdd.Contactos != null && entityToAdd.Contactos.Count > 0)
        {
          newPersona.Contactos = new List<Contacto>();
          Contacto contAdd = new Contacto();
          entityToAdd.Contactos.ToList<ContactoRequest>().ForEach(contacto => { contacto.Persona = newPersona.Id; this.AddContacto(contacto); } );
        }
        return newPersona;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public bool DeletePersona(Persona entityToRemove)
    {
      try 
      { 
        IEntityStore<Persona> personaStore = new PersonaStore(this.Context);
        return personaStore.RemoveEntity(entityToRemove);
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public Persona GetPersona(Guid Id)
    {
      try 
      { 
        IEntityStore<Persona> personaStore = new PersonaStore(this.Context);
        Persona datosPersonales = personaStore.GetEntity(Id);
        return datosPersonales;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public ICollection<Persona> GetPersonaCollection()
    {
      try 
      { 
        IEntityStore<Persona> personaStore = new PersonaStore(this.Context);
        List<Persona> persOut = personaStore.GetEntity().ToList();
        return persOut;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public ICollection<Persona> GetPersonas(IList<Guid> Ids)
    {
      try
      {
        PersonaStore personaStore = new PersonaStore(this.Context);
        List<Persona> persOut = personaStore.GetEntitys(Ids).ToList();
        return persOut;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public ICollection<Persona> GetPersonaCollection(string q, int page = 1)
    {
      try 
      {
        if (string.IsNullOrEmpty(q))
        {
          return null;
        }
        PersonaStore personaStore = new PersonaStore(this.Context);
        ICollection<Persona> persOut = personaStore.GetEntityContainsName(q);
        return persOut;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public Persona ModifyPersona(Guid id, PersonaRequest entityToModify)
    {
      try 
      { 
        PersonaStore personaStore = new(this.Context);
        Persona personaBD = personaStore.GetEntity(id);
        if (personaBD == null) return null;//excepcion detallando el error

        personaBD.Nombre = entityToModify.Nombre;
        personaBD.Apellido = entityToModify.Apellido;
        personaBD.Usuario = entityToModify.Usuario;
        personaBD.Contactos?.Clear();
        personaBD.RolId = entityToModify.Rol;
        personaBD.CentroId = entityToModify.Centro;
        personaBD.ZonaComercialId = entityToModify.ZonaComercial;
        personaBD.CuentaId = entityToModify.Cuenta;
        personaBD.Contactos = entityToModify.Contactos?.Select(contacto => new Contacto
        {
          TipoId = contacto.Tipo,
          Dato = contacto.Dato
        }).ToList();

        //if (entityToModify.Rol == null || entityToModify.Rol == Guid.Empty) 
        //{
        //  personaBD.RolId = null;
        //  personaBD.Rol = null;
        //}
        //else {
        //  personaBD.RolId = entityToModify.Rol;
        //}

        //if (entityToModify.Centro == null || entityToModify.Centro == Guid.Empty)
        //{
        //  personaBD.CentroId= null;
        //  personaBD.Centro = null;
        //}
        //else
        //{
        //  personaBD.CentroId = entityToModify.Centro;
        //}

        //if (entityToModify.ZonaComercial == null || entityToModify.ZonaComercial == Guid.Empty)
        //{
        //  personaBD.ZonaComercialId = null;
        //  personaBD.ZonaComercial = null;
        //}
        //else
        //{
        //  personaBD.ZonaComercialId = entityToModify.ZonaComercial;
        //}

        //if (entityToModify.Cuenta == null || entityToModify.Cuenta == Guid.Empty)
        //{
        //  personaBD.CuentaId = null;
        //  personaBD.Cuenta = null;
        //}
        //else
        //{
        //  personaBD.CuentaId = entityToModify.Cuenta;
        //}

        ///*Eliminar*/
        //List<Contacto> ContactosEliminar = personaBD.Contactos.Where(c => !entityToModify.Contactos.Select(z => z.Id).ToList().Contains(c.Id)).ToList<Contacto>();
        //ContactosEliminar.ForEach(x => { personaBD.Contactos.Remove(x); this.DeleteContacto(new Contacto() { Id = x.Id }); });

        ///*Modificamos*/
        //foreach (var entity in entityToModify.Contactos.Where(x=> x.Id.HasValue))
        //{
        //  personaBD.Contactos.Where(d => d.Id.Equals(entity.Id)).ToList().ForEach(e => { e.Dato = entity.Dato; e.TipoId = entity.Tipo; });
        //}
        ///*Agregamos*/
        //List<ContactoRequest> ContactosAgregar = entityToModify.Contactos.Where(x => !x.Id.HasValue).ToList<ContactoRequest>();
        ////ContactosAgregar.ForEach(x => x.Id= this.AddContacto(x).Id);
        //ContactosAgregar.Select(contacto => new Contacto
        //{
        //  TipoId = contacto.Tipo,
        //  Dato = contacto.Dato
        //})
        //.ToList()
        //.ForEach(contacto => personaBD.Contactos.Add(contacto));

        personaBD = personaStore.ModifyEntity(personaBD);
        return personaBD;
      }
      catch (Exception e) 
      {
        throw e;
      }
    }
    #endregion

    #region Contacto

    public Contacto AddContacto(ContactoRequest entityToAdd)
    {
      try { 
        Contacto contacto = new Contacto();
        contacto.Persona = this.GetPersona(entityToAdd.Persona);
        contacto.Tipo = new TipoContactoStore(this.Context).GetEntity(entityToAdd.Tipo);
        contacto.Dato = entityToAdd.Dato;
        IEntityStore<Contacto> store = new ContactoStore(this.Context);
        return store.AddEntity(contacto);
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public bool DeleteContacto(Contacto entityToRemove)
    {
      try 
      { 
        ContactoStore contactoStore = new ContactoStore(this.Context);
        return contactoStore.RemoveEntity(entityToRemove);
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public Contacto GetContacto(Guid Id)
    {
      try 
      { 
        IEntityStore<Contacto> store = new ContactoStore(this.Context);
        return store.GetEntity(Id);
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public ICollection<Contacto> GetContactos(ICollection<Guid> Ids)
    {
      try 
      { 
        ContactoStore store = new ContactoStore(this.Context);
        return store.GetEntitys(Ids);
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public ICollection<Contacto> GetContactoCollection(string personId = "")
    {
      try 
      {
        Guid idPersona = string.IsNullOrEmpty(personId) ? Guid.Empty : Guid.Parse(personId);
        ContactoStore contactoStore = new ContactoStore(this.Context);
        if (idPersona != Guid.Empty)
        {
          return contactoStore.GetEntityByPersonId(idPersona);
        }
        else
        {
          return contactoStore.GetEntity();
        }
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public ICollection<Contacto> GetContactoCollection(FilterRequest filter)
    {
      try
      {
        if (filter==null)
        {
          return null;
        }
        else
        {
          ContactoStore contactoStore = new ContactoStore(this.Context);
          return contactoStore.GetEntity(filter).ToList<Contacto>();
        }
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    /*consulta listas y contactos por nombre (contenido)*/
    public List<AutocompleteDataDTO> GetContactos(IList<FilterRequest> filers)
    {
      try
      {
        List<AutocompleteDataDTO> listaResult = new List<AutocompleteDataDTO>();
        ListaContactosStore listaContactoStore = new ListaContactosStore(this.Context);
        PersonaStore personaStore = new PersonaStore(this.Context);
        foreach (FilterRequest filt in filers) 
        {
          try
          {
            listaResult.AddRange(listaContactoStore.GetEntity(filt).ToList().Select(x => new AutocompleteDataDTO() { id = x.Id, nombre = x.getNombre(), esLista = true }).ToList());
            listaResult.AddRange(personaStore.GetEntity(filt).ToList().Select(x => new AutocompleteDataDTO() { id = x.Id, nombre = x.getNombre(), esLista = false }).ToList());
          }
          catch (ArgumentException) { }
          catch (Exception) { throw; }
        }
      
        return listaResult;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public Contacto ModifyContacto(ContactoRequest entityToModify)
    {
      throw new NotImplementedException();
    }
    #endregion

    #region Listas

    public ListaContactos AddListaContactos(ListaContactosRequest entityToAdd)
    {
      try
      {
        ListaContactos newLista = new ListaContactos();
        ListaContactosStore storeLista = new ListaContactosStore(this.Context);
        newLista.Nombre = entityToAdd.Nombre;
        newLista.Descripcion = entityToAdd.Descripcion;
        newLista.Personas = (entityToAdd.Personas != null && entityToAdd.Personas.Count() > 0) ? this.GetPersonas(entityToAdd.Personas).ToList() : null;
        //newLista.ContactosId = (entityToAdd.Contactos != null && entityToAdd.Contactos.Count() > 0) ? entityToAdd.Contactos : null;

        if (entityToAdd.ListasHijas != null && entityToAdd.ListasHijas.Count() > 0)
        {
          List<ListaContactos> listasHijas = storeLista.GetEntitys(entityToAdd.ListasHijas).ToList<ListaContactos>();
          newLista.ListasHijas = listasHijas;
        }

        newLista = storeLista.AddEntity(newLista);
        return newLista;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public bool DeleteListaContactos(ListaContactos entityToRemove)
    {
      try 
      { 
        IEntityStore<ListaContactos> store = new ListaContactosStore(this.Context);
        return store.RemoveEntity(entityToRemove);
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public ListaContactos GetListaContactos(Guid Id)
    {
      try 
      { 
        IEntityStore<ListaContactos> store = new ListaContactosStore(this.Context);
        ListaContactos listaOut = new ListaContactos();
        listaOut = store.GetEntity(Id);
        return listaOut;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public ICollection<ListaContactos> GetListaContactosCollection(IList<FilterRequest> filters)
    {
      try 
      {
        IEntityStore<ListaContactos> store = new ListaContactosStore(this.Context);
        List<ListaContactos> listas = new List<ListaContactos>();
        foreach (FilterRequest filter in filters)
        {
          var query = store.GetEntity(filter);
          listas.AddRange(query.ToList());
        }
        return listas;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public ICollection<ListaContactos> GetListaContactosCollection()
    {
      try
      {
        IEntityStore<ListaContactos> store = new ListaContactosStore(this.Context);
        IList<ListaContactos> listas = store.GetEntity().ToList();
        return listas;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public ICollection<ListaContactos> GetListaContactosCollection(ICollection<Guid> ids)
    {
      try 
      {
        ListaContactosStore store = new ListaContactosStore(this.Context);
        return store.GetEntitys(ids);
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    /// <summary>
    /// Validamos que el contacto que agregamos no exista en la lista o en las listas hijas ya asociadas
    /// De igual manera con las listas hijas
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entityToModify"></param>
    /// <returns></returns>
    public ListaContactos ModifyListaContactos(Guid id, ListaContactosRequest entityToModify)
    {
      try 
      { 
        ListaContactosStore storeLista = new ListaContactosStore(this.Context);
        PersonaStore storePersona = new PersonaStore(this.Context);
        ListaContactos listaBD = storeLista.GetEntity(id);
        if (listaBD == null) return null; // excepcion detallando el error

        listaBD.Nombre = entityToModify.Nombre;
        listaBD.Descripcion = entityToModify.Descripcion;


        /*Listas hijas*/
        listaBD.ListasHijas.Where(x => !entityToModify.ListasHijas.Contains(x.Id)).ToList().ForEach(d => listaBD.ListasHijas.Remove(d));
        entityToModify.ListasHijas.Where(x => !listaBD.ListasHijas.Select(x => x.Id).Contains(x)).ToList().ForEach(d => listaBD.addListaHija(storeLista.GetEntity(d)));

        /*contactos*/
        listaBD.Personas.Where(x => !entityToModify.Personas.Contains(x.Id)).ToList().ForEach(d => listaBD.Personas.Remove(d));
        entityToModify.Personas.Where(x=> !listaBD.Personas.Select(x => x.Id).Contains(x)).ToList().ForEach(d => listaBD.addPersona(storePersona.GetEntity(d)));
        storeLista.ModifyEntity(listaBD);
        return listaBD;
      }
      catch (Exception e)
      {
        throw e;
      }
    }
    #endregion
  }
}

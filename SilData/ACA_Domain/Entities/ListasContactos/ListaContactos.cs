using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Domain.Entities.Compartidos.Entity;
using Domain.Entities.Personas;

namespace Domain.Entities.ListasContactos
{
  public class ListaContactos : EntityGuid, IDestinatario
  {
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public virtual IList<ListaContactos>? ListasHijas { get; set; }
    public virtual IList<ListaContactos>? ListasPadres { get; set; }
    public virtual IList<Persona>? Personas { get; set; }

    public ListaContactos()
    {
      ListasHijas = new List<ListaContactos>();
      ListasPadres = new List<ListaContactos>();
      Personas = new List<Persona>();
    }

    ~ListaContactos()
    {

    }

    /// <summary>
    /// Obtiene y retorna las hojas del arbol de listas que corresponden al tipo de contacto indicado.
    /// </summary>
    /// <param name="tipo"></param>
    /// <returns></returns>
    public IList<IDestinatario> getContactos(TipoContacto? tipo = null)
    {
      List<IDestinatario> contactos = new List<IDestinatario>();
      if (tipo == null)
      {
        contactos.AddRange(this.Personas?.SelectMany(persona => persona.Contactos).ToList<IDestinatario>());
      }
      else
      {
        contactos.AddRange(this.Personas?.SelectMany(persona => persona.Contactos).Where(x => x.Tipo.Equals(tipo)).ToList<IDestinatario>());
      }

      List<IDestinatario> listResult = new List<IDestinatario>();
      foreach (var listaContactos in ListasHijas)
      {
        //listResult.AddRange((tipo != null) ? listaContactos.Contactos.Where(x => x.Tipo.Equals(tipo)).ToList<IDestinatario>():
        //                                     listaContactos.Contactos.ToList<IDestinatario>());
        listResult.AddRange(listaContactos.getContactos(tipo));
      }
      contactos.AddRange(listResult);
      return contactos;
    }

    /// <summary>
    /// Obtiene y retorna los hijos de la lista corriente (contactos y listas de contactos)
    /// </summary>
    /// <returns></returns>
    public IList<IDestinatario> getHijos()
    {
      IList<IDestinatario> listasHijas = (this.ListasHijas != null && this.ListasHijas.Count() > 0) ? this.ListasHijas.ToList<IDestinatario>() : new List<IDestinatario>();
      IList<IDestinatario> contactos = (this.Personas != null && this.Personas.Count() > 0) ? this.Personas.ToList<IDestinatario>() : new List<IDestinatario>();
      List<IDestinatario> hijos = new List<IDestinatario>();
      hijos.AddRange(listasHijas);
      hijos.AddRange(contactos);
      return hijos;
    }

    public bool addListaHija(ListaContactos nuevaHija) 
    {
      if (ListasHijas == null)
      {
        ListasHijas = new List<ListaContactos>() { nuevaHija };
      }
      else 
      {
        if (!esListaDescendiente(nuevaHija) && !estoyEnSusDesendientes(nuevaHija.ListasHijas, this))
        {
          ListasHijas.Add(nuevaHija);
        }
        else 
        {
          return false; //excepcion indicando que la list ya esta asinada
        }
      } 
      return true;
    }

    public bool estoyEnSusDesendientes(IList<ListaContactos> listas, ListaContactos miLista)
    {
      if (listas== null || miLista == null) return false;

      bool resultado = listas.Any(x => x.Equals(miLista));
      if (resultado) return true;

      foreach (ListaContactos item in listas)
      {
        resultado = resultado || estoyEnSusDesendientes(item.ListasHijas, miLista);
        if (resultado) return true;
      }

      return resultado;
    }

    public bool esListaDescendiente(ListaContactos nuevaHija) =>
      (ListasHijas != null) &&
      (ListasHijas.Any(x => x.Equals(nuevaHija)) || ListasHijas.Any(d => d.esListaDescendiente(nuevaHija)));

    /// <summary>
    /// Valida que el contacto no este ya incluido. (en la lista o en las listas hijas)
    /// </summary>
    /// <param name="nuevoContacto"></param>
    /// <returns></returns>
    public bool addPersona(Persona nuevoContacto)
    {
      List<IDestinatario> personas = getHijos().ToList();
      if (!personas.Where(x => x.esContacto()).Any(x => x.Equals(nuevoContacto))) 
      {
        if (Personas == null) Personas = new List<Persona>();

        Personas.Add(nuevoContacto);
        return true;
      }
      else { 
        return false; //exception indicando que el contacto ya existe
      }
    }

    public string getNombre()
    {
      return this.Nombre;
    }

    public bool esContacto()
    {
      return false;
    }

    public Guid getId()
    {
      return Id;
    }

    public override bool Equals(object? obj)
    {
      if (obj == null) return false;
      ListaContactos? other = obj as ListaContactos;
      return this.Id == other.Id;
    }
  }//end ListaContactos
}
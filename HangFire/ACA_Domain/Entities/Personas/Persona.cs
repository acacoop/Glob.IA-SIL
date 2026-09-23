namespace Domain.Entities.Personas
{
	using Domain.Entities.Compartidos.Entity;
  using Domain.Entities.ListasContactos;

  public class Persona: EntityGuid, IDestinatario
	{
		public string Apellido { get; set; }
		public string Nombre { get; set; }
		public virtual Guid? CuentaId { get; set; }
		public virtual Cuenta? Cuenta { get; set; }
		public virtual Guid? RolId { get; set; }
		public virtual Rol? Rol { get; set; }
		public virtual string? Usuario { get; set; }
		public virtual Guid? CentroId { get; set; }
		public virtual Centro? Centro { get; set; }
		public virtual Guid? ZonaComercialId { get; set; }
		public virtual ZonaComercial? ZonaComercial { get; set; }
    public virtual IList<ListaContactos>? ListasPadre { get; set; }
    public virtual ICollection<Contacto>? Contactos { get; set; }

		public Persona()
		{

		}

		~Persona()
		{

		}
		public override bool Equals(object? obj)
		{
			if (obj == null) return false;
			Persona? other = obj as Persona;
			return this.Id == other.Id;
		}

    public IList<IDestinatario> getContactos(TipoContacto? tipo = null)
    {
      List<IDestinatario> contactos = new List<IDestinatario>();
      if (tipo == null)
      {
        contactos.AddRange(this.Contactos?.ToList<IDestinatario>());
      }
      else
      {
        contactos.AddRange(this.Contactos?.Where(x => x.Tipo.Equals(tipo)).ToList<IDestinatario>());
      }

      return contactos;
    }

    public IList<IDestinatario> getHijos()
    {
      IList<IDestinatario> contactos = (this.Contactos != null && this.Contactos.Count() > 0) ? this.Contactos.ToList<IDestinatario>() : new List<IDestinatario>();
      List<IDestinatario> hijos = new List<IDestinatario>();
      hijos.AddRange(contactos);
      return hijos;
    }

    public string getNombre()
    {
      return $"{this.Nombre} {this.Apellido}";
    }

    public bool esContacto()
    {
      return true;
    }

    public Guid getId()
    {
      return this.Id;
    }
  }//end Persona
}
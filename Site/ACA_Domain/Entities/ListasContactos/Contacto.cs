using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Domain.Entities.Personas;
using Domain.Entities.Compartidos.Entity;
namespace Domain.Entities.ListasContactos
{
	public class Contacto : EntityGuid, IDestinatario 
	{
		public virtual Persona Persona { get; set; }
		public virtual Guid TipoId { get; set; }
		public virtual TipoContacto Tipo { get; set; }
		public string Dato { get; set; }

		public Contacto()
		{
		}

		~Contacto(){

		}

		/// 
		/// <param name="tipo"></param>
		public IList<IDestinatario> getContactos(TipoContacto? tipo = null)
		{
			List<IDestinatario> contactos = new List<IDestinatario>();
			if (tipo == null) contactos.Add(this);
			if (tipo != null && this.Tipo.Equals(tipo)) contactos.Add(this);
			return contactos;
		}

		/// 
		/// <param name="destinatario"></param>
		public IList<IDestinatario> getHijos(){

			return new List<IDestinatario>();
		}

		public string getNombre(){

			return $"{this.Tipo.Nombre}: {this.Dato}";
		}

		public bool esContacto()
		{
				return true;
		}

        public Guid getId()
        {
            return Id;
        }

		public override bool Equals(object? obj)
		{
			if (obj == null) return false;
			Contacto? other = obj as Contacto;
			return this.Id == other.Id;
		}

	}//end Contacto
}
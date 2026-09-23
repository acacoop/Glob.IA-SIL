using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Domain.Entities.Compartidos.Entity;

namespace Domain.Entities.Personas
{
	public class Rol : EntityGuid
	{
		public string Nombre { get; set; }
		public string Descripcion { get; set; }

		public Rol()
		{

		}

		~Rol()
		{

		}

		public override bool Equals(object? obj)
		{
			if (obj == null) return false;
			Rol? other = obj as Rol;
			return this.Id == other.Id;
		}

	}//end Rol
}
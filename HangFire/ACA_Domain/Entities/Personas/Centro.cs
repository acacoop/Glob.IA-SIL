using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Domain.Entities.Compartidos.Entity;

namespace Domain.Entities.Personas
{
	public class Centro : EntityGuid
	{
		public string Codigo { get; set; }
		public string Nombre { get; set; }

		public Centro()
		{

		}

		~Centro()
		{

		}
		public override bool Equals(object? obj)
		{
			if (obj == null) return false;
			Centro? other = obj as Centro;
			return this.Id == other.Id;
		}
	}//end Centro
}
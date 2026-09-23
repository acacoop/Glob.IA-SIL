using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Domain.Entities.Externo;
using Domain.Entities.Compartidos.Entity;

namespace Domain.Entities.Personas
{
	public class Cuenta : EntityGuid
	{
		public string NroCuenta { get; set; }
		public string Cuit { get; set; }
		public string Nombre { get; set; }


		public Cuenta()
		{

		}

		~Cuenta()
		{

		}

		public override bool Equals(object? obj)
		{
			if (obj == null) return false;
			Cuenta? other = obj as Cuenta;
			return this.Id == other.Id;
		}

	}//end Cuenta
}
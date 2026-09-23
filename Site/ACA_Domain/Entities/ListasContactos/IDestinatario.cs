using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Domain.Entities.ListasContactos
{
	public interface IDestinatario
	{
		/// 
		/// <param name="tipo"></param>
		IList<IDestinatario> getContactos(TipoContacto? tipo = null);

		/// 
		/// <param name="destinatario"></param>
		IList<IDestinatario> getHijos();

		string getNombre();

		bool esContacto();
		Guid getId();
	}//end IDestinatario
}
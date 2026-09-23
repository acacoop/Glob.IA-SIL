using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Domain.Entities.ListasContactos
{
	public interface IDestinatarioFactory 
	{
		/// <param name="params"></param>
		IList<Dictionary<string, IList<IDestinatario>>> getDestinatarios(IList<string> parameters);

		IList<Dictionary<string, IList<IDestinatario>>> getDestinatarios();
	}//end IDestinatarioFactory
}
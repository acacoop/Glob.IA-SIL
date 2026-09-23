using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Domain.Entities.ListasContactos
{
	public class ListaEstaticaFactory : IDestinatarioFactory 
	{

		public ListaEstaticaFactory(){

		}

		~ListaEstaticaFactory(){

		}

		/// 
		/// <param name="params"></param>
		public IList<Dictionary<string, IList<IDestinatario>>> getDestinatarios(IList<string> parameters){

			return null;
		}

		public IList<Dictionary<string, IList<IDestinatario>>> getDestinatarios(){

			return null;
		}

	}//end ListaEstaticaFactory
}
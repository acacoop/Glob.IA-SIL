using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Domain.Entities.ListasContactos
{
	public class ListaDinamicaPorCuentaFactory : IDestinatarioFactory {

	public ListaDinamicaPorCuentaFactory(){

	}

	~ListaDinamicaPorCuentaFactory(){

	}

	/// 
	/// <param name="params"></param>
	public IList<Dictionary<string, IList<IDestinatario>>> getDestinatarios(IList<string> parameters){

		return null;
	}

	public IList<Dictionary<string, IList<IDestinatario>>> getDestinatarios(){

		return null;
	}

}//end ListaDinamicaPorCuentaFactory
}
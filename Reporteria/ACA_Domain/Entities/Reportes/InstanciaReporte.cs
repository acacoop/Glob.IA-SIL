using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Domain.Entities.ListasContactos;

namespace Domain.Entities.Reportes
{
	/// <summary>
	/// Representa al reporte a ejecutar con ciertos parámetros específicos
	/// </summary>
	public class InstanciaReporte {
		public Guid Id { get; set; }
		public virtual IList<Parametro> Parametros { get; set; }
		public virtual Reporte Reporte { get; set; }

		public InstanciaReporte() {

		}

		~InstanciaReporte() {

		}

		/// 
		/// <param name="params"></param>
		public IList<Dictionary<string, IList<IDestinatario>>> getDestinatarios(IList<string> parameters){
			return null;
		}

	}//end InstanciaReporte
}
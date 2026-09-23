using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Domain.Entities.Reportes
{
	/// <summary>
	/// Representa a un reporte en su nivel de abstracción más alto
	/// </summary>
	public class Reporte
	{
		public string Descripcion { get; set; }
		public Guid Id { get; set; }
		public string Nombre { get; set; }
		public string Query { get; set; }

		public Reporte()
		{

		}

		~Reporte()
		{

		}

	}//end Reporte
}
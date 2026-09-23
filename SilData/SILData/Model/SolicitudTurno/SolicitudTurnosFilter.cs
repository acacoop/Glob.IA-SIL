using SILData.FormatExtension;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SILData.Model.SolicitudTurno
{
	public class SolicitudTurnosFilter
	{
		public required long CuentaVendedor { get; set; }
		[AllowNull] public long? CuentaComprador { get; set; }
		[AllowNull] public long? CuentaDestino { get; set; }

		private DateTime _desde;

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public required DateTime Desde
		{
			get => _desde;
			set => _desde = value.Date;
		}

		private DateTime _hasta;

		[JsonConverter(typeof(CustomDateTimeConverter))]
		public required DateTime Hasta
		{
			get => _hasta;
			set => _hasta = value.Date;
		}
	}
}

using SILData.FormatExtension;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SILData.Model.SolicitudTurno
{
    /// <summary>
    /// Filtro para el endpoint bulk de matching (<c>POST /api/ShiftRequest/Matches</c>).
    /// A diferencia de <see cref="SolicitudTurnosFilter"/>, NO exige <c>CuentaVendedor</c>:
    /// el motor de matching puede trabajar con solicitudes de cualquier vendedor (o
    /// acotar a uno si se pasa explícitamente).
    /// </summary>
    public class MatchesSolicitudFilter
    {
        /// <summary>Código de grano (obligatorio).</summary>
        public int CodigoGrano { get; set; }

        /// <summary>Cuenta del vendedor (opcional). Si se omite, devuelve solicitudes de cualquier vendedor.</summary>
        [AllowNull]
        public long? CuentaVendedor { get; set; }

        /// <summary>Cuenta del comprador (opcional). Si se omite, devuelve solicitudes de cualquier comprador.</summary>
        [AllowNull]
        public long? CuentaComprador { get; set; }

        /// <summary>Zona geográfica o cuenta de puerto destino (opcional). Si se omite, devuelve solicitudes de cualquier destino.</summary>
        [AllowNull]
        public long? ZonaGeograficaId { get; set; }

        private DateTime _desde;

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Desde
        {
            get => _desde;
            set => _desde = value.Date;
        }

        private DateTime _hasta;

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Hasta
        {
            get => _hasta;
            set => _hasta = value.Date;
        }
    }
}

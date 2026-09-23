using System.Collections.Generic;

namespace SILData.Model.SolicitudTurno
{
    /// <summary>
    /// Respuesta del endpoint bulk <c>POST /api/ShiftRequest/Matches</c>.
    /// Devuelve el resumen de filtros, métricas agregadas y la lista de pares
    /// (solicitud, cupo) clasificados.
    /// </summary>
    public class MatchesResultDto
    {
        /// <summary>Filtros aplicados (eco del request, más los defaults completados).</summary>
        public MatchesFiltrosAplicados FiltrosAplicados { get; set; } = new();

        /// <summary>Resumen agregado de la respuesta.</summary>
        public MatchesResumen Resumen { get; set; } = new();

        /// <summary>Items (pares solicitud-cupo) clasificados.</summary>
        public List<MatchItemDto> Items { get; set; } = new();
    }

    public class MatchesFiltrosAplicados
    {
        public long? CuentaComprador { get; set; }
        public int CodigoGrano { get; set; }
        public long? ZonaGeograficaId { get; set; }
        public long? CuentaVendedor { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public bool IncluirIncompatibles { get; set; }

        /// <summary>
        /// Si el request trajo <c>CuentaPuerto</c>, eco del número de cuenta recibido.
        /// </summary>
        public long? CuentaPuerto { get; set; }

        /// <summary>
        /// Zonas a las que se expande <c>CuentaPuerto</c> (JOIN PUERTOPORZONA →
        /// ZONASGEOGRAFICAS). Vacío si no se pasó <c>CuentaPuerto</c> o si el puerto
        /// no pertenece a ninguna zona. Sirve para que la UI muestre el
        /// "puente" que se aplicó.
        /// </summary>
        public List<long> ZonasResueltas { get; set; } = new List<long>();
    }

    public class MatchesResumen
    {
        public int TotalSolicitudesAnalizadas { get; set; }
        public int TotalCuposAnalizados { get; set; }
        public int MatchesDirectos { get; set; }
        public int MatchesParciales { get; set; }
        public int MatchesCondicionales { get; set; }
        public int Incompatibles { get; set; }
    }
}
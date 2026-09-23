using System.ComponentModel.DataAnnotations;

namespace SILData.Model.SolicitudTurno
{
    public class MatchesFilterDto
    {
        public long? CuentaComprador { get; set; }
        [Required]
        public int CodigoGrano { get; set; }
        public long? CuentaPuerto { get; set; }
        public long? ZonaGeograficaId { get; set; }
        public long? CuentaVendedor { get; set; }
        public string? Codcentro { get; set; }

        /// <summary>
        /// Centro de distribución del cupo (<c>cuposcorre.CentroDist</c>).
        /// Junto con <see cref="Codcentro"/> forma la clave natural del cupo
        /// para el flujo V2 de Distribución.
        /// </summary>
        public string? Codcentrodist { get; set; }

        /// <summary>
        /// Centros que el operador logueado puede manipular (claims). Restringe
        /// los cupos candidatos a los creados en esos centros
        /// (<c>cuposcorre.Centro</c>). Vacío o null = sin filtro.
        ///
        /// Distinto de <see cref="Codcentro"/>, que es un único centro y lo usa
        /// sólo el flujo V2 de Distribución.
        /// </summary>
        public List<string>? Centros { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool IncluirIncompatibles { get; set; }
        public MatchesAgrupacion AgruparPor { get; set; } = MatchesAgrupacion.Solicitud;
    }

    public enum MatchesAgrupacion
    {
        /// <summary>Cada item es (solicitud, cupo). Default.</summary>
        Solicitud = 0,

        /// <summary>Cada item sigue siendo (solicitud, cupo) pero la UI los agrupa visualmente por cupoId.</summary>
        Cupo = 1,

        /// <summary>Sin agrupación; lista plana.</summary>
        Ninguno = 2
    }
}
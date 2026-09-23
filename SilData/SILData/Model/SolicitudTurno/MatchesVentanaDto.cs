using System.Collections.Generic;

namespace SILData.Model.SolicitudTurno
{
    /// <summary>
    /// Request del endpoint bulk <c>POST /api/ShiftRequest/MatchesVentana</c>.
    /// A diferencia de <see cref="MatchesFilterDto"/> (que resuelve UNA
    /// combinación grano/vendedor/comprador/destino), este filtro describe
    /// únicamente la ventana temporal: el endpoint devuelve los matches de
    /// TODAS las solicitudes pendientes de esa ventana en una sola llamada.
    /// </summary>
    public class MatchesVentanaFilterDto
    {
        /// <summary>Primer día de la ventana. Si es null, se usa hoy.</summary>
        public DateTime? FechaDesde { get; set; }

        /// <summary>Cantidad de días de la ventana, incluyendo FechaDesde.</summary>
        public int Dias { get; set; } = 7;

        /// <summary>
        /// Centros del operador (claims). Aplica la misma regla de visibilidad
        /// que <c>GetAllPendingShiftRequestAsync</c>: entran las solicitudes sin
        /// zona geográfica (las puede tomar cualquier operador) y las cuya zona
        /// pertenece a alguno de estos centros.
        ///
        /// Vacío o null = sin filtro por centro. Acotarlo evita evaluar el
        /// matching de solicitudes que el operador nunca va a ver.
        /// </summary>
        public List<string>? Centros { get; set; }
    }

    /// <summary>
    /// Respuesta de <c>POST /api/ShiftRequest/MatchesVentana</c>.
    /// </summary>
    public class MatchesVentanaResultDto
    {
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }

        public MatchesVentanaResumen Resumen { get; set; } = new();

        /// <summary>Pares (solicitud, cupo) compatibles de toda la ventana.</summary>
        public List<MatchVentanaItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Item "flaco": sólo los campos que el consumidor necesita para contar
    /// matches por fila de grilla. A propósito NO incluye la solicitud ni el
    /// cupo hidratados (nombres de vendedor, comprador, destino, zona) como
    /// hace <see cref="MatchItemDto"/>: en una ventana completa eso multiplica
    /// el payload por dos órdenes de magnitud sin que nadie lo consuma.
    /// </summary>
    public class MatchVentanaItemDto
    {
        public long SolicitudId { get; set; }
        public long CupoId { get; set; }

        /// <summary>"Directo" | "Parcial" | "Condicional".</summary>
        public string? MatchType { get; set; }

        /// <summary>Fecha del cupo (siempre igual a la fecha de la solicitud).</summary>
        public DateTime? CupoFecha { get; set; }

        // No viajan las claves de la solicitud (grano, vendedor, comprador,
        // destino): el consumidor atribuye cada item a su fila de grilla por
        // SolicitudId, que es la relación real. Una fila es el conjunto de
        // solicitudes de esa combinación a lo largo de la ventana — una por
        // fecha — y su contador es la suma sobre ellas.
    }

    public class MatchesVentanaResumen
    {
        public int TotalSolicitudesAnalizadas { get; set; }
        public int TotalCuposAnalizados { get; set; }
        public int TotalGranos { get; set; }
        public int MatchesDirectos { get; set; }
        public int MatchesParciales { get; set; }
        public int MatchesCondicionales { get; set; }
        public int Incompatibles { get; set; }
    }
}

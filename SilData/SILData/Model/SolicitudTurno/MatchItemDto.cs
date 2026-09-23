using Domain.Entities.Externo;
using SILData.Model.SolicitudTurno;

namespace SILData.Model.SolicitudTurno
{
    /// <summary>
    /// Item individual de la respuesta del endpoint bulk <c>POST Matches</c>.
    /// Cada item es un par (solicitud, cupo) con su clasificación y desglose.
    /// </summary>
    public class MatchItemDto
    {
        public long SolicitudId { get; set; }
        public long CupoId { get; set; }
        /// <summary>"Directo" | "Parcial" | "Condicional" | null (si Incompatible).</summary>
        public string? MatchType { get; set; }
        /// <summary>Razón textual para incompatibles / Parcial / Condicional.</summary>
        public string? Razon { get; set; }

        /// <summary>
        /// true cuando el vendedor del cupo coincide con el de la solicitud,
        /// o cuando ambos no exigen vendedor. Sale del motor
        /// (<c>ResultadoParcial.VendedorCoincide</c>). La UI lo usa para
        /// mostrar u ocultar la fila Vendedor en Pantalla 2.
        /// </summary>
        public bool VendedorCoincide { get; set; }

        /// <summary>
        /// true cuando el comprador del cupo coincide con el de la solicitud,
        /// o cuando ambos no exigen comprador. Sale del motor
        /// (<c>ResultadoParcial.CompradorCoincide</c>). La UI lo usa para
        /// mostrar u ocultar la fila Comprador en Pantalla 2.
        /// </summary>
        public bool CompradorCoincide { get; set; }

        /// <summary>
        /// true cuando el destino del cupo coincide con el de la solicitud,
        /// o cuando ambos no exigen destino. Sale del motor
        /// (<c>ResultadoParcial.DestinoCoincide</c>). La UI lo usa para
        /// mostrar u ocultar la fila Destino en Pantalla 2.
        /// </summary>
        public bool DestinoCoincide { get; set; }

        /// <summary>Resumen de la solicitud involucrada.</summary>
        public MatchSolicitudCompleta Solicitud { get; set; } = new();

        /// <summary>Resumen del cupo involucrado.</summary>
        public MatchCupoResumen Cupo { get; set; } = new();
    }

    public class MatchSolicitudCompleta
    {
        public long Id { get; set; }
        public long CuentaVendedor { get; set; }
        /// <summary>
        /// Nombre del vendedor asociado a <see cref="CuentaVendedor"/>.
        /// Resuelto por la query de JOIN contra <c>cuposvendedor</c> en
        /// <c>SolicitudTurnoStore.GetCuposConMatchesAsync</c> (proyectado como
        /// <c>cv.nombre AS "NombreVendedor"</c>) y propagado por los servicios
        /// <c>SolicitudTurnoMatchingV2Service</c> y <c>SolicitudTurnoService</c>.
        /// Permanece nulo cuando la query no encuentra el código en la tabla
        /// maestra; la UI hace fallback a <see cref="CuentaVendedor"/>.
        /// </summary>
        public string? NombreVendedor { get; set; }
        public long? CuentaComprador { get; set; }
        public int CodigoGrano { get; set; }
        public long? CuentaDestino { get; set; }
        public string? TipoDestino { get; set; }
        /// <summary>Cantidad total pedida por la solicitud (SOLTURNOS.CANTIDAD).</summary>
        public int Cantidad { get; set; }
        /// <summary>
        /// Cantidad disponible para asignar en este Accept: Cantidad - CantidadAceptada - CantidadRechazada.
        /// La UI muestra este valor como "disponibles" y valida contra este límite.
        /// </summary>
        public int CantidadDisponible { get; set; }
        /// <summary>Cantidad rechazada acumulada (SOLTURNOS.CANTIDAD_RECHAZADA).</summary>
        public int CantidadRechazada { get; set; }
        public DateTime FechaSolicitado { get; set; }
        public string? Observacion { get; set; }

        /// <summary>
        /// Desglose de cupos asociados (parent + splits) según SOLTURNOS.
        /// </summary>
        public CuposAsociadosDesglose CuposAsociados { get; set; } = new();
    }

    public class MatchCupoResumen
    {
        public long Id { get; set; }
        public string? CodGrano { get; set; }
        public string? CodVendSIL { get; set; }
        public string? CodCompSIL { get; set; }
        public string? CodDestino { get; set; }
        public DateTime? Fecha { get; set; }

        /// <summary>
        /// Cupos disponibles para el vendedor de la solicitud. Se calcula
        /// desde la tabla HTML de Distribución (#TablaDistribuciones), no del
        /// backend, por lo que permanece en 0 en la respuesta del motor.
        /// </summary>
        public int Cupostotalesadist { get; set; }

        /// <summary>
        /// Nombres resolvers a partir de las columnas Nom* de cuposcorre. Se
        /// devuelven nulos cuando el JOIN no las trae (caso del query liviano
        /// en <c>FindAvailableCuposByPeriodAsync</c>) o cuando el código no
        /// existe en la tabla maestra. La UI los muestra cuando están
        /// disponibles, con fallback al ID cuando no.
        /// </summary>
        public string? NombreVendedor { get; set; }
        public string? NombreComprador { get; set; }
        public string? NombreDestino { get; set; }
        public string? NombreGrano { get; set; }
    }
}
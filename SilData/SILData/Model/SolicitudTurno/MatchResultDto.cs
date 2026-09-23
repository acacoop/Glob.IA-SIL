using Domain.Entities.Externo;
using SILData.Model;

namespace SILData.Model.SolicitudTurno
{
    /// <summary>
    /// Resultado del matching para los endpoints single-pair
    /// <c>GET MatchesPorCupo/{cupoId}</c> y <c>GET MatchesPorSolicitud/{solicitudId}</c>.
    /// Cada item representa una solicitud compatible (o no) con un cupo.
    /// </summary>
    public class MatchResultDto
    {
        public long CupoId { get; set; }
        public long SolicitudId { get; set; }
        /// <summary>
        /// <c>true</c> si la solicitud es compatible con el cupo (mandatorios OK).
        /// </summary>
        public bool Compatible { get; set; }

        /// <summary>
        /// Tipo de clasificación: "Directo" | "Parcial" | "Condicional" | <c>null</c> (si Incompatible).
        /// </summary>
        public string? Tipo { get; set; }

        /// <summary>
        /// Razón textual para incompatibles y Parcial/Condicional. <c>null</c> para Directo.
        /// </summary>
        public string? Razon { get; set; }

        /// <summary>
        /// Resumen compacto de la solicitud involucrada (útiles para que el front
        /// muestre contexto sin tener que volver a pedirla).
        /// </summary>
        public MatchSolicitudResumen Solicitud { get; set; } = new();

        public static MatchResultDto From(Cupo cupo, ACA.Matching.Modelos.SolicitudMatching solicitud, ACA.Matching.Modelos.MatchResult result)
        {
            return new MatchResultDto
            {
                CupoId = cupo.Id,
                SolicitudId = solicitud.Id,
                Compatible = result.Compatible,
                Tipo = result.Tipo?.ToString(),
                Razon = result.RazonIncompatibilidad,
                Solicitud = new MatchSolicitudResumen
                {
                    Id = solicitud.Id,
                    Grano = solicitud.Grano,
                    Vendedor = solicitud.Vendedor,
                    Comprador = solicitud.Comprador,
                    Destino = solicitud.Destino,
                    TipoDestino = solicitud.TipoDestino?.ToString(),
                    Observacion = solicitud.Observaciones
                }
            };
        }
    }

    public class MatchSolicitudResumen
    {
        public long Id { get; set; }
        public string Grano { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public string? Comprador { get; set; }
        public string? Destino { get; set; }
        public string? TipoDestino { get; set; }
        public string? Observacion { get; set; }
    }
}
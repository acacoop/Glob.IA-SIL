using SILData.Model;
using SILData.Model.SolicitudTurno;

namespace SILData.Services
{
    /// <summary>
    /// Conversor entre <see cref="SolicitudTurno"/> (modelo de SILData) y
    /// <see cref="ACA.Matching.Modelos.SolicitudMatching"/> (modelo del motor).
    /// Existe para que <c>ACA_Matching</c> NO dependa de <c>SILData</c>.
    /// </summary>
    public static class SolicitudMatchingAdapter
    {
        /// <summary>
        /// Mapea un <see cref="SolicitudTurno"/> a un
        /// <see cref="ACA.Matching.Modelos.SolicitudMatching"/>.
        /// </summary>
        public static ACA.Matching.Modelos.SolicitudMatching From(SolicitudTurno s)
        {
            return new ACA.Matching.Modelos.SolicitudMatching(
                Id: s.Id,
                Grano: s.CodigoGrano.ToString(),
                Vendedor: s.CuentaVendedor.ToString(),
                Comprador: s.CuentaComprador?.ToString(),
                Destino: s.CuentaDestino?.ToString(),
                TipoDestino: MapTipoDestino(s.TipoDestino),
                Observaciones: s.Observacion);
        }

        /// <summary>
        /// Mapea un <see cref="SolicitudTurnoView"/> (vista de lectura) a un
        /// <see cref="ACA.Matching.Modelos.SolicitudMatching"/>. Útil para
        /// los endpoints que reciben vistas del store.
        /// </summary>
        public static ACA.Matching.Modelos.SolicitudMatching From(SolicitudTurnoView s)
        {
            return new ACA.Matching.Modelos.SolicitudMatching(
                Id: s.Id,
                Grano: s.CodigoGrano.ToString(),
                Vendedor: s.CuentaVendedor.ToString(),
                Comprador: s.CuentaComprador?.ToString(),
                Destino: s.CuentaDestino?.ToString(),
                TipoDestino: MapTipoDestino(s.TipoDestino),
                Observaciones: s.Observacion);
        }

        private static ACA.Matching.Modelos.TipoDestino? MapTipoDestino(Model.TipoDestino? src)
        {
            if (!src.HasValue) return null;
            return src.Value switch
            {
                Model.TipoDestino.ZonaPortuaria => ACA.Matching.Modelos.TipoDestino.ZonaPortuaria,
                Model.TipoDestino.Destino => ACA.Matching.Modelos.TipoDestino.Destino,
                _ => null
            };
        }

        /// <summary>
        /// Predicado de la regla de negocio "fecha obligatoria del match":
        /// la fecha del cupo debe coincidir con la <c>FechaSolicitado</c>
        /// de la solicitud para que el par sea evaluado por el motor.
        /// Un cupo sin fecha (<c>null</c>) no puede satisfacer el requisito
        /// y queda descartado. Se aplica <c>.Date</c> en ambos lados para
        /// defenderse del componente horario (cuposcorre es tabla externa).
        /// </summary>
        public static bool FechasCoinciden(DateTime? fechaCupo, DateTime fechaSolicitud)
            => fechaCupo.HasValue && fechaCupo.Value.Date == fechaSolicitud.Date;
    }
}
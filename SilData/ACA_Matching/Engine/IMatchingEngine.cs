using ACA.Matching.Contexto;
using ACA.Matching.Modelos;

namespace ACA.Matching.Engine;

/// <summary>
/// Motor de matching entre solicitudes y cupos.
/// Clasifica cada par como Directo / Parcial / Condicional o Incompatible.
/// </summary>
public interface IMatchingEngine
{
    /// <summary>
    /// Evalúa un par (cupo, solicitud) individual.
    /// </summary>
    /// <param name="cupo">Cupo a evaluar.</param>
    /// <param name="solicitud">Solicitud a evaluar.</param>
    /// <param name="contextoZona">
    /// Contexto opcional para resolver pertenencia cupo→zona (vía JOIN externo).
    /// Si es <c>null</c>, se usa <see cref="ContextoZonaVacio"/>: las solicitudes
    /// con <see cref="Modelos.TipoDestino.ZonaPortuaria"/> no podrán matchear.
    /// </param>
    MatchResult Evaluar(
        Cupo cupo,
        SolicitudMatching solicitud,
        IContextoZona? contextoZona = null);

    /// <summary>
    /// Evalúa un cupo contra múltiples solicitudes.
    /// </summary>
    /// <returns>
    /// Lista de <see cref="MatchResult"/> en el mismo orden que <paramref name="solicitudes"/>.
    /// </returns>
    IReadOnlyList<MatchResult> EvaluarTodos(
        Cupo cupo,
        IEnumerable<SolicitudMatching> solicitudes,
        IContextoZona? contextoZona = null);

    /// <summary>
    /// Evalúa una solicitud contra múltiples cupos (caso inverso al anterior).
    /// Útil para el endpoint <c>MatchesPorSolicitud</c>.
    /// </summary>
    /// <returns>
    /// Lista de <see cref="MatchResult"/> en el mismo orden que <paramref name="cupos"/>.
    /// </returns>
    IReadOnlyList<MatchResult> CompatiblesCon(
        IEnumerable<Cupo> cupos,
        SolicitudMatching solicitud,
        IContextoZona? contextoZona = null);
}
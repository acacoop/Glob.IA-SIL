using ACA.Matching.Engine.Evaluacion;

namespace ACA.Matching.Engine.Reglas;

/// <summary>
/// Una regla del motor de matching. El motor itera una cadena de reglas en orden
/// preestablecido y aplica cada una hasta obtener un outcome terminal (Incompatible
/// o Clasificado).
/// </summary>
public interface IMatchRule
{
    /// <summary>Nombre legible de la regla (para logs y debugging).</summary>
    string Nombre { get; }

    /// <summary>
    /// Evalúa la regla contra el par (cupo, solicitud).
    /// Las reglas que necesitan consultar la pertenencia a zonas reciben
    /// el <see cref="ACA.Matching.Contexto.IContextoZona"/>; las que no lo necesitan lo ignoran.
    /// </summary>
    /// <param name="estado">Estado mutable compartido entre reglas (entrada/salida).</param>
    /// <param name="cupo">Cupo a evaluar.</param>
    /// <param name="solicitud">Solicitud a evaluar.</param>
    /// <param name="contextoZona">
    /// Contexto opcional para resolver pertenencia cupo→zona. <c>null</c> cuando no aplica
    /// (p.ej. en tests o cuando todas las solicitudes son TipoDestino.Destino).
    /// </param>
    MatchOutcome Evaluar(
        ResultadoParcial estado,
        Cupo cupo,
        Modelos.SolicitudMatching solicitud,
        ACA.Matching.Contexto.IContextoZona? contextoZona);
}
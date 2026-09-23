using ACA.Matching.Engine.Evaluacion;

namespace ACA.Matching.Engine.Reglas;

/// <summary>
/// Regla mandatoria: el código de grano de la solicitud debe coincidir
/// con el del cupo. Comparación case-insensitive sobre strings trimmed.
/// </summary>
/// <remarks>
/// Posición en la cadena: 1 (la primera). Si el grano no coincide, no tiene
/// sentido evaluar vendedor ni opcionales: se cortocircuita con Incompatible.
/// </remarks>
public sealed class GranoRule : IMatchRule
{
    /// <inheritdoc/>
    public string Nombre => "Grano";

    /// <inheritdoc/>
    public MatchOutcome Evaluar(
        ResultadoParcial estado,
        Cupo cupo,
        Modelos.SolicitudMatching solicitud,
        ACA.Matching.Contexto.IContextoZona? contextoZona)
    {
        // Defensa: si el cupo no tiene grano cargado, no podemos comparar.
        // Se trata como incompatible (no se puede confirmar match).
        if (string.IsNullOrWhiteSpace(cupo.CodGrano))
        {
            estado.MandatoriosOk = false;
            return MatchOutcome.Incompatible("El cupo no tiene CodGrano definido.");
        }

        // La solicitud siempre debe traer grano (es mandatorio para ella).
        if (string.IsNullOrWhiteSpace(solicitud.Grano))
        {
            estado.MandatoriosOk = false;
            return MatchOutcome.Incompatible("La solicitud no tiene grano definido.");
        }

        bool coincide = string.Equals(
            cupo.CodGrano.Trim(),
            solicitud.Grano.Trim(),
            StringComparison.OrdinalIgnoreCase);

        if (!coincide)
        {
            estado.MandatoriosOk = false;
            return MatchOutcome.Incompatible(
                $"Grano no coincide (cupo='{cupo.CodGrano}', solicitud='{solicitud.Grano}').");
        }

        return MatchOutcome.Continuar();
    }
}
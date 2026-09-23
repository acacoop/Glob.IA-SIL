namespace ACA.Matching.Engine.Reglas;

/// <summary>
/// Resultado de evaluar una regla: tipo de outcome + (opcional) clasificación + razón.
/// </summary>
/// <param name="Kind">Tipo de outcome (Incompatible / Clasificado / Continuar).</param>
/// <param name="Tipo">
/// Clasificación emitida cuando <paramref name="Kind"/> == <see cref="MatchOutcomeKind.Clasificado"/>.
/// <c>null</c> en otros casos.
/// </param>
/// <param name="Razon">
/// Razón textual para incompatibles y para clasificación Parcial/Condicional.
/// <c>null</c> cuando es Continuar o Clasificado-Directo.
/// </param>
public sealed record MatchOutcome(
    MatchOutcomeKind Kind,
    Modelos.MatchType? Tipo,
    string? Razon)
{
    /// <summary>Outcome que indica que la evaluación debe seguir con la siguiente regla.</summary>
    public static MatchOutcome Continuar() => new(MatchOutcomeKind.Continuar, null, null);

    /// <summary>Outcome terminal: el match es incompatible por la razón indicada.</summary>
    public static MatchOutcome Incompatible(string razon) =>
        new(MatchOutcomeKind.Incompatible, null, razon);

    /// <summary>Outcome terminal: el match quedó clasificado con el tipo y razón indicados.</summary>
    public static MatchOutcome Clasificado(Modelos.MatchType tipo, string? razon = null) =>
        new(MatchOutcomeKind.Clasificado, tipo, razon);
}
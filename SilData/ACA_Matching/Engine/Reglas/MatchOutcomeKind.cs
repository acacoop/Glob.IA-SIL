namespace ACA.Matching.Engine.Reglas;

/// <summary>
/// Resultado posible de evaluar una regla del motor.
/// </summary>
public enum MatchOutcomeKind
{
    /// <summary>
    /// La regla detectó una incompatibilidad dura (mandatorio no cumplido).
    /// Cortocircuita la cadena: el motor devuelve el resultado final sin evaluar más reglas.
    /// </summary>
    Incompatible,

    /// <summary>
    /// La regla pudo clasificar el match como <see cref="Modelos.MatchType.Directo"/>,
    /// <see cref="Modelos.MatchType.Parcial"/> o <see cref="Modelos.MatchType.Condicional"/>.
    /// Cortocircuita la cadena (aunque en la práctica solo <see cref="ACA.Matching.Engine.Reglas.ClasificadorMatch"/>
    /// lo emite, al final de la cadena).
    /// </summary>
    Clasificado,

    /// <summary>
    /// La regla no pudo decidir: la evaluación continúa con la siguiente regla de la cadena.
    /// </summary>
    Continuar
}
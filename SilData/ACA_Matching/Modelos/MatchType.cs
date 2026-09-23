namespace ACA.Matching.Modelos;

/// <summary>
/// Clasificación del match entre una solicitud y un cupo.
/// Determinado por el <see cref="ACA.Matching.Engine.MatchingEngine"/>
/// a partir de las reglas de matching.
/// </summary>
public enum MatchType
{
    /// <summary>
    /// Todos los criterios solicitados coinciden exactamente.
    /// Mandatorios OK y opcionales coinciden o no fueron pedidos.
    /// </summary>
    Directo = 0,

    /// <summary>
    /// Mandatorios OK pero algún opcional falta o no coincide.
    /// El operador debe revisar la diferencia antes de asignar.
    /// </summary>
    Parcial = 1,

    /// <summary>
    /// La solicitud contiene observaciones (<see cref="SolicitudMatching.Observaciones"/> no vacío).
    /// Gana sobre Directo/Parcial: requiere confirmación explícita del operador.
    /// </summary>
    Condicional = 2
}
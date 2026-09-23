namespace ACA.Matching.Contexto;

/// <summary>
/// Implementación por defecto de <see cref="IContextoZona"/> que no conoce ninguna
/// asociación cupo→zona. Útil cuando el motor se invoca sin haber pre-cargado
/// el contexto (p.ej. cuando todas las solicitudes son <see cref="Modelos.TipoDestino.Destino"/>
/// y por lo tanto el contexto no se consulta).
/// </summary>
public sealed class ContextoZonaVacio : IContextoZona
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<long, IReadOnlyList<long>> ZonasPorCupo { get; } =
        new Dictionary<long, IReadOnlyList<long>>(0);
}
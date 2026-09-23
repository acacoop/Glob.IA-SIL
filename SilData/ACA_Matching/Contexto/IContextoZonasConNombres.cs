namespace ACA.Matching.Contexto;

/// <summary>
/// Variante enriquecida de <see cref="IContextoZona"/> que además expone el
/// nombre legible de cada zona geográfica. Útil para que la UI muestre el
/// destino del cupo sin un viaje extra a la BD.
/// </summary>
/// <remarks>
/// <para>
/// Hereda <see cref="ZonasPorCupo"/> de <see cref="IContextoZona"/>, por lo que
/// sigue siendo consumible directamente por <c>IMatchingEngine.Evaluar</c>
/// (que solo consulta IDs).
/// </para>
/// <para>
/// La lista de nombres en <see cref="NombresPorCupo"/> está indexada de forma
/// paralela a la de <see cref="IContextoZona.ZonasPorCupo"/>: si el cupo
/// pertenece a <c>[ZonaGeoA, ZonaGeoB]</c>, los nombres son
/// <c>[NombreA, NombreB]</c>, en el mismo orden. Si una zonaGeográficaId
/// resuelve a NULL en <c>ZONASGEOGRAFICAS.NOMBRE</c>, el slot correspondiente
/// también es <c>null</c>.
/// </para>
/// <para>
/// Cupos que no aparecen en <see cref="NombresPorCupo"/> se consideran "sin
/// zonas conocidas" y la UI mostrará "—" o el código crudo según el caso.
/// </para>
/// </remarks>
public interface IContextoZonasConNombres : IContextoZona
{
    /// <summary>
    /// Diccionario cupoId → lista de nombres de zonas geográficas, paralelo a
    /// <see cref="IContextoZona.ZonasPorCupo"/>.
    /// </summary>
    IReadOnlyDictionary<long, IReadOnlyList<string?>> NombresPorCupo { get; }
}

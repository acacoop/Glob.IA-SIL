namespace SILData.Services
{
    /// <summary>
    /// Resuelve la pertenencia cupo → ZonaGeograficas en bloque.
    /// Encapsula el JOIN <c>PUERTOPORZONA + ZONASGEOGRAFICAS</c> que el motor
    /// de matching no conoce (porque el motor no consulta la base de datos).
    /// </summary>
    public interface IZonaGeograficaResolver
    {
        /// <summary>
        /// Devuelve un diccionario cupoId → lista de ZonaGeoIds a los que pertenece el cupo.
        /// Cupos que no aparecen en el resultado se consideran "sin zona" (no matchearán
        /// contra solicitudes con <c>TipoDestino.ZonaPortuaria</c>).
        /// </summary>
        Task<ACA.Matching.Contexto.IContextoZona> ResolverAsync(IEnumerable<long> cupoIds);

        /// <summary>
        /// Igual que <see cref="ResolverAsync(IEnumerable{long})"/> pero además expone el
        /// nombre legible de cada zona (de <c>ZONASGEOGRAFICAS.NOMBRE</c>) en una lista paralela.
        /// Útil para que la UI muestre el destino del cupo en una sola query, sin un viaje
        /// extra al catálogo de zonas.
        /// </summary>
        /// <remarks>
        /// Implementación de un solo <c>SELECT</c> contra el mismo JOIN batch
        /// (<c>cuposcorre → PUERTOPORZONA → ZONASGEOGRAFICAS</c>) trayendo
        /// <c>ZONAGEOID</c> y <c>NOMBRE</c> en el mismo <c>SELECT</c>.
        /// </remarks>
        Task<ACA.Matching.Contexto.IContextoZonasConNombres> ResolverConNombresAsync(IEnumerable<long> cupoIds);
    }
}
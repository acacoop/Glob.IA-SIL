namespace ACA.Matching.Contexto;

/// <summary>
/// Contexto inyectado al motor para resolver la pertenencia cupo → zonas geográficas.
/// El motor NO consulta la base de datos: la pertenencia se resuelve por un JOIN
/// <c>PUERTOPORZONA + ZONASGEOGRAFICAS</c> en el servicio que carga los cupos
/// (vía <c>ZonaGeograficaResolver</c> en SILData) y se inyecta como diccionario.
/// </summary>
/// <remarks>
/// Solo se consulta cuando la solicitud es <see cref="Modelos.TipoDestino.ZonaPortuaria"/>.
/// Cuando la solicitud es <see cref="Modelos.TipoDestino.Destino"/>, el motor compara
/// directamente contra <see cref="Cupo.CodDestino"/> e ignora el contexto.
/// </remarks>
public interface IContextoZona
{
    /// <summary>
    /// Diccionario cupoId → lista de ZonaGeoIds a las que pertenece el cupo
    /// (un cupo puede pertenecer a múltiples zonas si su puerto aparece en varias).
    /// Si un cupo no aparece en el diccionario, se considera "no pertenece a ninguna zona".
    /// </summary>
    IReadOnlyDictionary<long, IReadOnlyList<long>> ZonasPorCupo { get; }
}
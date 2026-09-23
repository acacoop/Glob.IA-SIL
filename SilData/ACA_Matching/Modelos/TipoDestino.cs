namespace ACA.Matching.Modelos;

/// <summary>
/// Discriminador del campo <see cref="SolicitudMatching.Destino"/>.
/// Re-declarado en el motor para evitar que ACA_Matching dependa de SILData.
/// El adaptador en SILData convierte entre este enum y
/// <c>SILData.Model.TipoDestino</c> cuando construye un <see cref="SolicitudMatching"/>.
/// </summary>
public enum TipoDestino
{
    /// <summary>
    /// El destino es una <b>zona geográfica</b>: <see cref="SolicitudMatching.Destino"/>
    /// contiene el <c>ZonaGeoId</c> y se compara vía
    /// <see cref="ACA.Matching.Contexto.IContextoZona"/>.
    /// Caso habitual en producción.
    /// </summary>
    ZonaPortuaria = 0,

    /// <summary>
    /// El destino es un <b>puerto físico puntual</b>: <see cref="SolicitudMatching.Destino"/>
    /// contiene la <c>Cuenta</c> del puerto y se compara directamente contra
    /// <see cref="Cupo.CodDestino"/> (case-insensitive).
    /// </summary>
    Destino = 1
}
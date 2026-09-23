namespace ACA.Matching.Modelos;

/// <summary>
/// Representación inmutable de una solicitud para el motor de matching.
/// Es un record interno del motor (no depende de <c>SILData.Model.SolicitudTurno</c>)
/// para permitir tests aislados sin dependencias externas.
/// </summary>
/// <param name="Id">Identificador lógico de la solicitud.</param>
/// <param name="Grano">
/// Código de grano. <b>Mandatorio</b> para el matching.
/// Compara contra <see cref="Cupo.CodGrano"/>.
/// </param>
/// <param name="Vendedor">
/// Cuenta del vendedor. <b>Mandatorio</b> para el matching.
/// Compara contra <see cref="Cupo.CodVendSIL"/>.
/// </param>
/// <param name="Comprador">
/// Cuenta del comprador (opcional). Si la solicitud no especifica comprador
/// (<c>null</c>), este criterio no se exige.
/// Compara contra <see cref="Cupo.CodCompSIL"/>.
/// </param>
/// <param name="Destino">
/// Identificador de destino/zona — significado depende de <paramref name="TipoDestino"/>.
/// Si la solicitud no especifica destino (<c>null</c>), este criterio no se exige.
/// <list type="bullet">
///   <item>Si <paramref name="TipoDestino"/> == <see cref="TipoDestino.ZonaPortuaria"/>, contiene el <c>ZonaGeoId</c>.</item>
///   <item>Si <paramref name="TipoDestino"/> == <see cref="TipoDestino.Destino"/>, contiene la <c>Cuenta</c> del puerto físico.</item>
/// </list>
/// Compara contra <see cref="Cupo.CodDestino"/> o vía <see cref="ACA.Matching.Contexto.IContextoZona"/>.
/// </param>
/// <param name="TipoDestino">
/// Discrimina si <paramref name="Destino"/> representa una zona geográfica o un puerto puntual.
/// Re-declarado localmente como <see cref="TipoDestino"/> para evitar dependencia de SILData.
/// </param>
/// <param name="Observaciones">
/// Texto libre del solicitante. Si no es vacío, fuerza el resultado a
/// <see cref="MatchType.Condicional"/> (regla de prioridad absoluta del motor).
/// </param>
public sealed record SolicitudMatching(
    long Id,
    string Grano,
    string Vendedor,
    string? Comprador,
    string? Destino,
    TipoDestino? TipoDestino,
    string? Observaciones);
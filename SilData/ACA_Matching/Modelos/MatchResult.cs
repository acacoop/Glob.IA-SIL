using ACA.Matching.Engine.Evaluacion;

namespace ACA.Matching.Modelos;

/// <summary>
/// Resultado de evaluar un par (cupo, solicitud) por el motor de matching.
/// </summary>
/// <param name="Compatible">
/// <c>true</c> si la solicitud y el cupo son compatibles (mandatorios OK).
/// <c>false</c> si algún criterio mandatorio no se cumple.
/// </param>
/// <param name="Tipo">
/// Tipo de clasificación del match. <c>null</c> cuando <paramref name="Compatible"/> es <c>false</c>.
/// </param>
/// <param name="Cupo">Cupo evaluado (referencia, no clonado).</param>
/// <param name="Solicitud">Solicitud evaluada (referencia).</param>
/// <param name="RazonIncompatibilidad">
/// Razón específica cuando el match es Incompatible, o cuando es Parcial
/// indica qué opcional no coincidió. <c>null</c> cuando es Directo.
/// </param>
/// <remarks>
/// Las propiedades <see cref="VendedorCoincide"/>, <see cref="CompradorCoincide"/> y
/// <see cref="DestinoCoincide"/> se exponen como <c>init</c> aditivas: las fábricas
/// estáticas reciben opcionalmente el <see cref="ResultadoParcial"/> evaluado por las
/// reglas y copian sus flags. Esto permite que la UI renderice cada fila
/// (Vendedor / Comprador / Destino) sólo cuando el flag correspondiente es <c>true</c>.
/// </remarks>
public sealed record MatchResult(
    bool Compatible,
    MatchType? Tipo,
    Cupo Cupo,
    SolicitudMatching Solicitud,
    string? RazonIncompatibilidad)
{
    /// <summary>Flag copiado desde <see cref="ResultadoParcial.VendedorCoincide"/>.</summary>
    public bool VendedorCoincide { get; init; }

    /// <summary>Flag copiado desde <see cref="ResultadoParcial.CompradorCoincide"/>.</summary>
    public bool CompradorCoincide { get; init; }

    /// <summary>Flag copiado desde <see cref="ResultadoParcial.DestinoCoincide"/>.</summary>
    public bool DestinoCoincide { get; init; }

    /// <summary>
    /// Fábrica de resultado incompatible.
    /// </summary>
    /// <param name="cupo">Cupo evaluado.</param>
    /// <param name="solicitud">Solicitud evaluada.</param>
    /// <param name="razon">Razón de la incompatibilidad.</param>
    /// <param name="estado">
    /// Estado mutable de las reglas; opcional. Si viene, sus flags se copian al
    /// resultado. Si es <c>null</c>, los flags quedan en <c>false</c> (defensa).
    /// </param>
    public static MatchResult Incompatible(Cupo cupo, SolicitudMatching solicitud, string razon, ResultadoParcial? estado = null)
        => new(false, null, cupo, solicitud, razon)
        {
            VendedorCoincide = estado?.VendedorCoincide ?? false,
            CompradorCoincide = estado?.CompradorCoincide ?? false,
            DestinoCoincide = estado?.DestinoCoincide ?? false,
        };

    /// <summary>
    /// Fábrica de resultado compatible directo.
    /// </summary>
    /// <param name="cupo">Cupo evaluado.</param>
    /// <param name="solicitud">Solicitud evaluada.</param>
    /// <param name="estado">
    /// Estado mutable de las reglas; opcional. Por construcción del clasificador,
    /// un Directo implica los 3 flags en <c>true</c>; cuando <c>estado</c> es null,
    /// se mantienen los defaults en <c>true</c>.
    /// </param>
    public static MatchResult Directo(Cupo cupo, SolicitudMatching solicitud, ResultadoParcial? estado = null)
        => new(true, MatchType.Directo, cupo, solicitud, null)
        {
            VendedorCoincide = estado?.VendedorCoincide ?? true,
            CompradorCoincide = estado?.CompradorCoincide ?? true,
            DestinoCoincide = estado?.DestinoCoincide ?? true,
        };

    /// <summary>
    /// Fábrica de resultado compatible parcial.
    /// </summary>
    /// <param name="cupo">Cupo evaluado.</param>
    /// <param name="solicitud">Solicitud evaluada.</param>
    /// <param name="razon">Razón por la que algún opcional no coincide.</param>
    /// <param name="estado">
    /// Estado mutable de las reglas; opcional. Si viene, sus flags se copian al
    /// resultado. Si es <c>null</c>, los flags quedan en <c>false</c> (defensa).
    /// </param>
    public static MatchResult Parcial(Cupo cupo, SolicitudMatching solicitud, string razon, ResultadoParcial? estado = null)
        => new(true, MatchType.Parcial, cupo, solicitud, razon)
        {
            VendedorCoincide = estado?.VendedorCoincide ?? false,
            CompradorCoincide = estado?.CompradorCoincide ?? false,
            DestinoCoincide = estado?.DestinoCoincide ?? false,
        };

    /// <summary>
    /// Fábrica de resultado compatible condicional.
    /// </summary>
    /// <param name="cupo">Cupo evaluado.</param>
    /// <param name="solicitud">Solicitud evaluada.</param>
    /// <param name="razon">Razón de la condicionalidad (típicamente observaciones).</param>
    /// <param name="estado">
    /// Estado mutable de las reglas; opcional. Por construcción del clasificador,
    /// un Condicional corta antes por observaciones y los flags quedan en su
    /// valor calculado por las reglas previas. Cuando <c>estado</c> es null,
    /// se mantienen los defaults en <c>true</c>.
    /// </param>
    public static MatchResult Condicional(Cupo cupo, SolicitudMatching solicitud, string razon, ResultadoParcial? estado = null)
        => new(true, MatchType.Condicional, cupo, solicitud, razon)
        {
            VendedorCoincide = estado?.VendedorCoincide ?? true,
            CompradorCoincide = estado?.CompradorCoincide ?? true,
            DestinoCoincide = estado?.DestinoCoincide ?? true,
        };
}
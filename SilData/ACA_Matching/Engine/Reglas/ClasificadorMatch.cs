using ACA.Matching.Engine.Evaluacion;

namespace ACA.Matching.Engine.Reglas;

/// <summary>
/// Regla terminal: aplica la jerarquía de clasificación Condicional > Directo > Parcial
/// sobre el estado acumulado por las reglas previas.
/// </summary>
/// <remarks>
/// <para>Posición en la cadena: 5 (la última).</para>
/// <para>Reglas:</para>
/// <list type="number">
///   <item>Si la solicitud tiene observaciones → <see cref="Modelos.MatchType.Condicional"/> (prioridad absoluta).</item>
///   <item>Si mandatorios OK y todos los opcionales coinciden → <see cref="Modelos.MatchType.Directo"/>.</item>
///   <item>Si mandatorios OK pero algún opcional no coincide → <see cref="Modelos.MatchType.Parcial"/>.</item>
/// </list>
/// <para>
/// Defensa: si por algún error de orden de reglas el clasificador se invoca
/// con <see cref="ResultadoParcial.MandatoriosOk"/> == <c>false</c>, retorna Incompatible.
/// </para>
/// </remarks>
public sealed class ClasificadorMatch : IMatchRule
{
    /// <inheritdoc/>
    public string Nombre => "Clasificador";

    /// <inheritdoc/>
    public MatchOutcome Evaluar(
        ResultadoParcial estado,
        Cupo cupo,
        Modelos.SolicitudMatching solicitud,
        ACA.Matching.Contexto.IContextoZona? contextoZona)
    {
        // Defensa: si llegamos acá con mandatorios no OK, las reglas anteriores
        // deberían haber cortado. Si no lo hicieron (bug), emitimos Incompatible.
        if (!estado.MandatoriosOk)
        {
            return MatchOutcome.Incompatible(
                "Mandatorios no OK al clasificar (defensa, no debería ocurrir).");
        }

        // 1) Observaciones tienen prioridad absoluta.
        if (estado.TieneObservaciones)
        {
            return MatchOutcome.Clasificado(
                Modelos.MatchType.Condicional,
                "La solicitud contiene observaciones.");
        }

        // 2) Directo: todos los opcionales coinciden (o no fueron pedidos).
        if (estado.VendedorCoincide && estado.CompradorCoincide && estado.DestinoCoincide)
        {
            return MatchOutcome.Clasificado(Modelos.MatchType.Directo);
        }

        // 3) Parcial: mandatorios OK pero algún opcional no coincide.
        return MatchOutcome.Clasificado(
            Modelos.MatchType.Parcial,
            ConstruirRazonParcial(solicitud, estado));
    }

    private static string ConstruirRazonParcial(
        Modelos.SolicitudMatching solicitud,
        ResultadoParcial estado)
    {
        var motivos = new List<string>(3);

        // Vendedor: si el flag está bajo lo reportamos siempre. La regla
        // VendedorRule sólo setea el flag en false cuando hay mismatch real
        // (cupo sin vendedor con vendedor opuesto) o cupo-sin-vendedor con
        // vendedor solicitado, así que es seguro reportar sin filtrar por el
        // valor de Solicitud.Vendedor.
        if (!estado.VendedorCoincide)
            motivos.Add("Vendedor no coincide (cupo sin vendedor asignado)");

        // Comprador: si el flag está bajo, lo reportamos. El caso "la solicitud
        // no exige comprador pero el cupo sí" ahora también baja el flag
        // (CompradorRule): la razón aparece en Pantalla 2 para que el
        // operador entienda por qué el match no es Directo.
        if (!estado.CompradorCoincide)
            motivos.Add("Comprador no coincide");

        // Destino: ídem. La regla baja el flag cuando la solicitud no exige
        // zona pero el cupo sí (con CodDestino poblado), o cuando la solicitud
        // exige zona que el cupo no tiene. Reportamos siempre que el flag
        // esté bajo para que el operador tenga toda la información.
        if (!estado.DestinoCoincide)
            motivos.Add("Destino/Zona no coincide");

        return motivos.Count > 0 ? string.Join("; ", motivos) : "Criterios opcionales no coinciden";
    }
}
using ACA.Matching.Engine.Evaluacion;

namespace ACA.Matching.Engine.Reglas;

/// <summary>
/// Regla opcional: si la solicitud exige destino, debe coincidir según el
/// discriminador <see cref="Modelos.SolicitudMatching.TipoDestino"/>.
/// <list type="bullet">
///   <item>
///     <see cref="Modelos.TipoDestino.ZonaPortuaria"/> (caso habitual en producción):
///     el destino de la solicitud es un <c>ZonaGeoId</c> y debe estar entre las
///     zonas a las que pertenece el cupo (vía <see cref="ACA.Matching.Contexto.IContextoZona"/>).
///     <para>
///       Excepciones (la solicitud NO exige zona específica; la regla decide
///       según lo que diga el cupo):
///       <list type="bullet">
///         <item><c>Destino = null/whitespace</c>: la solicitud no trae zona.
///         Si el cupo tampoco trae zona → <c>DestinoCoincide = true</c>. Si el
///         cupo trae zona → <c>DestinoCoincide = false</c> y el match queda
///         <see cref="Modelos.MatchType.Parcial"/>. Esto refleja el caso real
///         en el que el operador de Pantalla 1 no eligió zona y por lo tanto
///         el cupo no puede matchear Directo contra cupos con destino.
///         </item>
///         <item><c>Destino = "0"</c> con <see cref="Modelos.TipoDestino.ZonaPortuaria"/>:
///         wildcard histórico (convención del store: <c>NVL(S.DEST, 0)</c>).
///         Misma semántica que el caso anterior: el cupo sin CodDestino
///         coincide, el cupo con CodDestino no.</item>
///       </list>
///     </para>
///   </item>
///   <item>
///     <see cref="Modelos.TipoDestino.Destino"/>: el destino es la <c>Cuenta</c>
///     de un puerto físico. Compara case-insensitive contra <see cref="Cupo.CodDestino"/>.
///   </item>
///   <item>
///     Destino no vacío con <c>TipoDestino = null</c>: estado inconsistente,
///     se retorna <see cref="MatchOutcome.Incompatible"/> defensivo (el operador
///     debería haber explicitado el tipo).
///   </item>
/// </list>
/// </summary>
/// <remarks>
/// <para>Posición en la cadena: 4 (después de Comprador).</para>
/// <para>
/// Esta regla es **opcional** (sección "Campos usados en cada regla" del plan
/// técnico): NUNCA corta la cadena por mismatch. Si no coincide, marca
/// <see cref="ResultadoParcial.DestinoCoincide"/> = <c>false</c> y la cadena
/// continúa. El <see cref="ClasificadorMatch"/> lo lee al final para emitir
/// <see cref="Modelos.MatchType.Parcial"/> si corresponde.
/// </para>
/// </remarks>
public sealed class DestinoRule : IMatchRule
{
    /// <inheritdoc/>
    public string Nombre => "Destino";

    /// <inheritdoc/>
    public MatchOutcome Evaluar(
        ResultadoParcial estado,
        Cupo cupo,
        Modelos.SolicitudMatching solicitud,
        ACA.Matching.Contexto.IContextoZona? contextoZona)
    {
        // Solicitud sin destino específico (null/whitespace/empty). No exige
        // zona, pero el match no puede ser Directo si el cupo sí tiene una:
        // marcamos DestinoCoincide sólo cuando el cupo tampoco restringe
        // zona, para que el clasificador emita Parcial cuando haya mismatch.
        if (string.IsNullOrWhiteSpace(solicitud.Destino))
        {
            bool cupoSinDestino = string.IsNullOrWhiteSpace(cupo.CodDestino);
            estado.DestinoCoincide = cupoSinDestino;
            return MatchOutcome.Continuar();
        }

        // Wildcard explícito "0" con ZonaPortuaria = misma semántica que el
        // caso anterior: la solicitud NO exige zona específica, queda
        // Parcial si el cupo tiene destino. Mantenido por compatibilidad con
        // filas históricas del store que guardan "0" en vez de NULL.
        if (EsZonaWildcard(solicitud))
        {
            bool cupoSinDestino = string.IsNullOrWhiteSpace(cupo.CodDestino);
            estado.DestinoCoincide = cupoSinDestino;
            return MatchOutcome.Continuar();
        }

        // Defensa: TipoDestino null con destino no nulo → estado inconsistente.
        // Mantenemos el corte con Incompatible porque es un bug de datos, no
        // un mismatch normal.
        if (!solicitud.TipoDestino.HasValue)
        {
            estado.MandatoriosOk = false;
            return MatchOutcome.Incompatible(
                "La solicitud tiene destino pero TipoDestino es nulo. El operador debe explicitar el tipo.");
        }

        bool coincide = solicitud.TipoDestino.Value switch
        {
            Modelos.TipoDestino.ZonaPortuaria =>
                CupoPerteneceAZona(cupo, solicitud.Destino, contextoZona),

            Modelos.TipoDestino.Destino =>
                !string.IsNullOrWhiteSpace(cupo.CodDestino)
                && string.Equals(
                    cupo.CodDestino.Trim(),
                    solicitud.Destino.Trim(),
                    StringComparison.OrdinalIgnoreCase),

            _ => false
        };

        // Regla opcional: marcamos el flag y continuamos SIEMPRE. El
        // ClasificadorMatch decide si esto termina en Directo o Parcial.
        estado.DestinoCoincide = coincide;
        return MatchOutcome.Continuar();
    }

    private static bool EsZonaWildcard(Modelos.SolicitudMatching solicitud)
    {
        return solicitud.TipoDestino == Modelos.TipoDestino.ZonaPortuaria
            && string.Equals(solicitud.Destino.Trim(), "0", StringComparison.OrdinalIgnoreCase);
    }

    private static bool CupoPerteneceAZona(
        Cupo cupo,
        string destinoSolicitado,
        ACA.Matching.Contexto.IContextoZona? contextoZona)
    {
        if (contextoZona is null)
            return false;

        if (!long.TryParse(destinoSolicitado, out long zonaId))
            return false;

        if (!contextoZona.ZonasPorCupo.TryGetValue(cupo.Id, out var zonas))
            return false;

        return zonas.Contains(zonaId);
    }
}

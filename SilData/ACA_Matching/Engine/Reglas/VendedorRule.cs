using ACA.Matching.Engine.Evaluacion;

namespace ACA.Matching.Engine.Reglas;

/// <summary>
/// Regla de vendedor: cuando la solicitud exige un vendedor puntual, debe
/// coincidir con <c>CodVendSIL</c> del cupo.
/// </summary>
/// <remarks>
/// <para>Posición en la cadena: 2 (después de Grano).</para>
/// <para>
/// Semántica:
/// </para>
/// <list type="bullet">
///   <item>
///     Si el cupo no tiene <c>CodVendSIL</c> y la solicitud sí pide vendedor,
///     el match es <see cref="MatchOutcomeKind.Continuar"/> con
///     <see cref="ResultadoParcial.VendedorCoincide"/> = <c>false</c>: el
///     clasificador terminal lo convierte en <see cref="Modelos.MatchType.Parcial"/>
///     ("cupo sin vendedor"). Esto le permite al operador verlo y decidir
///     manualmente.
///   </item>
///   <item>
///     Si la solicitud no trae vendedor (string vacío), no se puede comparar
///     nada y el match se reporta <see cref="MatchOutcomeKind.Incompatible"/>
///     (la solicitud exige un vendedor concreto que no sabemos cuál es).
///   </item>
///   <item>
///     Si ambos tienen vendedor pero no coinciden, es Incompatible mandatorio:
///     no hay forma de reconciliar.
///   </item>
/// </list>
/// </remarks>
public sealed class VendedorRule : IMatchRule
{
    /// <inheritdoc/>
    public string Nombre => "Vendedor";

    /// <inheritdoc/>
    public MatchOutcome Evaluar(
        ResultadoParcial estado,
        Cupo cupo,
        Modelos.SolicitudMatching solicitud,
        ACA.Matching.Contexto.IContextoZona? contextoZona)
    {
        // Si la solicitud no pide vendedor puntual, no podemos comparar nada:
        // el match queda Incompatible (la operación requiere saber a quién
        // asignarle el cupo).
        if (string.IsNullOrWhiteSpace(solicitud.Vendedor))
        {
            estado.MandatoriosOk = false;
            return MatchOutcome.Incompatible("La solicitud no tiene vendedor definido.");
        }

        // Cupo sin vendedor: el operador decidirá después. Marcamos el opcional
        // como no coincidente y dejamos que la cadena siga hasta el
        // ClasificadorMatch para emitir Parcial con la razón adecuada.
        // Consideramos "sin vendedor" tanto el null/whitespace como el "0"
        // literal: en cuposcorre (Oracle) los cupos sin vendedor asignado
        // se persisten como VENDCTA = 0, que Dapper materializa como "0".
        if (CupoSinVendedor(cupo.CodVendSIL))
        {
            estado.VendedorCoincide = false;
            return MatchOutcome.Continuar();
        }

        bool coincide = string.Equals(
            cupo.CodVendSIL.Trim(),
            solicitud.Vendedor.Trim(),
            StringComparison.OrdinalIgnoreCase);

        if (!coincide)
        {
            // Vendedores distintos: incompatibilidad dura. No se puede reconciliar.
            estado.MandatoriosOk = false;
            return MatchOutcome.Incompatible(
                $"Vendedor no coincide (cupo='{cupo.CodVendSIL}', solicitud='{solicitud.Vendedor}').");
        }

        // Match: marcamos explícito para que el flag no quede en su default.
        estado.VendedorCoincide = true;
        return MatchOutcome.Continuar();
    }

    /// <summary>
    /// Determina si el cupo no tiene vendedor asignado. Considera:
    /// <list type="bullet">
    ///   <item>null / "" / whitespace</item>
    ///   <item>"0" (cómo Oracle persiste el VENDCTA sin asignar)</item>
    /// </list>
    /// </summary>
    private static bool CupoSinVendedor(string? codVendSIL)
    {
        if (string.IsNullOrWhiteSpace(codVendSIL)) return true;
        // Dapper materializa VENDCTA NUMBER como string; "0" es el caso real
        // del cuposcorre. También aceptamos variantes con espacios.
        return string.Equals(codVendSIL.Trim(), "0", StringComparison.OrdinalIgnoreCase);
    }
}
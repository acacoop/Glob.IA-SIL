using ACA.Matching.Engine.Evaluacion;

namespace ACA.Matching.Engine.Reglas;

/// <summary>
/// Regla opcional: si la solicitud exige comprador, debe coincidir con
/// <c>CodCompSIL</c> del cupo.
/// </summary>
/// <remarks>
/// <para>Posición en la cadena: 3 (después de los mandatorios).</para>
/// <para>
/// Semántica (consistente con <see cref="DestinoRule"/>):
/// </para>
/// <list type="bullet">
///   <item>
///     Solicitud exige comprador y cupo no tiene → flag = false (no coincide).
///   </item>
///   <item>
///     Solicitud exige comprador y cupo tiene el mismo → flag = true.
///     Solicitud exige comprador y cupo tiene otro distinto → flag = false.
///   </item>
///   <item>
///     Solicitud no exige comprador → flag = (cupo tampoco tiene).
///     Si el cupo SÍ tiene, flag = false → la UI ocultará la fila Comprador
///     en Pantalla 2 (el criterio "no se comparó contra nada del cupo").
///   </item>
/// </list>
/// <para>
/// Esta regla NUNCA corta la cadena: siempre retorna <see cref="MatchOutcome.Continuar"/>.
/// El clasificador lee <see cref="ResultadoParcial.CompradorCoincide"/> al final.
/// </para>
/// </remarks>
public sealed class CompradorRule : IMatchRule
{
    /// <inheritdoc/>
    public string Nombre => "Comprador";

    /// <inheritdoc/>
    public MatchOutcome Evaluar(
        ResultadoParcial estado,
        Cupo cupo,
        Modelos.SolicitudMatching solicitud,
        ACA.Matching.Contexto.IContextoZona? contextoZona)
    {
        // Solicitud no exige comprador → flag = (cupo tampoco tiene).
        // Si el cupo SÍ tiene comprador, no coinciden: flag=false → fila oculta.
        // Si el cupo NO tiene comprador, ambos "saltean" el criterio: flag=true
        // → fila se muestra como "No informado".
        if (string.IsNullOrWhiteSpace(solicitud.Comprador))
        {
            estado.CompradorCoincide = string.IsNullOrWhiteSpace(cupo.CodCompSIL);
            return MatchOutcome.Continuar();
        }

        // Solicitud exige comprador pero el cupo no lo tiene → no coincide.
        if (string.IsNullOrWhiteSpace(cupo.CodCompSIL))
        {
            estado.CompradorCoincide = false;
            return MatchOutcome.Continuar();
        }

        bool coincide = string.Equals(
            cupo.CodCompSIL.Trim(),
            solicitud.Comprador.Trim(),
            StringComparison.OrdinalIgnoreCase);

        estado.CompradorCoincide = coincide;
        return MatchOutcome.Continuar();
    }
}
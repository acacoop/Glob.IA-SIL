using ACA.Matching.Engine.Reglas;
using ACA.Matching.Modelos;

namespace ACA.Matching.Engine.Evaluacion;

/// <summary>
/// Estado mutable compartido por las reglas durante la evaluación de un par (cupo, solicitud).
/// Las reglas de opcionales (<see cref="ACA.Matching.Engine.Reglas.CompradorRule"/>,
/// <see cref="ACA.Matching.Engine.Reglas.DestinoRule"/>) actualizan flags aquí;
/// <see cref="ACA.Matching.Engine.Reglas.ClasificadorMatch"/> los lee para emitir
/// la clasificación final.
/// </summary>
/// <remarks>
/// Este tipo es parte del contrato de <see cref="IMatchRule"/>
/// (las reglas lo reciben y lo mutan), por lo que debe ser público. Conceptualmente
/// sigue siendo un detalle de implementación del motor: los consumidores externos
/// solo necesitan <see cref="MatchResult"/>.
/// </remarks>
public sealed class ResultadoParcial
{
    /// <summary>
    /// <c>true</c> mientras todos los criterios mandatorios (Grano, Vendedor) hayan coincidido.
    /// Se inicializa en <c>true</c> y se baja a <c>false</c> si alguna regla mandatoria falla.
    /// </summary>
    public bool MandatoriosOk { get; set; } = true;

    /// <summary>
    /// <c>true</c> cuando la solicitud tiene observaciones no vacías.
    /// </summary>
    public bool TieneObservaciones { get; set; }

    /// <summary>
    /// <c>true</c> cuando comprador del cupo coincide con el de la solicitud,
    /// o cuando ambos no exigen comprador (solicitud sin comprador y cupo sin
    /// CodCompSIL). Si la solicitud no exige comprador pero el cupo sí tiene uno,
    /// el flag queda en <c>false</c>: la UI ocultará la fila Comprador en Pantalla 2
    /// porque el criterio "no se comparó contra nada del cupo".
    /// </summary>
    public bool CompradorCoincide { get; set; }

    /// <summary>
    /// <c>true</c> cuando el destino del cupo coincide con el de la solicitud
    /// según el <see cref="TipoDestino"/> (o cuando la solicitud no exige destino).
    /// </summary>
    public bool DestinoCoincide { get; set; }

    /// <summary>
    /// <c>true</c> cuando el vendedor del cupo coincide con el de la solicitud,
    /// o cuando alguno de los dos no exige vendedor (cupo sin CodVendSIL ó
    /// solicitud sin Vendedor explícito). Se inicializa en <c>true</c>: si la
    /// <see cref="VendedorRule"/> no se ejecuta (porque no se llegó a evaluar),
    /// el clasificador trata el vendedor como "ok" (no agrega motivo de Parcial).
    /// </summary>
    public bool VendedorCoincide { get; set; } = true;
}
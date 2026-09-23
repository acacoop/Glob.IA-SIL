using ACA.Matching.Contexto;
using ACA.Matching.Engine.Evaluacion;
using ACA.Matching.Engine.Reglas;
using ACA.Matching.Modelos;

namespace ACA.Matching.Engine;

/// <summary>
/// Implementación por defecto del motor de matching.
/// Compone una cadena de reglas con ORDEN FIJO (parte del contrato):
/// <c>Grano → Vendedor → Comprador → Destino → Clasificador</c>.
/// El primer outcome terminal (Incompatible o Clasificado) gana.
/// </summary>
public sealed class MatchingEngine : IMatchingEngine
{
    private readonly IReadOnlyList<IMatchRule> _cadena;

    /// <summary>
    /// Crea el motor con la cadena por defecto.
    /// </summary>
    public MatchingEngine()
        : this(ConstruirCadenaDefault())
    {
    }

    /// <summary>
    /// Crea el motor con una cadena explícita (útil para tests que quieran
    /// aislar una regla). En producción, usar el constructor sin parámetros.
    /// </summary>
    public MatchingEngine(IReadOnlyList<IMatchRule> cadena)
    {
        _cadena = cadena ?? throw new ArgumentNullException(nameof(cadena));
        if (_cadena.Count == 0)
            throw new ArgumentException("La cadena de reglas no puede estar vacía.", nameof(cadena));
    }

    /// <inheritdoc/>
    public MatchResult Evaluar(
        Cupo cupo,
        SolicitudMatching solicitud,
        IContextoZona? contextoZona = null)
    {
        if (cupo is null) throw new ArgumentNullException(nameof(cupo));
        if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));

        var estado = new ResultadoParcial
        {
            TieneObservaciones = !string.IsNullOrWhiteSpace(solicitud.Observaciones)
        };

        var ctx = contextoZona ?? new ContextoZonaVacio();

        foreach (var regla in _cadena)
        {
            var outcome = regla.Evaluar(estado, cupo, solicitud, ctx);

            if (outcome.Kind == MatchOutcomeKind.Incompatible)
            {
                return MatchResult.Incompatible(cupo, solicitud, outcome.Razon ?? "Incompatible", estado);
            }

            if (outcome.Kind == MatchOutcomeKind.Clasificado)
            {
                return outcome.Tipo switch
                {
                    Modelos.MatchType.Directo => MatchResult.Directo(cupo, solicitud, estado),
                    Modelos.MatchType.Parcial => MatchResult.Parcial(cupo, solicitud, outcome.Razon ?? "Criterios opcionales no coinciden", estado),
                    Modelos.MatchType.Condicional => MatchResult.Condicional(cupo, solicitud, outcome.Razon ?? "La solicitud contiene observaciones", estado),
                    _ => MatchResult.Incompatible(cupo, solicitud, outcome.Razon ?? "Tipo de clasificación desconocido", estado)
                };
            }
        }

        // Defensa: si la cadena no emitió ningún outcome terminal (todas Continuar),
        // clasificamos como Parcial conservador.
        return MatchResult.Parcial(cupo, solicitud, "La cadena de reglas no produjo clasificación final", estado);
    }

    /// <inheritdoc/>
    public IReadOnlyList<MatchResult> EvaluarTodos(
        Cupo cupo,
        IEnumerable<SolicitudMatching> solicitudes,
        IContextoZona? contextoZona = null)
    {
        if (cupo is null) throw new ArgumentNullException(nameof(cupo));
        if (solicitudes is null) throw new ArgumentNullException(nameof(solicitudes));

        var ctx = contextoZona ?? new ContextoZonaVacio();
        return solicitudes
            .Select(s => Evaluar(cupo, s, ctx))
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<MatchResult> CompatiblesCon(
        IEnumerable<Cupo> cupos,
        SolicitudMatching solicitud,
        IContextoZona? contextoZona = null)
    {
        if (cupos is null) throw new ArgumentNullException(nameof(cupos));
        if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));

        var ctx = contextoZona ?? new ContextoZonaVacio();
        return cupos
            .Select(c => Evaluar(c, solicitud, ctx))
            .ToList();
    }

    /// <summary>
    /// Cadena canónica de reglas. El orden es PARTE DEL CONTRATO: cambiarlo
    /// cambia la semántica del motor (p.ej. mover Clasificador antes de
    /// Destino haría que un destino no coincidente se reporte como Directo).
    /// </summary>
    public static IReadOnlyList<IMatchRule> ConstruirCadenaDefault() => new IMatchRule[]
    {
        new GranoRule(),
        new VendedorRule(),
        new CompradorRule(),
        new DestinoRule(),
        new ClasificadorMatch()
    };
}
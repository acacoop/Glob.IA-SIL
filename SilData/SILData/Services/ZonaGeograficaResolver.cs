using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace SILData.Services
{
    /// <summary>
    /// Implementación por defecto de <see cref="IZonaGeograficaResolver"/>.
    /// Resuelve la pertenencia cupo → ZonaGeograficas con un único JOIN batch
    /// contra <c>PUERTOPORZONA</c> + <c>ZONASGEOGRAFICAS</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// El motor recibe el resultado como <see cref="ACA.Matching.Contexto.IContextoZona"/>.
    /// </para>
    /// <para>
    /// Cadena de JOIN (mismo patrón que <c>CuposDisponibles.sql</c> y
    /// <c>CuposDisponiblesForShift.sql</c>):
    /// <c>cuposcorre.PUERTOCTA (NUMBER) → PUERTOPORZONA.CUENTA
    /// → PUERTOPORZONA.ZONAGEOID → ZONASGEOGRAFICAS.ZONAGEOID</c>.
    /// </para>
    /// <para>
    /// OJO: la entidad <c>Cupo</c> expone <c>CodDestino</c> como un string
    /// (derivado de <c>REPLACE(CP.Cuenta,'-','')</c>) pero ese campo NO es
    /// comparable contra <c>PUERTOPORZONA.CUENTA</c> (que es NUMBER).
    /// Por eso el resolver consulta <c>cuposcorre</c> directamente y une por
    /// la columna raw <c>PUERTOCTA</c>, sin pasar por la entidad Cupo.
    /// </para>
    /// </remarks>
    public class ZonaGeograficaResolver : IZonaGeograficaResolver
    {
        /// <summary>
        /// Cantidad de ids por lote en la cláusula IN. Oracle corta en 1000
        /// (ORA-01795); dejamos margen para no quedar pegados al límite.
        /// </summary>
        private const int TamanioLote = 900;

        private readonly string? _connectionString;
        private readonly ILogger<ZonaGeograficaResolver> _logger;

        public ZonaGeograficaResolver(IConfiguration configuration, ILogger<ZonaGeograficaResolver> logger)
        {
            _connectionString = configuration.GetConnectionString("SilConnection");
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ACA.Matching.Contexto.IContextoZona> ResolverAsync(IEnumerable<long> cupoIds)
        {
            var resultado = await ResolverConNombresAsync(cupoIds);
            // Adaptador liviano: el caller que sólo quiere IContextoZona no
            // necesita los nombres. Mantiene la compatibilidad con los
            // 3 callsites existentes (AcceptRequestsAsync + AccountData +
            // ShiftRequest), que ya consumen esta firma.
            return new ContextoZonaSoloIds(resultado.ZonasPorCupo);
        }

        /// <inheritdoc/>
        public async Task<ACA.Matching.Contexto.IContextoZonasConNombres> ResolverConNombresAsync(IEnumerable<long> cupoIds)
        {
            var ids = (cupoIds ?? Enumerable.Empty<long>()).Distinct().ToList();
            if (ids.Count == 0)
            {
                _logger.LogDebug("ZonaGeograficaResolver: lista de cupoIds vacía, se devuelve contexto vacío.");
                return EmptyContext();
            }

            try
            {
                using var connection = new OracleConnection(_connectionString);

                // JOIN batch: cupo → puerto (cuenta) → zonas geográficas.
                // PUERTOCTA (NUMBER) es la FK real hacia PUERTOPORZONA.CUENTA.
                // No usamos c.CodDestino porque es un varchar con guiones
                // (REPLACE(CP.Cuenta, '-', '')) y nunca matchearía la columna NUMBER.
                // Traemos ZG.NOMBRE en el mismo SELECT para evitar un viaje extra
                // al catálogo de zonas desde la UI.
                const string sql = @"
                    SELECT c.Id AS CupoId, zg.ZonaGeoId AS ZonaGeoId, zg.NOMBRE AS ZonaNombre
                    FROM cuposcorre c
                    INNER JOIN puertoporzona    ppz ON c.PUERTOCTA   = ppz.CUENTA
                    INNER JOIN zonasgeograficas zg  ON ppz.ZONAGEOID = zg.ZONAGEOID
                    WHERE c.Id IN :CupoIds";

                // Construimos dos diccionarios en paralelo (uno de IDs, otro de
                // nombres), recorriendo las filas una sola vez. Para un cupo con N
                // zonas, la posición i-ésima de NombresPorCupo[cupo] corresponde
                // a la misma posición de ZonasPorCupo[cupo].
                var zonasPorCupo = new Dictionary<long, IReadOnlyList<long>>();
                var nombresPorCupo = new Dictionary<long, IReadOnlyList<string?>>();

                // La lista se consulta POR LOTES. Oracle no admite más de 1000
                // expresiones en una lista IN (ORA-01795), y Dapper expande
                // `IN :CupoIds` a un parámetro por elemento. Mientras el caller
                // fue el matching por fila la lista era chica y nunca se llegó
                // al límite; el matching por ventana resuelve los cupos de TODOS
                // los granos del rango de una sola vez y lo supera con facilidad.
                //
                // Sin lotes el síntoma es especialmente malo: la excepción la
                // atrapa el catch de abajo, que devuelve un contexto vacío, y el
                // motor concluye que ningún cupo pertenece a ninguna zona. No
                // falla: contesta mal. Las solicitudes con zona se quedan sin
                // matches y nadie se entera.
                foreach (var lote in EnLotes(ids, TamanioLote))
                {
                    var rows = await connection.QueryAsync<(long CupoId, long ZonaGeoId, string? ZonaNombre)>(
                        sql,
                        new { CupoIds = lote });

                    foreach (var row in rows)
                    {
                        if (!zonasPorCupo.TryGetValue(row.CupoId, out var idsList))
                        {
                            idsList = new List<long>();
                            zonasPorCupo[row.CupoId] = idsList;
                        }
                        ((List<long>)idsList).Add(row.ZonaGeoId);

                        if (!nombresPorCupo.TryGetValue(row.CupoId, out var nombresList))
                        {
                            nombresList = new List<string?>();
                            nombresPorCupo[row.CupoId] = nombresList;
                        }
                        ((List<string?>)nombresList).Add(row.ZonaNombre);
                    }
                }

                _logger.LogDebug(
                    "ZonaGeograficaResolver: {Count} cupos procesados, {Found} con al menos una zona.",
                    ids.Count, zonasPorCupo.Count);

                return new ContextoZonaEnMemoria(zonasPorCupo, nombresPorCupo);
            }
            catch (OracleException ex)
            {
                // OJO: devolver contexto vacío no es una degradación benigna. El
                // motor va a concluir que ningún cupo pertenece a ninguna zona, y
                // toda solicitud CON zona se queda sin matches — respuesta 200 con
                // resultados incorrectos, que es peor que un error. Se mantiene el
                // comportamiento para no cambiarlo por izquierda en los 4 callers,
                // pero el log tiene que gritar.
                _logger.LogError(ex,
                    "Error de BD en ZonaGeograficaResolver resolviendo {Count} cupos. " +
                    "Se devuelve contexto de zonas VACIO: las solicitudes con zona " +
                    "geografica no van a matchear ningun cupo en esta consulta.",
                    ids.Count);
                return EmptyContext();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en ZonaGeograficaResolver.");
                return EmptyContext();
            }
        }

        /// <summary>
        /// Parte una lista en lotes consecutivos de a lo sumo
        /// <paramref name="tamanio"/> elementos, sin copiar la lista entera.
        /// </summary>
        private static IEnumerable<List<long>> EnLotes(List<long> origen, int tamanio)
        {
            for (int i = 0; i < origen.Count; i += tamanio)
                yield return origen.GetRange(i, Math.Min(tamanio, origen.Count - i));
        }

        /// <summary>
        /// Singleton con diccionarios vacíos para los caminos de error / lista vacía.
        /// Evita asignar dos <c>Dictionary&lt;,&gt;</c> por error.
        /// </summary>
        private static ACA.Matching.Contexto.IContextoZonasConNombres EmptyContext()
        {
            return new ContextoZonaEnMemoria(
                new Dictionary<long, IReadOnlyList<long>>(0),
                new Dictionary<long, IReadOnlyList<string?>>(0));
        }

        private sealed class ContextoZonaEnMemoria : ACA.Matching.Contexto.IContextoZonasConNombres
        {
            public ContextoZonaEnMemoria(
                Dictionary<long, IReadOnlyList<long>> zonas,
                Dictionary<long, IReadOnlyList<string?>> nombres)
            {
                ZonasPorCupo = zonas;
                NombresPorCupo = nombres;
            }

            public IReadOnlyDictionary<long, IReadOnlyList<long>> ZonasPorCupo { get; }
            public IReadOnlyDictionary<long, IReadOnlyList<string?>> NombresPorCupo { get; }
        }

        /// <summary>
        /// Adaptador liviano: expone un <see cref="ACA.Matching.Contexto.IContextoZona"/>
        /// clásico (sólo IDs) cuando el caller original no necesita los nombres.
        /// </summary>
        private sealed class ContextoZonaSoloIds : ACA.Matching.Contexto.IContextoZona
        {
            public ContextoZonaSoloIds(IReadOnlyDictionary<long, IReadOnlyList<long>> zonas)
            {
                ZonasPorCupo = zonas;
            }

            public IReadOnlyDictionary<long, IReadOnlyList<long>> ZonasPorCupo { get; }
        }
    }
}

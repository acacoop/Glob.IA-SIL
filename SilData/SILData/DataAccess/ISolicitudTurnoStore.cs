using SILData.Model.SolicitudTurno;

namespace SILData.DataAccess
{
  public interface ISolicitudTurnoStore
  {
    public Task<IEnumerable<SolicitudTurnoView>> GetByVendedorAsync(long cuentaVendedor, DateTime fechaDesde, DateTime fechaHasta);
    public Task<IEnumerable<SolicitudTurnoView>> GetByFilterAsync(SolicitudTurnosFilter solicitudTurnosFilter, DateTime fechaDesde, DateTime fechaHasta);
    public Task<IEnumerable<SolicitudTurnoView>> GetByFilterAsync(DateTime fechaDesde, DateTime fechaHasta, List<string> centros);
    public Task<IEnumerable<SolicitudTurnoView>> GetAllAsync(DateTime desde, DateTime hasta);

    /// <summary>
    /// Devuelve solicitudes filtrando por grano + rango de fechas, con vendedor/comprador/destino
    /// opcionales (a diferencia de <see cref="GetByFilterAsync(SolicitudTurnosFilter, DateTime, DateTime)"/>
    /// que exige CuentaVendedor). Usado por el endpoint bulk de matching.
    /// </summary>
    public Task<IEnumerable<SolicitudTurnoView>> GetByMatchesFilterAsync(MatchesSolicitudFilter filter, DateTime fechaDesde, DateTime fechaHasta);

    /// <summary>
    /// Devuelve todas las solicitudes de la ventana indicada, sin filtrar por
    /// grano / vendedor / comprador / destino. Alimenta el matching bulk por
    /// ventana (<c>POST /api/ShiftRequest/MatchesVentana</c>): una sola lectura
    /// de SOLTURNOS reemplaza a las N llamadas a
    /// <see cref="GetByMatchesFilterAsync"/> que hacía la grilla de Pantalla 1,
    /// una por fila.
    /// </summary>
    /// <param name="centros">
    /// Centros del operador. Aplica la misma regla de visibilidad que
    /// <see cref="GetByFilterAsync(DateTime, DateTime, List{string})"/>: entran
    /// las solicitudes sin zona geográfica y las cuya zona pertenece a alguno
    /// de esos centros. Null o vacío = sin filtro.
    /// </param>
    public Task<IEnumerable<SolicitudTurnoView>> GetVentanaParaMatchingAsync(
        DateTime fechaDesde,
        DateTime fechaHasta,
        List<string>? centros = null);

    /// <summary>
    /// Variante específica del matching bulk (no afecta a Pantalla 2 ni a otros flujos):
    /// devuelve las solicitudes pendientes que matcheen por grano + vendedor + fecha, y
    /// cuyo comprador y destino coincidan con los del cupo o estén vacíos (parcial).
    /// Cada llamada resuelve en un solo SELECT, sin joins extra y descartando las
    /// solicitudes con un comprador o destino distinto al del filtro.
    /// </summary>
    public Task<IEnumerable<SolicitudTurnoView>> GetForMatchingAsync(
        int codigoGrano,
        long cuentaVendedor,
        long cuentaComprador,
        long zonaGeograficaId,
        DateTime fechaDesde,
        DateTime fechaHasta);

    /// <summary>
    /// Variante "unificada" del matching bulk para Distribución (flujo V2,
    /// expuesta vía <c>POST /api/ShiftRequest/MatchesDistribucionV2</c>):
    /// ejecuta un único SELECT con INNER JOIN entre <c>cuposcorre</c> y
    /// <c>SOLTURNOS</c>, ya aplicando los filtros del operador (comprador,
    /// vendedor, puerto, grano, rango de fechas) y las invariantes
    /// obligatorias (fecha, grano, vendedor, comprador, destino por zona).
    ///
    /// El motor ACA.Matching.Engine sólo clasifica cada par devuelto (no
    /// tiene que filtrar). Reduce el cartesiano en memoria cuando hay
    /// varios cupos con fechas distintas dentro del rango.
    ///
    /// Reglas del WHERE / JOIN:
    /// <list type="bullet">
    ///   <item><c>cuposcorre.STATUS = 0 AND TIPO = 1</c>.</item>
    ///   <item><c>cuposcorre.Grano = :codigoGrano</c>.</item>
    ///   <item><c>cuposcorre.COMPCTA = :cuentaComprador</c>.</item>
    ///   <item><c>cuposcorre.PUERTOCTA = :cuentaPuerto</c>.</item>
    ///   <item><c>cuposcorre.Fecha BETWEEN :fechaDesde AND :fechaHasta</c>.</item>
    ///   <item>Vendedor del cupo:
    ///     <c>:vendedor &gt; 0 → cuposcorre.VENDCTA = :vendedor</c>;
    ///     <c>:vendedor = 0 → cuposcorre.VENDCTA IS NULL OR = 0</c> (cupos
    ///     sin vendedor asignado).</item>
    ///   <item>JOIN <c>SOLTURNOS.GRANO = cuposcorre.Grano</c>.</item>
    ///   <item>JOIN <c>SOLTURNOS.Fechasolicitada = cuposcorre.Fecha</c>.</item>
    ///   <item>JOIN vendedor: si cuposcorre.VENDCTA está set, entonces
    ///     <c>SOLTURNOS.CTAVEND = cuposcorre.VENDCTA</c>; si no, sin restricción.</item>
    ///   <item>JOIN comprador: si SOLTURNOS.CTACOMP está set, entonces
    ///     <c>SOLTURNOS.CTACOMP = cuposcorre.COMPCTA</c>; si no, entra igual (Parcial).</item>
    ///   <item>JOIN destino: si SOLTURNOS.DEST está set, entonces existe fila
    ///     en <c>puertoporzona</c> que conecta cuposcorre.PUERTOCTA con
    ///     SOLTURNOS.DEST; si no, entra igual (Parcial).</item>
    ///   <item><c>(CANTIDAD_ACEPTADA + CANTIDAD_RECHAZADA) &lt; CANTIDAD</c>
    ///     (solicitud pendiente).</item>
    ///   <item><c>cuposcorre.CentroDist = :codcentrodist</c>.</item>
    /// </list>
    /// </summary>
    public Task<IEnumerable<CupoConSolicitudMatchDto>> GetCuposConMatchesAsync(
        int codigoGrano,
        long vendedor,
        long cuentaComprador,
        string cuentaPuerto,
        string codcentro,
        string codcentrodist,
        DateTime fechaDesde,
        DateTime fechaHasta);
    public Task InsertAsync(IList<SolicitudTurno> solicitudes);
    public Task SaveAsync(IList<SolicitudTurno> inserciones, IList<SolicitudTurno> actualizaciones, IList<long> idsEliminar);
    public Task DeleteAsync(IList<long> solicitudIds);
    public Task UpdateCantidadAsync(long solicitudId, int cantidad);

    /// <summary>
    /// Ejecuta N operaciones de aceptación en una sola transacción.
    /// Para cada <see cref="AcceptOperation"/>:
    /// <list type="number">
    ///   <item>Invoca <see cref="IncrementAcceptedAsync"/> bajo la guardia
    ///     por acumuladores <c>CantidadAceptada &lt; Cantidad AND CantidadRechazada = 0 AND CUPO_ID IS NULL</c>. Si rowcount = 0, reporta Fallo.</item>
    ///   <item>Inserta una fila en <c>SOLTURNOS_DETALLE</c> por cada cupo aceptado.</item>
    /// </list>
    /// Reemplaza el antiguo <c>AcceptAndSplitRequestsAsync</c>: ya no se
    /// generan splits de SOLTURNOS (una solicitud siempre es 1 fila).
    /// </summary>
    public Task<IList<AcceptOperationResult>> AcceptRequestsAsync(IList<AcceptOperation> operations);

    /// <summary>
    /// Devuelve la entidad <see cref="SolicitudTurno"/> por id. Usado por el MVC
    /// para construir el payload de Accept (necesita todos los campos para
    /// serializarlos correctamente al backend). Devuelve <c>null</c> si no existe.
    /// Incluye los acumuladores <c>CantidadAceptada</c> y <c>CantidadFuturoAceptada</c>.
    /// </summary>
    public Task<SolicitudTurno?> GetByIdAsync(long id);

    /// <summary>
    /// Versión batch de <see cref="GetByIdAsync"/> (flag <c>Features:AcceptBatchLookup</c>).
    /// Devuelve las solicitudes existentes indexadas por Id. Ids ≤ 0 se ignoran;
    /// los inexistentes no aparecen en el diccionario. Consulta en lotes para
    /// respetar el límite de 1000 expresiones del IN de Oracle (ORA-01795).
    /// </summary>
    public Task<Dictionary<long, SolicitudTurno>> GetByIdsAsync(IEnumerable<long> ids);

    /// <summary>
    /// Rechaza una o varias solicitudes en estado Pendiente. Bajo el modelo
    /// "detalle acumulativo":
    /// <list type="bullet">
    ///   <item>NO se borran filas en <c>SOLTURNOS_DETALLE</c> (quedan como
    ///     historial de aceptaciones previas).</item>
    ///   <item>NO se liberan cupos en <c>CUPOSCORRE</c> (las aceptaciones que
    ///     figuran en detalle siguen vigentes — STATUS=2 en cupos).</item>
    ///   <item>SÓLO se congelan los cupos pendientes en los acumuladores
    ///     <c>CantidadRechazada</c> y <c>CantidadFuturoRechazada</c>, bajo la
    ///     guardia <c>CantidadAceptada + CantidadRechazada &lt; Cantidad</c>
    ///     para no pisar aceptaciones previas.</item>
    /// </list>
    /// </summary>
    /// <param name="solicitudIds">IDs de solicitudes a rechazar.</param>
    /// <returns>
    /// Tuplas (SolicitudId, Aceptado) — Aceptado=true cuando el UPDATE afectó
    /// la fila; false cuando la solicitud ya no estaba pendiente (conflicto).
    /// </returns>
    public Task<IList<(long SolicitudId, bool Aceptado)>> RejectRequestsAsync(IList<long> solicitudIds);

    // ====================================================================
    // SOLTURNOS_DETALLE — soporte para acumulación de aceptados
    // ====================================================================

    /// <summary>
    /// UPDATE atómico de SOLTURNOS: incrementa el acumulador
    /// <c>CantidadAceptada</c> o <c>CantidadFuturoAceptada</c> según
    /// <paramref name="esFuturo"/>. La guardia por acumuladores
    /// <c>CantidadAceptada &lt; Cantidad AND CantidadRechazada = 0 AND
    /// cupo_id IS NULL</c> garantiza comportamiento de concurrencia
    /// optimista: si la solicitud ya no está disponible, devuelve false.
    /// </summary>
    public Task<bool> IncrementAcceptedAsync(
      long solicitudId,
      bool esFuturo,
      int incremento,
      System.Data.IDbTransaction transaction);

    /// <summary>
    /// Inserta N filas en <c>SOLTURNOS_DETALLE</c>, una por cada cupo
    /// aceptado. Cada fila representa exclusivamente una solicitud que
    /// matcheó con un cupo y fue aceptada (sin columna de estado).
    /// Constraint UNIQUE en <c>(solicitud_id, cupo_id)</c>: si el cupo ya
    /// está registrado para esta solicitud, Oracle lanza ORA-00001 → rollback.
    /// </summary>
    public Task InsertAsignacionesDetalleAsync(
      long solicitudId,
      IReadOnlyList<long> cupoIds,
      System.Data.IDbTransaction transaction);

    /// <summary>
    /// Devuelve el resumen del estado de aceptación para una solicitud,
    /// calculado desde SOLTURNOS: <c>Asignados = CantidadAceptada</c>,
    /// <c>Pendientes = Cantidad - CantidadAceptada</c>. Rechazados es
    /// siempre 0 (los rechazados viven en cuposcorre, no en detalle).
    /// </summary>
    public Task<DetalleEstadoResumen> GetDetalleResumenAsync(long solicitudId);

    /// <summary>
    /// Devuelve un mapa <c>solicitudId → List&lt;cupoId&gt;</c> con todos los
    /// cupos ACEPTADOS para las solicitudes dadas. Sale de
    /// <c>SOLTURNOS_DETALLE</c> (cada fila representa una aceptación).
    /// Sirve para que la UI pueda restar los cupos ya otorgados del conjunto
    /// de matches del motor y mostrar al operador solamente los cupos que
    /// a&uacute;n puede asignar. Una sola query batched.
    /// </summary>
    public Task<Dictionary<long, List<long>>> GetCuposAceptadosPorSolicitudesAsync(
      IEnumerable<long> solicitudIds);

    /// <summary>
    /// Anula la distribución de uno o varios cupos en una sola llamada batch.
    /// Por cada <c>cupoId</c> se ejecutan los siguientes pasos atómicos
    /// (transacción propia):
    /// <list type="number">
    ///   <item>Lookup en <c>SOLTURNOS_DETALLE</c> para resolver
    ///     <c>solicitud_id</c>. Si no hay fila, el item se reporta como
    ///     <c>Skipped</c> y no se toca la BD (el cupo fue anulado por el
    ///     flujo legacy y no requiere reversión lógica).</item>
    ///   <item><c>UPDATE SOLTURNOS</c>: <c>CANTIDAD_ACEPTADA -= 1</c>,
    ///     <c>CANTIDAD += 1</c>, bajo la guardia <c>CANTIDAD_ACEPTADA &gt; 0
    ///     AND CANTIDAD_RECHAZADA = 0</c>. Si la solicitud ya no está en
    ///     estado pendiente (p.ej. rechazada completamente), rowcount = 0 y
    ///     se reporta Fallo.</item>
    ///   <item><c>DELETE SOLTURNOS_DETALLE</c> para el par
    ///     (solicitudId, cupoId).</item>
    /// </list>
    /// NO toca <c>CUPOSCORRE</c>: el cupo ya fue marcado como anulado por el
    /// flujo legacy de Anular.
    /// </summary>
    /// <param name="cupoIds">PKs de CUPOSCORRE / FKs en SOLTURNOS_DETALLE.</param>
    /// <returns>
    /// Lista de <see cref="AnularDistribucionItemResult"/> con un item por
    /// cada id del request (en el mismo orden).
    /// </returns>
    public Task<List<AnularDistribucionItemResult>> AnularDistribucionPorCuposAsync(IList<long> cupoIds);
  }
}

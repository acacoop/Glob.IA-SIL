using Domain.Entities.Externo;
using SILData.Model.SolicitudTurno;

namespace SILData.Services
{
  public interface ISolicitudTurnoService
  {
    public Task<IEnumerable<SolicitudTurnoView>> GetByVendedorAsync(long cuentaVendedor);
		public Task<IEnumerable<SolicitudTurnoView>> GetByFilterAsync(SolicitudTurnosFilter solicitudTurnosFilter);
    public Task<IEnumerable<SolicitudTurnoView>> GetByFilterAsync(SILSolicitudDeTurnosFilter solicitudTurnosFilter);
    public Task<IEnumerable<SolicitudTurnoView>> GetAllAsync(DateTime desde, DateTime hasta);
    public Task AddRequest(SolicitudTurnoCreate solicitudTurnoCreate);
    public Task UpdateRequest(SolicitudTurnoCreate solicitudTurnoCreate);
    public Task DeleteRequest(SolicitudTurnoCreate solicitudTurnoCreate);
    public Task<ShiftRequestAcceptResult> AcceptRequestsAsync(ShiftRequestAcceptData shiftRequestAcceptData);
    public Task<ShiftRequestRejectResult> RejectRequestsAsync(ShiftRequestRejectData shiftRequestRejectData);

    /// <summary>
    /// Busca todos los matches entre solicitudes pendientes y cupos disponibles
    /// para los filtros dados, ya clasificados por el motor como Directo / Parcial / Condicional.
    /// </summary>
    public Task<MatchesResultDto> BuscarMatchesAsync(MatchesFilterDto filter);

    /// <summary>
    /// Matching bulk por ventana: devuelve en una sola llamada los pares
    /// (solicitud, cupo) compatibles de TODAS las solicitudes pendientes del
    /// rango. Reemplaza al fan-out de una llamada a
    /// <see cref="BuscarMatchesAsync"/> por fila de grilla en Pantalla 1.
    /// </summary>
    public Task<MatchesVentanaResultDto> BuscarMatchesVentanaAsync(MatchesVentanaFilterDto filter);

    /// <summary>
    /// Variante específica para la pantalla de Distribución (CuposMatchingController).
    /// Usa la query que soporta solicitudes con comprador/destino NULL como Parcial.
    /// No afecta a Pantalla 2 (Solicitudes), que sigue usando
    /// <see cref="BuscarMatchesAsync"/>.
    /// </summary>
    public Task<MatchesResultDto> BuscarMatchesParaDistribucionAsync(MatchesFilterDto filter);

    /// <summary>
    /// Devuelve solicitudes para el motor de matching, filtrando por grano + rango
    /// de fechas. A diferencia de <see cref="GetByFilterAsync(SolicitudTurnosFilter)"/>,
    /// NO exige CuentaVendedor: vendedor/comprador/destino son opcionales.
    /// </summary>
    public Task<IEnumerable<SolicitudTurnoView>> GetByMatchesFilterAsync(MatchesSolicitudFilter filter);

    /// <summary>
    /// Devuelve la entidad <see cref="SolicitudTurno"/> por id (sin vista).
    /// Usado por el MVC para construir el payload de Accept sin duplicar
    /// estado en el cliente. <c>null</c> si no existe.
    /// </summary>
    public Task<SolicitudTurno?> GetByIdAsync(long id);

    /// <summary>
    /// Devuelve los cupos completos (entidad <c>Cupo</c>) cuya PK esté en la
    /// lista. Usado por el MVC para armar el payload de Accept.
    /// </summary>
    public Task<List<Cupo>> GetCuposByIdsAsync(List<long> ids);

    /// <summary>
    /// Devuelve el resumen de aceptación para una solicitud
    /// (Asignados/Pendientes/Rechazados), calculado desde los acumuladores de
    /// <c>SOLTURNOS</c>. Usado por Pantalla 2 y la grilla Index para mostrar el
    /// badge "Parcial: N/M".
    /// </summary>
    public Task<DetalleEstadoResumen> GetDetalleResumenAsync(long solicitudId);

    /// <summary>
    /// Devuelve un mapa <c>solicitudId → List&lt;cupoId&gt;</c> con todos los
    /// cupos ACEPTADOS para las solicitudes dadas. Sale de <c>SOLTURNOS_DETALLE</c>
    /// (cada fila es una aceptación). Sirve para que la UI pueda restar los
    /// cupos ya otorgados del conjunto de matches del motor.
    /// </summary>
    public Task<Dictionary<long, List<long>>> GetCuposAceptadosPorSolicitudesAsync(
      IEnumerable<long> solicitudIds);

    /// <summary>
    /// Anula la distribución de uno o varios cupos (batch). Por cada cupo
    /// con solicitud asociada en <c>SOLTURNOS_DETALLE</c>: localiza la
    /// solicitud, la devuelve al estado Pendiente (decrementa
    /// <c>CANTIDAD_ACEPTADA</c>, incrementa <c>CANTIDAD</c>) y borra la
    /// fila de detalle. Los cupos sin solicitud asociada quedan como
    /// <c>Skipped</c> (el flujo legacy de <c>CuposDataController.Anular</c>
    /// ya los cubrió). No toca <c>CUPOSCORRE</c>.
    /// </summary>
    /// <param name="cupoIds">PKs de CUPOSCORRE / FKs en SOLTURNOS_DETALLE.</param>
    public Task<AnularDistribucionResult> AnularDistribucionAsync(IList<long> cupoIds);
  }
}

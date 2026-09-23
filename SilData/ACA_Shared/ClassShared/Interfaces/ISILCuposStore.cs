using Domain.Entities.Externo;
using Domain.Entities.PanelControlLogistico;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ClassShared.Interfaces
{
  public interface ISILCuposStore
  {
    /// <summary>
    /// Obtiene los cupos dentro del periodo indicadp
    /// Cruza los datos de la cuposstop con la cuposcorre
    /// <param name="fechaDesde">Incluye</param>
    /// <param name="fechaHasta">Excluida</param>
    /// <returns></returns>
    Task<IList<Cupo>> FindCuposByPeriod(DateTime fechaDesde, DateTime fechaHasta);
    Task<IList<Cupo>> FindCuposForDasshboardByPeriodBy(ShiftSILBoardRequest filters);
    Task UpdateCuposDistributionAsync(IList<Cupo> cupos);
    /// <param name="centros">
    /// Centros que el operador logueado puede manipular (claims). Filtra por
    /// <c>cuposcorre.Centro</c>, que es el centro con el que se creo el cupo —
    /// NO por <c>CentroDist</c>, que se asigna reci&#233;n al distribuir. Null o
    /// vacio = sin filtro por centro.
    /// </param>
    Task<IList<Cupo>> FindAvailableCuposByPeriodAsync(DateTime fechaDesde, DateTime fechaHasta, int codigoGrano, long? cuentaVendedor, long? cuentaDestino, short? estadoSil, long? zonaGeograficaId = null, long? cuentaComprador = null, List<string>? centros = null);

    /// <summary>
    /// Devuelve los cupos completos (entidad <see cref="Cupo"/>) cuya PK
    /// esté dentro de <paramref name="ids"/>. Usado por el MVC para
    /// reconstruir los cupos seleccionados en el payload de Accept sin
    /// duplicar estado en el cliente.
    /// </summary>
    /// <param name="ids">Lista de IDs (positivos). Vacía o nula → lista vacía.</param>
    /// <returns>Cupos que coincidan. El orden NO está garantizado.</returns>
    Task<IList<Cupo>> GetCuposByIdsAsync(IList<long> ids);

    /// <summary>
    /// Resetea los cupos identificados a su estado "libre": STATUS=0 y campos
    /// de asignación (VENDCTA, COMPCTA, FECHAYHORAINFORMADO) a NULL. Se usa en
    /// el flujo de rechazo completo para liberar cupos que habían sido Otorgados
    /// mediante un Accept parcial previo.
    /// </summary>
    /// <param name="cupoIds">IDs de cupos a liberar. Vacía/nula → no-op.</param>
    /// <param name="transaction">
    /// Transacción abierta del caller. Permite coordinar la liberación con el
    /// rechazo de la solicitud en una sola transacción atómica.
    /// </param>
    Task FreeCuposByIdsAsync(IList<long> cupoIds, System.Data.IDbTransaction transaction);

    /// <summary>
    /// Variante NO transaccional de <see cref="FreeCuposByIdsAsync"/>. Útil
    /// cuando la liberación se hace FUERA del contexto transaccional del caller
    /// (e.g. paso de compensación best-effort tras un rechazo exitoso).
    /// </summary>
    Task FreeCuposByIdsAsync(IList<long> cupoIds);
  }
}

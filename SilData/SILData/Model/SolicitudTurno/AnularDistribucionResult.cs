using System.Collections.Generic;

namespace SILData.Model.SolicitudTurno
{
  /// <summary>
  /// Resultado del endpoint <c>POST /api/ShiftRequest/AnularDistribucion</c>
  /// (versión batch). El backend procesa cada <c>cupoId</c> de forma
  /// independiente y devuelve un <see cref="AnularDistribucionItemResult"/>
  /// por cada uno. La operación global se considera exitosa si al menos un
  /// item tiene <c>Estado == Exitoso</c>; el resultado individual de cada
  /// cupo está disponible en la lista.
  /// </summary>
  public class AnularDistribucionResult
  {
    /// <summary>
    /// True si al menos un item se procesó como <c>Exitoso</c>. False si
    /// todos los items son <c>Skipped</c> o <c>Fallo</c>. Útil para que el
    /// cliente decida si mostrar un mensaje informativo ("se re-habilitaron
    /// N solicitudes") o no.
    /// </summary>
    public bool AlMenosUnoExitoso { get; set; }

    /// <summary>
    /// Cantidad de cupos para los que se revirtió la asociación (tienen
    /// solicitud asociada y la reversión prosperó).
    /// </summary>
    public int CantidadExitosos { get; set; }

    /// <summary>
    /// Cantidad de cupos sin solicitud asociada en SOLTURNOS_DETALLE. Estos
    /// cupos quedan cubiertos por el flujo legacy de Anular y no requieren
    /// reversión lógica.
    /// </summary>
    public int CantidadSkipped { get; set; }

    /// <summary>
    /// Cantidad de cupos cuya reversión falló (solicitud ya no estaba
    /// pendiente, conflicto de concurrencia, etc.).
    /// </summary>
    public int CantidadFallos { get; set; }

    /// <summary>Detalle por cupo. Una entrada por cada id del request.</summary>
    public List<AnularDistribucionItemResult> Items { get; set; } = new();
  }

  /// <summary>
  /// Resultado individual por <c>cupoId</c>. Refleja una de tres
  /// situaciones mutuamente excluyentes:
  /// <list type="bullet">
  ///   <item><see cref="AnularDistribucionItemEstado.Exitoso"/>: el cupo
  ///     estaba asociado a una solicitud y la reversión aplicó.</item>
  ///   <item><see cref="AnularDistribucionItemEstado.Skipped"/>: el cupo
  ///     no estaba asociado a ninguna solicitud (no figura en
  ///     SOLTURNOS_DETALLE). No requiere acción — el flujo legacy de Anular
  ///     ya cubrió el cupo en CUPOSCORRE.</item>
  ///   <item><see cref="AnularDistribucionItemEstado.Fallo"/>: el cupo
  ///     estaba asociado pero la solicitud ya no estaba en estado
  ///     pendiente, o hubo un error técnico. <see cref="MotivoFalla"/>
  ///     describe la razón.</item>
  /// </list>
  /// </summary>
  public class AnularDistribucionItemResult
  {
    /// <summary>PK del cupo procesado (eco del request).</summary>
    public long CupoId { get; set; }

    /// <summary>Estado de la reversión para este cupo.</summary>
    public AnularDistribucionItemEstado Estado { get; set; }

    /// <summary>
    /// Id de la solicitud re-habilitada. Sólo presente cuando
    /// <see cref="Estado"/> es <see cref="AnularDistribucionItemEstado.Exitoso"/>.
    /// </summary>
    public long SolicitudId { get; set; }

    /// <summary>
    /// Motivo de Skipped o Fallo. Null cuando el item es Exitoso.
    /// </summary>
    public string? MotivoFalla { get; set; }
  }

  public enum AnularDistribucionItemEstado
  {
    /// <summary>Reversión aplicada (UPDATE + DELETE prosperaron).</summary>
    Exitoso = 0,

    /// <summary>
    /// El cupo no tenía solicitud asociada. El flujo legacy de Anular ya
    /// cubrió el cupo en CUPOSCORRE, así que no hay nada más que hacer.
    /// </summary>
    Skipped = 1,

    /// <summary>
    /// La solicitud asociada ya no estaba pendiente (rechazada, sin
    /// aceptados, etc.) o hubo un error técnico al revertir.
    /// </summary>
    Fallo = 2
  }
}
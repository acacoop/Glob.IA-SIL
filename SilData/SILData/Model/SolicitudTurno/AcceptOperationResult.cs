using System.Collections.Generic;

namespace SILData.Model.SolicitudTurno
{
  /// <summary>
  /// Resultado a nivel de store de una <see cref="AcceptOperation"/>:
  /// indica si la operación prosperó bajo la guardia de concurrencia
  /// y, en caso afirmativo, los IDs de los cupos asignados.
  /// </summary>
  public class AcceptOperationResult
  {
    /// <summary>ID de la solicitud procesada.</summary>
    public long SolicitudId { get; set; }

    /// <summary>
    /// True si el UPDATE de SOLTURNOS prosperó bajo la guardia por acumuladores.
    /// False si otro operador actuó antes (conflicto de concurrencia).
    /// </summary>
    public bool Exitoso { get; set; }

    /// <summary>
    /// ID del cupo asignado al parent (primer cupo de la operación,
    /// compatibilidad histórica; en el modelo v2 todos los cupos se
    /// reflejan en <see cref="CuposAsignados"/>).
    /// </summary>
    public long? CupoAsignadoId { get; set; }

    /// <summary>
    /// IDs de los cupos efectivamente aceptados en esta operación.
    /// Vacío si <see cref="Exitoso"/> es false.
    /// </summary>
    public List<long> CuposAsignados { get; set; } = new();

    /// <summary>Motivo de falla, presente cuando <see cref="Exitoso"/> es false.</summary>
    public string? MotivoFalla { get; set; }
  }
}

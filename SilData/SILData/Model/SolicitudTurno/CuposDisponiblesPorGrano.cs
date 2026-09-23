namespace SILData.Model.SolicitudTurno
{
  /// <summary>
  /// Vista en árbol de los cupos disponibles agrupados por grano.
  /// totalGrano es el resumen del grano (no requiere sumar nada del frontend).
  /// Detalles contiene las filas per-(comprador, zona). Hay 2 subtipos distinguibles
  /// por ZonaGeograficaId:
  ///   - ZonaGeograficaId == 0 → síntesis del comprador (suma de sus zonas).
  ///   - ZonaGeograficaId >  0 → detalle de esa zona puntual del comprador.
  /// Para evitar doble conteo, sumar SOLO los detalles con ZonaGeograficaId > 0;
  /// el totalGrano y las síntesis son redundantes.
  /// </summary>
  public class CuposDisponiblesPorGrano
  {
    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }
    public string CodigoGrano { get; set; }
    public string NombreGrano { get; set; }
    public int TotalGrano { get; set; }
    public List<CuposDetalle> Detalles { get; set; } = new List<CuposDetalle>();
  }

  public class CuposDetalle
  {
    public long CuentaComprador { get; set; }
    public string NombreComprador { get; set; }
    public int ZonaGeograficaId { get; set; }
    public string ZonaGeografica { get; set; }
    public int CuposDisponibles { get; set; }
  }
}

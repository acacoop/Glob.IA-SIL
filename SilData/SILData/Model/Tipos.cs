using System.ComponentModel;

namespace SILData.Model
{
  public enum TipoDestino
  {
    ZonaPortuaria = 0,
    Destino = 1
  }
  public enum EstadoCupoSIL : short
  {
    /// <summary>Cupo disponible (CUPOSCORRE.STATUS = 0).</summary>
    Libre = 0,
    /// <summary>Cupo otorgado por un Accept (CUPOSCORRE.STATUS = 2).</summary>
    Otorgado = 2
  }
}
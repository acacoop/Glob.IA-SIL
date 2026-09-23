using SILData.FormatExtension;
using System.Text.Json.Serialization;

namespace SILData.Model.SolicitudTurno
{
  public class FiltroZonaGeografica
  {
    public required long CuentaVendedora { get; set; }
    public long? CuentaCompradora { get; set; }
    public int? Grano { get; set; }

    private DateTime _fecha;

    [JsonConverter(typeof(CustomDateTimeConverter))]
    public required DateTime Fecha
    {
      get => _fecha;
      set => _fecha = value.Date; 
    }
    //public required DateTime Fecha {  get; set; }
  }
}

namespace SILData.Model.SolicitudTurno
{
  public class SolicitudTurnoCreate
  {
    public long? CuentaComprador { get; set; }
    public long CuentaVendedor { get; set; }
    public long? CuentaDestino { get; set; }
    public TipoDestino TipoDestino { get; set; }
    public int CodigoGrano { get; set; }
    public bool EsFuturo { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public string Centro { get; set; }
    public string? Observacion { get; set; }
    public required IEnumerable<SolicitudTurnoDiaCreate> SolicitudesDias { get; set; }

    public string GetError()
    {
      return SolicitudesDias.Where(x => !string.IsNullOrWhiteSpace(x.GetError())).FirstOrDefault()?.GetError();
    }

    public SolicitudTurnoCreate Clone()
    {
      return new SolicitudTurnoCreate
      {
        CuentaComprador = this.CuentaComprador,
        CuentaVendedor = this.CuentaVendedor,
        CuentaDestino = this.CuentaDestino,
        TipoDestino = this.TipoDestino,
        CodigoGrano = this.CodigoGrano,
        EsFuturo = this.EsFuturo,
        FechaSolicitud = this.FechaSolicitud,
        Centro = this.Centro,
        Observacion = this.Observacion,
        SolicitudesDias = this.SolicitudesDias.Select(d => d.Clone()).ToList() // Clonación profunda de la lista
      };
    }
  }

  public class SolicitudTurnoDiaCreate
  {
    private const int Limit = 100;
    private string Error { get; set; }
    public DateTime FechaSolicitado { get; set; }
    public int Cantidad { get; set; }

    public bool Validate()
    {
      if (Cantidad < Limit)
      {
        return true;
      }
      else
      {
        Error = $"Superó el límite de {Limit}";
        return false;
      }
    }

    public string GetError()
    {
      return Error;
    }
    public SolicitudTurnoDiaCreate Clone()
    {
      return new SolicitudTurnoDiaCreate
      {
        FechaSolicitado = this.FechaSolicitado,
        Cantidad = this.Cantidad,
        Error = this.Error // Se copia el error también
      };
    }
  }
}

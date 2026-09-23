using System.ComponentModel.DataAnnotations;

namespace SILData.Model.SolicitudTurno
{
  public class CuposCorreResult
  {
    public long Id { get; set; }
    [MaxLength(150)]
    public string? Alfanumerico { get; set; }
    public DateTime? Fecha { get; set; }
    [MaxLength(50)]
    public string? CentroCupo { get; set; }
    [MaxLength(50)]
    public string? CentroDist { get; set; }
    [MaxLength(50)]
    public string? CodGrano { get; set; }
    [MaxLength(150)]
    public string? NomGrano { get; set; }
    [MaxLength(75)]
    public string? CodDestino { get; set; }
    [MaxLength(150)]
    public string? NomDestino { get; set; }
    [MaxLength(75)]
    public string? CodDestinatario { get; set; }
    [MaxLength(150)]
    public string? NomDestinatario { get; set; }
    [MaxLength(75)]
    public string? CodTitularDeCCPP { get; set; }
    [MaxLength(150)]
    public string? NomTitularDeCCPP { get; set; }
    [MaxLength(150)]
    public string? NomRteComProductor { get; set; }
    [MaxLength(75)]
    public string? CodRteComProductor { get; set; }
    [MaxLength(75)]
    public string? CodRteComVtaPrimaria { get; set; }
    [MaxLength(150)]
    public string? NomRteComVtaPrimaria { get; set; }
    [MaxLength(75)]
    public string? CodRteComVtaSecundaria { get; set; }
    [MaxLength(150)]
    public string? NomRteComVtaSecundaria { get; set; }
    [MaxLength(75)]
    public string? CodRteComVtaSecundaria2 { get; set; }
    [MaxLength(150)]
    public string? NomRteComVtaSecundaria2 { get; set; }
    [MaxLength(75)]
    public string? CodMercATermino { get; set; }
    [MaxLength(150)]
    public string? NomMercATermino { get; set; }
    [MaxLength(75)]
    public string? CodCorVtaPrimaria { get; set; }
    [MaxLength(150)]
    public string? NomCorVtaPrimaria { get; set; }
    [MaxLength(75)]
    public string? CodCorVtaSecundaria { get; set; }
    [MaxLength(150)]
    public string? NomCorVtaSecundaria { get; set; }
    [MaxLength(75)]
    public string? CodEntregador { get; set; }
    [MaxLength(150)]
    public string? NomEntregador { get; set; }
    [MaxLength(75)]
    public string? CodCompSIL { get; set; }
    [MaxLength(150)]
    public string? NomCompSIL { get; set; }
    [MaxLength(75)]
    public string? CodVendSIL { get; set; }
    [MaxLength(150)]
    public string? NomVendSIL { get; set; }
    public short EstadoSIL { get; set; }
    public DateTime? FechaInformadoSIL { get; set; }
    public string? MotivoBajaSIL { get; set; }
    public string? ObservacionSIL { get; set; }
    [MaxLength(75)]
    public string? UsuarioSIL { get; set; }
    public DateTime? SyncDate { get; set; }
  }
}

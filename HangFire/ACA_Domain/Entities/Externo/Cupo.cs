using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Externo
{
  [Index(nameof(Fecha), nameof(Alfanumerico), nameof(CodDestino), IsUnique =false, Name = "idx_fechaXalfaYdestino")]
  public class Cupo
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
    [MaxLength(150)]
    public string? EstadoSTOP { get; set; }
    [MaxLength(150)]
    public string? CTG { get; set; }
    public DateTime? CTGDesde { get; set; }
    public DateTime? CTGHasta { get; set; }
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
    [MaxLength(75)]
    public string? CodFlete { get; set; }
    [MaxLength(150)]
    public string? NomFlete { get; set; }
    [MaxLength(75)]
    public string? CodTransportista { get; set; }
    [MaxLength(150)]
    public string? NomTransportista { get; set; }
    [MaxLength(75)]
    public string? CodChofer { get; set; }
    [MaxLength(150)]
    public string? NomChofer { get; set; }
    [MaxLength(75)]
    public string? CartaPorte { get; set; }
    public DateTime? CartaPorteVto { get; set; }
    public DateTime? CartaPorteFechaCarga { get; set; }
    public bool EstaSIL { get; set; }
    public short EstadoSIL { get; set; }
    public DateTime? FechaInformadoSIL { get; set; }
    public bool EstaSTOP { get; set; }
    public bool CuitChoferAfip { get; set; }
    public bool CuitCorredorCAfip { get; set; }
    public bool CuitCorredorVAfip { get; set; }
    public bool CuitDestinatarioAfip { get; set; }
    public bool CuitDestinoAfip { get; set; }
    public bool CuitInterAfip { get; set; }
    public bool CuitInterFleteAfip { get; set; }
    public bool CuitMaterminoAfip { get; set; }
    public bool CuitOrigenAfip { get; set; }
    public bool CuitRemComercialAfip { get; set; }
    public bool CuitEntregadorAfip { get; set; }
    public bool CuitTransportistaAfip { get; set; }
    [MaxLength(50)]
    public string? Estado { get; set; }
    public bool EsAnulado { get; set; }
    public bool EsRechazado { get; set; }
    public bool Desvio { get; set; }
    public long IdCupoStop { get; set; }
    [MaxLength(75)]
    public string? IdTurnoDetalle { get; set; }
    [MaxLength(50)]
    public string? IdCupoEstadoStop { get; set; }
    public DateTime? FechaActivado { get; set; }
    public DateTime? FechaArribado { get; set; }
    public DateTime? FechaRechazado { get; set; }
    public DateTime? FechaDesviadoO { get; set; }
    public DateTime? FechaRegresado { get; set; }
    public DateTime? FechaDesviadoD { get; set; }
    public DateTime? FechaAnulado { get; set; }
    public DateTime? FechaConfirmado { get; set; }
    public DateTime? FechaDescargado { get; set; }
    public DateTime? FechaReactivado { get; set; }
    public DateTime? FechaTomado { get; set; }
    public DateTime? FechaCreado { get; set; }
    public DateTime? FechaModificado { get; set; }
    public int CreadoPor { get; set; }
    public int ModificadoPor { get; set; }
    public int CodLocalidadOrigen { get; set; }
    public int CodLocalidadDestino { get; set; }
    [MaxLength(50)]
    public string? Cosecha { get; set; }
    [MaxLength(75)]
    public string? RENSPA { get; set; }
    public int NroEstabOrigen { get; set; }
    public int PesoOriginal { get; set; }
    public int PesoNetoEstimado { get; set; }
    public double KMRecorrer { get; set; }
    public bool ValidaKM { get; set; }
    public int CantHsSalidaCamion { get; set; }
    [MaxLength(75)]
    public string? Dominio { get; set; }
    [MaxLength(75)]
    public string? Dominio1 { get; set; }
    [MaxLength(75)]
    public string? Dominio2 { get; set; }
    [MaxLength(75)]
    public string? NroContrato { get; set; }
    public int NroPlantaRUCA { get; set; }
    public short IdEstadoEnPlanta { get; set; }
    public bool ConsultadoPorAfip { get; set; }
    [MaxLength(150)]
    public string? UltimaLatitud { get; set; }
    [MaxLength(150)]
    public string? UltimaLongitud { get; set; }
    public string? MotivoBajaSIL { get; set; }    
    public string? ObservacionSIL { get; set; }
    [MaxLength(75)]
    public string? UsuarioSIL { get; set; }

    public DateTime? SyncDate { get; set; }

    public bool Turneable {  get; set; }

    public Cupo()
    {

    }

    //public dynamic GetPropertieValues(List<string> propertyNames) 
    //{
    //  dynamic result = this.get (propertyNames);
    //  return result;
    //}

  }//end Cupo
}
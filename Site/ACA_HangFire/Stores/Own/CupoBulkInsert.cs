using System.Data;
using System.Data.SqlClient;

namespace HangFire.Stores.Own
{
  public class CupoBulkInsert
  {
    private readonly string _connectionString;
    private ILogger _logger;
    public CupoBulkInsert(string connectionString)
    {
      _connectionString = connectionString;
    }

    public async Task<int> Insert(DataTable cuposDT, ILogger logger)
    {
      _logger = logger;
      _logger.LogInformation(_connectionString);
      using (SqlConnection destinationConnection = new SqlConnection(_connectionString))
      {
        if (destinationConnection.State == ConnectionState.Closed)
        {
          await destinationConnection.OpenAsync();
        }
        using (SqlBulkCopy bulkCopy = this.NewBulkCopyCupo(destinationConnection))
        {
          try
          {
            bulkCopy.WriteToServer(cuposDT);
            return cuposDT.Rows.Count;
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
            return 0;
          }
        }
      }
    }

    private SqlBulkCopy NewBulkCopyCupo(SqlConnection destinationConnection) 
    {
      SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection);
      bulkCopy.DestinationTableName = "dbo.Cupo";
      bulkCopy.BatchSize = 10000;
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Id", "Id"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Alfanumerico", "Alfanumerico"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Fecha", "Fecha"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CentroCupo", "CentroCupo"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CentroDist", "CentroDist"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodGrano", "CodGrano"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomGrano", "NomGrano"));     
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EstadoSTOP", "EstadoSTOP"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CTG", "CTG"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CTGDesde", "CTGDesde"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CTGHasta", "CTGHasta"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodDestino", "CodDestino"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomDestino", "NomDestino"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodDestinatario", "CodDestinatario"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomDestinatario", "NomDestinatario"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodTitularDeCCPP", "CodTitularDeCCPP"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomTitularDeCCPP", "NomTitularDeCCPP"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodRteComProductor", "CodRteComProductor"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomRteComProductor", "NomRteComProductor"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodRteComVtaPrimaria", "CodRteComVtaPrimaria"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomRteComVtaPrimaria", "NomRteComVtaPrimaria"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodRteComVtaSecundaria", "CodRteComVtaSecundaria"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomRteComVtaSecundaria", "NomRteComVtaSecundaria"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodRteComVtaSecundaria2", "CodRteComVtaSecundaria2"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomRteComVtaSecundaria2", "NomRteComVtaSecundaria2"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodMercATermino", "CodMercATermino"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomMercATermino", "NomMercATermino"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodCorVtaPrimaria", "CodCorVtaPrimaria"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomCorVtaPrimaria", "NomCorVtaPrimaria"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodCorVtaSecundaria", "CodCorVtaSecundaria"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomCorVtaSecundaria", "NomCorVtaSecundaria"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodEntregador", "CodEntregador"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomEntregador", "NomEntregador"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodVendSIL", "CodVendSIL"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomVendSIL", "NomVendSIL"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodCompSIL", "CodCompSIL"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomCompSIL", "NomCompSIL"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodFlete", "CodFlete"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomFlete", "NomFlete"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodTransportista", "CodTransportista"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomTransportista", "NomTransportista"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodChofer", "CodChofer"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NomChofer", "NomChofer"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CartaPorte", "CartaPorte"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CartaPorteVto", "CartaPorteVto"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CartaPorteFechaCarga", "CartaPorteFechaCarga"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EstaSIL", "EstaSIL"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EstadoSIL", "EstadoSIL"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaInformadoSIL", "FechaInformadoSIL"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EstaSTOP", "EstaSTOP"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitChoferAfip", "CuitChoferAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitCorredorCAfip", "CuitCorredorCAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitCorredorVAfip", "CuitCorredorVAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitDestinatarioAfip", "CuitDestinatarioAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitDestinoAfip", "CuitDestinoAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitInterAfip", "CuitInterAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitInterFleteAfip", "CuitInterFleteAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitMaterminoAfip", "CuitMaterminoAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitOrigenAfip", "CuitOrigenAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitRemComercialAfip", "CuitRemComercialAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitEntregadorAfip", "CuitEntregadorAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CuitTransportistaAfip", "CuitTransportistaAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Estado", "Estado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EsAnulado", "EsAnulado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EsRechazado", "EsRechazado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Desvio", "Desvio"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IdCupoStop", "IdCupoStop"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IdTurnoDetalle", "IdTurnoDetalle"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IdCupoEstadoStop", "IdCupoEstadoStop"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaActivado", "FechaActivado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaArribado", "FechaArribado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaRechazado", "FechaRechazado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaDesviadoO", "FechaDesviadoO"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaDesviadoD", "FechaDesviadoD"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaRegresado", "FechaRegresado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaAnulado", "FechaAnulado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaConfirmado", "FechaConfirmado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaDescargado", "FechaDescargado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaReactivado", "FechaReactivado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaTomado", "FechaTomado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaCreado", "FechaCreado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FechaModificado", "FechaModificado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CreadoPor", "CreadoPor"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ModificadoPor", "ModificadoPor"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodLocalidadOrigen", "CodLocalidadOrigen"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CodLocalidadDestino", "CodLocalidadDestino"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Cosecha", "Cosecha"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("RENSPA", "RENSPA"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NroEstabOrigen", "NroEstabOrigen"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PesoOriginal", "PesoOriginal"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PesoNetoEstimado", "PesoNetoEstimado"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("KMRecorrer", "KMRecorrer"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ValidaKM", "ValidaKM"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CantHsSalidaCamion", "CantHsSalidaCamion"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Dominio", "Dominio"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Dominio1", "Dominio1"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Dominio2", "Dominio2"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NroContrato", "NroContrato"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NroPlantaRUCA", "NroPlantaRUCA"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IdEstadoEnPlanta", "IdEstadoEnPlanta"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ConsultadoPorAfip", "ConsultadoPorAfip"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("UltimaLatitud", "UltimaLatitud"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("UltimaLongitud", "UltimaLongitud"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("MotivoBajaSIL", "MotivoBajaSIL"));      
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ObservacionSIL", "ObservacionSIL"));
      bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("UsuarioSIL", "UsuarioSIL"));
      return bulkCopy;

    }
  }
}

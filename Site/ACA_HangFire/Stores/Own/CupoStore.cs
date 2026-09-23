using Domain.Entities.Externo;
using Newtonsoft.Json;
using Shared.ClassShared;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace HangFire.Stores.Own
{
  public class CupoStore : IOwnCupoStore
  {
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;
    public ILogger logger { get; }

    public CupoStore(IConfiguration configuration, ILogger<CupoStore> logger)
    {
      _configuration = configuration;
      _connectionString = configuration.GetConnectionString("OwnConnection");
      this.logger = logger;
    }
    public async Task<DateTime> FindLastDate()
    {
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        string sql = "SELECT TOP 1 fecha FROM cupo ORDER BY fecha DESC";
        using (SqlCommand cmd = new SqlCommand(sql, connection))
        {
          await connection.OpenAsync();
          cmd.CommandType = CommandType.Text;
          DateTime fecha = new DateTime();
          using (var reader = await cmd.ExecuteReaderAsync())
          {
            if (await reader.ReadAsync())
            {
              fecha = Convert.ToDateTime(reader["fecha"]);
            }
          }
          
          return (fecha == default(DateTime))? new DateTime(2019,4,22): fecha;
        }
      }
    }

    public async Task<int> Insert(Cupo cupo)
    {
      int rowsAffected = 0;
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        await connection.OpenAsync();
        string sql = "INSERT INTO cupos(id,alfanumerico) VALUES(@param1,@param2)";
        using (SqlCommand cmd = new SqlCommand(sql, connection))
        {
          cmd.Parameters.Add("@param1", SqlDbType.BigInt).Value = cupo.Id;
          cmd.Parameters.Add("@param2", SqlDbType.NVarChar, 50).Value = cupo.Alfanumerico;
          cmd.CommandType = CommandType.Text;
          rowsAffected = await cmd.ExecuteNonQueryAsync();
        }
      }
      return rowsAffected;
    }

    public async Task<int> Insert(IList<Cupo> cupos)
    {
      logger.LogInformation("HangFire: Inserto cupos");
      int countReg = 0;
      using (SqlConnection conn = new SqlConnection(_connectionString))
      {
        conn.Open();
        using (SqlTransaction tran = conn.BeginTransaction())
        {
          string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
          string queryLocation = Path.Combine(executableLocation, _configuration["QueryCupoInsert"]);
          StreamReader fileReader = new StreamReader(queryLocation);
          string insert = fileReader.ReadToEnd();
          DateTime defaultDate = new DateTime();

          using (SqlCommand cmd = conn.CreateCommand())
          {
            cmd.Connection = conn;
            cmd.Transaction = tran;
            cmd.CommandText = insert;            
            try
            {
              cmd.CommandType = CommandType.Text;
              cmd.Connection = conn;
              cmd.Parameters.Add("@paramAlfanumerico", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramFecha", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramCentroCupo", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCentroDist", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodGrano", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomGrano", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramEstadoSTOP", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCTG", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodDestino", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomDestino", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodDestinatario", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomDestinatario", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodTitularDeCCPP", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomTitularDeCCPP", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomRteComProductor", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodRteComProductor", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodRteComVtaPrimaria", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomRteComVtaPrimaria", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodRteComVtaSecundaria", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomRteComVtaSecundaria", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodRteComVtaSecundaria2", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomRteComVtaSecundaria2", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodMercATermino", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomMercATermino", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodCorVtaPrimaria", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomCorVtaPrimaria", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodCorVtaSecundaria", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomCorVtaSecundaria", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramUsuarioSIL", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramEstaSIL", SqlDbType.Bit);
              cmd.Parameters.Add("@paramEstadoSIL", SqlDbType.Int);
              cmd.Parameters.Add("@paramFechaInformadoSIL", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramCTGDesde", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramCTGHasta", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramCantHsSalidaCamion", SqlDbType.Int);
              cmd.Parameters.Add("@paramCartaPorte", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCartaPorteFechaCarga", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramCartaPorteVto", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramCodChofer", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodEntregador", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodFlete", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodLocalidadDestino", SqlDbType.Int);
              cmd.Parameters.Add("@paramCodLocalidadOrigen", SqlDbType.Int);
              cmd.Parameters.Add("@paramCodTransportista", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCodVendSIL", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramConsultadoPorAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCosecha", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramCreadoPor", SqlDbType.Int);
              cmd.Parameters.Add("@paramCuitChoferAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCuitCorredorCAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCuitCorredorVAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCuitDestinatarioAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCuitDestinoAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCuitEntregadorAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCuitInterAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCuitInterFleteAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCuitMaterminoAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCuitOrigenAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCuitRemComercialAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCuitTransportistaAfip", SqlDbType.Bit);
              cmd.Parameters.Add("@paramDesvio", SqlDbType.Bit);
              cmd.Parameters.Add("@paramDominio", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramDominio1", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramDominio2", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramEsAnulado", SqlDbType.Bit);
              cmd.Parameters.Add("@paramEsRechazado", SqlDbType.Bit);
              cmd.Parameters.Add("@paramEstaSTOP", SqlDbType.Bit);
              cmd.Parameters.Add("@paramEstado", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramFechaActivado", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaAnulado", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaArribado", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaConfirmado", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaCreado", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaDescargado", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaDesviadoD", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaDesviadoO", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaModificado", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaReactivado", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaRechazado", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaRegresado", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramFechaTomado", SqlDbType.DateTime);
              cmd.Parameters.Add("@paramIdCupoEstadoStop", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramIdCupoStop", SqlDbType.BigInt);
              cmd.Parameters.Add("@paramIdEstadoEnPlanta", SqlDbType.Int);
              cmd.Parameters.Add("@paramIdTurnoDetalle", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramKMRecorrer", (SqlDbType)DbType.Double);
              cmd.Parameters.Add("@paramModificadoPor", SqlDbType.Int);
              cmd.Parameters.Add("@paramMotivoBajaSIL", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomChofer", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomEntregador", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomFlete", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomTransportista", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomVendSIL", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNroContrato", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNroEstabOrigen", SqlDbType.Int);
              cmd.Parameters.Add("@paramNroPlantaRUCA", SqlDbType.Int);
              cmd.Parameters.Add("@paramObservacionSIL", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramPesoNetoEstimado", SqlDbType.Int);
              cmd.Parameters.Add("@paramPesoOriginal", SqlDbType.Int);
              cmd.Parameters.Add("@paramRENSPA", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramUltimaLatitud", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramUltimaLongitud", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramValidaKM", SqlDbType.Bit);
              cmd.Parameters.Add("@paramCodCompSIL", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramNomCompSIL", SqlDbType.NVarChar);
              cmd.Parameters.Add("@paramSyncDate", SqlDbType.DateTime);

              foreach (var item in cupos)
              {
                cmd.Parameters[0].Value = string.IsNullOrEmpty(item.Alfanumerico) ? DBNull.Value : item.Alfanumerico;
                cmd.Parameters[1].Value = item.Fecha == null || item.Fecha == defaultDate ? DBNull.Value : item.Fecha;
                cmd.Parameters[2].Value = string.IsNullOrEmpty(item.CentroCupo) ? DBNull.Value : item.CentroCupo;
                cmd.Parameters[3].Value = string.IsNullOrEmpty(item.CentroDist) ? DBNull.Value : item.CentroDist;
                cmd.Parameters[4].Value = string.IsNullOrEmpty(item.CodGrano) ? DBNull.Value : item.CodGrano;
                cmd.Parameters[5].Value = string.IsNullOrEmpty(item.NomGrano) ? DBNull.Value : item.NomGrano;
                cmd.Parameters[6].Value = string.IsNullOrEmpty(item.EstadoSTOP) ? DBNull.Value : item.EstadoSTOP;
                cmd.Parameters[7].Value = string.IsNullOrEmpty(item.CTG) ? DBNull.Value : item.CTG;
                cmd.Parameters[8].Value = string.IsNullOrEmpty(item.CodDestino) ? DBNull.Value : item.CodDestino;
                cmd.Parameters[9].Value = string.IsNullOrEmpty(item.NomDestino) ? DBNull.Value : item.NomDestino;
                cmd.Parameters[10].Value = string.IsNullOrEmpty(item.CodDestinatario) ? DBNull.Value : item.CodDestinatario;
                cmd.Parameters[11].Value = string.IsNullOrEmpty(item.NomDestinatario) ? DBNull.Value : item.NomDestinatario;
                cmd.Parameters[12].Value = string.IsNullOrEmpty(item.CodTitularDeCCPP) ? DBNull.Value : item.CodTitularDeCCPP;
                cmd.Parameters[13].Value = string.IsNullOrEmpty(item.NomTitularDeCCPP) ? DBNull.Value : item.NomTitularDeCCPP;
                cmd.Parameters[14].Value = string.IsNullOrEmpty(item.NomRteComProductor) ? DBNull.Value : item.NomRteComProductor;
                cmd.Parameters[15].Value = string.IsNullOrEmpty(item.CodRteComProductor) ? DBNull.Value : item.CodRteComProductor;
                cmd.Parameters[16].Value = string.IsNullOrEmpty(item.CodRteComVtaPrimaria) ? DBNull.Value : item.CodRteComVtaPrimaria;
                cmd.Parameters[17].Value = string.IsNullOrEmpty(item.NomRteComVtaPrimaria) ? DBNull.Value : item.NomRteComVtaPrimaria;
                cmd.Parameters[18].Value = string.IsNullOrEmpty(item.CodRteComVtaSecundaria) ? DBNull.Value : item.CodRteComVtaSecundaria;
                cmd.Parameters[19].Value = string.IsNullOrEmpty(item.NomRteComVtaSecundaria) ? DBNull.Value : item.NomRteComVtaSecundaria;
                cmd.Parameters[20].Value = string.IsNullOrEmpty(item.CodRteComVtaSecundaria2) ? DBNull.Value : item.CodRteComVtaSecundaria2;
                cmd.Parameters[21].Value = string.IsNullOrEmpty(item.NomRteComVtaSecundaria2) ? DBNull.Value : item.NomRteComVtaSecundaria2;
                cmd.Parameters[22].Value = string.IsNullOrEmpty(item.CodMercATermino) ? DBNull.Value : item.CodMercATermino;
                cmd.Parameters[23].Value = string.IsNullOrEmpty(item.NomMercATermino) ? DBNull.Value : item.NomMercATermino;
                cmd.Parameters[24].Value = string.IsNullOrEmpty(item.CodCorVtaPrimaria) ? DBNull.Value : item.CodCorVtaPrimaria;
                cmd.Parameters[25].Value = string.IsNullOrEmpty(item.NomCorVtaPrimaria) ? DBNull.Value : item.NomCorVtaPrimaria;
                cmd.Parameters[26].Value = string.IsNullOrEmpty(item.CodCorVtaSecundaria) ? DBNull.Value : item.CodCorVtaSecundaria;
                cmd.Parameters[27].Value = string.IsNullOrEmpty(item.NomCorVtaSecundaria) ? DBNull.Value : item.NomCorVtaSecundaria;
                cmd.Parameters[28].Value = string.IsNullOrEmpty(item.UsuarioSIL) ? DBNull.Value : item.UsuarioSIL;
                cmd.Parameters[29].Value = item.EstaSIL;
                cmd.Parameters[30].Value = item.EstadoSIL;
                cmd.Parameters[31].Value = item.FechaInformadoSIL == null || item.FechaInformadoSIL == defaultDate ? DBNull.Value : item.FechaInformadoSIL;
                cmd.Parameters[32].Value = item.CTGDesde == null || item.CTGDesde == defaultDate ? DBNull.Value : item.CTGDesde;
                cmd.Parameters[33].Value = item.CTGHasta == null || item.CTGHasta == defaultDate ? DBNull.Value : item.CTGHasta;
                cmd.Parameters[34].Value = item.CantHsSalidaCamion;
                cmd.Parameters[35].Value = string.IsNullOrEmpty(item.CartaPorte) ? DBNull.Value : item.CartaPorte;
                cmd.Parameters[36].Value = item.CartaPorteFechaCarga == null || item.CartaPorteFechaCarga == defaultDate ? DBNull.Value : item.CartaPorteFechaCarga;
                cmd.Parameters[37].Value = item.CartaPorteVto == null || item.CartaPorteVto == defaultDate ? DBNull.Value : item.CartaPorteVto;
                cmd.Parameters[38].Value = string.IsNullOrEmpty(item.CodChofer) ? DBNull.Value : item.CodChofer;
                cmd.Parameters[39].Value = string.IsNullOrEmpty(item.CodEntregador) ? DBNull.Value : item.CodEntregador;
                cmd.Parameters[40].Value = string.IsNullOrEmpty(item.CodFlete) ? DBNull.Value : item.CodFlete;
                cmd.Parameters[41].Value = item.CodLocalidadDestino;
                cmd.Parameters[42].Value = item.CodLocalidadOrigen;
                cmd.Parameters[43].Value = string.IsNullOrEmpty(item.CodTransportista) ? DBNull.Value : item.CodTransportista;
                cmd.Parameters[44].Value = string.IsNullOrEmpty(item.CodVendSIL) ? DBNull.Value : item.CodVendSIL;
                cmd.Parameters[45].Value = item.ConsultadoPorAfip;
                cmd.Parameters[46].Value = string.IsNullOrEmpty(item.Cosecha) ? DBNull.Value : item.Cosecha;
                cmd.Parameters[47].Value = item.CreadoPor;
                cmd.Parameters[48].Value = item.CuitChoferAfip;
                cmd.Parameters[49].Value = item.CuitCorredorCAfip;
                cmd.Parameters[50].Value = item.CuitCorredorVAfip;
                cmd.Parameters[51].Value = item.CuitDestinatarioAfip;
                cmd.Parameters[52].Value = item.CuitDestinoAfip;
                cmd.Parameters[53].Value = item.CuitEntregadorAfip;
                cmd.Parameters[54].Value = item.CuitInterAfip;
                cmd.Parameters[55].Value = item.CuitInterFleteAfip;
                cmd.Parameters[56].Value = item.CuitMaterminoAfip;
                cmd.Parameters[57].Value = item.CuitOrigenAfip;
                cmd.Parameters[58].Value = item.CuitRemComercialAfip;
                cmd.Parameters[59].Value = item.CuitTransportistaAfip;
                cmd.Parameters[60].Value = item.Desvio;
                cmd.Parameters[61].Value = string.IsNullOrEmpty(item.Dominio) ? DBNull.Value : item.Dominio;
                cmd.Parameters[62].Value = string.IsNullOrEmpty(item.Dominio1) ? DBNull.Value : item.Dominio1;
                cmd.Parameters[63].Value = string.IsNullOrEmpty(item.Dominio2) ? DBNull.Value : item.Dominio2;
                cmd.Parameters[64].Value = item.EsAnulado;
                cmd.Parameters[65].Value = item.EsRechazado;
                cmd.Parameters[66].Value = item.EstaSTOP;
                cmd.Parameters[67].Value = string.IsNullOrEmpty(item.Estado) ? DBNull.Value : item.Estado;
                cmd.Parameters[68].Value = item.FechaActivado == null || item.FechaActivado == defaultDate ? DBNull.Value : item.FechaActivado;
                cmd.Parameters[69].Value = item.FechaAnulado == null || item.FechaAnulado == defaultDate ? DBNull.Value : item.FechaAnulado;
                cmd.Parameters[70].Value = item.FechaArribado == null || item.FechaArribado == defaultDate ? DBNull.Value : item.FechaArribado;
                cmd.Parameters[71].Value = item.FechaConfirmado == null || item.FechaConfirmado == defaultDate ? DBNull.Value : item.FechaConfirmado;
                cmd.Parameters[72].Value = item.FechaCreado == null || item.FechaCreado == defaultDate ? DBNull.Value : item.FechaCreado;
                cmd.Parameters[73].Value = item.FechaDescargado == null || item.FechaDescargado == defaultDate ? DBNull.Value : item.FechaDescargado;
                cmd.Parameters[74].Value = item.FechaDesviadoD == null || item.FechaDesviadoD== defaultDate ? DBNull.Value : item.FechaDesviadoD;
                cmd.Parameters[75].Value = item.FechaDesviadoO == null || item.FechaDesviadoO == defaultDate ? DBNull.Value : item.FechaDesviadoO;
                cmd.Parameters[76].Value = item.FechaModificado == null || item.FechaModificado == defaultDate ? DBNull.Value : item.FechaModificado;
                cmd.Parameters[77].Value = item.FechaReactivado == null || item.FechaReactivado == defaultDate ? DBNull.Value : item.FechaReactivado;
                cmd.Parameters[78].Value = item.FechaRechazado == null || item.FechaRechazado == defaultDate ? DBNull.Value : item.FechaRechazado;
                cmd.Parameters[79].Value = item.FechaRegresado == null || item.FechaRegresado == defaultDate ? DBNull.Value : item.FechaRegresado;
                cmd.Parameters[80].Value = item.FechaTomado == null || item.FechaTomado == defaultDate ? DBNull.Value : item.FechaTomado;
                cmd.Parameters[81].Value = string.IsNullOrEmpty(item.IdCupoEstadoStop) ? DBNull.Value : item.IdCupoEstadoStop;
                cmd.Parameters[82].Value = item.IdCupoStop;
                cmd.Parameters[83].Value = item.IdEstadoEnPlanta;
                cmd.Parameters[84].Value = string.IsNullOrEmpty(item.IdTurnoDetalle) ? DBNull.Value : item.IdTurnoDetalle;
                cmd.Parameters[85].Value = item.KMRecorrer;
                cmd.Parameters[86].Value = item.ModificadoPor;
                cmd.Parameters[87].Value = string.IsNullOrEmpty(item.MotivoBajaSIL) ? DBNull.Value : item.MotivoBajaSIL;
                cmd.Parameters[88].Value = string.IsNullOrEmpty(item.NomChofer) ? DBNull.Value : item.NomChofer;
                cmd.Parameters[89].Value = string.IsNullOrEmpty(item.NomEntregador) ? DBNull.Value : item.NomEntregador;
                cmd.Parameters[90].Value = string.IsNullOrEmpty(item.NomFlete) ? DBNull.Value : item.NomFlete;
                cmd.Parameters[91].Value = string.IsNullOrEmpty(item.NomTransportista) ? DBNull.Value : item.NomTransportista;
                cmd.Parameters[92].Value = string.IsNullOrEmpty(item.NomVendSIL) ? DBNull.Value : item.NomVendSIL;
                cmd.Parameters[93].Value = string.IsNullOrEmpty(item.NroContrato) ? DBNull.Value : item.NroContrato;
                cmd.Parameters[94].Value = item.NroEstabOrigen;
                cmd.Parameters[95].Value = item.NroPlantaRUCA;
                cmd.Parameters[96].Value = string.IsNullOrEmpty(item.ObservacionSIL) ? DBNull.Value : item.ObservacionSIL;
                cmd.Parameters[97].Value = item.PesoNetoEstimado;
                cmd.Parameters[98].Value = item.PesoOriginal;
                cmd.Parameters[99].Value = string.IsNullOrEmpty(item.RENSPA) ? DBNull.Value : item.RENSPA;
                cmd.Parameters[100].Value = string.IsNullOrEmpty(item.UltimaLatitud) ? DBNull.Value : item.UltimaLatitud;
                cmd.Parameters[101].Value = string.IsNullOrEmpty(item.UltimaLongitud) ? DBNull.Value : item.UltimaLongitud;
                cmd.Parameters[102].Value = item.ValidaKM;
                cmd.Parameters[103].Value = string.IsNullOrEmpty(item.CodCompSIL) ? DBNull.Value : item.CodCompSIL;
                cmd.Parameters[104].Value = string.IsNullOrEmpty(item.NomCompSIL) ? DBNull.Value : item.NomCompSIL;
                cmd.Parameters[105].Value = DateTime.Now;
                try
                {
                  cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                  logger.LogInformation("HangFire: fallo en la insecion: " + countReg + 1);
                }
                countReg++;
              }
              tran.Commit();
              logger.LogInformation("HangFire: Inserto cupos " + countReg);
            }
            catch (Exception ex)
            {
              tran.Rollback();
              countReg = -1;
              Console.WriteLine(ex.Message);
              logger.LogError(ex.Message);
              throw ex;
            }
          }
        }
      }
      return countReg;
    }

  }
}

--ALTER session set NLS_DATE_FORMAT=’DD/MM/YYYY’;
SELECT CC.ID
    ,CC.NroCupo AS Alfanumerico
    ,TO_CHAR(CC.Fecha, 'DD/MM/YYYY') AS Fecha 
    ,CC.Centro AS CentroCupo
    ,CC.CentroDist
    ,CG.Grano AS CodGrano
    ,CG.Nombre AS NomGrano
        ,   CASE
            WHEN CS.ESANULADO='N' AND  CS.ESRECHAZADO ='N' AND CS.CTG is null AND
                 (CS.FECHAACTIVADO is null or CS.FECHAACTIVADO = '1/1/0001')AND
                 (CS.FECHAARRIBADO is null or CS.FECHAARRIBADO = '1/1/0001') AND
                 (CS.FECHADESCARGADO is null or CS.FECHADESCARGADO = '1/1/0001') AND 
                 (CS.FECHADESVIADOD is null or CS.FECHADESVIADOD= '1/1/0001') AND
                 (CS.FECHADESVIADOO is null or CS.FECHADESVIADOO = '1/1/0001') THEN 0               /*'SIN CTG'*/
            WHEN CS.ESANULADO='N' AND  CS.ESRECHAZADO ='N' AND CS.CTG is not null AND
                 (CS.FECHAACTIVADO is not null and CS.FECHAACTIVADO <> '1/1/0001') AND
                 (CS.FECHAARRIBADO is null or CS.FECHAARRIBADO = '1/1/0001') AND
                 (CS.FECHADESCARGADO is null or CS.FECHADESCARGADO = '1/1/0001') AND 
                 (CS.FECHADESVIADOD is null or CS.FECHADESVIADOD= '1/1/0001') AND
                 (CS.FECHADESVIADOO is null or CS.FECHADESVIADOO = '1/1/0001') THEN 1               /*'ACTIVADO'*/
            WHEN CS.ESANULADO='N' AND  CS.ESRECHAZADO ='N' AND CS.CTG is not null AND
                 (CS.FECHAACTIVADO is not null and CS.FECHAACTIVADO <> '1/1/0001') AND
                 (CS.FECHAARRIBADO is not null and CS.FECHAARRIBADO <> '1/1/0001') AND
                 (CS.FECHADESCARGADO is null or CS.FECHADESCARGADO = '1/1/0001') AND 
                 (CS.FECHADESVIADOD is null or CS.FECHADESVIADOD= '1/1/0001') AND
                 (CS.FECHADESVIADOO is null or CS.FECHADESVIADOO = '1/1/0001') THEN 2               /*'ARRIBADO'*/
            WHEN CS.ESANULADO='N' AND  CS.ESRECHAZADO ='N' AND CS.CTG is not null AND
                 (CS.FECHAACTIVADO is not null and CS.FECHAACTIVADO <> '1/1/0001') AND
                 (CS.FECHAARRIBADO is not null and CS.FECHAARRIBADO <> '1/1/0001') AND
                 (CS.FECHADESCARGADO is not null and CS.FECHADESCARGADO <> '1/1/0001') AND 
                 (CS.FECHADESVIADOD is null or CS.FECHADESVIADOD= '1/1/0001') AND
                 (CS.FECHADESVIADOO is null or CS.FECHADESVIADOO = '1/1/0001') THEN 3               /*'DESCARGADO'*/
            WHEN  CS.ESRECHAZADO = 'S' AND
                 (CS.FECHARECHAZADO  is not NULL and CS.FECHARECHAZADO <> '1/1/0001') AND
                 (CS.CTG is NOT NULL) AND 
                 (CS.FECHAREACTIVADO IS NOT NULL AND  CS.FECHAREACTIVADO <>'1/1/0001')  THEN 3   /*'DESVIADO-REACTIVADO-DESCARGADO'*/ 
            WHEN 
                 (CS.FECHADESVIADOD is not null and CS.FECHADESVIADOD <> '1/1/0001') OR
                 (CS.FECHADESVIADOO is not null and CS.FECHADESVIADOO <> '1/1/0001') THEN 4         /*'DESVIADO'*/
            WHEN  CS.ESRECHAZADO = 'S' AND
                 (CS.FECHARECHAZADO  is not null and CS.FECHARECHAZADO <> '1/1/0001') AND
                 (CS.CTG is null) AND 
                 (CS.FECHAREACTIVADO IS NULL OR  CS.FECHAREACTIVADO='1/1/0001')  THEN 5   /*'RECHAZADO'*/
			WHEN  CS.ESANULADO = 'S' AND
                 (CS.FECHAANULADO  is not null and CS.FECHAANULADO <> '1/1/0001')  THEN 6           /*'ANULADO'*/
			ELSE -1
        END AS EstadoSTOP
    , CS.CTG AS CTG
    , TO_CHAR(CS.FECHACTG_DESDE, 'DD/MM/YYYY') AS CtgDesde
	, TO_CHAR(CS.FECHACTG_HASTA, 'DD/MM/YYYY') AS CtgHasta
    , REPLACE(CP.Cuenta,'-','') AS CodDestino
    , CP.NOMBRE AS NomDestino
    , REPLACE(TO_CHAR(CC.CUITDESTINATARIO),'-','') AS CodDestinatario
    , CC.NOMDESTINATARIO AS NomDestinatario
    , REPLACE(TO_CHAR(CC.CUITSOLICITANTE),'-','') AS CodTitularDeCCPP
    , CC.NOMSOLICITANTE AS NomTitularDeCCPP
    , REPLACE(TO_CHAR(CC.CUITRTECOMERCIALPROD),'-','') AS CodRteComProductor
    , CC.NOMRTECOMERCIALPROD AS NomRteComProductor
    , REPLACE(TO_CHAR(CC.CUITRTECOMERCIALVTAPRIM),'-','') AS CodRteComVtaPrimaria
    , CC.NOMRTECOMERCIALVTAPRIM AS NomRteComVtaPrimaria
    , REPLACE(TO_CHAR(CC.CUITINTERMEDIARIO),'-','') AS CodRteComVtaSecundaria
    , CC.NOMINTERMEDIARIO AS NomRteComVtaSecundaria
    , REPLACE(TO_CHAR(CC.CUITRTECOMERCIAL),'-','') AS CodRteComVtaSecundaria2
    , CC.NOMRTECOMERCIAL AS NomRteComVtaSecundaria2
    , REPLACE(TO_CHAR(CC.CUITMAT),'-','') AS CodMercATermino
    , CC.NOMMAT AS NomMercATermino
    , REPLACE(TO_CHAR(CC.CUITCORRVTA),'-','') AS CodCorVtaPrimaria
    , CC.NOMCORRVTA AS NomCorVtaPrimaria
    , REPLACE(TO_CHAR(CC.CUITCORRCOMP),'-','') AS CodCorVtaSecundaria
    , CC.NOMCORRCOMP AS NomCorVtaSecundaria
    , REPLACE(TO_CHAR(CC.CUITRTEENT),'-','') AS CodEntregador
    , CC.NOMRTEENT AS NomEntregador
    , REPLACE(TO_CHAR(CC.VENDCTA),'-','') AS CodVendSIL
    , CVEND.NOMBRE AS NomVendSIL
    , CASE WHEN CC.VENDCYO = CC.COMPCTA THEN REPLACE(TO_CHAR(NVL(CCP.COMPCTA, '')),'-','') ELSE REPLACE(TO_CHAR(NVL(CC.COMPCTA,'')),'-','') END AS CodCompSIL
    , CASE WHEN CC.VENDCYO = CC.COMPCTA THEN (SELECT nombre FROM CuposComprador WHERE cuenta = CCP.COMPCTA) ELSE (SELECT nombre FROM CuposComprador WHERE cuenta = CC.COMPCTA ) END AS NomCompSIL  
    , REPLACE(TO_CHAR(CS.IDCUITINTERFLETE),'-','') AS CodFlete
    , CODFLETE.NOMBRE AS NomFlete
    , REPLACE(TO_CHAR(CS.IDCUITTRANSPORTISTA),'-','') AS CodTransportista
    , CODTRANSPORTISTA.NOMBRE AS NomTransportista
    , REPLACE(TO_CHAR(CS.IDCUITCHOFER),'-','') AS CodChofer
    , CODCHOFER.NOMBRE AS NomChofer
    , TO_CHAR(CS.CARTAPORTE) AS CartaPorte
    , TO_CHAR(CS.FECHACP_VTO, 'DD/MM/YYYY') AS CartaPorteVto
    , TO_CHAR(CS.FECHACP_CARGA, 'DD/MM/YYYY') AS CartaPorteFechaCarga
    , 1 AS EstaSIL
    , CC.STATUS AS EstadoSIL
    , TO_CHAR(CC.FECHAYHORAINFORMADO, 'DD/MM/YYYY') AS FechaInformadoSIL
    , CASE WHEN CS.IDCUPO IS NULL THEN 0 ELSE 1 END AS EstaSTOP  
    , TO_CHAR(CS.CUITCHOFERAFIP) AS CuitChoferAfip
    , CS.CUITCORREDORCAFIP  AS CuitCorredorCAfip
    , CS.CUITCORREDORVAFIP  AS CuitCorredorVAfip
    , CS.CUITDESTINATARIOAFIP  AS CuitDestinatarioAfip
    , CS.CUITDESTINOAFIP  AS CuitDestinoAfip
    , CS.CUITINTERAFIP  AS CuitInterAfip
    , CS.CUITINTERFLETEAFIP  AS CuitInterFleteAfip
    , CS.CUITMATERMINOAFIP   AS CuitMaterminoAfip
    , CS.CUITORIGENAFIP  AS CuitOrigenAfip
    , CS.CUITREMCOMERCIALAFIP  AS CuitRemComercialAfip
    , CS.CUITENTREGADORAFIP  AS CuitEntregadorAfip
    , CS.CUITTRANSPORTISTAAFIP  AS CuitTransportistaAfip
    , TO_CHAR(CS.ESTADO) AS Estado
    , CS.ESANULADO AS EsAnulado
    , CS.ESRECHAZADO AS EsRechazado
    , CS.DESVIO AS Desvio
    , CS.IDCUPO AS IdCupoStop
    , CS.IDTURNODETALLE AS IdTurnoDetalle
    , CS.IDCUPOESTADO As IdCupoEstadoStop
    , TO_CHAR(CS.FECHAACTIVADO, 'DD/MM/YYYY') AS FechaActivado
    , TO_CHAR(CS.FECHAARRIBADO, 'DD/MM/YYYY') AS FechaArribado
    , TO_CHAR(CS.FECHARECHAZADO, 'DD/MM/YYYY') AS FechaRechazado
    , TO_CHAR(CS.FECHADESVIADOO, 'DD/MM/YYYY') AS FechaDesviadoO
    , TO_CHAR(CS.FECHADESVIADOD, 'DD/MM/YYYY') AS FechaDesviadoD
    , TO_CHAR(CS.FECHAREGRESADO, 'DD/MM/YYYY') AS FechaRegresado
    , TO_CHAR(CS.FECHAANULADO, 'DD/MM/YYYY') AS FechaAnulado
    , TO_CHAR(CS.FECHACONFIRMADO, 'DD/MM/YYYY') AS FechaConfirmado
    , TO_CHAR(CS.FECHADESCARGADO, 'DD/MM/YYYY') AS FechaDescargado
    , TO_CHAR(CS.FECHAREACTIVADO, 'DD/MM/YYYY') AS FechaReactivado
    , TO_CHAR(CS.FECHATOMADO, 'DD/MM/YYYY') AS FechaTomado
    , TO_CHAR(CS.CREADO, 'DD/MM/YYYY') AS FechaCreado
    --, TO_DATE(CS.MODIFICADO) AS FechaModificado
    , TO_CHAR(CS.MODIFICADO, 'DD/MM/YYYY') AS FechaModificado
    , CS.CREADOPOR AS CreadoPor
    , CS.MODIFICADOPOR AS ModificadoPor
    , CS.CODLOCALIDADORIGEN AS CodLocalidadOrigen
    , CS.CODLOCALIDADDESTINO AS CodLocalidadDestino
    , TO_CHAR(CS.COSECHA) AS Cosecha
    , TO_CHAR(CS.RENSPA) AS Renspa
    , CS.NROESTABORIGEN AS NroEstabOrigen
    , CS.PESOORIGINAL AS PesoOriginal
    , CS.PESONETOESTIMADO AS PesoNetoEstimado
    , CS.KMRECORRER AS KmRecorrer
    , CS.VALIDAKM AS ValidaKm
    , CS.CANTHSSALIDACAMION AS CantHsSalidaCamion
    , TO_CHAR(CS.DOMINIO) AS Dominio
    , TO_CHAR(CS.DOMINIO_1) AS Dominio1
    , TO_CHAR(CS.DOMINIO_2) AS Dominio2
    , TO_CHAR(CS.NROCONTRATO) AS NroContrato
    , CS.NROPLANTARUCA AS NroPlantaRuca
    , CS.IDESTADOENPLANTA AS IdEstadoEnPlanta
    , CS.CONSULTADOXAFIP AS ConsultadoPorAfip
    , TO_CHAR(CS.ULTIMA_LATITUD) AS UltimaLatitud
    , TO_CHAR(CS.ULTIMA_LONGITUD) AS UltimaLongitud
    , CC.MOTBAJA AS MotivoBajaSil
    , CC.OBSERVA AS ObservacionSil
    , CC.USUARIO AS UsuarioSil
FROM CuposCorre CC
LEFT OUTER JOIN Corretaje.CuposCorre CCP ON CCP.TIPO = 1 AND CCP.VENDCYO=CC.COMPCTA AND CCP.NROCUPO=CC.NROCUPO AND CCP.IDORIGEN=CC.IDORIGEN AND CCP.FECHA=CC.FECHA AND CCP.ID<>CC.ID 
INNER JOIN MVCuposGrano CG ON CC.Grano = CG.Grano
INNER JOIN MVCuposPuerto CP ON CC.PuertoCta = CP.Cuenta
LEFT OUTER JOIN MVCuposVendedor CVend ON CC.VENDCTA = CVEND.CUENTA
LEFT OUTER JOIN CuposStop CS ON CC.NroCupo = CS.idcupoterminal AND CC.FECHA=CS.FECHA
LEFT OUTER JOIN CUPOSCUITMV CodFlete ON CODFLETE.CUENTA =CS.IDCUITINTERFLETE              /*analiza si esta info esta en cuposcuit*/
LEFT OUTER JOIN CUPOSCUITMV CodTransportista ON CODTRANSPORTISTA.CUENTA = CS.IDCUITTRANSPORTISTA          /*analiza si esta info esta en cuposcuit*/
LEFT OUTER JOIN CUPOSCUITMV CodChofer ON CODCHOFER.CUENTA =CS.IDCUITCHOFER                /*analiza si esta info esta en cuposcuit*/
WHERE CC.FECHA >= TO_DATE(:fechaDesde, 'DD/MM/YYYY')
    AND CC.FECHA <= TO_DATE(:fechaHasta, 'DD/MM/YYYY')
    AND CC.TIPO = 1
    AND CC.VENDCTA <> CC.VENDCYO
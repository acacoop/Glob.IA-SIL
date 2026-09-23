--ALTER session set NLS_DATE_FORMAT=’DD/MM/YYYY’;
SELECT IdCupo AS Id
    ,IdCupoTerminal AS Alfanumerico
    ,TO_CHAR(CS.Fecha, 'DD/MM/YYYY') AS Fecha
    ,'' AS CentroCupo       /*vacio por mapeo de tablas - ver hoja de calculo*/
    ,'' AS CentroDist       /*vacio por mapeo de tablas - ver hoja de calculo*/
    ,CG.GRANO AS Grano
    ,CG.Nombre AS NomGrano
    ,   CASE
            WHEN CS.ESANULADO='N' AND  CS.ESRECHAZADO ='N' AND CS.CTG is null AND
                 (CS.FECHAACTIVADO is null or CS.FECHAACTIVADO = '1/1/0001')AND
                 (CS.FECHAARRIBADO is null or CS.FECHAARRIBADO = '1/1/0001') AND
                 (CS.FECHADESCARGADO is null or CS.FECHADESCARGADO = '1/1/0001') AND 
                 (CS.FECHADESVIADOD is null or CS.FECHADESVIADOD= '1/1/0001') AND
                 (CS.FECHADESVIADOO is null or CS.FECHADESVIADOO = '1/1/0001') THEN 0          /*'SIN CTG'*/
            WHEN CS.ESANULADO='N' AND  CS.ESRECHAZADO ='N' AND CS.CTG is not null AND
                 (CS.FECHAACTIVADO is not null and CS.FECHAACTIVADO <> '1/1/0001') AND
                 (CS.FECHAARRIBADO is null or CS.FECHAARRIBADO = '1/1/0001') AND
                 (CS.FECHADESCARGADO is null or CS.FECHADESCARGADO = '1/1/0001') AND 
                 (CS.FECHADESVIADOD is null or CS.FECHADESVIADOD= '1/1/0001') AND
                 (CS.FECHADESVIADOO is null or CS.FECHADESVIADOO = '1/1/0001') THEN 1          /*'ACTIVADO'*/
            WHEN CS.ESANULADO='N' AND  CS.ESRECHAZADO ='N' AND CS.CTG is not null AND
                 (CS.FECHAACTIVADO is not null and CS.FECHAACTIVADO <> '1/1/0001') AND
                 (CS.FECHAARRIBADO is not null and CS.FECHAARRIBADO <> '1/1/0001') AND
                 (CS.FECHADESCARGADO is null or CS.FECHADESCARGADO = '1/1/0001') AND 
                 (CS.FECHADESVIADOD is null or CS.FECHADESVIADOD= '1/1/0001') AND
                 (CS.FECHADESVIADOO is null or CS.FECHADESVIADOO = '1/1/0001') THEN 2           /*'ARRIBADO'*/
            WHEN CS.ESANULADO='N' AND  CS.ESRECHAZADO ='N' AND CS.CTG is not null AND
                 (CS.FECHAACTIVADO is not null and CS.FECHAACTIVADO <> '1/1/0001') AND
                 (CS.FECHAARRIBADO is not null and CS.FECHAARRIBADO <> '1/1/0001') AND
                 (CS.FECHADESCARGADO is not null and CS.FECHADESCARGADO <> '1/1/0001') AND 
                 (CS.FECHADESVIADOD is null or CS.FECHADESVIADOD= '1/1/0001') AND
                 (CS.FECHADESVIADOO is null or CS.FECHADESVIADOO = '1/1/0001') THEN 3           /*'DESCARGADO'*/
            WHEN  CS.ESRECHAZADO = 'S' AND
                 (CS.FECHARECHAZADO  is not NULL and CS.FECHARECHAZADO <> '1/1/0001') AND
                 (CS.CTG is NOT NULL) AND 
                 (CS.FECHAREACTIVADO IS NOT NULL AND  CS.FECHAREACTIVADO <>'1/1/0001')  THEN 3   /*'DESVIADO-REACTIVADO-DESCARGADO'*/               
            WHEN 
                 (CS.FECHADESVIADOD is not null and CS.FECHADESVIADOD <> '1/1/0001') OR
                 (CS.FECHADESVIADOO is not null and CS.FECHADESVIADOO <> '1/1/0001') THEN 4     /*'DESVIADO'*/
            WHEN  CS.ESRECHAZADO = 'S' AND
                 (CS.FECHARECHAZADO  is not null and CS.FECHARECHAZADO <> '1/1/0001') AND
                 (CS.CTG is null) AND 
                 (CS.FECHAREACTIVADO IS NULL OR  CS.FECHAREACTIVADO='1/1/0001')  THEN 5   /*'RECHAZADO'*/
            WHEN  CS.ESANULADO = 'S' AND
                 (CS.FECHAANULADO  is not null and CS.FECHAANULADO <> '1/1/0001')  THEN 6       /*'ANULADO'*/
			ELSE -1
        END AS EstadoSTOP
    ,CS.CTG AS CTG
    ,TO_CHAR(CS.FECHACTG_DESDE, 'DD/MM/YYYY') AS CTGDesde
    ,TO_CHAR(CS.FECHACTG_HASTA, 'DD/MM/YYYY') AS CTGHasta
    ,REPLACE(CP.Cuenta,'-','') AS CodDestino
    ,CP.NOMBRE AS NomDestino
    ,REPLACE(TO_CHAR(CS.IDCUITDESTINATARIO),'-','') AS CodDestinatario
    ,DESTINATARIO.NOMBRE AS NomDestinatario
    ,REPLACE(TO_CHAR(CS.IDCUITORIGEN),'-','') AS CodTitularDeCCPP
    ,TITULARCCPP.NOMBRE AS NomTitularDeCCPP
    ,'' AS CodRteComProductor       /*vacio por mapeo de tablas - ver hoja de calculo*/
    ,'' AS NomRteComProductor       /*vacio por mapeo de tablas - ver hoja de calculo*/
    ,'' AS CodRteComVtaPrimaria     /*vacio por mapeo de tablas - ver hoja de calculo*/
    ,'' AS NomRteComVtaPrimaria     /*vacio por mapeo de tablas - ver hoja de calculo*/
    ,REPLACE(TO_CHAR(CS.IDCUITINTERMEDIARIO),'-','') AS CodRteComVtaSecundaria
    ,RTECOMVTASEC.NOMBRE AS NomRteComVtaSecundaria
    ,REPLACE(TO_CHAR(CS.IDCUITREMCOMERCIAL),'-','') AS CodRteComVtaSecundaria2
    ,RTECOMVTASEC2.NOMBRE AS NomRteComVtaSecundaria2
    ,REPLACE(TO_CHAR(CS.IDCUITMATERMINO),'-','') AS CodMercATermino
    ,MERCATERMINO.NOMBRE AS NomMercATermino
    ,REPLACE(TO_CHAR(CS.IDCUITCORREDORV),'-','') AS CodCorVtaPrimaria
    ,CORVTAPRIMARIA.NOMBRE AS NomCorVtaPrimaria
    ,REPLACE(TO_CHAR(CS.IDCUITCORREDORC),'-','') AS CodCorVtaSecundaria
    ,CORVTASECUNDARIA.NOMBRE AS NomCorVtaSecundaria 
    ,REPLACE(TO_CHAR(CS.IDCUITENTREGADOR),'-','') AS CodEntregador
    ,CODENTREGADOR.NOMBRE AS NomEntregador
    ,REPLACE(TO_CHAR(CS.IDCUITDESTINO),'-','') AS CodDestino
    ,CODDESTINO.NOMBRE AS NomDestino
    , '' AS CodVendSIL              /*vacio por mapeo de tablas - ver hoja de calculo*/
    , '' AS NomVendSIL              /*vacio por mapeo de tablas - ver hoja de calculo*/
    , '' AS CodCompSIL              /*vacio por mapeo de tablas - ver hoja de calculo*/
    , '' AS NomCompSIL              /*vacio por mapeo de tablas - ver hoja de calculo*/
    ,REPLACE(TO_CHAR(CS.IDCUITINTERFLETE),'-','') AS CodFlete
    ,CODFLETE.NOMBRE AS NomFlete
    ,REPLACE(TO_CHAR(CS.IDCUITTRANSPORTISTA),'-','') AS CodTransportista
    , CODTRANSPORTISTA.NOMBRE AS NomTransportista
    ,REPLACE(TO_CHAR(CS.IDCUITCHOFER),'-','') AS CodChofer
    , CODCHOFER.NOMBRE AS NomChofer
    , TO_CHAR(CS.CARTAPORTE) AS CartaPorte
    , TO_CHAR(CS.FECHACP_VTO, 'DD/MM/YYYY') AS CartaPorteVto
    , TO_CHAR(CS.FECHACP_CARGA, 'DD/MM/YYYY') AS CartaPorteFechaCarga
    , 0 AS EstaSIL
    , - 1 AS EstadoSIL
    --, TO_DATE('01/01/2001', 'DD/MM/YYYY') AS FechaInformadoSIL
    , '01/01/2001' AS FechaActivado
    , 1 AS EstaSTOP
    , TO_CHAR(CS.CUITCHOFERAFIP) AS CuitChoferAfip
    , TO_CHAR(CS.CUITCORREDORCAFIP) AS CuitCorredorCAfip
    , TO_CHAR(CS.CUITCORREDORVAFIP) AS CuitCorredorVAfip
    , TO_CHAR(CS.CUITDESTINATARIOAFIP) AS CuitDestinatarioAfip
    , TO_CHAR(CS.CUITDESTINOAFIP) AS CuitDestinoAfip
    , TO_CHAR(CS.CUITINTERAFIP) AS CuitInterAfip
    , TO_CHAR(CS.CUITINTERFLETEAFIP) AS CuitInterFleteAfip
    , TO_CHAR(CS.CUITMATERMINOAFIP ) AS CuitMaterminoAfip
    , TO_CHAR(CS.CUITORIGENAFIP) AS CuitOrigenAfip
    , TO_CHAR(CS.CUITREMCOMERCIALAFIP) AS CuitRemComercialAfip
    , TO_CHAR(CS.CUITENTREGADORAFIP) AS CuitEntregadorAfip
    , TO_CHAR(CS.CUITTRANSPORTISTAAFIP) AS CuitTransportistaAfip
    , TO_CHAR(CS.ESTADO) AS Estado
    , TO_CHAR(CS.ESANULADO) AS EsAnulado
    , TO_CHAR(CS.ESRECHAZADO) AS EsRechazado
    , TO_CHAR(CS.DESVIO) AS Desvio
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
    , TO_CHAR(CS.MODIFICADO, 'DD/MM/YYYY') AS FechaCreado
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
    , TO_CHAR(CS.VALIDAKM) AS ValidaKm
    , CS.CANTHSSALIDACAMION AS CantHsSalidaCamion
    , TO_CHAR(CS.DOMINIO) AS Dominio
    , TO_CHAR(CS.DOMINIO_1) AS Dominio1
    , TO_CHAR(CS.DOMINIO_2) AS Dominio2
    , TO_CHAR(CS.NROCONTRATO) AS NroContrato
    , CS.NROPLANTARUCA AS NroPlantaRuca
    , CS.IDESTADOENPLANTA AS IdEstadoEnPlanta
    , TO_CHAR(CS.CONSULTADOXAFIP) AS ConsultadoPorAfip
    , TO_CHAR(CS.ULTIMA_LATITUD) AS UltimaLatitud
    , TO_CHAR(CS.ULTIMA_LONGITUD) AS UltimaLongitud
    , '' AS MotivoBajaSil               /*vacio por mapeo de tablas - ver hoja de calculo*/
    , '' AS ObservacionSil              /*vacio por mapeo de tablas - ver hoja de calculo*/
    , '' AS UsuarioSil                  /*vacio por mapeo de tablas - ver hoja de calculo*/
FROM CuposStop CS
    INNER JOIN RelacionGranoSilGranoStop RGS ON CS.CODGRANO = RGS.NROGRANOSTOP
    INNER JOIN MVCuposGrano CG ON RGS.NROGRANOSIL = CG.GRANO AND RGS.VALORPORDEFECTO = 1
    INNER JOIN RelacionPuertoSilPuertoStop RPS ON CS.IDTERMINAL = RPS.NROPUERTOSTOP AND RPS.VALORPORDEFECTO = 1
    INNER JOIN MVCuposPuerto CP ON RPS.NROPUERTOSIL = CP.Cuenta
    LEFT JOIN CUPOSCUITMV Destinatario ON DESTINATARIO.CUENTA=CS.IDCUITDESTINATARIO
    LEFT JOIN CUPOSCUITMV TitularCCPP ON TitularCCPP.CUENTA = CS.IDCUITORIGEN 
    LEFT JOIN CUPOSCUITMV RteComVtaSec ON RTECOMVTASEC.CUENTA = CS.IDCUITINTERMEDIARIO
    LEFT JOIN CUPOSCUITMV RteComVtaSec2 ON RTECOMVTASEC2.CUENTA=CS.IDCUITREMCOMERCIAL
    LEFT JOIN CUPOSCUITMV MercaTermino ON MERCATERMINO.CUENTA =CS.IDCUITMATERMINO
    LEFT JOIN CUPOSCUITMV CorVtaPrimaria ON CORVTAPRIMARIA.CUENTA =CS.IDCUITCORREDORV
    LEFT JOIN CUPOSCUITMV CorVtaSecundaria ON CORVTASECUNDARIA.CUENTA =CS.IDCUITCORREDORC
    LEFT JOIN CUPOSCUITMV CodEntregador ON CODENTREGADOR.CUENTA =CS.IDCUITENTREGADOR
    LEFT JOIN CUPOSCUITMV CodDestino ON CODDESTINO.CUENTA =CS.IDCUITDESTINO
    LEFT JOIN CUPOSCUITMV CodFlete ON CODFLETE.CUENTA =CS.IDCUITINTERFLETE                        /*analiza si esta info esta en cuposcuit*/
    LEFT JOIN CUPOSCUITMV CodTransportista ON CODTRANSPORTISTA.CUENTA = CS.IDCUITTRANSPORTISTA    /*analiza si esta info esta en cuposcuit*/
    LEFT JOIN CUPOSCUITMV CodChofer ON CODCHOFER.CUENTA =CS.IDCUITCHOFER                          /*analiza si esta info esta en cuposcuit*/
    LEFT JOIN CuposCorre CC ON CS.IDCUPOTERMINAL = CC.NROCUPO AND CS.FECHA = CC.FECHA AND CC.TIPO = 1 AND CC.VENDCTA <> CC.VENDCYO
WHERE  CS.FECHA >=  TO_date(:fechaDesde, 'DD/MM/YYYY')
    AND   CS.FECHA <= TO_date(:fechaHasta, 'DD/MM/YYYY')
    AND (CC.NROCUPO IS NULL OR CC.FECHA IS NULL)
    AND (CS.IDCUITDESTINATARIO = '30500120882' 
        OR CS.IDCUITCORREDORC = '30500120882' 
        OR CS.IDCUITCORREDORV = '30500120882' 
        OR CS.IDCUITINTERMEDIARIO = '30500120882'
        OR CS.IDCUITREMCOMERCIAL = '30500120882' 
        OR CS.IDCUITORIGEN= '30500120882')
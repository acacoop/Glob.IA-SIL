--ALTER session set NLS_DATE_FORMAT=’DD/MM/YYYY’;
SELECT CC.ID
    ,CC.NroCupo AS Alfanumerico
    ,TO_CHAR(CC.Fecha, 'DD/MM/YYYY') AS Fecha 
    ,CC.Centro AS CentroCupo
    ,CC.CentroDist
    ,CG.Grano AS CodGrano
    ,CG.Nombre AS NomGrano
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
    , CC.STATUS AS EstadoSIL
    , TO_CHAR(CC.FECHAYHORAINFORMADO, 'DD/MM/YYYY') AS FechaInformadoSIL
    , CC.MOTBAJA AS MotivoBajaSil
    , CC.OBSERVA AS ObservacionSil
    , CC.USUARIO AS UsuarioSil
FROM CuposCorre CC
LEFT OUTER JOIN CuposCorre CCP ON CCP.TIPO = 1 AND CCP.VENDCYO=CC.COMPCTA AND CCP.NROCUPO=CC.NROCUPO AND CCP.IDORIGEN=CC.IDORIGEN AND CCP.FECHA=CC.FECHA AND CCP.ID<>CC.ID 
INNER JOIN Corretaje.MVCuposGrano CG ON CC.Grano = CG.Grano
INNER JOIN Corretaje.MVCuposPuerto CP ON CC.PuertoCta = CP.Cuenta
LEFT OUTER JOIN Corretaje.MVCuposVendedor CVend ON CC.VENDCTA = CVEND.CUENTA
WHERE CC.FECHA >= TO_DATE(:fechaDesde, 'DD/MM/YYYY')
    AND CC.FECHA <= TO_DATE(:fechaHasta, 'DD/MM/YYYY')
    AND CC.TIPO = 1
    AND CC.VENDCTA <> CC.VENDCYO
    AND CC.STATUS = :status
    AND CC.GRANO = :grano
    
    /*ESTOY CONSULTANDO SOBRE PRODUCCION.........................*/
/* Formatted on 12/6/2024 18:50:11 (QP5 v5.163.1008.3004) */
SELECT a0.vendcta AS CuentaVendeddor,
       a0.vendedor AS NombreVendedor,
       a0.nomgrano AS NombreGrano,
       a0.producto AS CodigoGrano,
       a0.compcta AS CuentaComprador,
       a0.comprador AS NombreComprador,
       a0.pendentrega AS PendienteEntrega,
       a0.pendaplicar AS PendienteAplicar,
       a0.cuposadist AS CuposADistribuir,
       NVL(c0.cuposotorgados, 0) AS CuposOtorgados,
       (a0.cuposadist - NVL(c0.cuposotorgados, 0)) CuposTotalesADistribuir,
       a0.ZonaGeografica,
       a0.ZonaGeoId as ZonaGeograficaId
  FROM    (  SELECT a1.vendcta,
                    a1.vendedor,
                    a1.nomgrano,
                    a1.comprador,
                    a1.producto,
                    a1.compcta,
                    SUM (a1.pendentrega) pendentrega,
                    SUM (a1.pendaplicar) pendaplicar,
                    TRUNC (SUM (a1.pendentrega - a1.pendaplicar) / 30)
                    + (CASE
                          WHEN ( (SUM (a1.pendentrega - a1.pendaplicar) / 30)
                                - TRUNC (
                                     SUM (a1.pendentrega - a1.pendaplicar) / 30)) >
                                  0
                          THEN
                             1
                          ELSE
                             0
                       END)
                       cuposadist,
                    zg1.nombre AS zonageografica,
                    zg1.zonageoid
               FROM (SELECT vista_cuposcorre.empresa,
                            vista_cuposcorre.compcta,
                            vista_cuposcorre.cuitcomp,
                            vista_cuposcorre.comprador,
                            vista_cuposcorre.cuitvend,
                            vista_cuposcorre.vendcta,
                            trim(vista_cuposcorre.vendedor) as vendedor,
                            vista_cuposcorre.grano producto,
                            b_tb0 (
                               'GRANO',
                               LPAD (TO_CHAR (vista_cuposcorre.grano), 3, ' '),
                               'GR2')
                               nomgrano,
                            vista_cuposcorre.destino,
                            vista_cuposcorre.ctadestino,
                            vista_cuposcorre.cosecha,
                            vista_cuposcorre.zonainfluencia,
                            vista_cuposcorre.codcentro,
                            vista_cuposcorre.centro,
                            tipcta,
                            vista_cuposcorre.pactadas pactado,
                            vista_cuposcorre.pendientes pendentrega,
                            vista_cuposcorre.ittfijadas fijado,
                            vista_cuposcorre.ittparciales liquidado,
                            0 pendaplicar,
                            0 cupootorgado,
                            fechaent,
                            ndate_c (
                               CASE
                                  WHEN prorroga IS NULL THEN vtoent
                                  ELSE prorroga
                               END)
                               fechavto
                       FROM vista_cuposctocorre vista_cuposcorre
                      WHERE vista_cuposcorre.vendcta = :cuentavendedor
                            AND fechaent <= ndate_c (:fecha)
                            AND (:cuentacomprador = 0
                                 OR vista_cuposcorre.compcta = :cuentacomprador)
                            AND (:producto = 0
                                 OR vista_cuposcorre.grano = :producto)
                     UNION ALL
                       /*Pendiente Aplicar.*/
                       SELECT empresa,
                              compcta,
                              cuitcomp,
                              comprador,
                              cuitvend,
                              vendcta,
                              trim(vendedor) as vendedor,
                              producto,
                              nomgrano,
                              destino,
                              ctadestino,
                              cosecha,
                              zonainfluencia,
                              codcentro,
                              centro,
                              tipcta,
                              SUM (pactado) pactado,
                              SUM (pendentrega),
                              SUM (fijado) fijado,
                              SUM (liquidado) liquidado,
                              SUM (pendaplicar) pendaplicar,
                              SUM (cupotorgado) cupotorgado,
                              ndate_c (SYSDATE) fechaent,
                              ndate_c (SYSDATE) fechavto
                         FROM vista_cuposmpecorrev3
                        WHERE vendcta = :cuentavendedor
                              AND (:cuentacomprador = 0
                                   OR compcta = :cuentacomprador)
                              AND (:producto = 0 OR producto = :producto)
                     GROUP BY empresa,
                              compcta,
                              cuitcomp,
                              comprador,
                              cuitvend,
                              vendcta,
                              vendedor,
                              producto,
                              nomgrano,
                              cosecha,
                              zonainfluencia,
                              codcentro,
                              centro,
                              tipcta,
                              destino,
                              ctadestino) a1
                    LEFT JOIN puertoporzona ppz1
                       ON a1.ctadestino = ppz1.cuenta
                    LEFT JOIN zonasgeograficas zg1
                       ON ppz1.zonageoid = zg1.zonageoid
              WHERE (ZG1.ZONAGEOID = :zona OR :zona = 0)
           GROUP BY a1.vendcta,
                    a1.vendedor,
                    a1.producto,
                    a1.nomgrano,
                    a1.compcta,
                    a1.comprador,
                    zg1.nombre,
                    zg1.zonageoid) a0
       LEFT JOIN
          (  SELECT COUNT (*) cuposotorgados,
                    vendcta,
                    compcta,
                    grano,
                    zg.zonageoid
               FROM cuposcorre c
                    LEFT JOIN puertoporzona ppz
                       ON c.puertocta = ppz.cuenta
                    LEFT JOIN zonasgeograficas zg
                       ON ppz.zonageoid = zg.zonageoid
              WHERE     (ZG.ZONAGEOID = :zona OR :zona = 0)
                    AND c.fecha >= TO_CHAR (SYSDATE, 'dd-mm-yyyy')
                    AND c.Tipo = 1
                    AND c.status = 4
                    AND c.pdf = 0
                    AND (c.vendcyo = c.compcta OR c.vendcyo = 0)
                    AND c.vendcta = :cuentavendedor
                    AND (c.grano = :producto OR :producto = 0)
                    AND (:cuentacomprador = c.compcta OR :cuentacomprador = 0)
           GROUP BY c.vendcta,
                    c.grano,
                    c.compcta,
                    zg.ZONAGEOID) c0
       ON     c0.vendcta = a0.vendcta
          AND c0.grano = a0.producto
          AND a0.compcta = c0.compcta
          AND NVL(a0.ZONAGEOID, 0) = NVL(c0.zonageoid, 0)
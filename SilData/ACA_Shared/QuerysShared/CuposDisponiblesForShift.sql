SELECT
    a0.vendcta        AS CuentaVendedor,
    a0.vendedor       AS NombreVendedor,
    a0.nomgrano       AS NombreGrano,
    a0.producto       AS CodigoGrano,
    a0.compcta        AS CuentaComprador,
    a0.comprador      AS NombreComprador,
    (a0.cuposadist - NVL(c0.cuposotorgados, 0)) AS CuposDisponibles,
    NVL(a0.ZonaGeoId, 0)   AS ZonaGeograficaId,
    a0.ZonaGeografica
FROM (
    SELECT
        a1.vendcta,
        a1.vendedor,
        a1.nomgrano,
        a1.comprador,
        a1.producto,
        a1.compcta,
        TRUNC(SUM(a1.pendentrega - a1.pendaplicar) / 30)
        + (CASE
              WHEN ((SUM(a1.pendentrega - a1.pendaplicar) / 30)
                   - TRUNC(SUM(a1.pendentrega - a1.pendaplicar) / 30)) > 0
              THEN 1
              ELSE 0
           END) AS cuposadist,
        zg1.nombre    AS ZonaGeografica,
        zg1.zonageoid AS ZonaGeoId
    FROM (
        -- Pendiente Entrega
        SELECT
            vista_cuposcorre.vendcta,
            TRIM(vista_cuposcorre.vendedor)  AS vendedor,
            vista_cuposcorre.compcta,
            vista_cuposcorre.comprador,
            vista_cuposcorre.grano           AS producto,
            b_tb0('GRANO', LPAD(TO_CHAR(vista_cuposcorre.grano), 3, ' '), 'GR2') AS nomgrano,
            vista_cuposcorre.ctadestino,
            vista_cuposcorre.pendientes      AS pendentrega,
            0                                AS pendaplicar
        FROM vista_cuposctocorre vista_cuposcorre
        WHERE vista_cuposcorre.vendcta = :cuentavendedor
          AND fechaent <= ndate_c(:fechaTexto)
          AND (:cuentacomprador = 0 OR vista_cuposcorre.compcta = :cuentacomprador)
          AND (:producto = 0       OR vista_cuposcorre.grano   = :producto)

        UNION ALL

        -- Pendiente Aplicar
        SELECT
            vendcta,
            TRIM(vendedor)  AS vendedor,
            compcta,
            comprador,
            producto,
            nomgrano,
            ctadestino,
            SUM(pendentrega) AS pendentrega,
            SUM(pendaplicar) AS pendaplicar
        FROM vista_cuposmpecorrev3
        WHERE vendcta = :cuentavendedor
          AND (:cuentacomprador = 0 OR compcta  = :cuentacomprador)
          AND (:producto = 0        OR producto = :producto)
        GROUP BY vendcta, vendedor, compcta, comprador, producto, nomgrano,
                 cosecha, zonainfluencia, codcentro, centro, tipcta, destino, ctadestino
    ) a1
    LEFT JOIN puertoporzona    ppz1 ON a1.ctadestino  = ppz1.cuenta
    LEFT JOIN zonasgeograficas zg1  ON ppz1.zonageoid = zg1.zonageoid
    WHERE (zg1.zonageoid = :zona OR :zona = 0)
    GROUP BY
        a1.vendcta, a1.vendedor, a1.producto, a1.nomgrano,
        a1.compcta, a1.comprador, zg1.nombre, zg1.zonageoid
) a0
LEFT JOIN (
    SELECT
        COUNT(*)    AS cuposotorgados,
        c.vendcta,
        c.compcta,
        c.grano,
        zg.zonageoid
    FROM cuposcorre c
    LEFT JOIN puertoporzona    ppz ON c.puertocta   = ppz.cuenta
    LEFT JOIN zonasgeograficas zg  ON ppz.zonageoid = zg.zonageoid
    WHERE (zg.zonageoid = :zona OR :zona = 0)
      AND c.fecha   >= TO_CHAR(SYSDATE, 'dd-mm-yyyy')
      AND c.Tipo     = 1
      AND c.status   = 4
      AND c.pdf      = 0
      AND (c.vendcyo = c.compcta OR c.vendcyo = 0)
      AND c.vendcta  = :cuentavendedor
      AND (c.grano   = :producto       OR :producto        = 0)
      AND (:cuentacomprador = c.compcta OR :cuentacomprador = 0)
    GROUP BY c.vendcta, c.grano, c.compcta, zg.zonageoid
) c0
ON  c0.vendcta   = a0.vendcta
AND c0.grano     = a0.producto
AND a0.compcta   = c0.compcta
AND NVL(a0.ZonaGeoId, 0) = NVL(c0.zonageoid, 0)

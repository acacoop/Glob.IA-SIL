-- =====================================================================
-- PROPUESTA de índices para acelerar el matching (Turnos / Distribución).
--
-- *** NO APLICAR SIN REVISAR ***
-- Revisar EXPLAIN PLAN en TEST antes de aplicar. Verificar primero con
--   SELECT index_name, column_name, column_position FROM all_ind_columns
--    WHERE table_name IN ('CUPOSCORRE','SOLTURNOS','PUERTOPORZONA')
--    ORDER BY table_name, index_name, column_position;
-- que no exista ya un índice equivalente (o que lo cubra como prefijo).
-- Evaluar el impacto en los INSERT/UPDATE de cuposcorre (HangFire).
-- =====================================================================

-- 1) GetCuposConMatchesAsync (POST /api/ShiftRequest/MatchesDistribucionV2)
--    y FindAvailableCuposByPeriodAsync (Matches / MatchesDistribucion).
--    Predicados: c.Grano = :g AND c.Fecha BETWEEN :d AND :h AND c.COMPCTA = :c
--                AND c.PUERTOCTA = :p AND c.STATUS = 0 AND c.TIPO = 1
--                AND c.Centro = :centro AND c.CentroDist = :centrodist
-- EXPLAIN PLAN FOR <query de GetCuposConMatchesAsync>;
-- SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);
-- CREATE INDEX IX_CUPOSCORRE_MATCH ON cuposcorre (Grano, Fecha, STATUS, TIPO, COMPCTA, PUERTOCTA);

-- 2) JOIN de SOLTURNOS contra cuposcorre por (GRANO, FECHASOLICITADA) y
--    GetForMatchingAsync / GetByMatchesFilterAsync.
-- CREATE INDEX IX_SOLTURNOS_GRANO_FECHA ON SOLTURNOS (GRANO, FECHASOLICITADA);

-- 3) EXISTS / JOIN puerto → zona (ZonaGeograficaResolver, DestinoRule en SQL).
-- CREATE INDEX IX_PUERTOPORZONA_CUENTA_ZONA ON PUERTOPORZONA (CUENTA, ZONAGEOID);

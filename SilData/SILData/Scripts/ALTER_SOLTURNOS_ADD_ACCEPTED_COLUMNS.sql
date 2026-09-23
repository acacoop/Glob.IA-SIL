-- =============================================================================
-- Migración: agregar acumuladores de aceptados a SOLTURNOS.
--   - cantidad_aceptada          (acumulador principal)
--   - cantidad_futuro_aceptada   (sub-acumulador para solicitudes EsFuturo=true)
-- Invariantes:
--   - cantidad_aceptada ≤ cantidad
--   - cantidad_futuro_aceptada ≤ cantidad_futuro
--   - STATUS pasa a Otorgada (2) sólo cuando cantidad_aceptada == cantidad
-- (esa última regla NO se enforcea con CHECK, se aplica en el UPDATE de Accept)
-- =============================================================================

-- 1) Verificar si las columnas ya existen (idempotente)
SELECT column_name, data_type, data_default, nullable
  FROM user_tab_columns
 WHERE table_name = 'SOLTURNOS'
   AND column_name IN ('CANTIDAD_ACEPTADA', 'CANTIDAD_FUTURO_ACEPTADA')
 ORDER BY column_name;

-- =============================================================================
-- 2) ADD COLUMNs (con defaults seguros)
-- =============================================================================

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS ADD (cantidad_aceptada NUMBER(10) DEFAULT 0 NOT NULL)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -1430 THEN RAISE; END IF;  -- ORA-01430: ya existe
END;
/

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS ADD (cantidad_futuro_aceptada NUMBER(10) DEFAULT 0 NOT NULL)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -1430 THEN RAISE; END IF;
END;
/

-- =============================================================================
-- 3) CHECK constraints: la columna nunca puede superar lo pedido
-- =============================================================================

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS ADD CONSTRAINT ck_solt_acept_le_cant
                       CHECK (cantidad_aceptada <= cantidad)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -2261 AND SQLCODE != -2260 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS ADD CONSTRAINT ck_solt_futuroacept_le_cantfuturo
                       CHECK (cantidad_futuro_aceptada <= cantidad_futuro)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -2261 AND SQLCODE != -2260 THEN RAISE; END IF;
END;
/

-- =============================================================================
-- 4) Comentarios
-- =============================================================================

COMMENT ON COLUMN SOLTURNOS.cantidad_aceptada IS 'Acumulador de cupos aceptados (≤ Cantidad). STATUS pasa a Otorgada cuando esta alcanza Cantidad.';
COMMENT ON COLUMN SOLTURNOS.cantidad_futuro_aceptada IS 'Acumulador de cupos aceptados con EsFuturo=true (≤ CantidadFuturo). Subconjunto de cantidad_aceptada.';

-- =============================================================================
-- 5) Backfill histórico (opcional, sólo si hay datos legacy en demo/prod):
-- Recalcular cantidad_aceptada a partir del conteo de filas en SOLTURNOS_DETALLE.
-- =============================================================================
-- MERGE INTO SOLTURNOS t
-- USING (SELECT solicitud_id, COUNT(*) AS n
--          FROM SOLTURNOS_DETALLE
--         GROUP BY solicitud_id) d
--    ON (t.solturnos_id = d.solicitud_id)
--  WHEN MATCHED THEN UPDATE SET t.cantidad_aceptada = d.n;

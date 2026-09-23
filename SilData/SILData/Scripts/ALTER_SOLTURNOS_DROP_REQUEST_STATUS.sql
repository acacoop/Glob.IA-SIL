-- =============================================================================
-- Migración: eliminar SOLTURNOS.STATUS y SOLTURNOS_DETALLE.ESTADO.
-- El modelo deja de persistir el estado: las reglas derivan de los
-- acumuladores (CantidadAceptada, CantidadRechazada) y de la existencia
-- de filas en SOLTURNOS_DETALLE.
--
-- Esquema: cuposcorretaje (o el que use SilConnection).
--
-- IMPORTANTE: ejecutar DESPUÉS de desplegar la versión de la app que ya
-- no referencia estas columnas. Si la columna no existe, el script
-- absorbe ORA-00904 / ORA-02443 y finaliza OK (idempotente).
-- =============================================================================

-- 0) Auditoría previa (no aborta la ejecución; muestra dependencias).
SELECT constraint_name, constraint_type
  FROM user_constraints
 WHERE table_name IN ('SOLTURNOS', 'SOLTURNOS_DETALLE')
   AND (UPPER(constraint_name) LIKE '%STATUS%'
        OR UPPER(constraint_name) LIKE '%ESTADO%');

SELECT index_name, table_name
  FROM user_indexes
 WHERE table_name IN ('SOLTURNOS', 'SOLTURNOS_DETALLE')
   AND (UPPER(index_name) LIKE '%STATUS%'
        OR UPPER(index_name) LIKE '%ESTADO%');

SELECT trigger_name, table_name
  FROM user_triggers
 WHERE table_name IN ('SOLTURNOS', 'SOLTURNOS_DETALLE')
   AND (UPPER(trigger_name) LIKE '%STATUS%'
        OR UPPER(trigger_name) LIKE '%ESTADO%');

-- =============================================================================
-- 1) SOLTURNOS.STATUS
-- =============================================================================

-- 1a) DROP CHECK / FK que la usen (silencioso si no existen).
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS DROP CONSTRAINT ck_solt_status';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE NOT IN (-2433, -2443, -2427) THEN RAISE; END IF;
END;
/

-- 1b) DROP COLUMN.
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS DROP COLUMN status';
EXCEPTION WHEN OTHERS THEN
  -- ORA-00904: columna no existe. Esperable en re-run.
  IF SQLCODE != -904 THEN RAISE; END IF;
END;
/

-- =============================================================================
-- 2) SOLTURNOS_DETALLE.ESTADO
-- =============================================================================

-- 2a) DROP CHECK / FK que la usen.
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS_DETALLE DROP CONSTRAINT ck_soldet_estado';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE NOT IN (-2433, -2443, -2427) THEN RAISE; END IF;
END;
/

-- 2b) DROP índices asociados (silencioso si no existen).
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX idx_soldet_estado';
EXCEPTION WHEN OTHERS THEN
  -- ORA-01418: índice no existe. ORA-02443: constraint inexistente.
  IF SQLCODE NOT IN (-1418, -2443) THEN RAISE; END IF;
END;
/

-- 2c) DROP COLUMN.
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS_DETALLE DROP COLUMN estado';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -904 THEN RAISE; END IF;
END;
/

-- =============================================================================
-- 3) Verificación post-migración.
-- =============================================================================

SELECT column_name
  FROM user_tab_columns
 WHERE table_name = 'SOLTURNOS'
   AND UPPER(column_name) = 'STATUS';

SELECT column_name
  FROM user_tab_columns
 WHERE table_name = 'SOLTURNOS_DETALLE'
   AND UPPER(column_name) = 'ESTADO';

-- Si ambas queries devuelven 0 filas, la migración fue exitosa.
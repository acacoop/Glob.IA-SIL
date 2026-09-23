-- =============================================================================
-- Migración: crear / refactor de la tabla SOLTURNOS_DETALLE.
-- Modelo "Detalle Acumulativo": sólo guarda cupos ACEPTADOS.
-- Esquema: cuposcorretaje (o el que use SilConnection).
-- =============================================================================

-- 1) Verificar que la tabla exista (es idempotente; corre también si ya está creada)
SELECT object_type, owner, object_name
  FROM all_objects
 WHERE UPPER(object_name) = 'SOLTURNOS_DETALLE'
   AND owner = USER;

-- 2) Inspeccionar columnas actuales (por si ya existe con otra forma)
SELECT column_name, data_type, data_default, nullable
  FROM user_tab_columns
 WHERE table_name = 'SOLTURNOS_DETALLE'
 ORDER BY column_id;

-- =============================================================================
-- 3) Crear la tabla detalle si NO existe (modelo v2 = detalle acumulativo)
-- Columnas operativas: solicitud_id, cupo_id (siempre poblado en este modelo).
-- Columnas auxiliares: detalle_id (PK), fecha_creacion (auditoría).
-- =============================================================================

BEGIN
  EXECUTE IMMEDIATE '
    CREATE TABLE SOLTURNOS_DETALLE (
        detalle_id          NUMBER(19) PRIMARY KEY,
        solicitud_id        NUMBER(19) NOT NULL,
        cupo_id             NUMBER(19) NOT NULL,
        fecha_creacion      DATE       DEFAULT SYSDATE NOT NULL,
        CONSTRAINT fk_soldet_solicitud FOREIGN KEY (solicitud_id)
            REFERENCES SOLTURNOS (solturnos_id)
    )';
EXCEPTION WHEN OTHERS THEN
  -- ORA-00955: ya existe. Es esperable en re-ejecuciones.
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

-- =============================================================================
-- 4) Refactor idempotente: si la tabla YA existía con el modelo A.2
-- (cantidad, fecha_asignacion, estado), normalizamos a v2.
-- =============================================================================

-- 4a) DROP columnas heredadas (cantidad, fecha_asignacion, estado)
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS_DETALLE DROP COLUMN cantidad';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -904 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS_DETALLE DROP COLUMN fecha_asignacion';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -904 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS_DETALLE DROP COLUMN estado';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -904 THEN RAISE; END IF;
END;
/

-- 4b) Quitar el CHECK antiguo si existe (estado antes admitía 3 valores)
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS_DETALLE DROP CONSTRAINT ck_soldet_estado';
EXCEPTION WHEN OTHERS THEN
  -- ORA-02443: no existe la constraint. Es esperable.
  IF SQLCODE != -2433 AND SQLCODE != -2443 THEN RAISE; END IF;
END;
/

-- 4c) Hacer cupo_id NOT NULL (en el modelo v2 todo cupo del detalle ya está aceptado)
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS_DETALLE MODIFY (cupo_id NUMBER(19) NOT NULL)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -2257 AND SQLCODE != -1452 THEN RAISE; END IF;  -- already not null / FK violation
END;
/

-- =============================================================================
-- 5) Índices para consultas típicas
-- =============================================================================

BEGIN
  EXECUTE IMMEDIATE 'CREATE INDEX idx_soldet_solicitud ON SOLTURNOS_DETALLE (solicitud_id)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE INDEX idx_soldet_cupo ON SOLTURNOS_DETALLE (cupo_id)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

-- 5b) UNIQUE constraint: una solicitud no puede aceptar dos veces el mismo cupo
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS_DETALLE ADD CONSTRAINT uk_soldet_solicitud_cupo UNIQUE (solicitud_id, cupo_id)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -2261 AND SQLCODE != -2260 THEN RAISE; END IF;
END;
/

-- =============================================================================
-- 6) Sequence para detalle_id (autoincremental vía trigger)
-- =============================================================================

BEGIN
  EXECUTE IMMEDIATE 'CREATE SEQUENCE SOLTURNOS_DETALLE_SEQ START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

-- =============================================================================
-- 7) Comentarios
-- =============================================================================

COMMENT ON TABLE SOLTURNOS_DETALLE IS 'Acumulador inmutable de cupos aceptados por solicitud. Cada fila representa una solicitud que matcheó con un cupo y fue aceptada. Las solicitudes rechazadas NO tienen filas acá (los rechazados viven en cuposcorre). Los pendientes son implícitos: Cantidad - CantidadAceptada en SOLTURNOS.';
COMMENT ON COLUMN SOLTURNOS_DETALLE.detalle_id     IS 'PK autoincremental vía SEQ';
COMMENT ON COLUMN SOLTURNOS_DETALLE.solicitud_id   IS 'FK lógica → SOLTURNOS.solturnos_id';
COMMENT ON COLUMN SOLTURNOS_DETALLE.cupo_id        IS 'FK lógica → CUPOSCORRE.Id (siempre poblado en este modelo)';
COMMENT ON COLUMN SOLTURNOS_DETALLE.fecha_creacion IS 'Cuándo se aceptó el cupo (auditoría)';

-- =============================================================================
-- 8) Trigger para autoincrementar detalle_id desde la SEQ
-- =============================================================================

CREATE OR REPLACE TRIGGER trg_soldet_bi
BEFORE INSERT ON SOLTURNOS_DETALLE
FOR EACH ROW
WHEN (NEW.detalle_id IS NULL OR NEW.detalle_id = 0)
BEGIN
    :NEW.detalle_id := SOLTURNOS_DETALLE_SEQ.NEXTVAL;
END;
/

-- =============================================================================
-- 9) Backfill opcional (si ya hay datos pre-existentes del modelo A.2):
-- Copiar filas Asignadas previas a la nueva estructura. NO usar en dev.
-- =============================================================================
-- INSERT INTO SOLTURNOS_DETALLE (solicitud_id, cupo_id, fecha_creacion)
-- SELECT s.solturnos_id, s.cupo_id, NVL(s.FECHACREACION, SYSDATE)
--   FROM SOLTURNOS s
--  WHERE s.cupo_id IS NOT NULL
--    AND NOT EXISTS (
--        SELECT 1 FROM SOLTURNOS_DETALLE d
--         WHERE d.solicitud_id = s.solturnos_id AND d.cupo_id = s.cupo_id
--    );
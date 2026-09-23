-- =============================================================================
-- Migración: agregar acumuladores de rechazos a SOLTURNOS.
--   - cantidad_rechazada         (acumulador principal de rechazos)
--   - cantidad_futuro_rechazada  (sub-acumulador para solicitudes EsFuturo=true)
-- Invariantes:
--   - cantidad_rechazada <= cantidad
--   - cantidad_futuro_rechazada <= cantidad_futuro
--   - (cantidad_aceptada + cantidad_rechazada) <= cantidad
--   - (cantidad_futuro_aceptada + cantidad_futuro_rechazada) <= cantidad_futuro
--
-- Semántica:
--   En el flujo de rechazo completo de una solicitud, al momento del rechazo
--   se "congela" el pendiente implícito como rechazado:
--     cantidad_rechazada = cantidad - cantidad_aceptada (en ese momento)
--     cantidad_futuro_rechazada = cantidad_futuro - cantidad_futuro_aceptada
--   Los cupos en cuposcorre NO se tocan — las aceptaciones previas persisten
--   como Otorgado (decisión de negocio confirmada).
-- =============================================================================

-- 1) Verificar si las columnas ya existen (idempotente)
SELECT column_name, data_type, data_default, nullable
  FROM user_tab_columns
 WHERE table_name = 'SOLTURNOS'
   AND column_name IN ('CANTIDAD_RECHAZADA', 'CANTIDAD_FUTURO_RECHAZADA')
 ORDER BY column_name;

-- =============================================================================
-- 2) ADD COLUMNs
-- =============================================================================

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS ADD (cantidad_rechazada NUMBER(10) DEFAULT 0 NOT NULL)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -1430 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS ADD (cantidad_futuro_rechazada NUMBER(10) DEFAULT 0 NOT NULL)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -1430 THEN RAISE; END IF;
END;
/

-- =============================================================================
-- 3) CHECK constraints — individuales y suma
-- =============================================================================

-- 3a) Cada columna individual <= su valor solicitado
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS ADD CONSTRAINT ck_solt_rech_le_cant
                       CHECK (cantidad_rechazada <= cantidad)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -2261 AND SQLCODE != -2260 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS ADD CONSTRAINT ck_solt_futurorech_le_cantfuturo
                       CHECK (cantidad_futuro_rechazada <= cantidad_futuro)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -2261 AND SQLCODE != -2260 THEN RAISE; END IF;
END;
/

-- 3b) Suma aceptada + rechazada <= cantidad (no se puede doble-contar)
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS ADD CONSTRAINT ck_solt_acept_plus_rech_le_cant
                       CHECK (cantidad_aceptada + cantidad_rechazada <= cantidad)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -2261 AND SQLCODE != -2260 THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE SOLTURNOS ADD CONSTRAINT ck_solt_futuroacept_plus_futurorech_le_cantfuturo
                       CHECK (cantidad_futuro_aceptada + cantidad_futuro_rechazada <= cantidad_futuro)';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -2261 AND SQLCODE != -2260 THEN RAISE; END IF;
END;
/

-- =============================================================================
-- 4) Comentarios
-- =============================================================================

COMMENT ON COLUMN SOLTURNOS.cantidad_rechazada IS 'Acumulador de cupos rechazados al cierre de la solicitud (≤ Cantidad). Se setea en el rechazo completo: cantidad_rechazada = cantidad - cantidad_aceptada al momento del rechazo.';
COMMENT ON COLUMN SOLTURNOS.cantidad_futuro_rechazada IS 'Acumulador de cupos rechazados con EsFuturo=true (≤ CantidadFuturo). Subconjunto de cantidad_rechazada.';

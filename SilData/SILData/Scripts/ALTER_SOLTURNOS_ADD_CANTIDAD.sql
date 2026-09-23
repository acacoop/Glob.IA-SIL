-- =============================================================================
-- Migración SOLTURNOS: columnas CANTIDAD y CANTIDAD_FUTURO
-- Esquema: cuposcorretaje (o el que use SilConnection)
-- Ejecutar cada bloque por separado en SQL Developer / SQL*Plus (F5 o Run Script).
-- NO usar "Run Statement" si el cliente solo admite un comando SELECT.
-- =============================================================================

-- 1) Verificar que SOLTURNOS es una TABLA (no una vista)
SELECT object_type, owner, object_name
  FROM all_objects
 WHERE UPPER(object_name) = 'SOLTURNOS'
   AND owner = USER;

-- Si no devuelve filas, probar con el esquema explícito:
-- SELECT object_type, owner, object_name FROM all_objects
--  WHERE UPPER(object_name) = 'SOLTURNOS' AND UPPER(owner) = 'CUPOSCORRETAJE';

-- 2) Ver si las columnas ya existen (evita error ORA-01430)
SELECT column_name, data_type, data_default, nullable
  FROM user_tab_columns
 WHERE table_name = 'SOLTURNOS'
   AND column_name IN ('CANTIDAD', 'CANTIDAD_FUTURO')
 ORDER BY column_name;

-- =============================================================================
-- 3) Agregar columnas (una sentencia por línea; sintaxis compatible Oracle)
-- Si falla por privilegios: pedir ALTER ANY TABLE o ALTER sobre SOLTURNOS al DBA.
-- =============================================================================

ALTER TABLE SOLTURNOS ADD CANTIDAD NUMBER(10) DEFAULT 1 NOT NULL;

ALTER TABLE SOLTURNOS ADD CANTIDAD_FUTURO NUMBER(10) DEFAULT 0 NOT NULL;

-- =============================================================================
-- Si lo anterior falla con ORA-01735 u otro error en tablas con muchos datos,
-- usar este plan alternativo (descomentar y ejecutar en orden):
-- =============================================================================
-- ALTER TABLE SOLTURNOS ADD CANTIDAD NUMBER(10);
-- UPDATE SOLTURNOS SET CANTIDAD = 1 WHERE CANTIDAD IS NULL;
-- ALTER TABLE SOLTURNOS MODIFY CANTIDAD DEFAULT 1 NOT NULL;
--
-- ALTER TABLE SOLTURNOS ADD CANTIDAD_FUTURO NUMBER(10);
-- UPDATE SOLTURNOS SET CANTIDAD_FUTURO = 0 WHERE CANTIDAD_FUTURO IS NULL;
-- ALTER TABLE SOLTURNOS MODIFY CANTIDAD_FUTURO DEFAULT 0 NOT NULL;

-- 4) Comentarios (opcional; ejecutar después de crear las columnas)
COMMENT ON COLUMN SOLTURNOS.CANTIDAD IS 'Cantidad de cupos/turnos solicitados en esta solicitud';
COMMENT ON COLUMN SOLTURNOS.CANTIDAD_FUTURO IS 'Cupos de CANTIDAD que se contabilizan como solicitud futura';

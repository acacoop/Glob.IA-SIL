-- =============================================
-- Función para limpiar CUITs (remover guiones)
-- =============================================
--SELECT * FROM USER_ERRORS WHERE NAME = 'LIMPIAR_CUIT';

CREATE OR REPLACE FUNCTION LIMPIAR_CUIT(p_cuit IN VARCHAR2) 
RETURN VARCHAR2 
DETERMINISTIC
IS
BEGIN
    IF p_cuit IS NULL THEN
        RETURN NULL;
    END IF;
    RETURN REPLACE(p_cuit, '-', '');
END LIMPIAR_CUIT;
/
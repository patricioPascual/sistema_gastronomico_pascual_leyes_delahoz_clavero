-- =====================================================================
-- Ajustes de datos para las validaciones nuevas (correr UNA vez sobre
-- una base ya creada con el dump anterior). El dump principal ya viene
-- corregido, así que en una instalación desde cero no hace falta.
-- =====================================================================

START TRANSACTION;

-- 1) Unidad de medida: la lista cerrada usa 'lt' (igual que el <select> de la vista).
--    El dump anterior cargaba 'litro', que no estaba en el select ni pasa la validación nueva.
UPDATE producto SET unidad_medida = 'lt' WHERE unidad_medida = 'litro';

-- 2) CUIT: los proveedores de prueba tenían el dígito verificador mal calculado
--    (9 de 10). Con el chequeo módulo 11 no se podrían editar sin cambiar el CUIT.
UPDATE proveedor SET cuit = '30-70000000-8' WHERE cuit = '30-70000000-0';
UPDATE proveedor SET cuit = '30-72222222-5' WHERE cuit = '30-72222222-2';
UPDATE proveedor SET cuit = '30-73333333-9' WHERE cuit = '30-73333333-3';
UPDATE proveedor SET cuit = '30-74444444-2' WHERE cuit = '30-74444444-4';
UPDATE proveedor SET cuit = '30-75555555-6' WHERE cuit = '30-75555555-5';
UPDATE proveedor SET cuit = '30-76666667-8' WHERE cuit = '30-76666666-6';  -- 30-76666666 no admite dígito válido (da 10)
UPDATE proveedor SET cuit = '30-77777777-3' WHERE cuit = '30-77777777-7';
UPDATE proveedor SET cuit = '30-78888888-7' WHERE cuit = '30-78888888-8';
UPDATE proveedor SET cuit = '30-79999999-0' WHERE cuit = '30-79999999-9';

COMMIT;

-- Control: estas consultas deberían devolver 0 filas.
SELECT id_producto, nombre, unidad_medida
FROM producto
WHERE unidad_medida NOT IN ('kg','gr','lt','ml','unidad','docena','caja','lata');

SELECT id_empleado, legajo, dni
FROM empleado
WHERE legajo NOT REGEXP '^[0-9]{4,10}$' OR dni NOT REGEXP '^[0-9]{7,8}$';

SELECT id_usuario, username
FROM usuario
WHERE username NOT REGEXP '^[A-Za-z0-9._]{4,30}$';

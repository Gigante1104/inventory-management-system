-- ENTRADA: suma stock y deja registro en movimientos
CREATE OR REPLACE PROCEDURE sp_registrar_entrada(
    p_producto_id INTEGER,
    p_cantidad    INTEGER,
    p_observacion VARCHAR DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_cantidad <= 0 THEN
        RAISE EXCEPTION 'La cantidad debe ser mayor a cero';
    END IF;

    -- Bloquea la fila para evitar cambios simultáneos
    PERFORM 1 FROM productos WHERE id = p_producto_id FOR UPDATE;
    IF NOT FOUND THEN
        RAISE EXCEPTION 'El producto % no existe', p_producto_id;
    END IF;

    UPDATE productos
       SET stock_actual = stock_actual + p_cantidad
     WHERE id = p_producto_id;

    INSERT INTO movimientos (producto_id, tipo, cantidad, observacion)
    VALUES (p_producto_id, 'ENTRADA', p_cantidad, p_observacion);
END;
$$;

-- SALIDA: valida que haya stock suficiente antes de restar
CREATE OR REPLACE PROCEDURE sp_registrar_salida(
    p_producto_id INTEGER,
    p_cantidad    INTEGER,
    p_observacion VARCHAR DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_stock INTEGER;
BEGIN
    IF p_cantidad <= 0 THEN
        RAISE EXCEPTION 'La cantidad debe ser mayor a cero';
    END IF;

    SELECT stock_actual INTO v_stock
      FROM productos
     WHERE id = p_producto_id
       FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'El producto % no existe', p_producto_id;
    END IF;

    IF v_stock < p_cantidad THEN
        RAISE EXCEPTION 'Stock insuficiente. Disponible: %, solicitado: %', v_stock, p_cantidad;
    END IF;

    UPDATE productos
       SET stock_actual = stock_actual - p_cantidad
     WHERE id = p_producto_id;

    INSERT INTO movimientos (producto_id, tipo, cantidad, observacion)
    VALUES (p_producto_id, 'SALIDA', p_cantidad, p_observacion);
END;
$$;

-- INVENTARIO ACTUAL: devuelve una tabla, por eso es FUNCTION y no PROCEDURE
CREATE OR REPLACE FUNCTION fn_inventario_actual()
RETURNS TABLE (
    id            INTEGER,
    codigo        VARCHAR,
    nombre        VARCHAR,
    precio        NUMERIC,
    stock_actual  INTEGER,
    stock_minimo  INTEGER,
    valor_total   NUMERIC,
    bajo_stock    BOOLEAN
)
LANGUAGE sql
STABLE
AS $$
    SELECT p.id,
           p.codigo,
           p.nombre,
           p.precio,
           p.stock_actual,
           p.stock_minimo,
           p.precio * p.stock_actual,         -- valor del stock de ese producto
           p.stock_actual <= p.stock_minimo   -- true si está en o bajo el mínimo
      FROM productos p
     ORDER BY p.nombre;
$$;
-- Usuarios del sistema (la autenticación se usa en la Fase 5)
CREATE TABLE IF NOT EXISTS usuarios (
    id             SERIAL PRIMARY KEY,
    nombre_usuario VARCHAR(50)  NOT NULL UNIQUE,
    password_hash  VARCHAR(200) NOT NULL,          -- nunca guardamos la clave en texto plano
    rol            VARCHAR(20)  NOT NULL DEFAULT 'usuario',
    creado_en      TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Catálogo de productos con su stock actual
CREATE TABLE IF NOT EXISTS productos (
    id            SERIAL PRIMARY KEY,
    codigo        VARCHAR(30)   NOT NULL UNIQUE,   -- código/SKU, no se repite
    nombre        VARCHAR(100)  NOT NULL,
    descripcion   VARCHAR(300),
    precio        NUMERIC(12,2) NOT NULL CHECK (precio >= 0),
    stock_actual  INTEGER       NOT NULL DEFAULT 0 CHECK (stock_actual >= 0),
    stock_minimo  INTEGER       NOT NULL DEFAULT 0 CHECK (stock_minimo >= 0),
    creado_en     TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

-- Historial de entradas y salidas
CREATE TABLE IF NOT EXISTS movimientos (
    id           SERIAL PRIMARY KEY,
    producto_id  INTEGER      NOT NULL REFERENCES productos(id),
    tipo         VARCHAR(10)  NOT NULL CHECK (tipo IN ('ENTRADA', 'SALIDA')),
    cantidad     INTEGER      NOT NULL CHECK (cantidad > 0),
    observacion  VARCHAR(200),
    fecha        TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Índice para consultar rápido el historial de un producto
CREATE INDEX IF NOT EXISTS idx_movimientos_producto ON movimientos(producto_id);
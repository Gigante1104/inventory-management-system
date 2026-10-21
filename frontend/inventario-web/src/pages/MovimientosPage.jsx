import { useCallback, useEffect, useState } from 'react'
import { api, obtenerMensajeError } from '../api/client'
import Alerta from '../components/Alerta'

export default function MovimientosPage() {
  const [productos, setProductos] = useState([])
  const [movimientos, setMovimientos] = useState([])

  // Formulario de registro
  const [productoId, setProductoId] = useState('')
  const [tipo, setTipo] = useState('ENTRADA')
  const [cantidad, setCantidad] = useState('')
  const [observacion, setObservacion] = useState('')

  // Filtros del historial
  const [filtroProducto, setFiltroProducto] = useState('')
  const [filtroTipo, setFiltroTipo] = useState('')

  const [error, setError] = useState('')
  const [exito, setExito] = useState('')

  const cargarMovimientos = useCallback(() => {
    const params = {}
    if (filtroProducto) params.productoId = filtroProducto
    if (filtroTipo) params.tipo = filtroTipo
    return api.get('/inventario/movimientos', { params })
      .then(({ data }) => setMovimientos(data))
      .catch((err) => setError(obtenerMensajeError(err)))
  }, [filtroProducto, filtroTipo])

  const cargarProductos = () =>
    api.get('/productos')
      .then(({ data }) => setProductos(data))
      .catch((err) => setError(obtenerMensajeError(err)))

  useEffect(() => { cargarProductos() }, [])
  useEffect(() => { cargarMovimientos() }, [cargarMovimientos])

  const registrar = async (e) => {
    e.preventDefault()
    setError('')
    setExito('')

    if (!productoId) return setError('Selecciona un producto')
    if (!Number.isInteger(Number(cantidad)) || Number(cantidad) <= 0) {
      return setError('La cantidad debe ser un número entero mayor a cero')
    }

    try {
      const ruta = tipo === 'ENTRADA' ? '/inventario/entrada' : '/inventario/salida'
      const { data } = await api.post(ruta, {
        productoId: Number(productoId),
        cantidad: Number(cantidad),
        observacion: observacion.trim() || null,
      })
      setExito(`${data.mensaje}. Stock actual: ${data.stockActual}`)
      setCantidad('')
      setObservacion('')
      cargarProductos()
      cargarMovimientos()
    } catch (err) {
      setError(obtenerMensajeError(err))
    }
  }

  const productoElegido = productos.find((p) => p.id === Number(productoId))

  return (
    <>
      <h1 className="h3 mb-3">Movimientos</h1>
      <Alerta tipo="danger" mensaje={error} onCerrar={() => setError('')} />
      <Alerta tipo="success" mensaje={exito} onCerrar={() => setExito('')} />

      <div className="card shadow-sm mb-4">
        <div className="card-body">
          <h2 className="h6 mb-3">Registrar entrada o salida</h2>
          <form onSubmit={registrar} noValidate>
            <div className="row g-3">
              <div className="col-12 col-md-5">
                <label className="form-label" htmlFor="producto">Producto</label>
                <select id="producto" className="form-select" value={productoId} onChange={(e) => setProductoId(e.target.value)}>
                  <option value="">Selecciona...</option>
                  {productos.map((p) => (
                    <option key={p.id} value={p.id}>{p.codigo} — {p.nombre}</option>
                  ))}
                </select>
                {productoElegido && <div className="form-text">Stock actual: {productoElegido.stockActual}</div>}
              </div>
              <div className="col-6 col-md-2">
                <label className="form-label" htmlFor="tipo">Tipo</label>
                <select id="tipo" className="form-select" value={tipo} onChange={(e) => setTipo(e.target.value)}>
                  <option value="ENTRADA">Entrada</option>
                  <option value="SALIDA">Salida</option>
                </select>
              </div>
              <div className="col-6 col-md-2">
                <label className="form-label" htmlFor="cantidad">Cantidad</label>
                <input id="cantidad" type="number" min="1" className="form-control" value={cantidad} onChange={(e) => setCantidad(e.target.value)} />
              </div>
              <div className="col-12 col-md-3">
                <label className="form-label" htmlFor="observacion">Observación</label>
                <input id="observacion" className="form-control" value={observacion} onChange={(e) => setObservacion(e.target.value)} />
              </div>
            </div>
            <button className={`btn mt-3 ${tipo === 'ENTRADA' ? 'btn-success' : 'btn-warning'}`}>
              Registrar {tipo === 'ENTRADA' ? 'entrada' : 'salida'}
            </button>
          </form>
        </div>
      </div>

      <div className="card shadow-sm">
        <div className="card-body">
          <h2 className="h6 mb-3">Historial</h2>
          <div className="row g-2 mb-3">
            <div className="col-12 col-md-6">
              <select className="form-select" aria-label="Filtrar por producto" value={filtroProducto} onChange={(e) => setFiltroProducto(e.target.value)}>
                <option value="">Todos los productos</option>
                {productos.map((p) => <option key={p.id} value={p.id}>{p.codigo} — {p.nombre}</option>)}
              </select>
            </div>
            <div className="col-12 col-md-3">
              <select className="form-select" aria-label="Filtrar por tipo" value={filtroTipo} onChange={(e) => setFiltroTipo(e.target.value)}>
                <option value="">Entradas y salidas</option>
                <option value="ENTRADA">Solo entradas</option>
                <option value="SALIDA">Solo salidas</option>
              </select>
            </div>
          </div>
        </div>
        <div className="table-responsive">
          <table className="table table-hover align-middle mb-0">
            <thead className="table-light">
              <tr>
                <th>Fecha</th>
                <th>Producto</th>
                <th>Tipo</th>
                <th className="text-end">Cantidad</th>
                <th>Observación</th>
              </tr>
            </thead>
            <tbody>
              {movimientos.length === 0 && (
                <tr><td colSpan="5" className="text-center text-muted py-4">Sin movimientos</td></tr>
              )}
              {movimientos.map((m) => (
                <tr key={m.id}>
                  <td className="text-nowrap">{new Date(m.fecha).toLocaleString('es-CO')}</td>
                  <td>{m.codigoProducto} — {m.nombreProducto}</td>
                  <td>
                    <span className={`badge ${m.tipo === 'ENTRADA' ? 'text-bg-success' : 'text-bg-warning'}`}>{m.tipo}</span>
                  </td>
                  <td className="text-end">{m.cantidad}</td>
                  <td>{m.observacion}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </>
  )
}

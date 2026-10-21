import { useEffect, useState } from 'react'
import { api, obtenerMensajeError } from '../api/client'
import Alerta from '../components/Alerta'

const moneda = (valor) =>
  new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(valor)

export default function InventarioPage() {
  const [items, setItems] = useState([])
  const [busqueda, setBusqueda] = useState('')
  const [error, setError] = useState('')
  const [cargando, setCargando] = useState(true)

  useEffect(() => {
    api.get('/inventario')
      .then(({ data }) => setItems(data))
      .catch((err) => setError(obtenerMensajeError(err)))
      .finally(() => setCargando(false))
  }, [])

  const texto = busqueda.trim().toLowerCase()
  const filtrados = items.filter(
    (i) => i.nombre.toLowerCase().includes(texto) || i.codigo.toLowerCase().includes(texto),
  )

  // Números de las tarjetas resumen
  const totalUnidades = items.reduce((suma, i) => suma + i.stockActual, 0)
  const valorTotal = items.reduce((suma, i) => suma + i.valorTotal, 0)
  const bajoStock = items.filter((i) => i.bajoStock).length

  return (
    <>
      <h1 className="h3 mb-3">Inventario actual</h1>
      <Alerta tipo="danger" mensaje={error} onCerrar={() => setError('')} />

      <div className="row g-3 mb-4">
        <Resumen titulo="Productos" valor={items.length} />
        <Resumen titulo="Unidades en stock" valor={totalUnidades} />
        <Resumen titulo="Valor del inventario" valor={moneda(valorTotal)} />
        <Resumen titulo="Con stock bajo" valor={bajoStock} alerta={bajoStock > 0} />
      </div>

      <input
        className="form-control mb-3"
        placeholder="Buscar por código o nombre..."
        value={busqueda}
        onChange={(e) => setBusqueda(e.target.value)}
      />

      <div className="card shadow-sm">
        <div className="table-responsive">
          <table className="table table-hover align-middle mb-0">
            <thead className="table-light">
              <tr>
                <th className="text-nowrap">Código</th>
                <th>Producto</th>
                <th className="text-end d-none d-md-table-cell">Precio</th>
                <th className="text-end">Stock</th>
                <th className="text-end d-none d-md-table-cell">Mínimo</th>
                <th className="text-end d-none d-md-table-cell">Valor</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              {cargando && <tr><td colSpan="7" className="text-center py-4">Cargando...</td></tr>}
              {!cargando && filtrados.length === 0 && (
                <tr><td colSpan="7" className="text-center text-muted py-4">No hay productos para mostrar</td></tr>
              )}
              {filtrados.map((i) => (
                <tr key={i.id}>
                  <td className="text-nowrap">{i.codigo}</td>
                  <td>{i.nombre}</td>
                  <td className="text-end d-none d-md-table-cell">{moneda(i.precio)}</td>
                  <td className="text-end fw-semibold">{i.stockActual}</td>
                  <td className="text-end d-none d-md-table-cell">{i.stockMinimo}</td>
                  <td className="text-end d-none d-md-table-cell">{moneda(i.valorTotal)}</td>
                  <td>
                    {i.bajoStock
                      ? <span className="badge text-bg-danger">Stock bajo</span>
                      : <span className="badge text-bg-success">OK</span>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </>
  )
}

function Resumen({ titulo, valor, alerta = false }) {
  return (
    <div className="col-6 col-lg-3">
      <div className={`card shadow-sm tarjeta-resumen${alerta ? ' alerta' : ''}`}>
        <div className="card-body">
          <div className="text-muted small">{titulo}</div>
          <div className="fs-5 fw-bold">{valor}</div>
        </div>
      </div>
    </div>
  )
}

import { useEffect, useState } from 'react'
import { api, obtenerMensajeError } from '../api/client'
import Alerta from '../components/Alerta'

const FORM_VACIO = { codigo: '', nombre: '', descripcion: '', precio: '', stockMinimo: 0 }

export default function ProductosPage() {
  const [productos, setProductos] = useState([])
  const [form, setForm] = useState(FORM_VACIO)
  const [editandoId, setEditandoId] = useState(null) // null = estamos creando
  const [error, setError] = useState('')
  const [exito, setExito] = useState('')

  const cargar = () =>
    api.get('/productos')
      .then(({ data }) => setProductos(data))
      .catch((err) => setError(obtenerMensajeError(err)))

  useEffect(() => { cargar() }, [])

  // Cuando aparece un mensaje, sube al inicio para que se vea aunque la acción fuera abajo (en la tabla)
  useEffect(() => {
    if (error || exito) window.scrollTo({ top: 0, behavior: 'smooth' })
  }, [error, exito])

  const cambiar = (e) => setForm({ ...form, [e.target.name]: e.target.value })

  const cancelar = () => {
    setForm(FORM_VACIO)
    setEditandoId(null)
  }

  const editar = (p) => {
    setEditandoId(p.id)
    setForm({
      codigo: p.codigo,
      nombre: p.nombre,
      descripcion: p.descripcion ?? '',
      precio: p.precio,
      stockMinimo: p.stockMinimo,
    })
    setError('')
    setExito('')
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const guardar = async (e) => {
    e.preventDefault()
    setError('')
    setExito('')

    // Validaciones del formulario
    if (!form.codigo.trim()) return setError('El código es obligatorio')
    if (!form.nombre.trim()) return setError('El nombre es obligatorio')
    if (form.precio === '' || Number(form.precio) < 0) return setError('El precio debe ser un número mayor o igual a cero')
    if (Number(form.stockMinimo) < 0) return setError('El stock mínimo no puede ser negativo')

    const cuerpo = {
      codigo: form.codigo.trim(),
      nombre: form.nombre.trim(),
      descripcion: form.descripcion.trim() || null,
      precio: Number(form.precio),
      stockMinimo: Number(form.stockMinimo),
    }

    try {
      if (editandoId) {
        await api.put(`/productos/${editandoId}`, cuerpo)
        setExito('Producto actualizado')
      } else {
        await api.post('/productos', cuerpo)
        setExito('Producto creado')
      }
      cancelar()
      cargar()
    } catch (err) {
      setError(obtenerMensajeError(err))
    }
  }

  const eliminar = async (p) => {
    if (!window.confirm(`¿Eliminar el producto "${p.nombre}"?`)) return
    setError('')
    setExito('')
    try {
      await api.delete(`/productos/${p.id}`)
      setExito('Producto eliminado')
      cargar()
    } catch (err) {
      setError(obtenerMensajeError(err))
    }
  }

  return (
    <>
      <h1 className="h3 mb-3">Productos</h1>
      <Alerta tipo="danger" mensaje={error} onCerrar={() => setError('')} />
      <Alerta tipo="success" mensaje={exito} onCerrar={() => setExito('')} />

      <div className="card shadow-sm mb-4">
        <div className="card-body">
          <h2 className="h6 mb-3">{editandoId ? 'Editar producto' : 'Nuevo producto'}</h2>
          <form onSubmit={guardar} noValidate>
            <div className="row g-3">
              <div className="col-12 col-md-3">
                <label className="form-label" htmlFor="codigo">Código</label>
                <input id="codigo" name="codigo" className="form-control" value={form.codigo} onChange={cambiar} />
              </div>
              <div className="col-12 col-md-5">
                <label className="form-label" htmlFor="nombre">Nombre</label>
                <input id="nombre" name="nombre" className="form-control" value={form.nombre} onChange={cambiar} />
              </div>
              <div className="col-6 col-md-2">
                <label className="form-label" htmlFor="precio">Precio</label>
                <input id="precio" name="precio" type="number" min="0" step="0.01" className="form-control" value={form.precio} onChange={cambiar} />
              </div>
              <div className="col-6 col-md-2">
                <label className="form-label" htmlFor="stockMinimo">Stock mínimo</label>
                <input id="stockMinimo" name="stockMinimo" type="number" min="0" className="form-control" value={form.stockMinimo} onChange={cambiar} />
              </div>
              <div className="col-12">
                <label className="form-label" htmlFor="descripcion">Descripción (opcional)</label>
                <input id="descripcion" name="descripcion" className="form-control" value={form.descripcion} onChange={cambiar} />
              </div>
            </div>
            <div className="mt-3 d-flex gap-2">
              <button className="btn btn-primary">{editandoId ? 'Guardar cambios' : 'Crear producto'}</button>
              {editandoId && <button type="button" className="btn btn-outline-secondary" onClick={cancelar}>Cancelar</button>}
            </div>
          </form>
        </div>
      </div>

      <div className="card shadow-sm">
        <div className="table-responsive">
          <table className="table table-hover align-middle mb-0">
            <thead className="table-light">
              <tr>
                <th className="text-nowrap">Código</th>
                <th>Nombre</th>
                <th className="text-end d-none d-md-table-cell">Precio</th>
                <th className="text-end">Stock</th>
                <th className="text-end">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {productos.length === 0 && (
                <tr><td colSpan="5" className="text-center text-muted py-4">Aún no hay productos</td></tr>
              )}
              {productos.map((p) => (
                <tr key={p.id}>
                  <td className="text-nowrap">{p.codigo}</td>
                  <td>{p.nombre}</td>
                  <td className="text-end d-none d-md-table-cell">{p.precio.toLocaleString('es-CO')}</td>
                  <td className="text-end">{p.stockActual}</td>
                  <td className="text-end text-nowrap">
                    <button className="btn btn-sm btn-outline-primary me-2" onClick={() => editar(p)}>Editar</button>
                    <button className="btn btn-sm btn-outline-danger" onClick={() => eliminar(p)}>Eliminar</button>
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

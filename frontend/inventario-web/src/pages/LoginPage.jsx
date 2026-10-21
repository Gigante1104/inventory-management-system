import { useState } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { api, obtenerMensajeError } from '../api/client'
import { useAuth } from '../context/AuthContext'
import Alerta from '../components/Alerta'

export default function LoginPage() {
  const { sesion, iniciarSesion } = useAuth()
  const navigate = useNavigate()

  const [modoRegistro, setModoRegistro] = useState(false)
  const [nombreUsuario, setNombreUsuario] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [cargando, setCargando] = useState(false)

  // Si ya hay sesión, no tiene sentido ver el login
  if (sesion?.token) return <Navigate to="/" replace />

  const enviar = async (e) => {
    e.preventDefault()
    setError('')

    // Validaciones básicas antes de llamar a la API
    if (nombreUsuario.trim().length < 3) return setError('El usuario debe tener al menos 3 caracteres')
    if (password.length < 6) return setError('La contraseña debe tener al menos 6 caracteres')

    setCargando(true)
    try {
      const ruta = modoRegistro ? '/auth/register' : '/auth/login'
      const { data } = await api.post(ruta, { nombreUsuario: nombreUsuario.trim(), password })
      iniciarSesion(data)
      navigate('/')
    } catch (err) {
      setError(obtenerMensajeError(err))
    } finally {
      setCargando(false)
    }
  }

  return (
    <div className="container login-contenedor py-5">
      <div className="card shadow-sm">
        <div className="card-body p-4">
          <h1 className="h4 text-center mb-4">📦 Gestión de Inventario</h1>
          <h2 className="h6 text-center text-muted mb-3">
            {modoRegistro ? 'Crear cuenta' : 'Iniciar sesión'}
          </h2>

          <Alerta tipo="danger" mensaje={error} onCerrar={() => setError('')} />

          <form onSubmit={enviar} noValidate>
            <div className="mb-3">
              <label htmlFor="usuario" className="form-label">Usuario</label>
              <input
                id="usuario"
                className="form-control"
                value={nombreUsuario}
                onChange={(e) => setNombreUsuario(e.target.value)}
                autoComplete="username"
              />
            </div>
            <div className="mb-3">
              <label htmlFor="password" className="form-label">Contraseña</label>
              <input
                id="password"
                type="password"
                className="form-control"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                autoComplete={modoRegistro ? 'new-password' : 'current-password'}
              />
            </div>
            <button className="btn btn-primary w-100" disabled={cargando}>
              {cargando ? 'Procesando...' : modoRegistro ? 'Registrarme' : 'Entrar'}
            </button>
          </form>

          <button
            type="button"
            className="btn btn-link w-100 mt-2"
            onClick={() => { setModoRegistro(!modoRegistro); setError('') }}
          >
            {modoRegistro ? 'Ya tengo cuenta' : 'No tengo cuenta, quiero registrarme'}
          </button>
        </div>
      </div>
    </div>
  )
}

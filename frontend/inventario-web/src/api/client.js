import axios from 'axios'

const SESION_KEY = 'sesion'

// Guardamos token y usuario en localStorage para que la sesión sobreviva al recargar la página
export function guardarSesion(sesion) {
  localStorage.setItem(SESION_KEY, JSON.stringify(sesion))
}

export function leerSesion() {
  try {
    return JSON.parse(localStorage.getItem(SESION_KEY))
  } catch {
    return null
  }
}

export function borrarSesion() {
  localStorage.removeItem(SESION_KEY)
}

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5073/api',
})

// Antes de cada petición, agrega el token JWT en la cabecera Authorization
api.interceptors.request.use((config) => {
  const sesion = leerSesion()
  if (sesion?.token) config.headers.Authorization = `Bearer ${sesion.token}`
  return config
})

// Si el token venció o es inválido (401), cerramos sesión y volvemos al login
api.interceptors.response.use(
  (respuesta) => respuesta,
  (error) => {
    const esAuth = error.config?.url?.startsWith('/auth')
    if (error.response?.status === 401 && !esAuth) {
      borrarSesion()
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)

// Convierte cualquier error de la API en un texto legible para el usuario
export function obtenerMensajeError(error) {
  if (!error.response) {
    return 'No se pudo conectar con el servidor. Verifica que la API esté encendida.'
  }
  const data = error.response.data
  if (data?.errors) return Object.values(data.errors).flat().join(' ') // errores de validación
  return data?.detail ?? data?.title ?? 'Ocurrió un error inesperado'
}

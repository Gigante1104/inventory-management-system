import { createContext, useContext, useState } from 'react'
import { borrarSesion, guardarSesion, leerSesion } from '../api/client'

const AuthContext = createContext(null)

// Comparte "quién está logueado" con toda la aplicación
export function AuthProvider({ children }) {
  const [sesion, setSesion] = useState(() => leerSesion())

  const iniciarSesion = (datos) => {
    guardarSesion(datos)
    setSesion(datos)
  }

  const cerrarSesion = () => {
    borrarSesion()
    setSesion(null)
  }

  return (
    <AuthContext.Provider value={{ sesion, iniciarSesion, cerrarSesion }}>
      {children}
    </AuthContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  return useContext(AuthContext)
}

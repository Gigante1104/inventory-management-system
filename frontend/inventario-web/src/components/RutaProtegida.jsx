import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import Navbar from './Navbar'

// Si no hay sesión, redirige al login. Si la hay, muestra el menú y la página hija.
export default function RutaProtegida() {
  const { sesion } = useAuth()

  if (!sesion?.token) return <Navigate to="/login" replace />

  return (
    <>
      <Navbar />
      <main className="container py-4">
        <Outlet />
      </main>
    </>
  )
}

import { useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function Navbar() {
  const { sesion, cerrarSesion } = useAuth()
  const [abierto, setAbierto] = useState(false) // menú colapsable en móvil
  const navigate = useNavigate()

  const salir = () => {
    cerrarSesion()
    navigate('/login')
  }

  const claseLink = ({ isActive }) => `nav-link${isActive ? ' active fw-semibold' : ''}`

  return (
    <nav className="navbar navbar-expand-md navbar-dark bg-dark">
      <div className="container">
        <span className="navbar-brand">📦 Inventario</span>
        <button
          className="navbar-toggler"
          type="button"
          aria-label="Abrir menú"
          onClick={() => setAbierto(!abierto)}
        >
          <span className="navbar-toggler-icon" />
        </button>

        <div className={`collapse navbar-collapse${abierto ? ' show' : ''}`}>
          <ul className="navbar-nav me-auto" onClick={() => setAbierto(false)}>
            <li className="nav-item">
              <NavLink to="/" end className={claseLink}>Inventario</NavLink>
            </li>
            <li className="nav-item">
              <NavLink to="/productos" className={claseLink}>Productos</NavLink>
            </li>
            <li className="nav-item">
              <NavLink to="/movimientos" className={claseLink}>Movimientos</NavLink>
            </li>
          </ul>
          <span className="navbar-text me-md-3">👤 {sesion?.nombreUsuario}</span>
          <button className="btn btn-outline-light btn-sm" onClick={salir}>Salir</button>
        </div>
      </div>
    </nav>
  )
}

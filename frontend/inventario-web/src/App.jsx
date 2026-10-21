import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import RutaProtegida from './components/RutaProtegida'
import LoginPage from './pages/LoginPage'
import InventarioPage from './pages/InventarioPage'
import ProductosPage from './pages/ProductosPage'
import MovimientosPage from './pages/MovimientosPage'

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />

          {/* Todo lo que está aquí dentro exige sesión iniciada */}
          <Route element={<RutaProtegida />}>
            <Route index element={<InventarioPage />} />
            <Route path="productos" element={<ProductosPage />} />
            <Route path="movimientos" element={<MovimientosPage />} />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}

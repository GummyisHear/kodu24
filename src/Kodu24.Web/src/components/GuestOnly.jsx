import { Navigate } from 'react-router'
import { useAuth } from '../auth/useAuth.js'

/** Sisselogimise ja registreerimise leht: sisselogitud kasutaja suunatakse avalehele. */
export default function GuestOnly({ children }) {
  const { user, loading } = useAuth()

  if (loading) return <p className="muted">Laen…</p>
  if (user) return <Navigate to="/" replace />
  return children
}

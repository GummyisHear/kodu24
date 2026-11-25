import { Navigate, useLocation } from 'react-router'
import { useAuth } from '../auth/useAuth.js'

/**
 * Kaitseb lehte: külaline suunatakse sisselogimisele (ja pärast tagasi),
 * vale rolliga kasutaja näeb teadet. Näide: <RequireAuth roles={['Landlord', 'Admin']}>.
 * NB! See on ainult mugavus kasutajale – päris kontroll toimub API-s.
 */
export default function RequireAuth({ roles, children }) {
  const { user, loading } = useAuth()
  const location = useLocation()

  if (loading) return <p className="muted">Laen…</p>
  if (!user) return <Navigate to="/login" replace state={{ from: location }} />
  if (roles && !roles.includes(user.role)) {
    return (
      <section className="hero">
        <h1>Ligipääs puudub</h1>
        <p>Selle lehe vaatamiseks ei ole sinu kontol õigusi.</p>
      </section>
    )
  }
  return children
}

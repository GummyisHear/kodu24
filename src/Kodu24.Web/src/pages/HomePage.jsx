import { Link } from 'react-router'
import { useAuth } from '../auth/useAuth.js'

export default function HomePage() {
  const { user } = useAuth()

  return (
    <section className="hero">
      <h1>{user ? `Tere, ${user.fullName}!` : 'Leia endale uus kodu'}</h1>
      <p>Pikaajalised üürikorterid Tallinnas – otse üürileandjalt.</p>
      {!user && (
        <div className="hero__actions">
          <Link to="/register" className="button">
            Loo konto
          </Link>
          <Link to="/login" className="button button--secondary">
            Logi sisse
          </Link>
        </div>
      )}
    </section>
  )
}

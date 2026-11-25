import { Link } from 'react-router'

export default function HomePage() {
  return (
    <section className="hero">
      <h1>Leia endale uus kodu</h1>
      <p>Pikaajalised üürikorterid Tallinnas – otse üürileandjalt.</p>
      <div className="hero__actions">
        <Link to="/register" className="button">
          Loo konto
        </Link>
        <Link to="/login" className="button button--secondary">
          Logi sisse
        </Link>
      </div>
    </section>
  )
}

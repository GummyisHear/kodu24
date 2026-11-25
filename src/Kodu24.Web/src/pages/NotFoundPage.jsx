import { Link } from 'react-router'

export default function NotFoundPage() {
  return (
    <section className="hero">
      <h1>Lehte ei leitud</h1>
      <p>Otsitud lehte ei ole olemas või see on eemaldatud.</p>
      <Link to="/" className="button">
        Avalehele
      </Link>
    </section>
  )
}

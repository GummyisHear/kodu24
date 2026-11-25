import { Link } from 'react-router'

export default function Header() {
  return (
    <header className="header">
      <div className="container header__inner">
        <Link to="/" className="header__logo">
          <img src="/logo.svg" alt="" />
          Kodu24
        </Link>

        <div className="header__user">
          <Link to="/login">Logi sisse</Link>
          <Link to="/register" className="button">
            Registreeri
          </Link>
        </div>
      </div>
    </header>
  )
}

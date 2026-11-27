import { Link, NavLink, useNavigate } from 'react-router'
import { roleLabels, Roles } from '../auth/roles.js'
import { useAuth } from '../auth/useAuth.js'
import { useInquirySocket } from '../realtime/useInquirySocket.js'

export default function Header() {
  const { user, loading, logout } = useAuth()
  const navigate = useNavigate()
  const { newCount } = useInquirySocket()

  function handleLogout() {
    logout()
    navigate('/')
  }

  return (
    <header className="header">
      <div className="container header__inner">
        <Link to="/" className="header__logo">
          <img src="/logo.svg" alt="" />
          Kodu24
        </Link>

        <nav className="header__nav" aria-label="Peamenüü">
          <NavLink to="/listings" end>
            Kuulutused
          </NavLink>
          <NavLink to="/map">Kaart</NavLink>
          {(user?.role === Roles.Landlord || user?.role === Roles.Admin) && (
            <>
              <NavLink to="/my-listings">Minu kuulutused</NavLink>
              <NavLink to="/inquiries">
                Päringud
                {newCount > 0 && (
                  <span className="badge header__badge" aria-label={`${newCount} uut päringut`}>
                    {newCount}
                  </span>
                )}
              </NavLink>
            </>
          )}
          {user?.role === Roles.Seeker && (
            <>
              <NavLink to="/favorites">Lemmikud</NavLink>
              <NavLink to="/saved-searches">Minu otsingud</NavLink>
            </>
          )}
        </nav>

        {!loading && (
          <div className="header__user">
            {user ? (
              <>
                <span className="header__name">
                  {user.fullName} ({roleLabels[user.role] ?? user.role})
                </span>
                <button type="button" className="button button--secondary" onClick={handleLogout}>
                  Logi välja
                </button>
              </>
            ) : (
              <>
                <Link to="/login">Logi sisse</Link>
                <Link to="/register" className="button">
                  Registreeri
                </Link>
              </>
            )}
          </div>
        )}
      </div>
    </header>
  )
}

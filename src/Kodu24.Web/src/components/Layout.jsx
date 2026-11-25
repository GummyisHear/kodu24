import { Outlet } from 'react-router'
import Footer from './Footer.jsx'
import Header from './Header.jsx'

/** Kõigi lehtede ühine raam: päis, sisu ja jalus. */
export default function Layout() {
  return (
    <div className="app">
      <Header />
      <main className="main container">
        <Outlet />
      </main>
      <Footer />
    </div>
  )
}

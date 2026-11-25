import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { searchListings } from '../api/listings.js'
import { toQueryString } from '../api/client.js'
import { useAuth } from '../auth/useAuth.js'
import ListingCard from '../components/ListingCard.jsx'

const LATEST_COUNT = 6

export default function HomePage() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [latest, setLatest] = useState(null)

  useEffect(() => {
    // Avalehe viimased kuulutused; vea korral jätame selle ploki lihtsalt ära.
    searchListings({ sort: 'newest', pageSize: LATEST_COUNT })
      .then((data) => setLatest(data.items))
      .catch(() => setLatest([]))
  }, [])

  function handleSearch(event) {
    event.preventDefault()
    navigate('/listings' + toQueryString({ q: search.trim() }))
  }

  return (
    <>
      <section className="hero">
        <h1>{user ? `Tere, ${user.fullName}!` : 'Leia endale uus kodu'}</h1>
        <p>Pikaajalised üürikorterid Tallinnas – otse üürileandjalt.</p>
        <form className="hero__search" onSubmit={handleSearch} role="search">
          <input
            type="search"
            aria-label="Otsingusõna"
            placeholder="Aadress, linnaosa või märksõna"
            maxLength={100}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <button type="submit" className="button">
            Otsi
          </button>
        </form>
        {!user && (
          <div className="hero__actions">
            <Link to="/register">Loo konto</Link>
            <span className="muted">või</span>
            <Link to="/login">logi sisse</Link>
          </div>
        )}
      </section>

      {latest && latest.length > 0 && (
        <section>
          <div className="section-header">
            <h2>Viimased kuulutused</h2>
            <Link to="/listings">Kõik kuulutused →</Link>
          </div>
          <div className="listing-grid">
            {latest.map((listing) => (
              <ListingCard key={listing.id} listing={listing} />
            ))}
          </div>
        </section>
      )}
    </>
  )
}

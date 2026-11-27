import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { getFavorites } from '../api/favorites.js'
import ListingCard from '../components/ListingCard.jsx'
import { useFavorites } from '../favorites/useFavorites.js'
import { formatListingCount } from '../utils/format.js'

/** /favorites: üürija lemmikud. Ligipääs: üürija (vt App.jsx). */
export default function FavoritesPage() {
  const [favorites, setFavorites] = useState(null)
  const [error, setError] = useState('')
  const { ready, isFavorite } = useFavorites()

  useEffect(() => {
    let cancelled = false
    getFavorites()
      .then((data) => !cancelled && setFavorites(data))
      .catch((err) => !cancelled && setError(err.message))
    return () => {
      cancelled = true
    }
  }, [])

  // Südamest eemaldatud kuulutus kaob lehelt kohe; nimekirja uuesti ei laeta.
  const visible = ready ? favorites?.filter((f) => isFavorite(f.listingId)) : favorites

  return (
    <div>
      <div className="section-header">
        <div>
          <h1>Lemmikud</h1>
          {visible && <p className="muted">{formatListingCount(visible.length)}</p>}
        </div>
      </div>

      {error && (
        <p className="alert" role="alert">
          {error}
        </p>
      )}

      {!favorites && !error && <p className="muted">Laen lemmikuid…</p>}

      {visible?.length === 0 && (
        <div className="card empty-state">
          <p>Sul ei ole veel lemmikuid. Vajuta kuulutusel südamele, et see siia salvestada.</p>
          <Link to="/listings" className="button">
            Vaata kuulutusi
          </Link>
        </div>
      )}

      {visible?.length > 0 && (
        <div className="listing-grid">
          {visible.map((favorite) => (
            <ListingCard key={favorite.listingId} listing={favorite.listing} />
          ))}
        </div>
      )}
    </div>
  )
}

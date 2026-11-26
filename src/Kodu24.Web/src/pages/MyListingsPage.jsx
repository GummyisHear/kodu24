import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { getMyListings } from '../api/listings.js'
import { formatArea, formatDate, formatListingCount, formatPrice, formatRooms, imageUrl } from '../utils/format.js'

/** /my-listings: üürileandja enda kuulutused. Ligipääs: üürileandja ja admin (vt App.jsx). */
export default function MyListingsPage() {
  const [listings, setListings] = useState(null)
  const [error, setError] = useState('')
  // "Praegune aeg" võetakse üks kord, et renderdamine oleks puhas (sama tulemus iga kord).
  const [now] = useState(() => Date.now())

  useEffect(() => {
    let cancelled = false
    getMyListings()
      .then((data) => !cancelled && setListings(data))
      .catch((err) => !cancelled && setError(err.message))
    return () => {
      cancelled = true
    }
  }, [])

  return (
    <div className="my-listings">
      <div className="section-header">
        <div>
          <h1>Minu kuulutused</h1>
          {listings && <p className="muted">{formatListingCount(listings.length)}</p>}
        </div>
        <Link to="/my-listings/new" className="button">
          Lisa kuulutus
        </Link>
      </div>

      {error && (
        <p className="alert" role="alert">
          {error}
        </p>
      )}

      {!listings && !error && <p className="muted">Laen kuulutusi…</p>}

      {listings?.length === 0 && (
        <div className="card empty-state">
          <p>Sul ei ole veel ühtegi kuulutust.</p>
          <Link to="/my-listings/new" className="button">
            Lisa esimene kuulutus
          </Link>
        </div>
      )}

      {listings?.length > 0 && (
        <ul className="my-listings__list">
          {listings.map((listing) => (
            <MyListingRow key={listing.id} listing={listing} expired={new Date(listing.expiresAt).getTime() < now} />
          ))}
        </ul>
      )}
    </div>
  )
}

function MyListingRow({ listing, expired }) {
  const image = imageUrl(listing.primaryImageUrl)

  return (
    <li className="card my-listing">
      {image ? (
        <img className="my-listing__image" src={image} alt="" loading="lazy" />
      ) : (
        <div className="my-listing__image listing-card__image--empty">Pilt puudub</div>
      )}

      <div className="my-listing__body">
        <h2 className="my-listing__title">
          <Link to={`/listings/${listing.id}`}>{listing.title}</Link>
        </h2>
        <p className="muted">
          {listing.address}, {listing.districtName}
        </p>
        <p>
          <strong>{formatPrice(listing.price)}</strong> / kuu · {formatRooms(listing.rooms)} · {formatArea(listing.areaM2)}
        </p>
        <p className="my-listing__meta">
          {expired ? (
            <span className="badge badge--muted">Aegunud {formatDate(listing.expiresAt)}</span>
          ) : (
            <span className="badge badge--success">Aktiivne kuni {formatDate(listing.expiresAt)}</span>
          )}
          <span>{listing.imageCount} pilti</span>
          <span>
            {listing.inquiryCount} {listing.inquiryCount === 1 ? 'päring' : 'päringut'}
            {listing.unreadInquiryCount > 0 && (
              <>
                {' '}
                <span className="badge">{listing.unreadInquiryCount} uut</span>
              </>
            )}
          </span>
        </p>
      </div>

      <div className="my-listing__actions">
        <Link to={`/listings/${listing.id}`} className="button button--secondary">
          Vaata
        </Link>
        <Link to={`/my-listings/${listing.id}/edit`} className="button">
          Muuda
        </Link>
      </div>
    </li>
  )
}

import { useEffect, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router'
import { getListing } from '../api/listings.js'
import { Roles } from '../auth/roles.js'
import { useAuth } from '../auth/useAuth.js'
import FavoriteButton from '../components/FavoriteButton.jsx'
import ImageGallery from '../components/ImageGallery.jsx'
import InquiryForm from '../components/InquiryForm.jsx'
import LocationMap from '../map/LocationMap.jsx'
import { formatArea, formatDate, formatPrice, formatPricePerM2, formatRooms } from '../utils/format.js'

export default function ListingDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const location = useLocation()
  const { user } = useAuth()

  // Vastus koos ID-ga, millele see kuulub (sama muster mis ListingsPage'il).
  const [response, setResponse] = useState({ id: null, listing: null, error: null })
  const loading = response.id !== id
  const { listing, error } = response

  useEffect(() => {
    let cancelled = false
    getListing(id)
      .then((data) => {
        if (!cancelled) setResponse({ id, listing: data, error: null })
      })
      .catch((err) => {
        if (!cancelled) setResponse({ id, listing: null, error: err })
      })
    return () => {
      cancelled = true
    }
  }, [id])

  // Kui kasutaja tuli nimekirjast, viib "Tagasi" samale lehele samade filtritega.
  function handleBack() {
    if (location.key !== 'default') navigate(-1)
    else navigate('/listings')
  }

  if (loading) return <p className="muted">Laen kuulutust…</p>

  if (error) {
    const notFound = error.status === 404
    return (
      <section className="hero">
        <h1>{notFound ? 'Kuulutust ei leitud' : 'Kuulutuse laadimine ebaõnnestus'}</h1>
        <p>{notFound ? 'Kuulutus on eemaldatud või aegunud.' : error.message}</p>
        <Link to="/listings" className="button">
          Kõik kuulutused
        </Link>
      </section>
    )
  }

  // Muutmise nupp omanikule ja adminile (API kontrollib õigust uuesti).
  const canEdit = user && (user.id === listing.landlord.id || user.role === Roles.Admin)

  const mapUrl =
    `https://www.openstreetmap.org/?mlat=${listing.latitude}&mlon=${listing.longitude}` +
    `#map=17/${listing.latitude}/${listing.longitude}`

  return (
    <article className="detail">
      <button type="button" className="link-button" onClick={handleBack}>
        ← Tagasi
      </button>

      <div className="detail__top">
        <ImageGallery key={listing.id} images={listing.images} title={listing.title} />

        <div className="card detail__info">
          <h1>{listing.title}</h1>
          <p className="muted">
            {listing.address}, {listing.districtName}
          </p>
          {canEdit && (
            <Link to={`/my-listings/${listing.id}/edit`} className="button button--secondary">
              Muuda kuulutust
            </Link>
          )}
          <p className="detail__price">
            {formatPrice(listing.price)} <span className="muted">/ kuu</span>
          </p>
          <FavoriteButton listingId={listing.id} variant="full" />

          <dl className="detail__facts">
            <div>
              <dt>Tubade arv</dt>
              <dd>{formatRooms(listing.rooms)}</dd>
            </div>
            <div>
              <dt>Pindala</dt>
              <dd>{formatArea(listing.areaM2)}</dd>
            </div>
            <div>
              <dt>Hind ruutmeetri kohta</dt>
              <dd>{formatPricePerM2(listing.pricePerM2)}</dd>
            </div>
            <div>
              <dt>Lisatud</dt>
              <dd>{formatDate(listing.createdAt)}</dd>
            </div>
            <div>
              <dt>Kehtib kuni</dt>
              <dd>{formatDate(listing.expiresAt)}</dd>
            </div>
          </dl>

          <div className="detail__landlord">
            <h2>Üürileandja</h2>
            <p>{listing.landlord.fullName}</p>
            {listing.landlord.phone && (
              <p>
                <a href={`tel:${listing.landlord.phone.replace(/\s/g, '')}`}>{listing.landlord.phone}</a>
              </p>
            )}
          </div>
        </div>
      </div>

      <section className="card detail__section">
        <h2>Kirjeldus</h2>
        <p className="detail__description">{listing.description}</p>
      </section>

      <InquiryForm listing={listing} />

      <section className="card detail__section">
        <h2>Asukoht</h2>
        <LocationMap latitude={listing.latitude} longitude={listing.longitude} />
        <p className="detail__map-link">
          {listing.address}, {listing.districtName}.{' '}
          <a href={mapUrl} target="_blank" rel="noreferrer">
            Ava suurem kaart (OpenStreetMap)
          </a>
        </p>
      </section>
    </article>
  )
}

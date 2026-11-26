import { useEffect, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router'
import { getDistricts } from '../api/districts.js'
import { createListing, deleteListing, getListing, updateListing, uploadListingImages } from '../api/listings.js'
import { Roles } from '../auth/roles.js'
import { useAuth } from '../auth/useAuth.js'
import ImageManager from '../components/ImageManager.jsx'
import ListingForm from '../components/ListingForm.jsx'

/** /my-listings/new ja /my-listings/:id/edit. Ligipääs: üürileandja ja admin (vt App.jsx). */
export default function ListingFormPage() {
  const { id } = useParams()
  const [districts, setDistricts] = useState([])

  useEffect(() => {
    getDistricts()
      .then(setDistricts)
      .catch(() => setDistricts([]))
  }, [])

  return id ? <EditListing id={id} districts={districts} /> : <CreateListing districts={districts} />
}

function CreateListing({ districts }) {
  const navigate = useNavigate()

  async function handleSubmit(request, files) {
    const listing = await createListing(request)
    if (files.length > 0) {
      try {
        await uploadListingImages(listing.id, files)
      } catch (err) {
        // Kuulutus on juba olemas: viime muutmise lehele, kus pilte saab uuesti lisada.
        navigate(`/my-listings/${listing.id}/edit`, {
          replace: true,
          state: { notice: `Kuulutus loodi, kuid pilte ei õnnestunud lisada: ${err.message}` },
        })
        return
      }
    }
    navigate(`/listings/${listing.id}`, { replace: true })
  }

  return (
    <div className="form-page">
      <h1>Lisa kuulutus</h1>
      <ListingForm districts={districts} submitLabel="Lisa kuulutus" onSubmit={handleSubmit} withImages />
    </div>
  )
}

function EditListing({ id, districts }) {
  const { user } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  // Sama laadimise muster mis ListingDetailPage'il: vastus koos ID-ga.
  const [response, setResponse] = useState({ id: null, listing: null, error: null })
  const [images, setImages] = useState(null) // null = kasuta laaditud kuulutuse pilte
  const [deleting, setDeleting] = useState(false)
  const [deleteError, setDeleteError] = useState('')
  const loading = response.id !== id
  const { listing, error } = response

  useEffect(() => {
    let cancelled = false
    getListing(id)
      .then((data) => !cancelled && setResponse({ id, listing: data, error: null }))
      .catch((err) => !cancelled && setResponse({ id, listing: null, error: err }))
    return () => {
      cancelled = true
    }
  }, [id])

  if (loading) return <p className="muted">Laen kuulutust…</p>
  if (error) {
    return (
      <section className="hero">
        <h1>{error.status === 404 ? 'Kuulutust ei leitud' : 'Kuulutuse laadimine ebaõnnestus'}</h1>
        <p>{error.message}</p>
      </section>
    )
  }

  // Mugavuskontroll; päris kontroll on API-s (403, kui kuulutus on teise üürileandja oma).
  if (listing.landlord.id !== user.id && user.role !== Roles.Admin) {
    return (
      <section className="hero">
        <h1>Ligipääs puudub</h1>
        <p>Saad muuta ainult enda kuulutusi.</p>
      </section>
    )
  }

  async function handleSubmit(request) {
    await updateListing(id, request)
    navigate(`/listings/${id}`, { replace: true })
  }

  async function handleDelete() {
    if (!window.confirm('Kas kustutada kuulutus? Seda ei saa tagasi võtta.')) return
    setDeleting(true)
    setDeleteError('')
    try {
      await deleteListing(id)
      navigate('/listings', { replace: true })
    } catch (err) {
      setDeleteError(err.message)
      setDeleting(false)
    }
  }

  return (
    <div className="form-page">
      <Link to={`/listings/${id}`}>← Tagasi kuulutuse juurde</Link>
      <h1>Muuda kuulutust</h1>

      {location.state?.notice && (
        <p className="alert" role="alert">
          {location.state.notice}
        </p>
      )}

      <ListingForm key={listing.id} listing={listing} districts={districts} submitLabel="Salvesta muudatused" onSubmit={handleSubmit} />

      <ImageManager listingId={listing.id} images={images ?? listing.images} onImagesChange={setImages} />

      <section className="card form-section danger-zone">
        <h2>Kustuta kuulutus</h2>
        <p className="muted">Kuulutus kaob otsingust ja seda ei saa enam muuta.</p>
        {deleteError && (
          <p className="alert" role="alert">
            {deleteError}
          </p>
        )}
        <button type="button" className="button button--danger" disabled={deleting} onClick={handleDelete}>
          {deleting ? 'Kustutan…' : 'Kustuta kuulutus'}
        </button>
      </section>
    </div>
  )
}

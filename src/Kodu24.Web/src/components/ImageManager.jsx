import { useRef, useState } from 'react'
import { deleteListingImage, getListing, setPrimaryListingImage, uploadListingImages } from '../api/listings.js'
import { imageUrl } from '../utils/format.js'
import { IMAGE_TYPES, MAX_IMAGES, validateImageFiles } from '../utils/validation.js'

/**
 * Olemasoleva kuulutuse piltide haldus: lisamine, kustutamine ja põhipildi valik.
 * Iga tegevus saadetakse kohe API-le; pärast seda loetakse pildid API-st uuesti,
 * et ekraanil oleks täpselt see, mis serveris (nt kes sai uueks põhipildiks).
 */
export default function ImageManager({ listingId, images, onImagesChange }) {
  const inputRef = useRef(null)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  async function run(action) {
    setBusy(true)
    setError('')
    try {
      await action()
      const listing = await getListing(listingId)
      onImagesChange(listing.images)
    } catch (err) {
      setError(Object.values(err.fieldErrors ?? {})[0] ?? err.message)
    } finally {
      setBusy(false)
    }
  }

  function handleFiles(event) {
    const files = [...event.target.files]
    event.target.value = '' // sama faili saab uuesti valida
    if (files.length === 0) return

    const message = validateImageFiles(files, images.length)
    if (message) {
      setError(message)
      return
    }
    run(() => uploadListingImages(listingId, files))
  }

  function handleDelete(image) {
    if (!window.confirm('Kas kustutada see pilt?')) return
    run(() => deleteListingImage(listingId, image.id))
  }

  return (
    <section className="card form-section" aria-labelledby="images-title">
      <h2 id="images-title">Pildid</h2>
      <p className="muted">
        Kuni {MAX_IMAGES} pilti (JPEG, PNG või WebP, kuni 5 MB). Põhipilti näidatakse kuulutuste nimekirjas.
      </p>

      {error && (
        <p className="alert" role="alert">
          {error}
        </p>
      )}

      {images.length > 0 && (
        <ul className="image-manager">
          {images.map((image) => (
            <li key={image.id} className="image-manager__item">
              <img src={imageUrl(image.url)} alt="" />
              <div className="image-manager__actions">
                {image.isPrimary ? (
                  <span className="badge">Põhipilt</span>
                ) : (
                  <button
                    type="button"
                    className="link-button"
                    disabled={busy}
                    onClick={() => run(() => setPrimaryListingImage(listingId, image.id))}
                  >
                    Tee põhipildiks
                  </button>
                )}
                <button
                  type="button"
                  className="link-button link-button--danger"
                  disabled={busy}
                  onClick={() => handleDelete(image)}
                >
                  Kustuta
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}

      <input
        ref={inputRef}
        type="file"
        accept={IMAGE_TYPES.join(',')}
        multiple
        hidden
        onChange={handleFiles}
      />
      <button
        type="button"
        className="button button--secondary"
        disabled={busy || images.length >= MAX_IMAGES}
        onClick={() => inputRef.current.click()}
      >
        {busy ? 'Palun oota…' : 'Lisa pilte'}
      </button>
    </section>
  )
}

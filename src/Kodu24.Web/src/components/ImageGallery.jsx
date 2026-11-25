import { useState } from 'react'
import { imageUrl } from '../utils/format.js'

/**
 * Kuulutuse pildigalerii: suur pilt ja pisipildid, millel klõpsates suur pilt vahetub.
 * images = [{ id, url, isPrimary, sortOrder }] (API annab need juba sortOrder järjekorras).
 */
export default function ImageGallery({ images, title }) {
  const [selected, setSelected] = useState(() => Math.max(0, images.findIndex((i) => i.isPrimary)))

  if (images.length === 0) {
    return <div className="gallery__main gallery__main--empty">Pilte ei ole lisatud</div>
  }

  const current = images[selected] ?? images[0]

  function show(index) {
    setSelected((index + images.length) % images.length) // ringiratast: viimase järel esimene
  }

  return (
    <div className="gallery">
      <div className="gallery__frame">
        <img className="gallery__main" src={imageUrl(current.url)} alt={`${title} – pilt ${selected + 1}`} />
        {images.length > 1 && (
          <>
            <button
              type="button"
              className="gallery__arrow gallery__arrow--prev"
              onClick={() => show(selected - 1)}
              aria-label="Eelmine pilt"
            >
              ‹
            </button>
            <button
              type="button"
              className="gallery__arrow gallery__arrow--next"
              onClick={() => show(selected + 1)}
              aria-label="Järgmine pilt"
            >
              ›
            </button>
            <span className="gallery__counter">
              {selected + 1} / {images.length}
            </span>
          </>
        )}
      </div>

      {images.length > 1 && (
        <div className="gallery__thumbs">
          {images.map((image, index) => (
            <button
              key={image.id}
              type="button"
              className={index === selected ? 'gallery__thumb gallery__thumb--active' : 'gallery__thumb'}
              onClick={() => setSelected(index)}
              aria-label={`Näita pilti ${index + 1}`}
              aria-current={index === selected ? 'true' : undefined}
            >
              <img src={imageUrl(image.url)} alt="" loading="lazy" />
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

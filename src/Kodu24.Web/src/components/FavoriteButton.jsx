import { useState } from 'react'
import { useLocation, useNavigate } from 'react-router'
import { useAuth } from '../auth/useAuth.js'
import { useFavorites } from '../favorites/useFavorites.js'

/**
 * Süda kuulutuse lemmikuks lisamiseks. Üürija lisab/eemaldab, külaline suunatakse
 * sisselogimisele. Teistele rollidele (üürileandja, admin) nuppu ei näidata, sest
 * lemmikud on ainult üürijal.
 * variant="icon" = väike ümmargune nupp kaardi nurgas, variant="full" = nupp tekstiga.
 */
export default function FavoriteButton({ listingId, variant = 'icon' }) {
  const { user } = useAuth()
  const { enabled, isFavorite, toggle } = useFavorites()
  const navigate = useNavigate()
  const location = useLocation()
  const [error, setError] = useState('')

  if (user && !enabled) return null

  const active = enabled && isFavorite(listingId)
  const label = active ? 'Eemalda lemmikutest' : 'Lisa lemmikuks'

  function handleClick(event) {
    // Kaardil asub nupp pildi peal: klõps ei tohi jõuda kaardi lingini.
    event.preventDefault()
    event.stopPropagation()
    if (!user) {
      navigate('/login', { state: { from: location } })
      return
    }
    setError('')
    toggle(listingId).catch((err) => setError(err.message))
  }

  const className = [
    'favorite-button',
    `favorite-button--${variant}`,
    active && 'favorite-button--active',
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <span className={`favorite-button__wrap favorite-button__wrap--${variant}`}>
      <button
        type="button"
        className={className}
        onClick={handleClick}
        aria-pressed={active}
        aria-label={variant === 'icon' ? label : undefined}
        title={label}
      >
        <span aria-hidden="true">{active ? '♥' : '♡'}</span>
        {variant === 'full' && ` ${active ? 'Lemmikutes' : 'Lisa lemmikuks'}`}
      </button>
      {error && (
        <span className="favorite-button__error" role="alert">
          {error}
        </span>
      )}
    </span>
  )
}

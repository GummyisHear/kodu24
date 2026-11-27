import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { getInquiries, markInquiryRead } from '../api/inquiries.js'
import { Roles } from '../auth/roles.js'
import { useAuth } from '../auth/useAuth.js'

const formatDateTime = (iso) => new Date(iso).toLocaleString('et-EE', { dateStyle: 'short', timeStyle: 'short' })

/**
 * /inquiries: üürileandja kuulutustele saabunud päringud (admin näeb kõiki).
 * Ligipääs: üürileandja ja admin (vt App.jsx).
 */
export default function InquiriesPage() {
  const { user } = useAuth()
  const [inquiries, setInquiries] = useState(null)
  const [error, setError] = useState('')
  const [onlyUnread, setOnlyUnread] = useState(false)

  useEffect(() => {
    let cancelled = false
    getInquiries()
      .then((data) => !cancelled && setInquiries(data))
      .catch((err) => !cancelled && setError(err.message))
    return () => {
      cancelled = true
    }
  }, [])

  async function handleMarkRead(inquiry) {
    setError('')
    try {
      await markInquiryRead(inquiry.id)
      setInquiries((current) => current.map((i) => (i.id === inquiry.id ? { ...i, isRead: true } : i)))
    } catch (err) {
      setError(err.message)
    }
  }

  const unreadCount = inquiries?.filter((i) => !i.isRead).length ?? 0
  const visible = onlyUnread ? inquiries?.filter((i) => !i.isRead) : inquiries

  return (
    <div className="inquiries">
      <div className="section-header">
        <div>
          <h1>Saabunud päringud</h1>
          {user.role === Roles.Admin && <p className="muted">Admin näeb kõigi kuulutuste päringuid.</p>}
        </div>
      </div>

      {error && (
        <p className="alert" role="alert">
          {error}
        </p>
      )}

      {!inquiries && !error && <p className="muted">Laen päringuid…</p>}

      {inquiries && (
        <div className="tabs" role="group" aria-label="Näita">
          <button
            type="button"
            className={onlyUnread ? 'tabs__tab' : 'tabs__tab tabs__tab--active'}
            aria-pressed={!onlyUnread}
            onClick={() => setOnlyUnread(false)}
          >
            Kõik ({inquiries.length})
          </button>
          <button
            type="button"
            className={onlyUnread ? 'tabs__tab tabs__tab--active' : 'tabs__tab'}
            aria-pressed={onlyUnread}
            onClick={() => setOnlyUnread(true)}
          >
            Lugemata ({unreadCount})
          </button>
        </div>
      )}

      {visible?.length === 0 && (
        <div className="card empty-state">
          <p>{onlyUnread ? 'Kõik päringud on loetud.' : 'Sinu kuulutustele ei ole veel päringuid saabunud.'}</p>
        </div>
      )}

      {visible?.length > 0 && (
        <ul className="inquiries__list">
          {visible.map((inquiry) => (
            <InquiryItem key={inquiry.id} inquiry={inquiry} onMarkRead={handleMarkRead} />
          ))}
        </ul>
      )}
    </div>
  )
}

function InquiryItem({ inquiry, onMarkRead }) {
  const replySubject = encodeURIComponent(`Kodu24: ${inquiry.listingTitle}`)
  const className = ['card', 'inquiry', !inquiry.isRead && 'inquiry--unread'].filter(Boolean).join(' ')

  return (
    <li className={className}>
      <div className="inquiry__head">
        <p className="inquiry__listing">
          Kuulutus: <Link to={`/listings/${inquiry.listingId}`}>{inquiry.listingTitle}</Link>
        </p>
        <p className="muted inquiry__date">
          {!inquiry.isRead && <span className="badge">Uus</span>} {formatDateTime(inquiry.createdAt)}
        </p>
      </div>
      <p className="inquiry__sender">
        Saatja: <strong>{inquiry.seekerName}</strong>,{' '}
        <a href={`mailto:${inquiry.seekerEmail}?subject=${replySubject}`}>{inquiry.seekerEmail}</a>
      </p>
      <p className="inquiry__message">{inquiry.message}</p>
      <div className="inquiry__actions">
        <a href={`mailto:${inquiry.seekerEmail}?subject=${replySubject}`} className="button button--secondary">
          Vasta e-postiga
        </a>
        {!inquiry.isRead && (
          <button type="button" className="link-button" onClick={() => onMarkRead(inquiry)}>
            Märgi loetuks
          </button>
        )}
      </div>
    </li>
  )
}

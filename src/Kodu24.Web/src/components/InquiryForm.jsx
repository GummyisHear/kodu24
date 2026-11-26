import { useState } from 'react'
import { Link, useLocation } from 'react-router'
import { sendInquiry } from '../api/inquiries.js'
import { Roles } from '../auth/roles.js'
import { useAuth } from '../auth/useAuth.js'
import { hasErrors, INQUIRY_MAX_LENGTH, validateInquiry } from '../utils/validation.js'
import FormField from './FormField.jsx'

/**
 * Päringu vorm kuulutuse lehel. Päringu saab saata ainult üürija (Seeker);
 * teistele näidatakse selgitust. API kontrollib rolli igal juhul uuesti.
 */
export default function InquiryForm({ listing }) {
  const { user, loading } = useAuth()
  const location = useLocation()

  if (loading) return null

  if (!user) {
    return (
      <InquiryBox>
        <p className="muted">
          Päringu saatmiseks <Link to="/login" state={{ from: location }}>logi sisse</Link> või{' '}
          <Link to="/register">loo üürija konto</Link>.
        </p>
      </InquiryBox>
    )
  }

  if (user.id === listing.landlord.id) {
    return (
      <InquiryBox>
        <p className="muted">See on sinu kuulutus. Saabunud päringud näed päringute lehel.</p>
      </InquiryBox>
    )
  }

  if (user.role !== Roles.Seeker) {
    return (
      <InquiryBox>
        <p className="muted">Päringuid saavad saata üürija kontoga kasutajad.</p>
      </InquiryBox>
    )
  }

  return <SeekerInquiryForm listingId={listing.id} />
}

function InquiryBox({ children }) {
  return (
    <section className="card detail__section" aria-labelledby="inquiry-title">
      <h2 id="inquiry-title">Saada päring üürileandjale</h2>
      {children}
    </section>
  )
}

function SeekerInquiryForm({ listingId }) {
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [formError, setFormError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [sent, setSent] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setFormError('')

    const clientErrors = validateInquiry({ message })
    setError(clientErrors.message ?? '')
    if (hasErrors(clientErrors)) return

    setSubmitting(true)
    try {
      await sendInquiry(listingId, message.trim())
      setSent(true)
      setMessage('')
    } catch (err) {
      if (err.fieldErrors?.message) setError(err.fieldErrors.message)
      else setFormError(err.message)
    } finally {
      setSubmitting(false)
    }
  }

  if (sent) {
    return (
      <InquiryBox>
        <p className="success" role="status">
          Päring on saadetud! Üürileandja vastab sulle e-posti teel.
        </p>
        <button type="button" className="button button--secondary" onClick={() => setSent(false)}>
          Saada uus päring
        </button>
      </InquiryBox>
    )
  }

  return (
    <InquiryBox>
      <form className="form" onSubmit={handleSubmit} noValidate>
        {formError && (
          <p className="alert" role="alert">
            {formError}
          </p>
        )}
        <FormField
          label="Sõnum"
          name="message"
          multiline
          rows={5}
          maxLength={INQUIRY_MAX_LENGTH}
          placeholder="Tere! Kas korter on veel vaba ja millal saaks seda vaatama tulla?"
          hint={`${message.trim().length} / ${INQUIRY_MAX_LENGTH} tähemärki. Üürileandja näeb sinu nime ja e-posti.`}
          value={message}
          onChange={(e) => {
            setMessage(e.target.value)
            setError('')
          }}
          error={error}
        />
        <div>
          <button type="submit" className="button" disabled={submitting}>
            {submitting ? 'Saadan…' : 'Saada päring'}
          </button>
        </div>
      </form>
    </InquiryBox>
  )
}

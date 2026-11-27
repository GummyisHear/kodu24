import { useState } from 'react'
import { Link, useLocation } from 'react-router'
import { createSavedSearch } from '../api/savedSearches.js'
import { Roles } from '../auth/roles.js'
import { useAuth } from '../auth/useAuth.js'
import { describeSearch, fromListingParams, toSavedSearchRequest } from '../utils/savedSearch.js'
import { hasErrors, validateSavedSearch } from '../utils/validation.js'
import FormField from './FormField.jsx'

/**
 * "Salvesta otsing" kuulutuste nimekirja filtrite all. Salvestab aadressiribal olevad
 * (st rakendatud) filtrid; taustateenus teavitab siis uutest sobivatest kuulutustest.
 * Otsingusõna (q) salvestatud otsingul ei ole, sellest antakse teada.
 */
export default function SaveSearchPanel({ searchParams, districts }) {
  const { user } = useAuth()
  const location = useLocation()
  const [open, setOpen] = useState(false)
  const [name, setName] = useState('')
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(null)

  if (!user) {
    return (
      <p className="card save-search muted">
        <Link to="/login" state={{ from: location }}>
          Logi sisse
        </Link>
        , et otsing salvestada ja saada teavitusi uutest kuulutustest.
      </p>
    )
  }
  if (user.role !== Roles.Seeker) return null

  const request = toSavedSearchRequest(fromListingParams(searchParams, name))
  const districtName = districts.find((d) => String(d.id) === searchParams.get('district'))?.name
  const summary = describeSearch(request, districtName)

  function handleOpen() {
    setName(summary.slice(0, 100)) // nimeks pakutakse filtrite kirjeldust
    setError('')
    setSaved(null)
    setOpen(true)
  }

  async function handleSubmit(event) {
    event.preventDefault()
    const errors = validateSavedSearch(fromListingParams(searchParams, name))
    if (hasErrors(errors)) {
      setError(Object.values(errors)[0])
      return
    }
    setSaving(true)
    setError('')
    try {
      setSaved(await createSavedSearch(request))
      setOpen(false)
    } catch (err) {
      setError(Object.values(err.fieldErrors ?? {})[0] ?? err.message)
    } finally {
      setSaving(false)
    }
  }

  if (!open) {
    return (
      <div className="card save-search">
        {saved && (
          <p className="success" role="status">
            Otsing „{saved.name}” on salvestatud. <Link to="/saved-searches">Minu otsingud</Link>
          </p>
        )}
        <button type="button" className="button button--secondary" onClick={handleOpen}>
          Salvesta otsing
        </button>
        <p className="field__hint">Saad teavituse, kui lisandub uus sobiv kuulutus.</p>
      </div>
    )
  }

  return (
    <form className="card form save-search" onSubmit={handleSubmit} noValidate>
      <h2 className="filters__title">Salvesta otsing</h2>
      <p className="muted">Filtrid: {summary}</p>
      {searchParams.get('q') && <p className="field__hint">Otsingusõna salvestatud otsingusse ei lähe.</p>}
      <FormField
        label="Otsingu nimi"
        name="name"
        id="save-search-name"
        maxLength={100}
        value={name}
        onChange={(e) => setName(e.target.value)}
        error={error}
      />
      <button type="submit" className="button" disabled={saving}>
        {saving ? 'Salvestan…' : 'Salvesta'}
      </button>
      <button type="button" className="button button--secondary" onClick={() => setOpen(false)} disabled={saving}>
        Loobu
      </button>
    </form>
  )
}

import { useState } from 'react'
import { toSavedSearchRequest } from '../utils/savedSearch.js'
import { hasErrors, validateSavedSearch } from '../utils/validation.js'
import FormField from './FormField.jsx'

/**
 * Salvestatud otsingu vorm (lisamine ja muutmine).
 * initialValues = vormi väärtused (vt toSavedSearchForm), onSubmit(request) saab API keha
 * ja peab tagastama promise'i; API väljade vead näidatakse vastava välja all.
 * idPrefix eristab väljade ID-sid, kui lehel on mitu vormi korraga.
 */
export default function SavedSearchForm({ initialValues, districts, submitLabel, onSubmit, onCancel, idPrefix = 'search' }) {
  const [values, setValues] = useState(initialValues)
  const [errors, setErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  function handleChange(event) {
    const { name, value } = event.target
    setValues((current) => ({ ...current, [name]: value }))
    setErrors((current) => ({ ...current, [name]: undefined }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    const clientErrors = validateSavedSearch(values)
    setErrors(clientErrors)
    setFormError('')
    if (hasErrors(clientErrors)) return

    setSaving(true)
    try {
      await onSubmit(toSavedSearchRequest(values))
    } catch (err) {
      setErrors(err.fieldErrors ?? {})
      if (!hasErrors(err.fieldErrors ?? {})) setFormError(err.message)
      setSaving(false)
    }
  }

  // Väljade nimed on samad mis API-l (name, minPrice, ...), et API vead jõuaksid õige välja alla.
  const field = (name) => ({
    id: `${idPrefix}-${name}`,
    name,
    value: values[name],
    onChange: handleChange,
    error: errors[name],
  })

  return (
    <form className="form saved-search-form" onSubmit={handleSubmit} noValidate>
      {formError && (
        <p className="alert" role="alert">
          {formError}
        </p>
      )}
      <FormField label="Otsingu nimi" maxLength={100} placeholder="nt Kesklinna 2-toaline" {...field('name')} />
      <div className="form-grid">
        <FormField label="Hind min (€)" type="number" min="0" inputMode="numeric" {...field('minPrice')} />
        <FormField label="Hind max (€)" type="number" min="0" inputMode="numeric" {...field('maxPrice')} />
      </div>
      <div className="form-grid">
        <FormField label="Tubade arv" type="number" min="1" max="20" inputMode="numeric" {...field('rooms')} />
        <div className="field">
          <label htmlFor={`${idPrefix}-districtId`}>Linnaosa</label>
          <select id={`${idPrefix}-districtId`} name="districtId" value={values.districtId} onChange={handleChange}>
            <option value="">Kõik linnaosad</option>
            {districts.map((d) => (
              <option key={d.id} value={d.id}>
                {d.name}
              </option>
            ))}
          </select>
          {errors.districtId && <span className="field__error">{errors.districtId}</span>}
        </div>
      </div>
      <div className="saved-search-form__actions">
        <button type="submit" className="button" disabled={saving}>
          {saving ? 'Salvestan…' : submitLabel}
        </button>
        {onCancel && (
          <button type="button" className="button button--secondary" onClick={onCancel} disabled={saving}>
            Loobu
          </button>
        )}
      </div>
    </form>
  )
}

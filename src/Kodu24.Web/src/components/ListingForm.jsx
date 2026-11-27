import { useState } from 'react'
import { DESCRIPTION_MAX_LENGTH, hasErrors, IMAGE_TYPES, validateImageFiles, validateListing } from '../utils/validation.js'
import LocationPicker from '../map/LocationPicker.jsx'
import FormField from './FormField.jsx'

const EMPTY = {
  title: '',
  address: '',
  districtId: '',
  price: '',
  rooms: '',
  areaM2: '',
  description: '',
  latitude: '',
  longitude: '',
}

/** Kuulutus API-st → vormi väärtused (vormis on kõik stringid). */
function toFormValues(listing) {
  if (!listing) return EMPTY
  return Object.fromEntries(Object.keys(EMPTY).map((key) => [key, String(listing[key] ?? '')]))
}

/** Vormi väärtused → ListingRequestDto (numbrid numbriteks). */
function toRequest(v) {
  return {
    title: v.title.trim(),
    address: v.address.trim(),
    districtId: Number(v.districtId),
    price: Number(v.price),
    rooms: Number(v.rooms),
    areaM2: Number(v.areaM2),
    description: v.description.trim(),
    latitude: Number(v.latitude),
    longitude: Number(v.longitude),
  }
}

/**
 * Kuulutuse lisamise ja muutmise vorm. onSubmit(request, files) saab valmis API keha;
 * pildifaile (files) saab valida ainult uue kuulutuse puhul (muutmisel on eraldi piltide haldus).
 * Kui onSubmit viskab ApiError'i, näidatakse API väljavead õigete väljade juures.
 */
export default function ListingForm({ listing, districts, submitLabel, onSubmit, withImages = false }) {
  const [values, setValues] = useState(() => toFormValues(listing))
  const [files, setFiles] = useState([])
  const [errors, setErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  function handleChange(event) {
    const { name, value } = event.target
    setValues((current) => ({ ...current, [name]: value }))
    setErrors((current) => ({ ...current, [name]: undefined }))
  }

  // Kaardilt valitud asukoht täidab koordinaatide väljad.
  function handlePick(latitude, longitude) {
    setValues((current) => ({ ...current, latitude: String(latitude), longitude: String(longitude) }))
    setErrors((current) => ({ ...current, latitude: undefined, longitude: undefined }))
  }

  function handleFiles(event) {
    const selected = [...event.target.files]
    const message = validateImageFiles(selected)
    setErrors((current) => ({ ...current, files: message || undefined }))
    setFiles(message ? [] : selected)
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setFormError('')

    const clientErrors = validateListing(values)
    if (errors.files) clientErrors.files = errors.files
    setErrors(clientErrors)
    if (hasErrors(clientErrors)) {
      setFormError('Palun paranda vormis märgitud vead.')
      return
    }

    setSubmitting(true)
    try {
      await onSubmit(toRequest(values), files)
    } catch (err) {
      setErrors(err.fieldErrors ?? {})
      setFormError(err.message)
      setSubmitting(false)
    }
    // Õnnestumisel liigub leht mujale, seega submitting jääb tõeseks (nupp ei vilgu).
  }

  return (
    <form className="card form form-section" onSubmit={handleSubmit} noValidate>
      {formError && (
        <p className="alert" role="alert">
          {formError}
        </p>
      )}

      <FormField label="Pealkiri" name="title" maxLength={150} value={values.title} onChange={handleChange} error={errors.title} />

      <div className="form-grid">
        <FormField label="Aadress" name="address" maxLength={200} placeholder="nt Kotzebue 12-4" value={values.address} onChange={handleChange} error={errors.address} />
        <div className={errors.districtId ? 'field field--invalid' : 'field'}>
          <label htmlFor="districtId">Linnaosa</label>
          <select id="districtId" name="districtId" value={values.districtId} onChange={handleChange} aria-invalid={Boolean(errors.districtId)}>
            <option value="">Vali linnaosa</option>
            {districts.map((d) => (
              <option key={d.id} value={d.id}>
                {d.name}
              </option>
            ))}
          </select>
          {errors.districtId && <span className="field__error">{errors.districtId}</span>}
        </div>
      </div>

      <div className="form-grid form-grid--3">
        <FormField label="Kuu üür (€)" name="price" type="number" min="1" step="1" value={values.price} onChange={handleChange} error={errors.price} />
        <FormField label="Tubade arv" name="rooms" type="number" min="1" max="20" step="1" value={values.rooms} onChange={handleChange} error={errors.rooms} />
        <FormField label="Pind (m²)" name="areaM2" type="number" min="5" step="0.1" value={values.areaM2} onChange={handleChange} error={errors.areaM2} />
      </div>

      <FormField
        label="Kirjeldus (valikuline)"
        name="description"
        multiline
        rows={6}
        maxLength={DESCRIPTION_MAX_LENGTH}
        hint={`${values.description.length} / ${DESCRIPTION_MAX_LENGTH} tähemärki`}
        value={values.description}
        onChange={handleChange}
        error={errors.description}
      />

      <div className="field">
        <span className="field__label">Asukoht kaardil</span>
        <LocationPicker latitude={values.latitude} longitude={values.longitude} onPick={handlePick} />
        <span className="field__hint">Klõpsa kaardil maja kohal või lohista nõela. Koordinaadid täituvad ise.</span>
      </div>
      <div className="form-grid">
        <FormField label="Laiuskraad" name="latitude" type="number" step="any" placeholder="59.4370" value={values.latitude} onChange={handleChange} error={errors.latitude} />
        <FormField label="Pikkuskraad" name="longitude" type="number" step="any" placeholder="24.7536" value={values.longitude} onChange={handleChange} error={errors.longitude} />
      </div>

      {withImages && (
        <div className={errors.files ? 'field field--invalid' : 'field'}>
          <label htmlFor="files">Pildid (valikuline)</label>
          <input id="files" type="file" accept={IMAGE_TYPES.join(',')} multiple onChange={handleFiles} />
          <span className="field__hint">
            {files.length > 0 ? `Valitud ${files.length} pilti. ` : ''}Kuni 10 pilti, JPEG/PNG/WebP, kuni 5 MB. Esimene on põhipilt.
          </span>
          {errors.files && <span className="field__error">{errors.files}</span>}
        </div>
      )}

      <div>
        <button type="submit" className="button" disabled={submitting}>
          {submitting ? 'Salvestan…' : submitLabel}
        </button>
      </div>
    </form>
  )
}

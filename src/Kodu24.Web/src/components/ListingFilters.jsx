import { useState } from 'react'
import { hasErrors, validateListingFilters } from '../utils/validation.js'
import FormField from './FormField.jsx'

const EMPTY_FILTERS = { q: '', minPrice: '', maxPrice: '', rooms: '', district: '' }

/** Filtrite nimed, mis on ühtlasi API ja aadressiriba parameetrid (?minPrice=400&rooms=2). */
const FILTER_KEYS = Object.keys(EMPTY_FILTERS)

/** Loeb filtrite väärtused URLSearchParams'ist; puuduvad väljad on tühjad. */
function readFilters(searchParams) {
  return Object.fromEntries(FILTER_KEYS.map((key) => [key, searchParams.get(key) ?? '']))
}

/**
 * Otsingufiltrite vorm. Algväärtused tulevad aadressiribalt (searchParams); sisestatud
 * väärtusi hoiab vorm enda olekus ja annab need "Otsi" vajutamisel vanemale (onSearch).
 */
export default function ListingFilters({ searchParams, districts, onSearch }) {
  const [values, setValues] = useState(() => readFilters(searchParams))
  const [errors, setErrors] = useState({})

  function handleChange(event) {
    const { name, value } = event.target
    setValues((current) => ({ ...current, [name]: value }))
    setErrors((current) => ({ ...current, [name]: undefined }))
  }

  function handleSubmit(event) {
    event.preventDefault()
    const clientErrors = validateListingFilters(values)
    setErrors(clientErrors)
    if (!hasErrors(clientErrors)) onSearch({ ...values, q: values.q.trim() })
  }

  function handleReset() {
    setValues(EMPTY_FILTERS)
    setErrors({})
    onSearch(EMPTY_FILTERS)
  }

  return (
    <form className="card form filters" onSubmit={handleSubmit} noValidate>
      <h2 className="filters__title">Filtrid</h2>
      <FormField
        label="Otsingusõna"
        name="q"
        type="search"
        placeholder="nt rõdu, Kalamaja"
        value={values.q}
        onChange={handleChange}
        error={errors.q}
      />
      <div className="filters__row">
        <FormField
          label="Hind min (€)"
          name="minPrice"
          type="number"
          min="0"
          inputMode="numeric"
          value={values.minPrice}
          onChange={handleChange}
          error={errors.minPrice}
        />
        <FormField
          label="Hind max (€)"
          name="maxPrice"
          type="number"
          min="0"
          inputMode="numeric"
          value={values.maxPrice}
          onChange={handleChange}
          error={errors.maxPrice}
        />
      </div>
      <FormField
        label="Tubade arv"
        name="rooms"
        type="number"
        min="1"
        max="20"
        inputMode="numeric"
        value={values.rooms}
        onChange={handleChange}
        error={errors.rooms}
      />
      <div className="field">
        <label htmlFor="district">Linnaosa</label>
        <select id="district" name="district" value={values.district} onChange={handleChange}>
          <option value="">Kõik linnaosad</option>
          {districts.map((d) => (
            <option key={d.id} value={d.id}>
              {d.name}
            </option>
          ))}
        </select>
      </div>
      <button type="submit" className="button">
        Otsi
      </button>
      <button type="button" className="button button--secondary" onClick={handleReset}>
        Tühjenda filtrid
      </button>
    </form>
  )
}

import { formatPrice } from './format.js'

// Abifunktsioonid salvestatud otsingu ja kuulutuste otsingu (/listings?...) vahel.
// Nimekirja aadressiribal on linnaosa nimega "district", API salvestatud otsingus "districtId".

/** Vormi väärtused (stringid) → API keha (arvud või null). */
export function toSavedSearchRequest({ name, minPrice, maxPrice, rooms, districtId }) {
  const numberOrNull = (value) => (value === '' || value === null || value === undefined ? null : Number(value))
  return {
    name: name.trim(),
    minPrice: numberOrNull(minPrice),
    maxPrice: numberOrNull(maxPrice),
    rooms: numberOrNull(rooms),
    districtId: numberOrNull(districtId),
  }
}

/** API vastus → vormi väärtused (null → ''). */
export function toSavedSearchForm(search) {
  const text = (value) => (value === null || value === undefined ? '' : String(value))
  return {
    name: search?.name ?? '',
    minPrice: text(search?.minPrice),
    maxPrice: text(search?.maxPrice),
    rooms: text(search?.rooms),
    districtId: text(search?.districtId),
  }
}

/** Kuulutuste nimekirja aadressiriba filtrid → salvestatud otsingu vormi väärtused. */
export function fromListingParams(searchParams, name = '') {
  return {
    name,
    minPrice: searchParams.get('minPrice') ?? '',
    maxPrice: searchParams.get('maxPrice') ?? '',
    rooms: searchParams.get('rooms') ?? '',
    districtId: searchParams.get('district') ?? '',
  }
}

/** Link kuulutuste nimekirja sama otsinguga: "/listings?minPrice=400&rooms=2". */
export function listingsUrl(search) {
  const params = new URLSearchParams()
  if (search.minPrice !== null) params.set('minPrice', search.minPrice)
  if (search.maxPrice !== null) params.set('maxPrice', search.maxPrice)
  if (search.rooms !== null) params.set('rooms', search.rooms)
  if (search.districtId !== null) params.set('district', search.districtId)
  const text = params.toString()
  return text ? `/listings?${text}` : '/listings'
}

/**
 * Filtrite lühikirjeldus: "Kesklinn · 2 tuba · 400–800 €".
 * search = API vastus või toSavedSearchRequest tulemus; districtName antakse eraldi, kui see puudub.
 */
export function describeSearch(search, districtName = search.districtName) {
  const parts = []
  if (districtName) parts.push(districtName)
  if (search.rooms !== null) parts.push(`${search.rooms} tuba`)
  const { minPrice: min, maxPrice: max } = search
  if (min !== null && max !== null) parts.push(`${formatPrice(min)}–${formatPrice(max)}`)
  else if (min !== null) parts.push(`alates ${formatPrice(min)}`)
  else if (max !== null) parts.push(`kuni ${formatPrice(max)}`)
  return parts.length > 0 ? parts.join(' · ') : 'Kõik kuulutused'
}

import { apiFetch } from './client.js'

/**
 * Üürija salvestatud otsingud: CRUD /api/saved-searches.
 * Keha: { name, minPrice, maxPrice, rooms, districtId } – filtrid võivad olla null (= ükskõik).
 */
export const getSavedSearches = () => apiFetch('/api/saved-searches')

export const createSavedSearch = (search) => apiFetch('/api/saved-searches', { method: 'POST', body: search })

/** Muutmine: kõik väljad saadetakse uuesti (puuduv filter = ükskõik). */
export const updateSavedSearch = (id, search) =>
  apiFetch(`/api/saved-searches/${id}`, { method: 'PUT', body: search })

/** Kustutamisel kaovad ka selle otsingu teavitused. */
export const deleteSavedSearch = (id) => apiFetch(`/api/saved-searches/${id}`, { method: 'DELETE' })

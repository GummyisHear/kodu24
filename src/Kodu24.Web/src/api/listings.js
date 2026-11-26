import { apiFetch, toQueryString } from './client.js'

/**
 * Kuulutuste otsing: GET /api/listings.
 * params: { q, minPrice, maxPrice, rooms, district, sort, page, pageSize } – kõik valikulised.
 * Tagastab { items, page, pageSize, totalCount, totalPages }.
 */
export const searchListings = (params = {}) => apiFetch('/api/listings' + toQueryString(params))

/** Ühe kuulutuse detailid: GET /api/listings/{id}. */
export const getListing = (id) => apiFetch(`/api/listings/${encodeURIComponent(id)}`)

/** Sisselogitud üürileandja enda kuulutused: GET /api/listings/mine. */
export const getMyListings = () => apiFetch('/api/listings/mine')

/** Uus kuulutus (üürileandja võetakse tokenist). Tagastab loodud kuulutuse. */
export const createListing = (listing) => apiFetch('/api/listings', { method: 'POST', body: listing })

/** Kuulutuse muutmine: kõik väljad saadetakse uuesti. */
export const updateListing = (id, listing) => apiFetch(`/api/listings/${id}`, { method: 'PUT', body: listing })

export const deleteListing = (id) => apiFetch(`/api/listings/${id}`, { method: 'DELETE' })

/** Piltide üleslaadimine (multipart/form-data, väli "files"). Tagastab kuulutuse kõik pildid. */
export function uploadListingImages(id, files) {
  const form = new FormData()
  for (const file of files) form.append('files', file)
  return apiFetch(`/api/listings/${id}/images`, { method: 'POST', body: form })
}

export const deleteListingImage = (id, imageId) =>
  apiFetch(`/api/listings/${id}/images/${imageId}`, { method: 'DELETE' })

export const setPrimaryListingImage = (id, imageId) =>
  apiFetch(`/api/listings/${id}/images/${imageId}/primary`, { method: 'PUT' })

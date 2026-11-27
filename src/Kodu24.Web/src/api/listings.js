import { apiFetch, toQueryString } from './client.js'

/**
 * Kuulutuste otsing: GET /api/listings.
 * params: { q, minPrice, maxPrice, rooms, district, sort, page, pageSize } – kõik valikulised.
 * Tagastab { items, page, pageSize, totalCount, totalPages }.
 */
export const searchListings = (params = {}) => apiFetch('/api/listings' + toQueryString(params))

/** API lubab leheküljel kuni 50 kuulutust (ListingQueryDto.PageSize). */
const MAP_PAGE_SIZE = 50
/** Kaardil näidatakse kuni 500 kuulutust (10 lehekülge), et päringuid ei tuleks lõputult. */
const MAP_MAX_PAGES = 10

/**
 * Kõik filtritele vastavad kuulutused kaardi jaoks: loeb lehekülgi järjest, kuni need saavad otsa.
 * Tagastab { items, totalCount }; totalCount > items.length tähendab, et kõiki ei näidata.
 */
export async function searchAllListings(params = {}) {
  const items = []
  let page = 1
  let totalPages = 1
  let totalCount = 0
  do {
    const result = await searchListings({ ...params, page, pageSize: MAP_PAGE_SIZE })
    items.push(...result.items)
    totalPages = result.totalPages
    totalCount = result.totalCount
    page += 1
  } while (page <= totalPages && page <= MAP_MAX_PAGES)
  return { items, totalCount }
}

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

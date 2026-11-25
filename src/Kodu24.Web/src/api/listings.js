import { apiFetch, toQueryString } from './client.js'

/**
 * Kuulutuste otsing: GET /api/listings.
 * params: { q, minPrice, maxPrice, rooms, district, sort, page, pageSize } – kõik valikulised.
 * Tagastab { items, page, pageSize, totalCount, totalPages }.
 */
export const searchListings = (params = {}) => apiFetch('/api/listings' + toQueryString(params))

/** Ühe kuulutuse detailid: GET /api/listings/{id}. */
export const getListing = (id) => apiFetch(`/api/listings/${encodeURIComponent(id)}`)

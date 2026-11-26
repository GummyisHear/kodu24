import { apiFetch } from './client.js'

/** Üürija saadab päringu: POST /api/listings/{id}/inquiries. */
export const sendInquiry = (listingId, message) =>
  apiFetch(`/api/listings/${listingId}/inquiries`, { method: 'POST', body: { message } })

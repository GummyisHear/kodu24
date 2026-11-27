import { apiFetch } from './client.js'

/** Üürija saadab päringu: POST /api/listings/{id}/inquiries. */
export const sendInquiry = (listingId, message) =>
  apiFetch(`/api/listings/${listingId}/inquiries`, { method: 'POST', body: { message } })

/** Üürileandja kuulutustele saabunud päringud (admin näeb kõiki), uuemad eespool: GET /api/inquiries. */
export const getInquiries = () => apiFetch('/api/inquiries')

/** Märgib päringu loetuks: PATCH /api/inquiries/{id}/read (204). */
export const markInquiryRead = (id) => apiFetch(`/api/inquiries/${id}/read`, { method: 'PATCH' })

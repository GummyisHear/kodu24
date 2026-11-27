import { apiFetch } from './client.js'

/** Üürija teavitused uutest kuulutustest (loob taustateenus), uuemad eespool. */
export const getNotifications = () => apiFetch('/api/notifications')

export const markNotificationRead = (id) => apiFetch(`/api/notifications/${id}/read`, { method: 'PATCH' })

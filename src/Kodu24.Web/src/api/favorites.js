import { apiFetch } from './client.js'

/** Üürija lemmikud koos kuulutustega, uuemad eespool: GET /api/favorites. */
export const getFavorites = () => apiFetch('/api/favorites')

/** Ainult lemmikute kuulutuste ID-d (südamete jaoks nimekirjas): GET /api/favorites/ids. */
export const getFavoriteIds = () => apiFetch('/api/favorites/ids')

/** Lisab lemmikuks. Kui juba on lemmik, vastab API 200 ja midagi ei muutu. */
export const addFavorite = (listingId) => apiFetch(`/api/favorites/${listingId}`, { method: 'POST' })

export const removeFavorite = (listingId) => apiFetch(`/api/favorites/${listingId}`, { method: 'DELETE' })

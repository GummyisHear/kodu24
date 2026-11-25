import { apiFetch } from './client.js'

/** Kõik linnaosad: [{ id, name }]. */
export const getDistricts = () => apiFetch('/api/districts')

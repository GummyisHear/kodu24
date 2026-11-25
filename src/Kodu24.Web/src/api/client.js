import { getToken } from '../auth/tokenStorage.js'

export const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5134'

/** Sündmus, mille AuthContext kuulab: token on aegunud või kehtetu (401). */
export const UNAUTHORIZED_EVENT = 'kodu24:unauthorized'

/**
 * API viga. API tagastab vead ProblemDetails kujul:
 * { title, status, detail, errors: { Email: ["..."] } }.
 */
export class ApiError extends Error {
  constructor(status, problem) {
    super(problem?.detail ?? problem?.title ?? 'Päring ebaõnnestus.')
    this.name = 'ApiError'
    this.status = status
    this.title = problem?.title
    /** Väljade vead kujul { email: 'Teade' }, võtmed väikese algustähega nagu vormi väljad. */
    this.fieldErrors = normalizeErrors(problem?.errors)
  }
}

/**
 * Kõik API päringud käivad läbi selle funktsiooni: lisab JWT päise,
 * saadab/loeb JSON-i ja muudab vead ApiError'iks.
 */
export async function apiFetch(path, { method = 'GET', body } = {}) {
  const headers = { Accept: 'application/json' }
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`
  if (body !== undefined) headers['Content-Type'] = 'application/json'

  let response
  try {
    response = await fetch(API_URL + path, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new ApiError(0, { title: 'Serveriga ei saa ühendust. Kontrolli, kas API töötab.' })
  }

  if (response.status === 204) return null

  const isJson = response.headers.get('Content-Type')?.includes('json')
  const data = isJson ? await response.json() : null

  if (!response.ok) {
    if (response.status === 401 && token) window.dispatchEvent(new Event(UNAUTHORIZED_EVENT))
    throw new ApiError(response.status, data)
  }
  return data
}

/** Teeb objektist päringustringi, jättes tühjad väärtused välja: { rooms: 2, q: '' } → "?rooms=2". */
export function toQueryString(params) {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') search.set(key, value)
  }
  const text = search.toString()
  return text ? `?${text}` : ''
}

// "Email" → "email", "$.password" → "password"; iga välja kohta esimene teade.
function normalizeErrors(errors) {
  if (!errors) return {}
  const result = {}
  for (const [key, messages] of Object.entries(errors)) {
    const name = key.replace(/^\$\./, '')
    const field = name.charAt(0).toLowerCase() + name.slice(1)
    result[field] = Array.isArray(messages) ? messages[0] : String(messages)
  }
  return result
}

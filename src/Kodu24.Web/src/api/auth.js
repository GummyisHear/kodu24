import { apiFetch } from './client.js'

/** POST /api/auth/login → { token, expiresAt, user } */
export function login(email, password) {
  return apiFetch('/api/auth/login', { method: 'POST', body: { email, password } })
}

/** POST /api/auth/register → { token, expiresAt, user } */
export function register({ email, password, fullName, phone, role }) {
  return apiFetch('/api/auth/register', {
    method: 'POST',
    body: { email, password, fullName, phone: phone || null, role },
  })
}

/** GET /api/auth/me → sisselogitud kasutaja */
export function getMe() {
  return apiFetch('/api/auth/me')
}

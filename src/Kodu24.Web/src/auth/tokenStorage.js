// JWT hoitakse brauseri localStorage'is, et sisselogimine püsiks lehe värskendamisel.
const STORAGE_KEY = 'kodu24.auth'

/** Tagastab salvestatud tokeni või nulli, kui seda pole või see on aegunud. */
export function getToken() {
  const saved = read()
  if (!saved) return null
  if (new Date(saved.expiresAt) <= new Date()) {
    clearToken()
    return null
  }
  return saved.token
}

export function saveToken(token, expiresAt) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({ token, expiresAt }))
}

export function clearToken() {
  localStorage.removeItem(STORAGE_KEY)
}

function read() {
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY))
  } catch {
    return null
  }
}

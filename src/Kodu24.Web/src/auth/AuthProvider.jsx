import { useCallback, useEffect, useMemo, useState } from 'react'
import * as authApi from '../api/auth.js'
import { UNAUTHORIZED_EVENT } from '../api/client.js'
import { AuthContext } from './AuthContext.js'
import { clearToken, getToken, saveToken } from './tokenStorage.js'

/**
 * Hoiab sisselogitud kasutajat. Lehe laadimisel taastatakse sessioon salvestatud
 * tokeni abil (GET /api/auth/me); kehtetu tokeni korral logitakse välja.
 */
export default function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(() => getToken() !== null)

  const logout = useCallback(() => {
    clearToken()
    setUser(null)
  }, [])

  useEffect(() => {
    if (!getToken()) return
    let cancelled = false
    authApi
      .getMe()
      .then((me) => !cancelled && setUser(me))
      .catch(() => !cancelled && logout())
      .finally(() => !cancelled && setLoading(false))
    return () => {
      cancelled = true
    }
  }, [logout])

  // API klient annab teada, kui server vastas 401 (token aegus) → logime välja.
  useEffect(() => {
    window.addEventListener(UNAUTHORIZED_EVENT, logout)
    return () => window.removeEventListener(UNAUTHORIZED_EVENT, logout)
  }, [logout])

  const startSession = useCallback(({ token, expiresAt, user }) => {
    saveToken(token, expiresAt)
    setUser(user)
    return user
  }, [])

  const login = useCallback(
    async (email, password) => startSession(await authApi.login(email, password)),
    [startSession],
  )

  const register = useCallback(
    async (data) => startSession(await authApi.register(data)),
    [startSession],
  )

  const value = useMemo(
    () => ({ user, loading, login, register, logout }),
    [user, loading, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

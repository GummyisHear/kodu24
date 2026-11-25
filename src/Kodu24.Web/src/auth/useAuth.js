import { useContext } from 'react'
import { AuthContext } from './AuthContext.js'

/** Sisselogimise olek ja tegevused: const { user, login, logout } = useAuth() */
export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth peab olema AuthProvider sees.')
  return context
}

import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { API_URL } from '../api/client.js'
import { Roles } from '../auth/roles.js'
import { getToken } from '../auth/tokenStorage.js'
import { useAuth } from '../auth/useAuth.js'
import { InquirySocketContext } from './InquirySocketContext.js'

const MAX_RETRY_DELAY_MS = 30_000

/** http://localhost:5134 → ws://localhost:5134/ws/inquiries?access_token=... */
function socketUrl(token) {
  return `${API_URL.replace(/^http/, 'ws')}/ws/inquiries?access_token=${encodeURIComponent(token)}`
}

/**
 * Üks WebSocket-ühendus API-ga kogu rakenduse jaoks (üürileandja ja admin).
 * Kui üürija saadab päringu, saadab API sõnumi { type: "InquiryReceived", data: InquiryDto }.
 * Brauseri WebSocket ei saa saata Authorization päist, seepärast läheb JWT aadressi
 * parameetris access_token (API loeb seda ainult /ws aadressidel).
 *
 * status: 'off' (pole üürileandja), 'connecting', 'open' või 'reconnecting'.
 * Ühenduse katkemisel proovitakse uuesti: 1 s, 2 s, 4 s … kuni 30 s.
 */
export default function InquirySocketProvider({ children }) {
  const { user } = useAuth()
  const userId = user && (user.role === Roles.Landlord || user.role === Roles.Admin) ? user.id : null

  // Olek koos kasutajaga, kellele see kuulub (teise kasutaja sisselogimisel ei näidata vana olekut).
  const [connection, setConnection] = useState({ userId: null, status: 'connecting' })
  const [unseen, setUnseen] = useState({ userId: null, count: 0 })
  const listeners = useRef(new Set())

  useEffect(() => {
    if (userId === null) return
    let stopped = false
    let socket = null
    let retryTimer = null
    let attempt = 0

    function connect() {
      const token = getToken()
      if (!token) return // token aegus; AuthProvider logib välja
      socket = new WebSocket(socketUrl(token))

      socket.onopen = () => {
        attempt = 0
        setConnection({ userId, status: 'open' })
      }

      socket.onmessage = (event) => {
        let message
        try {
          message = JSON.parse(event.data)
        } catch {
          return
        }
        if (message.type !== 'InquiryReceived') return
        setUnseen((current) => ({ userId, count: (current.userId === userId ? current.count : 0) + 1 }))
        for (const listener of listeners.current) listener(message.data)
      }

      socket.onclose = () => {
        if (stopped) return
        setConnection({ userId, status: 'reconnecting' })
        const delay = Math.min(MAX_RETRY_DELAY_MS, 1000 * 2 ** attempt)
        attempt += 1
        retryTimer = setTimeout(connect, delay)
      }
    }

    connect()
    return () => {
      // Väljalogimisel või lehelt lahkudes: ühendus kinni ja uuesti ei proovita.
      stopped = true
      clearTimeout(retryTimer)
      socket?.close()
    }
  }, [userId])

  /** Kuulaja saab iga uue päringu (InquiryDto). Tagastab funktsiooni kuulamise lõpetamiseks. */
  const subscribe = useCallback((listener) => {
    listeners.current.add(listener)
    return () => listeners.current.delete(listener)
  }, [])

  /** Päringute lehel nullitakse "uute" loendur (päises olev märk). */
  const clearNew = useCallback(() => setUnseen({ userId, count: 0 }), [userId])

  let status = 'off'
  if (userId !== null) status = connection.userId === userId ? connection.status : 'connecting'
  const newCount = unseen.userId === userId ? unseen.count : 0

  const value = useMemo(
    () => ({ status, newCount, clearNew, subscribe }),
    [status, newCount, clearNew, subscribe],
  )

  return <InquirySocketContext.Provider value={value}>{children}</InquirySocketContext.Provider>
}

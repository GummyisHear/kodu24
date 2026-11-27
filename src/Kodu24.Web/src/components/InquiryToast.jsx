import { useEffect, useState } from 'react'
import { Link, useLocation } from 'react-router'
import { useInquirySocket } from '../realtime/useInquirySocket.js'

const VISIBLE_MS = 8000

/**
 * Väike teade ekraani nurgas, kui üürileandjale saabub päring (ükskõik millisel lehel).
 * Päringute lehel teadet ei näidata, sest seal ilmub päring kohe nimekirja.
 */
export default function InquiryToast() {
  const { subscribe } = useInquirySocket()
  const location = useLocation()
  const [inquiry, setInquiry] = useState(null)

  useEffect(() => subscribe(setInquiry), [subscribe])

  // Peida teade mõne sekundi pärast; uus päring alustab aega otsast.
  useEffect(() => {
    if (!inquiry) return
    const timer = setTimeout(() => setInquiry(null), VISIBLE_MS)
    return () => clearTimeout(timer)
  }, [inquiry])

  if (!inquiry || location.pathname === '/inquiries') return null

  return (
    <div className="toast" role="status">
      <p className="toast__title">Uus päring!</p>
      <p>
        {inquiry.seekerName} kuulutusele „{inquiry.listingTitle}”
      </p>
      <div className="toast__actions">
        <Link to="/inquiries" className="button" onClick={() => setInquiry(null)}>
          Vaata
        </Link>
        <button type="button" className="link-button" onClick={() => setInquiry(null)}>
          Sulge
        </button>
      </div>
    </div>
  )
}

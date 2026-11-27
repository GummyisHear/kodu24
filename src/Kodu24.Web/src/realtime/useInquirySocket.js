import { useContext } from 'react'
import { InquirySocketContext } from './InquirySocketContext.js'

/** Reaalajas päringud: const { status, newCount, clearNew, subscribe } = useInquirySocket() */
export function useInquirySocket() {
  const context = useContext(InquirySocketContext)
  if (!context) throw new Error('useInquirySocket peab olema InquirySocketProvider sees.')
  return context
}

import { API_URL } from '../api/client.js'

// Eesti vormingus numbrid ja kuupäevad: "1 250 €", "45,5 m²", "25.11.2026".
const euro = new Intl.NumberFormat('et-EE', { style: 'currency', currency: 'EUR', maximumFractionDigits: 0 })
const euroCents = new Intl.NumberFormat('et-EE', { style: 'currency', currency: 'EUR', maximumFractionDigits: 2 })
const decimal = new Intl.NumberFormat('et-EE', { maximumFractionDigits: 1 })

export const formatPrice = (value) => euro.format(value)
export const formatPricePerM2 = (value) => `${euroCents.format(value)}/m²`
export const formatArea = (value) => `${decimal.format(value)} m²`
// Eesti keeles on arvsõna järel ainsuse osastav: "1 tuba", "3 tuba".
export const formatRooms = (rooms) => `${rooms} tuba`
export const formatDate = (iso) => new Date(iso).toLocaleDateString('et-EE')

/** "1 kuulutus", "5 kuulutust". */
export const formatListingCount = (count) => `${count} ${count === 1 ? 'kuulutus' : 'kuulutust'}`

/** API annab pildi aadressi kujul "/uploads/x.jpg"; pilt asub API serveris. */
export const imageUrl = (path) => (path ? API_URL + path : null)

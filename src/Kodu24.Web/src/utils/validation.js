// Kliendipoolne valideerimine. Reeglid ja teated on samad mis API DTO-del (AuthDtos.cs),
// et kasutaja näeks viga kohe, ilma serverisse saatmata. API kontrollib igal juhul uuesti.

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const PHONE_PATTERN = /^\+?[\d\s-]{5,30}$/

export function validateLogin({ email, password }) {
  const errors = {}
  if (!email.trim()) errors.email = 'E-post on kohustuslik.'
  if (!password) errors.password = 'Parool on kohustuslik.'
  return errors
}

export function validateRegister({ email, password, passwordConfirm, fullName, phone, role }) {
  const errors = {}

  if (!email.trim()) errors.email = 'E-post on kohustuslik.'
  else if (!EMAIL_PATTERN.test(email.trim())) errors.email = 'E-posti aadress ei ole korrektne.'
  else if (email.length > 200) errors.email = 'E-post võib olla kuni 200 tähemärki.'

  if (!password) errors.password = 'Parool on kohustuslik.'
  else if (password.length < 8) errors.password = 'Parool peab olema vähemalt 8 tähemärki.'
  else if (password.length > 100) errors.password = 'Parool võib olla kuni 100 tähemärki.'

  if (password && passwordConfirm !== password) errors.passwordConfirm = 'Paroolid ei kattu.'

  if (!fullName.trim()) errors.fullName = 'Nimi on kohustuslik.'
  else if (fullName.length > 100) errors.fullName = 'Nimi võib olla kuni 100 tähemärki.'

  if (phone.trim() && !PHONE_PATTERN.test(phone.trim())) errors.phone = 'Telefoninumber ei ole korrektne.'

  if (role !== 'Landlord' && role !== 'Seeker') errors.role = 'Vali, kas otsid korterit või annad üürile.'

  return errors
}

// Otsingufiltrid: samad piirid mis API ListingQueryDto-l.
export function validateListingFilters({ q, minPrice, maxPrice, rooms }) {
  const errors = {}
  const min = minPrice === '' ? null : Number(minPrice)
  const max = maxPrice === '' ? null : Number(maxPrice)

  if (q.length > 100) errors.q = 'Otsingusõna võib olla kuni 100 tähemärki.'
  if (min !== null && !(min >= 0 && min <= 100_000)) {
    errors.minPrice = 'Minimaalne hind peab olema vahemikus 0–100 000 €.'
  }
  if (max !== null && !(max >= 0 && max <= 100_000)) {
    errors.maxPrice = 'Maksimaalne hind peab olema vahemikus 0–100 000 €.'
  }
  if (!errors.minPrice && !errors.maxPrice && min !== null && max !== null && min > max) {
    errors.minPrice = 'Minimaalne hind ei saa olla suurem kui maksimaalne hind.'
  }
  if (rooms !== '' && !(Number.isInteger(Number(rooms)) && rooms >= 1 && rooms <= 20)) {
    errors.rooms = 'Tubade arv peab olema täisarv vahemikus 1–20.'
  }
  return errors
}

// Päring üürileandjale: samad piirid mis API InquiryRequestDto-l.
export const INQUIRY_MAX_LENGTH = 2000

export function validateInquiry({ message }) {
  const errors = {}
  const length = message.trim().length
  if (length === 0) errors.message = 'Sõnum on kohustuslik.'
  else if (length < 10 || length > INQUIRY_MAX_LENGTH) errors.message = 'Sõnum peab olema 10–2000 tähemärki.'
  return errors
}

export const hasErrors = (errors) => Object.keys(errors).length > 0

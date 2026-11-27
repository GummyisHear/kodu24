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

// Salvestatud otsing: samad piirid mis API SavedSearchRequestDto-l (hinna ja tubade reeglid nagu otsingufiltritel).
export function validateSavedSearch(values) {
  const errors = validateListingFilters({ ...values, q: '' })
  const name = values.name.trim()
  if (!name) errors.name = 'Nimi on kohustuslik.'
  else if (name.length < 2 || name.length > 100) errors.name = 'Nimi peab olema 2–100 tähemärki.'
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

// Kuulutuse vorm: samad reeglid ja teated mis API ListingRequestDto-l.
export const DESCRIPTION_MAX_LENGTH = 4000

const inRange = (value, min, max) => value !== '' && Number(value) >= min && Number(value) <= max

export function validateListing(v) {
  const errors = {}
  const title = v.title.trim()
  const address = v.address.trim()

  if (!title) errors.title = 'Pealkiri on kohustuslik.'
  else if (title.length < 5 || title.length > 150) errors.title = 'Pealkiri peab olema 5–150 tähemärki.'

  if (!address) errors.address = 'Aadress on kohustuslik.'
  else if (address.length < 3 || address.length > 200) errors.address = 'Aadress peab olema 3–200 tähemärki.'

  if (!v.districtId) errors.districtId = 'Vali linnaosa.'
  if (!inRange(v.price, 1, 100_000)) errors.price = 'Hind peab olema vahemikus 1–100 000 €.'
  if (!inRange(v.rooms, 1, 20) || !Number.isInteger(Number(v.rooms))) {
    errors.rooms = 'Tubade arv peab olema vahemikus 1–20.'
  }
  if (!inRange(v.areaM2, 5, 1000)) errors.areaM2 = 'Pind peab olema vahemikus 5–1000 m².'
  if (v.description.length > DESCRIPTION_MAX_LENGTH) errors.description = 'Kirjeldus võib olla kuni 4000 tähemärki.'
  if (!inRange(v.latitude, 57.5, 59.9)) errors.latitude = 'Asukoht peab olema Eestis (laiuskraad 57,5–59,9).'
  if (!inRange(v.longitude, 21.5, 28.3)) errors.longitude = 'Asukoht peab olema Eestis (pikkuskraad 21,5–28,3).'

  return errors
}

// Pildid: samad piirid mis API ListingImageService'il. Tüüpi kontrollib API faili sisu järgi uuesti.
export const MAX_IMAGES = 10
export const MAX_IMAGE_BYTES = 5 * 1024 * 1024
export const IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp']

/** Tagastab veateate või tühja stringi, kui valitud failid sobivad. */
export function validateImageFiles(files, existingCount = 0) {
  if (existingCount + files.length > MAX_IMAGES) {
    return `Kuulutusel võib olla kuni ${MAX_IMAGES} pilti (praegu ${existingCount}).`
  }
  for (const file of files) {
    if (!IMAGE_TYPES.includes(file.type)) return `Fail "${file.name}" ei ole pilt. Lubatud on JPEG, PNG ja WebP.`
    if (file.size > MAX_IMAGE_BYTES) return `Fail "${file.name}" on liiga suur (lubatud kuni 5 MB).`
  }
  return ''
}

export const hasErrors = (errors) => Object.keys(errors).length > 0

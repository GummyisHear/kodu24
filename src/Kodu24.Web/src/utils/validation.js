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

export const hasErrors = (errors) => Object.keys(errors).length > 0

import { useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { Roles } from '../auth/roles.js'
import { useAuth } from '../auth/useAuth.js'
import FormField from '../components/FormField.jsx'
import { hasErrors, validateRegister } from '../utils/validation.js'

const EMPTY_FORM = {
  email: '',
  password: '',
  passwordConfirm: '',
  fullName: '',
  phone: '',
  role: Roles.Seeker,
}

export default function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()

  const [form, setForm] = useState(EMPTY_FORM)
  const [errors, setErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  function handleChange(event) {
    const { name, value } = event.target
    setForm((current) => ({ ...current, [name]: value }))
    setErrors((current) => ({ ...current, [name]: undefined }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setFormError('')

    const clientErrors = validateRegister(form)
    setErrors(clientErrors)
    if (hasErrors(clientErrors)) return

    setSubmitting(true)
    try {
      await register({
        email: form.email.trim(),
        password: form.password,
        fullName: form.fullName.trim(),
        phone: form.phone.trim(),
        role: form.role,
      })
      navigate('/', { replace: true })
    } catch (error) {
      const fieldErrors = error.fieldErrors ?? {}
      // 409 (e-post on juba kasutusel) tuleb ilma väljata – näitame selle e-posti juures.
      if (error.status === 409) fieldErrors.email = error.message
      setErrors(fieldErrors)
      if (!hasErrors(fieldErrors)) setFormError(error.message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <section className="card auth-card">
      <h1>Loo konto</h1>
      <form className="form" onSubmit={handleSubmit} noValidate>
        {formError && (
          <p className="alert" role="alert">
            {formError}
          </p>
        )}

        <fieldset className={errors.role ? 'field field--invalid radio-group' : 'field radio-group'}>
          <legend>Olen</legend>
          <label>
            <input
              type="radio"
              name="role"
              value={Roles.Seeker}
              checked={form.role === Roles.Seeker}
              onChange={handleChange}
            />
            Üürija – otsin korterit
          </label>
          <label>
            <input
              type="radio"
              name="role"
              value={Roles.Landlord}
              checked={form.role === Roles.Landlord}
              onChange={handleChange}
            />
            Üürileandja – annan korterit üürile
          </label>
          {errors.role && <span className="field__error">{errors.role}</span>}
        </fieldset>

        <FormField
          label="Nimi"
          name="fullName"
          autoComplete="name"
          value={form.fullName}
          onChange={handleChange}
          error={errors.fullName}
        />
        <FormField
          label="E-post"
          name="email"
          type="email"
          autoComplete="email"
          value={form.email}
          onChange={handleChange}
          error={errors.email}
        />
        <FormField
          label="Telefon (valikuline)"
          name="phone"
          type="tel"
          autoComplete="tel"
          value={form.phone}
          onChange={handleChange}
          error={errors.phone}
        />
        <FormField
          label="Parool (vähemalt 8 tähemärki)"
          name="password"
          type="password"
          autoComplete="new-password"
          value={form.password}
          onChange={handleChange}
          error={errors.password}
        />
        <FormField
          label="Parool uuesti"
          name="passwordConfirm"
          type="password"
          autoComplete="new-password"
          value={form.passwordConfirm}
          onChange={handleChange}
          error={errors.passwordConfirm}
        />

        <button type="submit" className="button" disabled={submitting}>
          {submitting ? 'Loon kontot…' : 'Registreeri'}
        </button>
      </form>
      <p className="muted">
        Konto on juba olemas? <Link to="/login">Logi sisse</Link>
      </p>
    </section>
  )
}

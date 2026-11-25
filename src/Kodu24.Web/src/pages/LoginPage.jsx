import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router'
import { useAuth } from '../auth/useAuth.js'
import FormField from '../components/FormField.jsx'
import { hasErrors, validateLogin } from '../utils/validation.js'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [form, setForm] = useState({ email: '', password: '' })
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

    const clientErrors = validateLogin(form)
    setErrors(clientErrors)
    if (hasErrors(clientErrors)) return

    setSubmitting(true)
    try {
      await login(form.email.trim(), form.password)
      // Tagasi lehele, kuhu kasutaja enne sisselogimist minna tahtis (RequireAuth).
      navigate(location.state?.from?.pathname ?? '/', { replace: true })
    } catch (error) {
      setErrors(error.fieldErrors ?? {})
      setFormError(error.message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <section className="card auth-card">
      <h1>Logi sisse</h1>
      <form className="form" onSubmit={handleSubmit} noValidate>
        {formError && (
          <p className="alert" role="alert">
            {formError}
          </p>
        )}
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
          label="Parool"
          name="password"
          type="password"
          autoComplete="current-password"
          value={form.password}
          onChange={handleChange}
          error={errors.password}
        />
        <button type="submit" className="button" disabled={submitting}>
          {submitting ? 'Login sisse…' : 'Logi sisse'}
        </button>
      </form>
      <p className="muted">
        Pole veel kontot? <Link to="/register">Registreeri</Link>
      </p>
      {import.meta.env.DEV && (
        <p className="muted">
          Testkontod (parool <code>Parool123!</code>): liis@kodu24.ee (üürija), mari@kodu24.ee
          (üürileandja), admin@kodu24.ee
        </p>
      )}
    </section>
  )
}

/**
 * Sisestusväli koos sildi ja veateatega. multiline = mitmerealine tekstiväli (<textarea>).
 * Ülejäänud omadused lähevad <input>-ile või <textarea>-le.
 */
export default function FormField({ label, name, error, hint, multiline = false, ...inputProps }) {
  const errorId = `${name}-error`
  const hintId = `${name}-hint`
  const Input = multiline ? 'textarea' : 'input'
  const describedBy = [error && errorId, hint && hintId].filter(Boolean).join(' ') || undefined

  return (
    <div className={error ? 'field field--invalid' : 'field'}>
      <label htmlFor={name}>{label}</label>
      <Input id={name} name={name} aria-invalid={Boolean(error)} aria-describedby={describedBy} {...inputProps} />
      {hint && (
        <span id={hintId} className="field__hint">
          {hint}
        </span>
      )}
      {error && (
        <span id={errorId} className="field__error">
          {error}
        </span>
      )}
    </div>
  )
}

/**
 * Sisestusväli koos sildi ja veateatega. multiline = mitmerealine tekstiväli (<textarea>).
 * id = välja ID (vaikimisi sama mis name; vaja, kui lehel on mitu sama väljaga vormi).
 * Ülejäänud omadused lähevad <input>-ile või <textarea>-le.
 */
export default function FormField({ label, name, id = name, error, hint, multiline = false, ...inputProps }) {
  const errorId = `${id}-error`
  const hintId = `${id}-hint`
  const Input = multiline ? 'textarea' : 'input'
  const describedBy = [error && errorId, hint && hintId].filter(Boolean).join(' ') || undefined

  return (
    <div className={error ? 'field field--invalid' : 'field'}>
      <label htmlFor={id}>{label}</label>
      <Input id={id} name={name} aria-invalid={Boolean(error)} aria-describedby={describedBy} {...inputProps} />
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

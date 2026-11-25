/** Sisestusväli koos sildi ja veateatega. Ülejäänud omadused lähevad <input>-ile. */
export default function FormField({ label, name, error, ...inputProps }) {
  const errorId = `${name}-error`
  return (
    <div className={error ? 'field field--invalid' : 'field'}>
      <label htmlFor={name}>{label}</label>
      <input
        id={name}
        name={name}
        aria-invalid={Boolean(error)}
        aria-describedby={error ? errorId : undefined}
        {...inputProps}
      />
      {error && (
        <span id={errorId} className="field__error">
          {error}
        </span>
      )}
    </div>
  )
}

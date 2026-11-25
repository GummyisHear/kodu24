/**
 * Lehekülgede valik: "‹ 1 … 4 5 6 … 12 ›".
 * Näitab esimest, viimast ja kahte lehekülge mõlemal pool praegust.
 */
export default function Pagination({ page, totalPages, onChange }) {
  if (totalPages <= 1) return null

  const pages = []
  for (let p = 1; p <= totalPages; p++) {
    if (p === 1 || p === totalPages || Math.abs(p - page) <= 2) pages.push(p)
    else if (pages.at(-1) !== '…') pages.push('…')
  }

  return (
    <nav className="pagination" aria-label="Lehekülgede valik">
      <button type="button" disabled={page <= 1} onClick={() => onChange(page - 1)} aria-label="Eelmine lehekülg">
        ‹
      </button>
      {pages.map((p, index) =>
        p === '…' ? (
          <span key={`gap-${index}`} className="pagination__gap">
            …
          </span>
        ) : (
          <button
            key={p}
            type="button"
            className={p === page ? 'pagination__current' : undefined}
            aria-current={p === page ? 'page' : undefined}
            onClick={() => onChange(p)}
          >
            {p}
          </button>
        ),
      )}
      <button
        type="button"
        disabled={page >= totalPages}
        onClick={() => onChange(page + 1)}
        aria-label="Järgmine lehekülg"
      >
        ›
      </button>
    </nav>
  )
}

import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router'
import { getDistricts } from '../api/districts.js'
import { searchListings } from '../api/listings.js'
import ListingCard from '../components/ListingCard.jsx'
import ListingFilters from '../components/ListingFilters.jsx'
import Pagination from '../components/Pagination.jsx'
import { formatListingCount } from '../utils/format.js'

// Väärtused vastavad API ListingSort konstantidele.
const SORT_OPTIONS = [
  ['newest', 'Uuemad enne'],
  ['oldest', 'Vanemad enne'],
  ['price_asc', 'Odavamad enne'],
  ['price_desc', 'Kallimad enne'],
  ['area_desc', 'Suuremad enne'],
  ['price_m2_asc', 'Soodsaim €/m²'],
]

/**
 * Kuulutuste nimekiri. Filtrid, sortimine ja lehekülg on aadressiribal
 * (/listings?rooms=2&sort=price_asc&page=2), nii et otsingut saab jagada
 * ja brauseri "Tagasi" nupp töötab. Sama päringustring läheb otse API-le.
 */
export default function ListingsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const query = searchParams.toString()

  const [districts, setDistricts] = useState([])
  // Viimane API vastus ja päring, millele see kuulub. Kui aadressiriba päring
  // erineb vastuse omast, on uus päring pooleli ("loading").
  const [response, setResponse] = useState({ query: null, result: null, error: '' })
  const loading = response.query !== query
  const { result, error } = response

  useEffect(() => {
    // Linnaosad on ainult filtri jaoks; kui need ei lae, töötab otsing ikka.
    getDistricts()
      .then(setDistricts)
      .catch(() => setDistricts([]))
  }, [])

  useEffect(() => {
    // "cancelled" hoiab ära vana vastuse kuvamise, kui kasutaja jõudis vahepeal uue otsingu teha.
    let cancelled = false
    searchListings(Object.fromEntries(new URLSearchParams(query)))
      .then((data) => {
        if (!cancelled) setResponse({ query, result: data, error: '' })
      })
      .catch((err) => {
        if (cancelled) return
        // 400 korral (nt käsitsi muudetud URL) näitame esimest väljaviga.
        const message = Object.values(err.fieldErrors ?? {})[0] ?? err.message
        setResponse({ query, result: null, error: message })
      })
    return () => {
      cancelled = true
    }
  }, [query])

  /** Muudab aadressiriba parameetreid; tühjad väärtused eemaldatakse. */
  function updateParams(changes) {
    const next = new URLSearchParams(searchParams)
    for (const [key, value] of Object.entries(changes)) {
      if (value === '' || value === null || value === undefined) next.delete(key)
      else next.set(key, value)
    }
    setSearchParams(next)
  }

  function handleSearch(filters) {
    updateParams({ ...filters, page: null }) // uue otsinguga tagasi esimesele lehele
  }

  function handlePageChange(page) {
    updateParams({ page: page === 1 ? null : page })
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <div className="listings-layout">
      <aside>
        <ListingFilters key={query} searchParams={searchParams} districts={districts} onSearch={handleSearch} />
      </aside>

      <section>
        <div className="listings-toolbar">
          <h1>Kuulutused</h1>
          <p className="muted listings-toolbar__count" aria-live="polite">
            {result && `Leitud: ${formatListingCount(result.totalCount)}`}
          </p>
          <label className="listings-toolbar__sort">
            Sorteeri:
            <select
              value={searchParams.get('sort') ?? 'newest'}
              onChange={(e) => updateParams({ sort: e.target.value === 'newest' ? null : e.target.value, page: null })}
            >
              {SORT_OPTIONS.map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </label>
        </div>

        {error && (
          <p className="alert" role="alert">
            {error}
          </p>
        )}

        {loading && !result && <p className="muted">Laen kuulutusi…</p>}

        {result && result.items.length === 0 && (
          <div className="card empty-state">
            <p>Ühtegi kuulutust ei leitud. Proovi filtreid muuta.</p>
          </div>
        )}

        {result && result.items.length > 0 && (
          <>
            <div className={loading ? 'listing-grid listing-grid--loading' : 'listing-grid'}>
              {result.items.map((listing) => (
                <ListingCard key={listing.id} listing={listing} />
              ))}
            </div>
            <Pagination page={result.page} totalPages={result.totalPages} onChange={handlePageChange} />
          </>
        )}
      </section>
    </div>
  )
}

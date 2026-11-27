import { useEffect, useState } from 'react'
import { Marker, Popup } from 'react-leaflet'
import { Link, useSearchParams } from 'react-router'
import { getDistricts } from '../api/districts.js'
import { searchAllListings } from '../api/listings.js'
import ListingFilters from '../components/ListingFilters.jsx'
import BaseMap from '../map/BaseMap.jsx'
import FitToMarkers from '../map/FitToMarkers.jsx'
import { priceIcon } from '../map/mapConfig.js'
import { formatArea, formatListingCount, formatPrice, formatRooms, imageUrl } from '../utils/format.js'

/**
 * /map: kuulutused kaardil. Filtrid on aadressiribal samamoodi nagu /listings lehel,
 * nii et "Näita kaardil" ja "Näita nimekirjana" viivad sama otsinguga teisele lehele.
 */
export default function MapPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  // Kaardil pole lehekülgi ega sortimist: need parameetrid jäetakse API päringust välja.
  const filterParams = new URLSearchParams(searchParams)
  filterParams.delete('page')
  filterParams.delete('sort')
  const query = filterParams.toString()

  const [districts, setDistricts] = useState([])
  // Sama muster mis ListingsPage'il: vastus koos päringuga, millele see kuulub.
  const [response, setResponse] = useState({ query: null, result: null, error: '' })
  const loading = response.query !== query
  const { result, error } = response

  useEffect(() => {
    getDistricts()
      .then(setDistricts)
      .catch(() => setDistricts([]))
  }, [])

  useEffect(() => {
    let cancelled = false
    searchAllListings(Object.fromEntries(new URLSearchParams(query)))
      .then((data) => !cancelled && setResponse({ query, result: data, error: '' }))
      .catch((err) => {
        if (cancelled) return
        const message = Object.values(err.fieldErrors ?? {})[0] ?? err.message
        setResponse({ query, result: null, error: message })
      })
    return () => {
      cancelled = true
    }
  }, [query])

  function handleSearch(filters) {
    const next = new URLSearchParams()
    for (const [key, value] of Object.entries(filters)) if (value !== '') next.set(key, value)
    setSearchParams(next)
  }

  const items = result?.items ?? []
  const points = items.map((l) => [l.latitude, l.longitude])

  return (
    <div className="listings-layout">
      <aside>
        <ListingFilters key={query} searchParams={searchParams} districts={districts} onSearch={handleSearch} />
      </aside>

      <section>
        <div className="listings-toolbar">
          <h1>Kaart</h1>
          <p className="muted listings-toolbar__count" aria-live="polite">
            {loading ? 'Laen…' : result && `Kaardil: ${formatListingCount(items.length)}`}
            {result && result.totalCount > items.length && ` (kokku ${result.totalCount}, täpsusta otsingut)`}
          </p>
          <Link to={`/listings${query ? `?${query}` : ''}`} className="button button--secondary">
            Näita nimekirjana
          </Link>
        </div>

        {error && (
          <p className="alert" role="alert">
            {error}
          </p>
        )}

        <BaseMap className="map--large">
          {items.map((listing) => (
            <Marker key={listing.id} position={[listing.latitude, listing.longitude]} icon={priceIcon(listing.price)}>
              <Popup>
                <MapPopup listing={listing} />
              </Popup>
            </Marker>
          ))}
          <FitToMarkers points={points} />
        </BaseMap>

        {result && items.length === 0 && <p className="muted map__empty">Ühtegi kuulutust ei leitud. Proovi filtreid muuta.</p>}
      </section>
    </div>
  )
}

function MapPopup({ listing }) {
  const image = imageUrl(listing.primaryImageUrl)
  return (
    <div className="map-popup">
      {image && <img src={image} alt="" className="map-popup__image" />}
      <p className="map-popup__price">{formatPrice(listing.price)} / kuu</p>
      <p className="map-popup__title">{listing.title}</p>
      <p className="muted">
        {formatRooms(listing.rooms)} · {formatArea(listing.areaM2)} · {listing.districtName}
      </p>
      <Link to={`/listings/${listing.id}`}>Vaata kuulutust →</Link>
    </div>
  )
}

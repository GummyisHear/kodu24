import 'leaflet/dist/leaflet.css'
import { MapContainer, TileLayer } from 'react-leaflet'
import { TALLINN_CENTER, TILE_ATTRIBUTION, TILE_URL } from './mapConfig.js'

/**
 * Leafleti kaart OpenStreetMapi aluskaardiga. Markerid jm antakse children'ina.
 * NB! center ja zoom kehtivad ainult esimesel renderdamisel (Leafleti omadus);
 * vaate muutmiseks kasuta kaardi sees useMap() (vt FitToMarkers).
 */
export default function BaseMap({ center = TALLINN_CENTER, zoom = 12, className = '', children, ...props }) {
  return (
    <MapContainer center={center} zoom={zoom} className={`map ${className}`} scrollWheelZoom {...props}>
      <TileLayer url={TILE_URL} attribution={TILE_ATTRIBUTION} />
      {children}
    </MapContainer>
  )
}

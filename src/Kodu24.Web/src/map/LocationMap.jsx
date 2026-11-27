import { Marker } from 'react-leaflet'
import BaseMap from './BaseMap.jsx'
import { pinIcon } from './mapConfig.js'

/** Väike kaart ühe kuulutuse asukohaga (detailvaade). */
export default function LocationMap({ latitude, longitude }) {
  const position = [latitude, longitude]
  return (
    <BaseMap center={position} zoom={15} className="map--small">
      <Marker position={position} icon={pinIcon} />
    </BaseMap>
  )
}

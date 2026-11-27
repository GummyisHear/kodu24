import { useEffect } from 'react'
import { Marker, useMap, useMapEvents } from 'react-leaflet'
import BaseMap from './BaseMap.jsx'
import { isInEstonia, pinIcon } from './mapConfig.js'

/** Koordinaadid 6 komakohaga (~10 cm täpsus), et väljadele ei tuleks pikki sabasid. */
const round = (value) => Math.round(value * 1e6) / 1e6

/**
 * Kuulutuse asukoha valimine: klõps kaardil või nõela lohistamine annab koordinaadid
 * (onPick(lat, lng)). latitude/longitude on vormi väärtused (stringid); kui need on
 * käsitsi sisestatud, liigub nõel ka kaardil.
 */
export default function LocationPicker({ latitude, longitude, onPick }) {
  const lat = Number(latitude)
  const lng = Number(longitude)
  const position = latitude !== '' && longitude !== '' && isInEstonia(lat, lng) ? [lat, lng] : null

  return (
    <BaseMap center={position ?? undefined} zoom={position ? 15 : 12} className="map--small">
      <ClickToPick onPick={onPick} />
      {position && (
        <>
          <Marker
            position={position}
            icon={pinIcon}
            draggable
            eventHandlers={{
              dragend: (event) => {
                const { lat: newLat, lng: newLng } = event.target.getLatLng()
                onPick(round(newLat), round(newLng))
              },
            }}
          />
          <KeepInView position={position} />
        </>
      )}
    </BaseMap>
  )
}

function ClickToPick({ onPick }) {
  useMapEvents({
    click: (event) => onPick(round(event.latlng.lat), round(event.latlng.lng)),
  })
  return null
}

/** Kui nõel on käsitsi sisestatud koordinaatidega vaatest väljas, liigutab kaardi selle juurde. */
function KeepInView({ position }) {
  const map = useMap()
  const [lat, lng] = position
  useEffect(() => {
    if (!map.getBounds().contains([lat, lng])) map.panTo([lat, lng])
  }, [map, lat, lng])
  return null
}

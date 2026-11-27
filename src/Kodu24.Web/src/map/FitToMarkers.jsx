import { useEffect } from 'react'
import { useMap } from 'react-leaflet'

/**
 * Suumib kaardi nii, et kõik punktid ([lat, lng]) oleksid näha.
 * Käivitub ainult siis, kui punktid päriselt muutuvad (nt uue otsinguga). Selleks on
 * sõltuvuseks tekst "lat,lng;lat,lng": uus massiiv samade punktidega iga renderdusel
 * ei tohi kasutaja liigutatud kaarti tagasi hüpata.
 */
export default function FitToMarkers({ points }) {
  const map = useMap()
  const key = points.map((p) => p.join(',')).join(';')

  useEffect(() => {
    if (!key) return
    const bounds = key.split(';').map((p) => p.split(',').map(Number))
    map.fitBounds(bounds, { padding: [40, 40], maxZoom: 15 })
  }, [map, key])

  return null
}

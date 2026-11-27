import L from 'leaflet'
import { formatPrice } from '../utils/format.js'

// Kaardi ühised seaded. Aluskaart: OpenStreetMap (tasuta, API võtit pole vaja;
// litsents nõuab, et kaardi nurgas oleks viide OpenStreetMapi kaastöötajatele).
export const TILE_URL = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png'
export const TILE_ATTRIBUTION = '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> kaastöötajad'

/** Tallinna kesklinn: kaardi algvaade, kui kuulutusi pole. */
export const TALLINN_CENTER = [59.437, 24.7536]

/** Eesti piirid, samad mis API kontrollil (ListingRequestDto): laius 57,5–59,9, pikkus 21,5–28,3. */
export const ESTONIA_BOUNDS = { minLat: 57.5, maxLat: 59.9, minLng: 21.5, maxLng: 28.3 }

export const isInEstonia = (lat, lng) =>
  lat >= ESTONIA_BOUNDS.minLat && lat <= ESTONIA_BOUNDS.maxLat && lng >= ESTONIA_BOUNDS.minLng && lng <= ESTONIA_BOUNDS.maxLng

// Markerid on HTML-ist (divIcon), mitte Leafleti vaikepildid: need ei leia Vite'i
// ehituses oma pildifaile üles ja hinnaga silt on kaardil informatiivsem.

/** Hinnasilt kuulutuste kaardile: "780 €". */
export const priceIcon = (price) =>
  L.divIcon({
    className: 'map-marker',
    html: `<span class="map-pin map-pin--price">${formatPrice(price)}</span>`,
    iconSize: null,
    iconAnchor: [0, 0],
    popupAnchor: [0, -34],
  })

/** Lihtne asukohanõel (detailvaade ja asukoha valimine). */
export const pinIcon = L.divIcon({
  className: 'map-marker',
  html: '<span class="map-pin map-pin--dot"></span>',
  iconSize: null,
  iconAnchor: [0, 0],
  popupAnchor: [0, -28],
})

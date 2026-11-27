import { useCallback, useEffect, useMemo, useState } from 'react'
import { addFavorite, getFavoriteIds, removeFavorite } from '../api/favorites.js'
import { Roles } from '../auth/roles.js'
import { useAuth } from '../auth/useAuth.js'
import { FavoritesContext } from './FavoritesContext.js'

/**
 * Hoiab üürija lemmikute ID-sid ühes kohas, et süda oleks sama nii nimekirjas,
 * detailvaates kui lemmikute lehel. ID-d loetakse üks kord pärast sisselogimist
 * (GET /api/favorites/ids). Lemmikud on ainult üürijal (API: Policies.Seeker).
 */
export default function FavoritesProvider({ children }) {
  const { user } = useAuth()
  const userId = user?.role === Roles.Seeker ? user.id : null

  // ID-d koos kasutajaga, kellele need kuuluvad: teise kasutaja sisselogimisel ei näidata vana kasutaja lemmikuid.
  const [state, setState] = useState({ userId: null, ids: new Set() })
  const ids = state.userId === userId ? state.ids : null

  useEffect(() => {
    if (userId === null) return
    let cancelled = false
    getFavoriteIds()
      .then((list) => !cancelled && setState({ userId, ids: new Set(list) }))
      .catch(() => !cancelled && setState({ userId, ids: new Set() }))
    return () => {
      cancelled = true
    }
  }, [userId])

  const isFavorite = useCallback((listingId) => ids?.has(listingId) ?? false, [ids])

  /**
   * Lisab või eemaldab lemmiku. Süda muutub kohe (ootamata API vastust);
   * kui API annab vea, pannakse eelmine olek tagasi ja viga visatakse edasi.
   */
  const toggle = useCallback(
    async (listingId) => {
      const wasFavorite = ids?.has(listingId) ?? false
      const update = (add) =>
        setState((current) => {
          const next = new Set(current.userId === userId ? current.ids : [])
          if (add) next.add(listingId)
          else next.delete(listingId)
          return { userId, ids: next }
        })

      update(!wasFavorite)
      try {
        if (wasFavorite) await removeFavorite(listingId)
        else await addFavorite(listingId)
      } catch (err) {
        // 404 eemaldamisel = polnudki lemmik (nt teises vahelehes eemaldatud); olek on juba õige.
        if (wasFavorite && err.status === 404) return
        update(wasFavorite)
        throw err
      }
    },
    [ids, userId],
  )

  const value = useMemo(
    // ready = ID-d on laetud (enne seda ei tea, kas kuulutus on lemmik).
    () => ({ enabled: userId !== null, ready: ids !== null, isFavorite, toggle }),
    [userId, ids, isFavorite, toggle],
  )

  return <FavoritesContext.Provider value={value}>{children}</FavoritesContext.Provider>
}

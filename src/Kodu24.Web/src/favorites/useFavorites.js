import { useContext } from 'react'
import { FavoritesContext } from './FavoritesContext.js'

/** Lemmikute olek: const { enabled, isFavorite, toggle } = useFavorites() */
export function useFavorites() {
  const context = useContext(FavoritesContext)
  if (!context) throw new Error('useFavorites peab olema FavoritesProvider sees.')
  return context
}

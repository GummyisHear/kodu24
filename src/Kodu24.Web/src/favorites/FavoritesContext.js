import { createContext } from 'react'

/** { enabled, ready, isFavorite(id), toggle(id) } – vt FavoritesProvider.jsx */
export const FavoritesContext = createContext(null)

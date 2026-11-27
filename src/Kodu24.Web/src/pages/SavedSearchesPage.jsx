import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { getDistricts } from '../api/districts.js'
import { getNotifications, markNotificationRead } from '../api/notifications.js'
import { createSavedSearch, deleteSavedSearch, getSavedSearches, updateSavedSearch } from '../api/savedSearches.js'
import SavedSearchForm from '../components/SavedSearchForm.jsx'
import { formatDate } from '../utils/format.js'
import { describeSearch, listingsUrl, toSavedSearchForm } from '../utils/savedSearch.js'

const MAX_SEARCHES = 20 // sama mis API SavedSearchService.MaxPerUser

/**
 * /saved-searches: üürija salvestatud otsingud ja teavitused uutest kuulutustest.
 * Teavitused loob taustateenus (Kodu24.Worker) kord tunnis. Ligipääs: üürija (vt App.jsx).
 */
export default function SavedSearchesPage() {
  const [searches, setSearches] = useState(null)
  const [notifications, setNotifications] = useState(null)
  const [districts, setDistricts] = useState([])
  const [error, setError] = useState('')
  // null = vorm kinni, 'new' = uue otsingu vorm, number = selle otsingu muutmine
  const [editing, setEditing] = useState(null)

  useEffect(() => {
    let cancelled = false
    Promise.all([getSavedSearches(), getNotifications()])
      .then(([searchList, notificationList]) => {
        if (cancelled) return
        setSearches(searchList)
        setNotifications(notificationList)
      })
      .catch((err) => !cancelled && setError(err.message))
    // Linnaosad on ainult vormi valiku jaoks; kui need ei lae, töötab leht ikka.
    getDistricts()
      .then((list) => !cancelled && setDistricts(list))
      .catch(() => {})
    return () => {
      cancelled = true
    }
  }, [])

  async function handleCreate(request) {
    const created = await createSavedSearch(request)
    setSearches((current) => [created, ...current])
    setEditing(null)
  }

  async function handleUpdate(id, request) {
    const updated = await updateSavedSearch(id, request)
    setSearches((current) => current.map((s) => (s.id === id ? updated : s)))
    setEditing(null)
  }

  async function handleDelete(search) {
    if (!window.confirm(`Kas kustutada otsing „${search.name}”? Ka selle teavitused kustutatakse.`)) return
    setError('')
    try {
      await deleteSavedSearch(search.id)
      setSearches((current) => current.filter((s) => s.id !== search.id))
      setNotifications((current) => current.filter((n) => n.savedSearchId !== search.id))
    } catch (err) {
      setError(err.message)
    }
  }

  function handleRead(notification) {
    if (notification.isRead) return
    // Märgime kohe loetuks; kui API päring ebaõnnestub, on teavitus järgmisel laadimisel jälle lugemata.
    setNotifications((current) => current.map((n) => (n.id === notification.id ? { ...n, isRead: true } : n)))
    markNotificationRead(notification.id).catch(() => {})
  }

  if (error && !searches) {
    return (
      <p className="alert" role="alert">
        {error}
      </p>
    )
  }
  if (!searches) return <p className="muted">Laen otsinguid…</p>

  const searchNames = Object.fromEntries(searches.map((s) => [s.id, s.name]))
  const unreadCount = notifications.filter((n) => !n.isRead).length

  return (
    <div className="saved-searches">
      {error && (
        <p className="alert" role="alert">
          {error}
        </p>
      )}

      <section>
        <div className="section-header">
          <div>
            <h1>Minu otsingud</h1>
            <p className="muted">
              {searches.length} / {MAX_SEARCHES} otsingut. Uutest sobivatest kuulutustest saad teavituse.
            </p>
          </div>
          {editing !== 'new' && searches.length < MAX_SEARCHES && (
            <button type="button" className="button" onClick={() => setEditing('new')}>
              Lisa otsing
            </button>
          )}
        </div>

        {editing === 'new' && (
          <div className="card form-section">
            <h2>Uus otsing</h2>
            <SavedSearchForm
              idPrefix="new"
              initialValues={toSavedSearchForm(null)}
              districts={districts}
              submitLabel="Salvesta otsing"
              onSubmit={handleCreate}
              onCancel={() => setEditing(null)}
            />
          </div>
        )}

        {searches.length === 0 && editing !== 'new' && (
          <div className="card empty-state">
            <p>Sul ei ole salvestatud otsinguid. Vali kuulutuste lehel filtrid ja vajuta „Salvesta otsing”.</p>
            <Link to="/listings" className="button">
              Kuulutuste juurde
            </Link>
          </div>
        )}

        <ul className="saved-searches__list">
          {searches.map((search) =>
            editing === search.id ? (
              <li key={search.id} className="card form-section">
                <h2>Muuda otsingut</h2>
                <SavedSearchForm
                  idPrefix={`edit-${search.id}`}
                  initialValues={toSavedSearchForm(search)}
                  districts={districts}
                  submitLabel="Salvesta muudatused"
                  onSubmit={(request) => handleUpdate(search.id, request)}
                  onCancel={() => setEditing(null)}
                />
              </li>
            ) : (
              <li key={search.id} className="card saved-search">
                <div>
                  <h2 className="saved-search__name">{search.name}</h2>
                  <p>{describeSearch(search)}</p>
                  <p className="muted saved-search__date">Salvestatud {formatDate(search.createdAt)}</p>
                </div>
                <div className="saved-search__actions">
                  <Link to={listingsUrl(search)} className="button">
                    Vaata kuulutusi
                  </Link>
                  <button type="button" className="button button--secondary" onClick={() => setEditing(search.id)}>
                    Muuda
                  </button>
                  <button
                    type="button"
                    className="link-button link-button--danger"
                    onClick={() => handleDelete(search)}
                  >
                    Kustuta
                  </button>
                </div>
              </li>
            ),
          )}
        </ul>
      </section>

      <section className="notifications">
        <h2>
          Teavitused {unreadCount > 0 && <span className="badge">{unreadCount} uut</span>}
        </h2>
        {notifications.length === 0 ? (
          <p className="muted">
            Teavitusi veel ei ole. Taustateenus kontrollib uusi kuulutusi kord tunnis ja annab siin teada, kui mõni
            sobib sinu otsinguga.
          </p>
        ) : (
          <ul className="notifications__list">
            {notifications.map((n) => (
              <li key={n.id} className={n.isRead ? 'card notification' : 'card notification notification--unread'}>
                <div>
                  <p className="notification__message">
                    <Link to={`/listings/${n.listingId}`} onClick={() => handleRead(n)}>
                      {n.message}
                    </Link>
                  </p>
                  <p className="muted notification__meta">
                    {searchNames[n.savedSearchId] && `${searchNames[n.savedSearchId]} · `}
                    {new Date(n.createdAt).toLocaleString('et-EE', { dateStyle: 'short', timeStyle: 'short' })}
                  </p>
                </div>
                {!n.isRead && (
                  <button type="button" className="link-button" onClick={() => handleRead(n)}>
                    Märgi loetuks
                  </button>
                )}
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}

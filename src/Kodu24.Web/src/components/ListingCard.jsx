import { Link } from 'react-router'
import { formatArea, formatPrice, formatPricePerM2, formatRooms, imageUrl } from '../utils/format.js'
import FavoriteButton from './FavoriteButton.jsx'

/**
 * Kuulutuse kaart nimekirjas. listing = ListingListItemDto API-st.
 * Lemmiku süda on lingi kõrval (mitte sees), sest nupp lingi sees ei ole lubatud HTML.
 */
export default function ListingCard({ listing }) {
  const image = imageUrl(listing.primaryImageUrl)

  return (
    <div className="listing-card">
      <Link to={`/listings/${listing.id}`} className="listing-card__link">
        {image ? (
          <img className="listing-card__image" src={image} alt="" loading="lazy" />
        ) : (
          <div className="listing-card__image listing-card__image--empty">Pilt puudub</div>
        )}
        <div className="listing-card__body">
          <p className="listing-card__price">
            {formatPrice(listing.price)} <span className="muted">/ kuu</span>
          </p>
          <h3 className="listing-card__title">{listing.title}</h3>
          <p className="listing-card__facts">
            {formatRooms(listing.rooms)} · {formatArea(listing.areaM2)} · {formatPricePerM2(listing.pricePerM2)}
          </p>
          <p className="muted">
            {listing.districtName}, {listing.address}
          </p>
        </div>
      </Link>
      <FavoriteButton listingId={listing.id} />
    </div>
  )
}

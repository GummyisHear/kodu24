using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Mapping;
using Kodu24.Api.Repositories;

namespace Kodu24.Api.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favorites;
    private readonly IListingRepository _listings;
    private readonly TimeProvider _time;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(
        IFavoriteRepository favorites,
        IListingRepository listings,
        TimeProvider time,
        ILogger<FavoriteService> logger)
    {
        _favorites = favorites;
        _listings = listings;
        _time = time;
        _logger = logger;
    }

    public async Task<(FavoriteDto Favorite, bool Created)> AddAsync(int listingId, CurrentUser user)
    {
        // Kustutatud kuulutus on päringufiltriga peidetud → 404.
        var listing = await _listings.GetWithDetailsAsync(listingId)
                      ?? throw new NotFoundException($"Kuulutust {listingId} ei leitud.");

        var existing = await _favorites.FindAsync(user.Id, listingId);
        if (existing is not null)
        {
            existing.Listing = listing;
            return (existing.ToDto(), false);
        }

        // Kasutaja tuleb tokenist, mitte päringust: teise kasutaja nimel lemmikut lisada ei saa.
        var favorite = new Favorite
        {
            UserId = user.Id,
            ListingId = listing.Id,
            Listing = listing,
            CreatedAt = _time.GetUtcNow().UtcDateTime,
        };

        await _favorites.AddAsync(favorite);
        await _favorites.SaveChangesAsync();
        _logger.LogInformation("Kasutaja {UserId} lisas kuulutuse {ListingId} lemmikuks", user.Id, listing.Id);

        return (favorite.ToDto(), true);
    }

    public async Task<List<FavoriteDto>> GetMineAsync(CurrentUser user)
    {
        var favorites = await _favorites.GetByUserAsync(user.Id);
        return favorites.Select(f => f.ToDto()).ToList();
    }

    public Task<List<int>> GetMyListingIdsAsync(CurrentUser user) => _favorites.GetListingIdsAsync(user.Id);

    public async Task RemoveAsync(int listingId, CurrentUser user)
    {
        var favorite = await _favorites.FindAsync(user.Id, listingId)
                       ?? throw new NotFoundException($"Kuulutus {listingId} ei ole sinu lemmikute hulgas.");

        _favorites.Remove(favorite);
        await _favorites.SaveChangesAsync();
        _logger.LogInformation("Kasutaja {UserId} eemaldas kuulutuse {ListingId} lemmikutest", user.Id, listingId);
    }
}

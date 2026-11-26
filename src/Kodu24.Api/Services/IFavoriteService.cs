using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

public interface IFavoriteService
{
    /// <summary>
    /// Lisab kuulutuse lemmikuks. Kui see on juba lemmik, tagastatakse olemasolev
    /// (<c>Created = false</c>), et topeltklõps ei annaks viga ega duplikaati.
    /// </summary>
    Task<(FavoriteDto Favorite, bool Created)> AddAsync(int listingId, CurrentUser user);

    Task<List<FavoriteDto>> GetMineAsync(CurrentUser user);

    Task<List<int>> GetMyListingIdsAsync(CurrentUser user);

    Task RemoveAsync(int listingId, CurrentUser user);
}

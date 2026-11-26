using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

public interface IListingImageService
{
    /// <summary>
    /// Lisab kuulutusele pildid ja tagastab kuulutuse kõik pildid. Esimene pilt muutub
    /// põhipildiks, kui seda veel pole. Lubatud kuulutuse omanikul ja adminil.
    /// </summary>
    Task<List<ListingImageDto>> UploadAsync(int listingId, IReadOnlyList<IFormFile> files, CurrentUser user);

    /// <summary>Kustutab pildi (andmebaasist ja kettalt). Põhipildi kustutamisel saab põhipildiks järgmine.</summary>
    Task DeleteAsync(int listingId, int imageId, CurrentUser user);

    /// <summary>Teeb pildi kuulutuse põhipildiks (see näidatakse nimekirjas).</summary>
    Task SetPrimaryAsync(int listingId, int imageId, CurrentUser user);
}

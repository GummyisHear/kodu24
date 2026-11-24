using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

public interface IListingService
{
    /// <summary>Aktiivne kuulutus. Puudub või kustutatud → <c>NotFoundException</c>.</summary>
    Task<ListingDetailDto> GetByIdAsync(int id);

    /// <summary>Loob kuulutuse sisselogitud üürileandjale.</summary>
    Task<ListingDetailDto> CreateAsync(ListingRequestDto request, CurrentUser user);

    /// <summary>Muudab kuulutust. Lubatud omanikul ja adminil.</summary>
    Task<ListingDetailDto> UpdateAsync(int id, ListingRequestDto request, CurrentUser user);

    /// <summary>Kustutab kuulutuse (soft delete). Lubatud omanikul ja adminil.</summary>
    Task DeleteAsync(int id, CurrentUser user);

    /// <summary>
    /// Märgib aegunud kuulutused kustutatuks (soft delete).
    /// Tagastab märgitud kuulutuste arvu.
    /// </summary>
    Task<int> ExpireListingsAsync();
}

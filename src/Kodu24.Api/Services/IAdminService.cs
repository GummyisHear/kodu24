using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

/// <summary>Kasutajate ja kuulutuste haldus. Ainult admin (vt AdminController).</summary>
public interface IAdminService
{
    Task<PagedResultDto<AdminUserDto>> GetUsersAsync(AdminUserQueryDto query);

    /// <summary>
    /// Muudab kasutaja rolli. Enda rolli muuta ei saa (admin ei tohi end kogemata välja lukustada)
    /// ja teenusekonto rolli ei saa anda ega ära võtta.
    /// </summary>
    Task<AdminUserDto> ChangeRoleAsync(int userId, ChangeRoleRequestDto request, CurrentUser admin);

    Task<PagedResultDto<AdminListingDto>> GetListingsAsync(AdminListingQueryDto query);

    /// <summary>
    /// Taastab kustutatud kuulutuse. Kui kehtivus on läbi, pikendatakse seda
    /// <see cref="ListingService.ListingLifetimeDays"/> päeva võrra, muidu kustutaks taustateenus selle kohe uuesti.
    /// </summary>
    Task<AdminListingDto> RestoreListingAsync(int listingId);

    Task<AdminStatsDto> GetStatsAsync();
}

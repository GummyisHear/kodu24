using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

public interface IListingService
{
    Task<ListingDetailDto?> GetByIdAsync(int id);

    /// <summary>
    /// Märgib aegunud kuulutused kustutatuks (soft delete).
    /// Tagastab märgitud kuulutuste arvu.
    /// </summary>
    Task<int> ExpireListingsAsync();
}

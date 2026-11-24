using Kodu24.Api.Entities;

namespace Kodu24.Api.Services;

public interface IListingService
{
    Task<Listing?> GetByIdAsync(int id);

    /// <summary>
    /// Märgib aegunud kuulutused kustutatuks (soft delete).
    /// Tagastab märgitud kuulutuste arvu.
    /// </summary>
    Task<int> ExpireListingsAsync();
}

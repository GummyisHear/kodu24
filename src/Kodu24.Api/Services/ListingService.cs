using Kodu24.Api.Dtos;
using Kodu24.Api.Mapping;
using Kodu24.Api.Repositories;

namespace Kodu24.Api.Services;

public class ListingService : IListingService
{
    private readonly IListingRepository _listings;
    private readonly TimeProvider _time;
    private readonly ILogger<ListingService> _logger;

    public ListingService(IListingRepository listings, TimeProvider time, ILogger<ListingService> logger)
    {
        _listings = listings;
        _time = time;
        _logger = logger;
    }

    public async Task<ListingDetailDto?> GetByIdAsync(int id)
    {
        var listing = await _listings.GetWithDetailsAsync(id);
        return listing?.ToDetailDto();
    }

    public async Task<int> ExpireListingsAsync()
    {
        var now = _time.GetUtcNow().UtcDateTime;
        var expired = await _listings.GetExpiredAsync(now);
        if (expired.Count == 0)
            return 0;

        foreach (var listing in expired)
        {
            listing.IsDeleted = true;
            listing.DeletedAt = now;
        }

        await _listings.SaveChangesAsync();
        _logger.LogInformation("Märgiti aegunuks {Count} kuulutust", expired.Count);
        return expired.Count;
    }
}

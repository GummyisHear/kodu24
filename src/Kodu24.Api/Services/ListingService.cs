using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Mapping;
using Kodu24.Api.Repositories;

namespace Kodu24.Api.Services;

public class ListingService : IListingService
{
    /// <summary>Mitu päeva on uus kuulutus aktiivne, enne kui Worker selle aegunuks märgib.</summary>
    public const int ListingLifetimeDays = 30;

    private readonly IListingRepository _listings;
    private readonly IRepository<District> _districts;
    private readonly TimeProvider _time;
    private readonly ILogger<ListingService> _logger;

    public ListingService(
        IListingRepository listings,
        IRepository<District> districts,
        TimeProvider time,
        ILogger<ListingService> logger)
    {
        _listings = listings;
        _districts = districts;
        _time = time;
        _logger = logger;
    }

    public async Task<PagedResultDto<ListingListItemDto>> SearchAsync(ListingQueryDto query)
    {
        if (query.MinPrice > query.MaxPrice)
            throw new ValidationFailedException(nameof(ListingQueryDto.MinPrice),
                "Minimaalne hind ei saa olla suurem kui maksimaalne hind.");

        var (items, total) = await _listings.SearchAsync(query);
        return new PagedResultDto<ListingListItemDto>(
            items.Select(l => l.ToListItemDto()).ToList(),
            query.Page,
            query.PageSize,
            total);
    }

    public async Task<ListingDetailDto> GetByIdAsync(int id) =>
        (await FindAsync(id)).ToDetailDto();

    public async Task<List<MyListingDto>> GetMineAsync(CurrentUser user) =>
        (await _listings.GetByLandlordAsync(user.Id)).Select(l => l.ToMyListingDto()).ToList();

    public async Task<ListingDetailDto> CreateAsync(ListingRequestDto request, CurrentUser user)
    {
        await EnsureDistrictExistsAsync(request.DistrictId);

        var now = _time.GetUtcNow().UtcDateTime;
        var listing = new Listing
        {
            LandlordId = user.Id,
            CreatedAt = now,
            ExpiresAt = now.AddDays(ListingLifetimeDays),
        };
        request.ApplyTo(listing);

        await _listings.AddAsync(listing);
        await _listings.SaveChangesAsync();
        _logger.LogInformation("Kasutaja {UserId} lisas kuulutuse {ListingId}", user.Id, listing.Id);

        // Laeme uuesti koos linnaosa ja üürileandjaga, et DTO oleks täielik.
        return (await FindAsync(listing.Id)).ToDetailDto();
    }

    public async Task<ListingDetailDto> UpdateAsync(int id, ListingRequestDto request, CurrentUser user)
    {
        var listing = await FindAsync(id);
        user.EnsureCanManageListing(listing);

        if (request.DistrictId != listing.DistrictId)
            listing.District = await EnsureDistrictExistsAsync(request.DistrictId);

        request.ApplyTo(listing);
        await _listings.SaveChangesAsync();
        _logger.LogInformation("Kasutaja {UserId} muutis kuulutust {ListingId}", user.Id, id);

        return listing.ToDetailDto();
    }

    public async Task DeleteAsync(int id, CurrentUser user)
    {
        var listing = await FindAsync(id);
        user.EnsureCanManageListing(listing);

        listing.IsDeleted = true;
        listing.DeletedAt = _time.GetUtcNow().UtcDateTime;
        await _listings.SaveChangesAsync();
        _logger.LogInformation("Kasutaja {UserId} kustutas kuulutuse {ListingId}", user.Id, id);
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

    private async Task<Listing> FindAsync(int id) =>
        await _listings.GetWithDetailsAsync(id)
        ?? throw new NotFoundException($"Kuulutust {id} ei leitud.");

    private async Task<District> EnsureDistrictExistsAsync(int districtId) =>
        await _districts.GetByIdAsync(districtId)
        ?? throw new ValidationFailedException(nameof(ListingRequestDto.DistrictId), "Valitud linnaosa ei ole olemas.");
}

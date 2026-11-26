using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Mapping;
using Kodu24.Api.Repositories;

namespace Kodu24.Api.Services;

public class SavedSearchService : ISavedSearchService
{
    /// <summary>Ühel kasutajal võib olla kuni nii palju salvestatud otsinguid (taustateenus käib need kõik läbi).</summary>
    public const int MaxPerUser = 20;

    private readonly ISavedSearchRepository _searches;
    private readonly IRepository<District> _districts;
    private readonly TimeProvider _time;
    private readonly ILogger<SavedSearchService> _logger;

    public SavedSearchService(
        ISavedSearchRepository searches,
        IRepository<District> districts,
        TimeProvider time,
        ILogger<SavedSearchService> logger)
    {
        _searches = searches;
        _districts = districts;
        _time = time;
        _logger = logger;
    }

    public async Task<List<SavedSearchDto>> GetMineAsync(CurrentUser user)
    {
        var searches = await _searches.GetByUserAsync(user.Id);
        return searches.Select(s => s.ToDto()).ToList();
    }

    public async Task<SavedSearchDto> GetByIdAsync(int id, CurrentUser user) =>
        (await FindOwnAsync(id, user)).ToDto();

    public async Task<SavedSearchDto> CreateAsync(SavedSearchRequestDto request, CurrentUser user)
    {
        if (await _searches.CountByUserAsync(user.Id) >= MaxPerUser)
            throw new ValidationFailedException(nameof(SavedSearchRequestDto.Name),
                $"Sul võib olla kuni {MaxPerUser} salvestatud otsingut. Kustuta mõni vana otsing.");

        var district = await ValidateAsync(request);
        var now = _time.GetUtcNow().UtcDateTime;

        var search = new SavedSearch
        {
            UserId = user.Id,
            CreatedAt = now,
            // Teavitused tulevad ainult kuulutustest, mis lisatakse pärast otsingu salvestamist.
            LastNotifiedAt = now,
        };
        Apply(search, request, district);

        await _searches.AddAsync(search);
        await _searches.SaveChangesAsync();
        _logger.LogInformation("Kasutaja {UserId} salvestas otsingu {SavedSearchId}", user.Id, search.Id);

        return search.ToDto();
    }

    public async Task<SavedSearchDto> UpdateAsync(int id, SavedSearchRequestDto request, CurrentUser user)
    {
        var search = await FindOwnAsync(id, user);
        var district = await ValidateAsync(request);

        // LastNotifiedAt jääb samaks: juba teavitatud kuulutuste kohta uut teadet ei tule.
        Apply(search, request, district);
        await _searches.SaveChangesAsync();

        return search.ToDto();
    }

    public async Task DeleteAsync(int id, CurrentUser user)
    {
        var search = await FindOwnAsync(id, user);

        // Otsingu teavitused kustuvad kaasa (cascade, vt AppDbContext).
        _searches.Remove(search);
        await _searches.SaveChangesAsync();
        _logger.LogInformation("Kasutaja {UserId} kustutas otsingu {SavedSearchId}", user.Id, id);
    }

    private async Task<SavedSearch> FindOwnAsync(int id, CurrentUser user)
    {
        var search = await _searches.GetWithDistrictAsync(id)
                     ?? throw new NotFoundException($"Salvestatud otsingut {id} ei leitud.");
        user.EnsureOwner(search.UserId);
        return search;
    }

    /// <summary>Kontrollid, mis vajavad andmebaasi või mitut välja korraga. Tagastab valitud linnaosa.</summary>
    private async Task<District?> ValidateAsync(SavedSearchRequestDto request)
    {
        if (request.MinPrice > request.MaxPrice)
            throw new ValidationFailedException(nameof(SavedSearchRequestDto.MinPrice),
                "Minimaalne hind ei saa olla suurem kui maksimaalne hind.");

        if (request.DistrictId is not int districtId)
            return null;

        return await _districts.GetByIdAsync(districtId)
               ?? throw new ValidationFailedException(nameof(SavedSearchRequestDto.DistrictId), "Valitud linnaosa ei ole olemas.");
    }

    private static void Apply(SavedSearch search, SavedSearchRequestDto request, District? district)
    {
        search.Name = request.Name.Trim();
        search.MinPrice = request.MinPrice;
        search.MaxPrice = request.MaxPrice;
        search.Rooms = request.Rooms;
        search.DistrictId = district?.Id;
        search.District = district;
    }
}

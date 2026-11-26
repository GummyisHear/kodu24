using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

/// <summary>Kasutaja salvestatud otsingud. Neid näeb ja muudab ainult omanik (ka admin mitte).</summary>
public interface ISavedSearchService
{
    Task<List<SavedSearchDto>> GetMineAsync(CurrentUser user);

    Task<SavedSearchDto> GetByIdAsync(int id, CurrentUser user);

    Task<SavedSearchDto> CreateAsync(SavedSearchRequestDto request, CurrentUser user);

    Task<SavedSearchDto> UpdateAsync(int id, SavedSearchRequestDto request, CurrentUser user);

    Task DeleteAsync(int id, CurrentUser user);
}

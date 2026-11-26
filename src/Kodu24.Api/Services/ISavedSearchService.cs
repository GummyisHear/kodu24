using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

/// <summary>
/// Kasutaja salvestatud otsingud. Neid näeb ja muudab ainult omanik (ka admin mitte).
/// Taustateenus loeb kõiki otsinguid ja liigutab nende teavituste kursorit.
/// </summary>
public interface ISavedSearchService
{
    /// <summary>Üürijale tema otsingud, taustateenusele kõigi kasutajate otsingud.</summary>
    Task<List<SavedSearchDto>> GetListAsync(CurrentUser user);

    Task<SavedSearchDto> GetByIdAsync(int id, CurrentUser user);

    Task<SavedSearchDto> CreateAsync(SavedSearchRequestDto request, CurrentUser user);

    Task<SavedSearchDto> UpdateAsync(int id, SavedSearchRequestDto request, CurrentUser user);

    Task DeleteAsync(int id, CurrentUser user);

    /// <summary>
    /// Taustateenus: liigutab <c>LastNotifiedAt</c> kursori edasi. Tagasi seda ei liigutata,
    /// et samade kuulutuste kohta ei tuleks uuesti teavitusi.
    /// </summary>
    Task MarkNotifiedAsync(int id, DateTime notifiedAt);
}

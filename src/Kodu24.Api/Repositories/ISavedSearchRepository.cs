using Kodu24.Api.Entities;

namespace Kodu24.Api.Repositories;

public interface ISavedSearchRepository : IRepository<SavedSearch>
{
    /// <summary>Salvestatud otsing koos linnaosaga või null.</summary>
    Task<SavedSearch?> GetWithDistrictAsync(int id);

    /// <summary>Kasutaja salvestatud otsingud koos linnaosaga, uuemad eespool.</summary>
    Task<List<SavedSearch>> GetByUserAsync(int userId);

    Task<int> CountByUserAsync(int userId);

    /// <summary>Kõigi kasutajate salvestatud otsingud (taustateenusele), vanemad eespool.</summary>
    Task<List<SavedSearch>> GetAllWithDistrictAsync();
}

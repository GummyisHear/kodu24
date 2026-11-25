using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;

namespace Kodu24.Api.Repositories;

public interface IListingRepository : IRepository<Listing>
{
    /// <summary>Kuulutus koos linnaosa, üürileandja ja piltidega.</summary>
    Task<Listing?> GetWithDetailsAsync(int id);

    /// <summary>
    /// Aktiivsed kuulutused filtrite, sortimise ja lehekülgedega (koos linnaosa ja piltidega).
    /// Tagastab ühe lehekülje ja kõigi filtrile vastavate kuulutuste arvu.
    /// </summary>
    Task<(List<Listing> Items, int TotalCount)> SearchAsync(ListingQueryDto query);

    /// <summary>Aktiivsed kuulutused, mille <c>ExpiresAt</c> on möödas.</summary>
    Task<List<Listing>> GetExpiredAsync(DateTime now);
}

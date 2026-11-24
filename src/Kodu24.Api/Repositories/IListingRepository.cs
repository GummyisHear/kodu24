using Kodu24.Api.Entities;

namespace Kodu24.Api.Repositories;

public interface IListingRepository : IRepository<Listing>
{
    /// <summary>Kuulutus koos linnaosa, üürileandja ja piltidega.</summary>
    Task<Listing?> GetWithDetailsAsync(int id);

    /// <summary>Aktiivsed kuulutused, mille <c>ExpiresAt</c> on möödas.</summary>
    Task<List<Listing>> GetExpiredAsync(DateTime now);
}

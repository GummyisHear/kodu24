using Kodu24.Api.Entities;

namespace Kodu24.Api.Repositories;

public interface INotificationRepository : IRepository<Notification>
{
    /// <summary>Kasutaja teavitused, uuemad eespool.</summary>
    Task<List<Notification>> GetByUserAsync(int userId);

    /// <summary>Kuulutused, mille kohta selle otsingu teavitus on juba olemas (duplikaatide vältimiseks).</summary>
    Task<List<int>> GetNotifiedListingIdsAsync(int savedSearchId, IReadOnlyCollection<int> listingIds);
}

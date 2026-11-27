namespace Kodu24.Worker.Api;

/// <summary>Kodu24 API otspunktid, mida taustateenus kasutab. Kõik päringud käivad teenusekonto tokeniga.</summary>
public interface IKodu24ApiClient
{
    /// <summary><c>POST /api/listings/expire</c></summary>
    Task<ExpireResult> ExpireListingsAsync(CancellationToken ct);

    /// <summary><c>GET /api/saved-searches</c> (teenusekonto näeb kõiki)</summary>
    Task<List<SavedSearch>> GetSavedSearchesAsync(CancellationToken ct);

    /// <summary><c>GET /api/listings?…&amp;createdAfter=…&amp;sort=oldest</c>: otsingu filtritega uued kuulutused.</summary>
    Task<ListingPage> GetNewListingsAsync(SavedSearch search, int page, int pageSize, CancellationToken ct);

    /// <summary><c>POST /api/notifications</c></summary>
    Task<CreateNotificationsResult> CreateNotificationsAsync(int savedSearchId, IReadOnlyCollection<int> listingIds, CancellationToken ct);

    /// <summary><c>PATCH /api/saved-searches/{id}/notified</c></summary>
    Task MarkNotifiedAsync(int savedSearchId, DateTime notifiedAt, CancellationToken ct);
}

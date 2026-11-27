using Kodu24.Worker.Api;

namespace Kodu24.Worker;

/// <summary>Ühe käivituse tulemus (logi ja testide jaoks).</summary>
public record HourlyJobResult(int ExpiredCount, int SearchesChecked, int NotificationsCreated, int FailedSearches);

/// <summary>
/// Taustateenuse töö, mida tehakse kord tunnis:
/// 1) aegunud kuulutused kustutatakse, 2) loetakse kõik salvestatud otsingud,
/// 3) iga otsingu kohta otsitakse uued kuulutused (<c>createdAfter = LastNotifiedAt</c>),
/// 4) neist luuakse teavitused ja otsingu kursor liigutatakse edasi.
/// Kõik käib ainult API kaudu.
/// </summary>
public class HourlyJob
{
    /// <summary>API lubab kuni 50 kuulutust lehel ja 50 teavitust korraga.</summary>
    public const int PageSize = 50;

    private readonly IKodu24ApiClient _api;
    private readonly ILogger<HourlyJob> _logger;

    public HourlyJob(IKodu24ApiClient api, ILogger<HourlyJob> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<HourlyJobResult> RunAsync(CancellationToken ct)
    {
        // Enne otsinguid, et aegunud kuulutustest teavitusi ei tuleks.
        var expired = await _api.ExpireListingsAsync(ct);
        _logger.LogInformation("Aegunuks märgiti {Count} kuulutust", expired.ExpiredCount);

        var searches = await _api.GetSavedSearchesAsync(ct);
        var created = 0;
        var failed = 0;

        foreach (var search in searches)
        {
            try
            {
                created += await NotifyAsync(search, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Üks vigane otsing ei tohi teisi peatada. Kursor jäi paigale, seega järgmisel korral proovitakse uuesti.
                failed++;
                _logger.LogError(ex, "Otsingu {SavedSearchId} töötlemine ebaõnnestus", search.Id);
            }
        }

        var result = new HourlyJobResult(expired.ExpiredCount, searches.Count, created, failed);
        _logger.LogInformation(
            "Töö valmis: {Searches} otsingut, {Created} uut teavitust, {Failed} ebaõnnestus",
            result.SearchesChecked, result.NotificationsCreated, result.FailedSearches);
        return result;
    }

    private async Task<int> NotifyAsync(SavedSearch search, CancellationToken ct)
    {
        var created = 0;
        DateTime? newest = null;

        for (var page = 1; ; page++)
        {
            var result = await _api.GetNewListingsAsync(search, page, PageSize, ct);
            if (result.Items.Count > 0)
            {
                var notified = await _api.CreateNotificationsAsync(search.Id, result.Items.Select(l => l.Id).ToList(), ct);
                created += notified.CreatedCount;
                var pageNewest = result.Items.Max(l => l.CreatedAt);
                if (newest is null || pageNewest > newest)
                    newest = pageNewest;
            }

            if (page >= result.TotalPages)
                break;
        }

        // Kursoriks saab viimase teavitatud kuulutuse loomise aja, mitte "praegu": nii ei jää vahele kuulutus,
        // mis lisati töö ajal. Kui uusi kuulutusi polnud, jääb kursor paigale.
        if (newest is DateTime notifiedAt)
        {
            await _api.MarkNotifiedAsync(search.Id, notifiedAt, ct);
            _logger.LogInformation("Otsing {SavedSearchId} \"{Name}\": {Created} uut teavitust", search.Id, search.Name, created);
        }

        return created;
    }
}

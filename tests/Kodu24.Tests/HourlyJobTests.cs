using Kodu24.Worker;
using Kodu24.Worker.Api;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kodu24.Tests;

/// <summary>Taustateenuse töö loogika; API asemel on mock, seega testid ei vaja töötavat API-t.</summary>
public class HourlyJobTests
{
    private static readonly DateTime Start = new(2026, 11, 27, 8, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IKodu24ApiClient> _api = new();

    public HourlyJobTests()
    {
        _api.Setup(a => a.ExpireListingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ExpireResult(1));
        _api.Setup(a => a.CreateNotificationsAsync(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, IReadOnlyCollection<int> ids, CancellationToken _) => new CreateNotificationsResult(ids.Count, 0));
    }

    private HourlyJob CreateJob() => new(_api.Object, NullLogger<HourlyJob>.Instance);

    private static SavedSearch Search(int id) => new(id, 4, $"Otsing {id}", null, 800, 2, 1, Start);

    private static ListingItem Listing(int id, int minutesAfterStart) => new(id, $"Kuulutus {id}", Start.AddMinutes(minutesAfterStart));

    private void SearchesAre(params SavedSearch[] searches) =>
        _api.Setup(a => a.GetSavedSearchesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(searches.ToList());

    private void PagesFor(int searchId, params List<ListingItem>[] pages)
    {
        for (var i = 0; i < pages.Length; i++)
        {
            var page = new ListingPage(pages[i], i + 1, pages.Length);
            _api.Setup(a => a.GetNewListingsAsync(It.Is<SavedSearch>(s => s.Id == searchId), i + 1, HourlyJob.PageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
        }
    }

    [Fact]
    public async Task RunAsync_NotifiesEveryPage_AndMovesCursorToNewestListing()
    {
        SearchesAre(Search(1));
        PagesFor(1, [Listing(10, 5), Listing(11, 30)], [Listing(12, 20)]);

        var result = await CreateJob().RunAsync(CancellationToken.None);

        Assert.Equal(new HourlyJobResult(1, 1, 3, 0), result);
        _api.Verify(a => a.CreateNotificationsAsync(1, It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { 10, 11 })), It.IsAny<CancellationToken>()));
        _api.Verify(a => a.CreateNotificationsAsync(1, It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { 12 })), It.IsAny<CancellationToken>()));
        // Kursor = uusima kuulutuse loomise aeg (mitte "praegu"), et töö ajal lisatud kuulutused vahele ei jääks.
        _api.Verify(a => a.MarkNotifiedAsync(1, Start.AddMinutes(30), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_NoNewListings_DoesNotNotifyOrMoveCursor()
    {
        SearchesAre(Search(1));
        PagesFor(1, []);

        var result = await CreateJob().RunAsync(CancellationToken.None);

        Assert.Equal(0, result.NotificationsCreated);
        _api.Verify(a => a.CreateNotificationsAsync(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        _api.Verify(a => a.MarkNotifiedAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_OneSearchFails_OthersAreStillProcessed()
    {
        SearchesAre(Search(1), Search(2));
        _api.Setup(a => a.GetNewListingsAsync(It.Is<SavedSearch>(s => s.Id == 1), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API maas"));
        PagesFor(2, [Listing(20, 1)]);

        var result = await CreateJob().RunAsync(CancellationToken.None);

        Assert.Equal(1, result.FailedSearches);
        Assert.Equal(1, result.NotificationsCreated);
        // Ebaõnnestunud otsingu kursor jääb paigale, nii et järgmisel korral proovitakse uuesti.
        _api.Verify(a => a.MarkNotifiedAsync(1, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _api.Verify(a => a.MarkNotifiedAsync(2, Start.AddMinutes(1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ExpiresListingsBeforeReadingSearches()
    {
        var calls = new List<string>();
        _api.Setup(a => a.ExpireListingsAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("expire")).ReturnsAsync(new ExpireResult(0));
        _api.Setup(a => a.GetSavedSearchesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("searches")).ReturnsAsync([]);

        await CreateJob().RunAsync(CancellationToken.None);

        Assert.Equal(["expire", "searches"], calls);
    }
}

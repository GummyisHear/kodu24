using Kodu24.Api.Data;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Tests;

/// <summary>
/// Otsingut testitakse päris SQLite'iga (mälus), sest filtrid ja sortimine tõlgitakse SQL-iks
/// ning mock seda ei kontrolliks.
/// </summary>
public class ListingRepositorySearchTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly ListingRepository _repo;
    private readonly District _kesklinn = new() { Name = "Kesklinn" };
    private readonly District _kalamaja = new() { Name = "Kalamaja" };

    public ListingRepositorySearchTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();

        var landlord = new User { Email = "mari@kodu24.ee", FullName = "Mari", PasswordHash = "x", Role = UserRole.Landlord };
        _db.AddRange(
            Listing(landlord, _kesklinn, "Valgusküllane kolmetoaline", 950, 3, 72, daysAgo: 2),
            Listing(landlord, _kesklinn, "Stuudio kesklinnas", 520, 1, 28, daysAgo: 5),
            Listing(landlord, _kalamaja, "Kahetoaline Rõduga", 780, 2, 48, daysAgo: 1),
            Listing(landlord, _kalamaja, "Odav kahetoaline", 450, 2, 50, daysAgo: 10),
            Listing(landlord, _kalamaja, "Kustutatud kuulutus", 300, 2, 40, daysAgo: 3, deleted: true));
        _db.SaveChanges();

        _repo = new ListingRepository(_db);
    }

    private static Listing Listing(User landlord, District district, string title, decimal price, int rooms,
        decimal area, int daysAgo, bool deleted = false) => new()
    {
        Landlord = landlord,
        District = district,
        Title = title,
        Address = "Tänav 1",
        Price = price,
        Rooms = rooms,
        AreaM2 = area,
        CreatedAt = Now.AddDays(-daysAgo),
        ExpiresAt = Now.AddDays(30),
        IsDeleted = deleted,
    };

    [Fact]
    public async Task GetByLandlordAsync_ReturnsOwnListingsNewestFirst_WithoutDeleted()
    {
        var landlordId = _db.Users.Single().Id;

        var mine = await _repo.GetByLandlordAsync(landlordId);

        Assert.Equal(["Kahetoaline Rõduga", "Valgusküllane kolmetoaline", "Stuudio kesklinnas", "Odav kahetoaline"],
            mine.Select(l => l.Title));
        Assert.Empty(await _repo.GetByLandlordAsync(landlordId + 100));
    }

    [Fact]
    public async Task DatesAreReadBackAsUtc()
    {
        // SQLite ei salvesta DateTime.Kind'i; AppDbContext'i teisendus peab selle UTC-ks tagasi panema,
        // muidu läheks JSON-i aeg ilma "Z"-ta ja brauser näitaks valet kellaaega.
        _db.ChangeTracker.Clear();

        var listing = await _repo.GetWithDetailsAsync(1);

        Assert.NotNull(listing);
        Assert.Equal(DateTimeKind.Utc, listing.CreatedAt.Kind);
        Assert.Equal(Now.AddDays(-2), listing.CreatedAt);
    }

    private async Task<List<string>> Titles(ListingQueryDto query) =>
        (await _repo.SearchAsync(query)).Items.Select(l => l.Title).ToList();

    [Fact]
    public async Task Search_NoFilters_ReturnsActiveNewestFirst()
    {
        var (items, total) = await _repo.SearchAsync(new ListingQueryDto());

        Assert.Equal(4, total);
        Assert.Equal(new[] { "Kahetoaline Rõduga", "Valgusküllane kolmetoaline", "Stuudio kesklinnas", "Odav kahetoaline" },
            items.Select(l => l.Title));
        Assert.All(items, l => Assert.NotNull(l.District));
    }

    [Fact]
    public async Task Search_PriceRangeAndRooms_CombinesFilters()
    {
        var titles = await Titles(new ListingQueryDto { MinPrice = 400, MaxPrice = 800, Rooms = 2 });

        Assert.Equal(new[] { "Kahetoaline Rõduga", "Odav kahetoaline" }, titles);
    }

    [Fact]
    public async Task Search_ByDistrict()
    {
        var titles = await Titles(new ListingQueryDto { DistrictId = _kesklinn.Id });

        Assert.Equal(2, titles.Count);
        Assert.All(titles, t => Assert.DoesNotContain("kahetoaline", t, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Search_TextIsCaseInsensitive()
    {
        var titles = await Titles(new ListingQueryDto { Search = "STUUDIO" });

        Assert.Equal(new[] { "Stuudio kesklinnas" }, titles);
    }

    [Theory]
    [InlineData(ListingSort.PriceAsc, "Odav kahetoaline")]
    [InlineData(ListingSort.PriceDesc, "Valgusküllane kolmetoaline")]
    [InlineData(ListingSort.AreaDesc, "Valgusküllane kolmetoaline")]
    [InlineData(ListingSort.PricePerM2Asc, "Odav kahetoaline")]
    [InlineData(ListingSort.Oldest, "Odav kahetoaline")]
    public async Task Search_Sorting_PutsExpectedListingFirst(string sort, string expectedFirst)
    {
        var titles = await Titles(new ListingQueryDto { Sort = sort });

        Assert.Equal(expectedFirst, titles[0]);
    }

    [Fact]
    public async Task Search_Paging_ReturnsRequestedPageAndFullTotal()
    {
        var (items, total) = await _repo.SearchAsync(new ListingQueryDto { Page = 2, PageSize = 3 });

        Assert.Equal(4, total);
        Assert.Equal(new[] { "Odav kahetoaline" }, items.Select(l => l.Title));
    }

    [Fact]
    public async Task Search_CreatedAfter_ReturnsOnlyNewer()
    {
        var titles = await Titles(new ListingQueryDto { CreatedAfter = Now.AddDays(-3) });

        Assert.Equal(new[] { "Kahetoaline Rõduga", "Valgusküllane kolmetoaline" }, titles);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}

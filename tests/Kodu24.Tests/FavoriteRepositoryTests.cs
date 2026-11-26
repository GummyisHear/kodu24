using Kodu24.Api.Data;
using Kodu24.Api.Entities;
using Kodu24.Api.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Tests;

/// <summary>Lemmikute päringud päris SQLite'iga (mälus): kontrollib päringufiltrit ja unikaalset indeksit.</summary>
public class FavoriteRepositoryTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 26, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly FavoriteRepository _repo;
    private readonly User _liis = new() { Email = "liis@kodu24.ee", FullName = "Liis", PasswordHash = "x", Role = UserRole.Seeker };
    private readonly User _other = new() { Email = "teine@kodu24.ee", FullName = "Teine", PasswordHash = "x", Role = UserRole.Seeker };

    public FavoriteRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();

        var landlord = new User { Email = "mari@kodu24.ee", FullName = "Mari", PasswordHash = "x", Role = UserRole.Landlord };
        var district = new District { Name = "Kesklinn" };
        Listing Listing(string title, bool deleted = false) => new()
        {
            Landlord = landlord, District = district, Title = title, Address = "Tänav 1",
            Price = 500, Rooms = 2, AreaM2 = 40, CreatedAt = Now, ExpiresAt = Now.AddDays(30), IsDeleted = deleted,
        };

        var first = Listing("Esimene");
        var second = Listing("Teine");
        var deleted = Listing("Kustutatud", deleted: true);
        _db.AddRange(
            new Favorite { User = _liis, Listing = first, CreatedAt = Now.AddHours(-2) },
            new Favorite { User = _liis, Listing = second, CreatedAt = Now.AddHours(-1) },
            new Favorite { User = _liis, Listing = deleted, CreatedAt = Now },
            new Favorite { User = _other, Listing = first, CreatedAt = Now });
        _db.SaveChanges();
        _db.ChangeTracker.Clear();

        _repo = new FavoriteRepository(_db);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsOwnNewestFirst_WithoutDeletedListings()
    {
        var favorites = await _repo.GetByUserAsync(_liis.Id);

        Assert.Equal(["Teine", "Esimene"], favorites.Select(f => f.Listing.Title));
        Assert.All(favorites, f => Assert.NotNull(f.Listing.District));
    }

    [Fact]
    public async Task GetListingIdsAsync_SkipsDeletedListings()
    {
        var ids = await _repo.GetListingIdsAsync(_liis.Id);

        Assert.Equal(2, ids.Count);
    }

    [Fact]
    public async Task SameListingTwice_IsRejectedByUniqueIndex()
    {
        var listingId = (await _repo.GetListingIdsAsync(_liis.Id)).First();

        await _repo.AddAsync(new Favorite { UserId = _liis.Id, ListingId = listingId, CreatedAt = Now });

        await Assert.ThrowsAsync<DbUpdateException>(() => _repo.SaveChangesAsync());
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}

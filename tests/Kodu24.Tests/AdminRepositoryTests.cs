using Kodu24.Api.Data;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Tests;

/// <summary>Halduse päringud päris SQLite'iga: admin peab nägema ka kustutatud kuulutusi (IgnoreQueryFilters).</summary>
public class AdminRepositoryTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 27, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly AdminRepository _repo;
    private readonly User _mari = new() { Email = "mari@kodu24.ee", FullName = "Mari Tamm", PasswordHash = "x", Role = UserRole.Landlord, CreatedAt = Now.AddDays(-3) };
    private readonly User _liis = new() { Email = "liis@kodu24.ee", FullName = "Liis Mägi", PasswordHash = "x", Role = UserRole.Seeker, CreatedAt = Now.AddDays(-1) };

    public AdminRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();

        var district = new District { Name = "Kesklinn" };
        Listing Listing(string title, int daysAgo, bool deleted = false) => new()
        {
            Landlord = _mari, District = district, Title = title, Address = "Tänav 1", Price = 500, Rooms = 2, AreaM2 = 40,
            CreatedAt = Now.AddDays(-daysAgo), ExpiresAt = Now.AddDays(30), IsDeleted = deleted, DeletedAt = deleted ? Now : null,
        };
        _db.AddRange(_liis, Listing("Aktiivne", 2), Listing("Kustutatud", 1, deleted: true));
        _db.SaveChanges();
        _db.ChangeTracker.Clear();

        _repo = new AdminRepository(_db);
    }

    [Theory]
    [InlineData(AdminListingQueryDto.All, new[] { "Kustutatud", "Aktiivne" })]
    [InlineData(AdminListingQueryDto.Active, new[] { "Aktiivne" })]
    [InlineData(AdminListingQueryDto.Deleted, new[] { "Kustutatud" })]
    public async Task SearchListingsAsync_FiltersByStatus_IncludingDeleted(string status, string[] expected)
    {
        var (items, total) = await _repo.SearchListingsAsync(new AdminListingQueryDto { Status = status });

        Assert.Equal(expected, items.Select(l => l.Title));
        Assert.Equal(expected.Length, total);
        Assert.All(items, l => Assert.Equal("mari@kodu24.ee", l.Landlord.Email));
    }

    [Fact]
    public async Task SearchUsersAsync_CountsOnlyActiveListings_AndFiltersByRole()
    {
        var (all, total) = await _repo.SearchUsersAsync(new AdminUserQueryDto());
        var (landlords, _) = await _repo.SearchUsersAsync(new AdminUserQueryDto { Role = "Landlord" });

        Assert.Equal(2, total);
        Assert.Equal(["liis@kodu24.ee", "mari@kodu24.ee"], all.Select(x => x.User.Email)); // uuemad eespool
        Assert.Equal(1, Assert.Single(landlords).ActiveListingCount);
    }

    [Fact]
    public async Task GetStatsAsync_CountsActiveAndDeletedListingsSeparately()
    {
        var stats = await _repo.GetStatsAsync();

        Assert.Equal(1, stats.ActiveListings);
        Assert.Equal(1, stats.DeletedListings);
        Assert.Equal(1, stats.UsersByRole["Landlord"]);
        Assert.Equal(0, stats.UsersByRole["Admin"]); // puuduvad rollid on samuti olemas, väärtusega 0
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}

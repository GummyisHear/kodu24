using Kodu24.Api.Auth;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kodu24.Tests;

public class FavoriteServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly CurrentUser Seeker = new(5, UserRole.Seeker);

    private readonly Mock<IFavoriteRepository> _favorites = new();
    private readonly Mock<IListingRepository> _listings = new();
    private readonly Listing _listing = new()
    {
        Id = 10,
        LandlordId = 1,
        Title = "Kahetoaline",
        District = new District { Name = "Kalamaja" },
        Price = 700,
        AreaM2 = 50,
    };

    public FavoriteServiceTests()
    {
        _listings.Setup(l => l.GetWithDetailsAsync(10)).ReturnsAsync(_listing);
    }

    private FavoriteService CreateService()
    {
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Returns(Now);
        return new FavoriteService(_favorites.Object, _listings.Object, time.Object, NullLogger<FavoriteService>.Instance);
    }

    [Fact]
    public async Task AddAsync_NewFavorite_SavesForTokenUserWithServerTime()
    {
        Favorite? saved = null;
        _favorites.Setup(f => f.AddAsync(It.IsAny<Favorite>())).Callback<Favorite>(f => saved = f);

        var (dto, created) = await CreateService().AddAsync(10, Seeker);

        Assert.True(created);
        Assert.NotNull(saved);
        Assert.Equal(Seeker.Id, saved.UserId);
        Assert.Equal(10, saved.ListingId);
        Assert.Equal(Now.UtcDateTime, saved.CreatedAt);
        Assert.Equal("Kalamaja", dto.Listing.DistrictName);
        _favorites.Verify(f => f.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_AlreadyFavorite_ReturnsExistingWithoutSaving()
    {
        var existing = new Favorite { Id = 3, UserId = Seeker.Id, ListingId = 10, CreatedAt = Now.UtcDateTime.AddDays(-1) };
        _favorites.Setup(f => f.FindAsync(Seeker.Id, 10)).ReturnsAsync(existing);

        var (dto, created) = await CreateService().AddAsync(10, Seeker);

        Assert.False(created);
        Assert.Equal(existing.CreatedAt, dto.CreatedAt);
        _favorites.Verify(f => f.AddAsync(It.IsAny<Favorite>()), Times.Never);
        _favorites.Verify(f => f.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AddAsync_MissingListing_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().AddAsync(99, Seeker));

        _favorites.Verify(f => f.AddAsync(It.IsAny<Favorite>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_OwnFavorite_RemovesAndSaves()
    {
        var favorite = new Favorite { Id = 3, UserId = Seeker.Id, ListingId = 10 };
        _favorites.Setup(f => f.FindAsync(Seeker.Id, 10)).ReturnsAsync(favorite);

        await CreateService().RemoveAsync(10, Seeker);

        _favorites.Verify(f => f.Remove(favorite), Times.Once);
        _favorites.Verify(f => f.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_NotFavorite_ThrowsNotFound()
    {
        // FindAsync otsib ainult sisselogitud kasutaja lemmikute seast, nii et teise kasutaja lemmikut kustutada ei saa.
        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().RemoveAsync(10, Seeker));

        _favorites.Verify(f => f.SaveChangesAsync(), Times.Never);
    }
}

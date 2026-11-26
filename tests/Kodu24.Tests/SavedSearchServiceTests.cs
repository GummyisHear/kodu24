using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kodu24.Tests;

public class SavedSearchServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly CurrentUser Seeker = new(5, UserRole.Seeker);
    private static readonly CurrentUser OtherSeeker = new(6, UserRole.Seeker);

    private readonly Mock<ISavedSearchRepository> _searches = new();
    private readonly Mock<IRepository<District>> _districts = new();
    private readonly District _kalamaja = new() { Id = 2, Name = "Kalamaja" };

    public SavedSearchServiceTests()
    {
        _districts.Setup(d => d.GetByIdAsync(2)).ReturnsAsync(_kalamaja);
    }

    private SavedSearchService CreateService()
    {
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Returns(Now);
        return new SavedSearchService(_searches.Object, _districts.Object, time.Object, NullLogger<SavedSearchService>.Instance);
    }

    private SavedSearch ExistingSearch(int ownerId) => new()
    {
        Id = 7,
        UserId = ownerId,
        Name = "Vana",
        Rooms = 3,
        DistrictId = 2,
        District = _kalamaja,
        LastNotifiedAt = Now.UtcDateTime.AddDays(-3),
    };

    [Fact]
    public async Task CreateAsync_SavesForTokenUser_NotifiesOnlyAboutNewListings()
    {
        SavedSearch? saved = null;
        _searches.Setup(s => s.AddAsync(It.IsAny<SavedSearch>())).Callback<SavedSearch>(s => saved = s);

        var dto = await CreateService().CreateAsync(
            new SavedSearchRequestDto { Name = "  Kalamaja  ", MaxPrice = 800, DistrictId = 2 }, Seeker);

        Assert.NotNull(saved);
        Assert.Equal(Seeker.Id, saved.UserId);
        Assert.Equal("Kalamaja", saved.Name);
        // LastNotifiedAt = loomise aeg, et taustateenus ei saadaks teavitusi vanade kuulutuste kohta.
        Assert.Equal(Now.UtcDateTime, saved.LastNotifiedAt);
        Assert.Equal("Kalamaja", dto.DistrictName);
        Assert.Null(dto.Rooms);
        _searches.Verify(s => s.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_MinPriceAboveMaxPrice_ThrowsValidationError()
    {
        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() =>
            CreateService().CreateAsync(new SavedSearchRequestDto { Name = "Vigane", MinPrice = 900, MaxPrice = 500 }, Seeker));

        Assert.True(ex.Errors.ContainsKey(nameof(SavedSearchRequestDto.MinPrice)));
        _searches.Verify(s => s.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_UnknownDistrict_ThrowsValidationError()
    {
        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() =>
            CreateService().CreateAsync(new SavedSearchRequestDto { Name = "Tundmatu", DistrictId = 99 }, Seeker));

        Assert.True(ex.Errors.ContainsKey(nameof(SavedSearchRequestDto.DistrictId)));
    }

    [Fact]
    public async Task CreateAsync_LimitReached_ThrowsValidationError()
    {
        _searches.Setup(s => s.CountByUserAsync(Seeker.Id)).ReturnsAsync(SavedSearchService.MaxPerUser);

        await Assert.ThrowsAsync<ValidationFailedException>(() =>
            CreateService().CreateAsync(new SavedSearchRequestDto { Name = "Liiga palju" }, Seeker));

        _searches.Verify(s => s.AddAsync(It.IsAny<SavedSearch>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesFilters_KeepsLastNotifiedAt()
    {
        var search = ExistingSearch(Seeker.Id);
        var lastNotified = search.LastNotifiedAt;
        _searches.Setup(s => s.GetWithDistrictAsync(7)).ReturnsAsync(search);

        var dto = await CreateService().UpdateAsync(7, new SavedSearchRequestDto { Name = "Uus", MaxPrice = 900 }, Seeker);

        Assert.Equal("Uus", dto.Name);
        Assert.Null(dto.Rooms);         // puuduv filter = "ükskõik"
        Assert.Null(dto.DistrictId);
        Assert.Equal(lastNotified, search.LastNotifiedAt);
        _searches.Verify(s => s.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_OtherUsersSearch_ThrowsForbidden()
    {
        var search = ExistingSearch(Seeker.Id);
        _searches.Setup(s => s.GetWithDistrictAsync(7)).ReturnsAsync(search);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            CreateService().UpdateAsync(7, new SavedSearchRequestDto { Name = "Häkitud" }, OtherSeeker));

        Assert.Equal("Vana", search.Name);
        _searches.Verify(s => s.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_OwnSearch_Removes()
    {
        var search = ExistingSearch(Seeker.Id);
        _searches.Setup(s => s.GetWithDistrictAsync(7)).ReturnsAsync(search);

        await CreateService().DeleteAsync(7, Seeker);

        _searches.Verify(s => s.Remove(search), Times.Once);
        _searches.Verify(s => s.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Missing_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetByIdAsync(404, Seeker));
    }
}

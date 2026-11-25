using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kodu24.Tests;

public class ListingServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);
    private static readonly CurrentUser Owner = new(1, UserRole.Landlord);
    private static readonly CurrentUser OtherLandlord = new(2, UserRole.Landlord);

    private readonly Mock<IListingRepository> _listings = new();
    private readonly Mock<IRepository<District>> _districts = new();

    private ListingService CreateService()
    {
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Returns(Now);
        return new ListingService(_listings.Object, _districts.Object, time.Object, NullLogger<ListingService>.Instance);
    }

    private static ListingRequestDto Request(int districtId = 1) => new()
    {
        Title = "  Hubane kahetoaline  ",
        Address = "Tartu mnt 1",
        DistrictId = districtId,
        Price = 650,
        Rooms = 2,
        AreaM2 = 48,
        Latitude = 59.43,
        Longitude = 24.76,
    };

    private static Listing ExistingListing() => new()
    {
        Id = 10,
        LandlordId = Owner.Id,
        Landlord = new User { Id = Owner.Id, FullName = "Mari Tamm" },
        DistrictId = 1,
        District = new District { Id = 1, Name = "Kesklinn" },
        Title = "Vana pealkiri",
        Address = "Vana 1",
    };

    [Fact]
    public async Task CreateAsync_SetsOwnerAndExpiryFromServer()
    {
        var district = new District { Id = 1, Name = "Kesklinn" };
        _districts.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(district);
        Listing? saved = null;
        _listings.Setup(l => l.AddAsync(It.IsAny<Listing>())).Callback<Listing>(l =>
        {
            saved = l;
            l.District = district;
            l.Landlord = new User { Id = Owner.Id, FullName = "Mari Tamm" };
        });
        _listings.Setup(l => l.GetWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(() => saved);

        var dto = await CreateService().CreateAsync(Request(), Owner);

        Assert.NotNull(saved);
        Assert.Equal(Owner.Id, saved.LandlordId);
        Assert.Equal(Now.UtcDateTime, saved.CreatedAt);
        Assert.Equal(Now.UtcDateTime.AddDays(ListingService.ListingLifetimeDays), saved.ExpiresAt);
        Assert.Equal("Hubane kahetoaline", dto.Title);
        _listings.Verify(l => l.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_UnknownDistrict_ThrowsValidationError()
    {
        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() => CreateService().CreateAsync(Request(99), Owner));

        Assert.True(ex.Errors.ContainsKey("DistrictId"));
        _listings.Verify(l => l.AddAsync(It.IsAny<Listing>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_OtherLandlord_ThrowsForbiddenAndDoesNotSave()
    {
        var listing = ExistingListing();
        _listings.Setup(l => l.GetWithDetailsAsync(10)).ReturnsAsync(listing);

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateService().UpdateAsync(10, Request(), OtherLandlord));

        Assert.Equal("Vana pealkiri", listing.Title);
        _listings.Verify(l => l.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Owner_AppliesChanges()
    {
        var listing = ExistingListing();
        _listings.Setup(l => l.GetWithDetailsAsync(10)).ReturnsAsync(listing);

        var dto = await CreateService().UpdateAsync(10, Request(), Owner);

        Assert.Equal("Hubane kahetoaline", listing.Title);
        Assert.Equal(650, dto.Price);
        _listings.Verify(l => l.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Owner_SoftDeletes()
    {
        var listing = ExistingListing();
        _listings.Setup(l => l.GetWithDetailsAsync(10)).ReturnsAsync(listing);

        await CreateService().DeleteAsync(10, Owner);

        Assert.True(listing.IsDeleted);
        Assert.Equal(Now.UtcDateTime, listing.DeletedAt);
        _listings.Verify(l => l.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_MinPriceAboveMax_ThrowsValidationError()
    {
        var query = new ListingQueryDto { MinPrice = 900, MaxPrice = 500 };

        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() => CreateService().SearchAsync(query));

        Assert.True(ex.Errors.ContainsKey("MinPrice"));
        _listings.Verify(l => l.SearchAsync(It.IsAny<ListingQueryDto>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_MapsItemsAndPaging()
    {
        var listing = ExistingListing();
        listing.Price = 600;
        listing.AreaM2 = 50;
        _listings.Setup(l => l.SearchAsync(It.IsAny<ListingQueryDto>())).ReturnsAsync((new List<Listing> { listing }, 25));

        var result = await CreateService().SearchAsync(new ListingQueryDto { Page = 2, PageSize = 10 });

        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(12m, Assert.Single(result.Items).PricePerM2);
    }

    [Fact]
    public async Task GetByIdAsync_Missing_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetByIdAsync(404));
    }

    [Fact]
    public async Task ExpireListingsAsync_MarksExpiredListingsAsDeleted()
    {
        var expired = new List<Listing>
        {
            new() { Id = 1, ExpiresAt = Now.UtcDateTime.AddDays(-1) },
            new() { Id = 2, ExpiresAt = Now.UtcDateTime.AddHours(-1) },
        };
        _listings.Setup(r => r.GetExpiredAsync(Now.UtcDateTime)).ReturnsAsync(expired);

        var count = await CreateService().ExpireListingsAsync();

        Assert.Equal(2, count);
        Assert.All(expired, l =>
        {
            Assert.True(l.IsDeleted);
            Assert.Equal(Now.UtcDateTime, l.DeletedAt);
        });
        _listings.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExpireListingsAsync_NothingExpired_DoesNotSave()
    {
        _listings.Setup(r => r.GetExpiredAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<Listing>());

        var count = await CreateService().ExpireListingsAsync();

        Assert.Equal(0, count);
        _listings.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}

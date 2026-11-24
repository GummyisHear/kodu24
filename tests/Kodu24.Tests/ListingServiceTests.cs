using Kodu24.Api.Entities;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kodu24.Tests;

public class ListingServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);

    private static Mock<TimeProvider> FixedTime()
    {
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Returns(Now);
        return time;
    }

    private static ListingService CreateService(Mock<IListingRepository> repo) =>
        new(repo.Object, FixedTime().Object, NullLogger<ListingService>.Instance);

    [Fact]
    public async Task ExpireListingsAsync_MarksExpiredListingsAsDeleted()
    {
        var expired = new List<Listing>
        {
            new() { Id = 1, ExpiresAt = Now.UtcDateTime.AddDays(-1) },
            new() { Id = 2, ExpiresAt = Now.UtcDateTime.AddHours(-1) },
        };
        var repo = new Mock<IListingRepository>();
        repo.Setup(r => r.GetExpiredAsync(Now.UtcDateTime)).ReturnsAsync(expired);

        var service = CreateService(repo);

        var count = await service.ExpireListingsAsync();

        Assert.Equal(2, count);
        Assert.All(expired, l =>
        {
            Assert.True(l.IsDeleted);
            Assert.Equal(Now.UtcDateTime, l.DeletedAt);
        });
        repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExpireListingsAsync_NothingExpired_DoesNotSave()
    {
        var repo = new Mock<IListingRepository>();
        repo.Setup(r => r.GetExpiredAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<Listing>());

        var service = CreateService(repo);

        var count = await service.ExpireListingsAsync();

        Assert.Equal(0, count);
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}

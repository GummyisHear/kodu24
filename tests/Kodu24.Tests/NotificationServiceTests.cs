using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kodu24.Tests;

public class NotificationServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly CurrentUser Seeker = new(5, UserRole.Seeker);
    private static readonly CurrentUser OtherSeeker = new(6, UserRole.Seeker);

    private readonly Mock<INotificationRepository> _notifications = new();
    private readonly Mock<ISavedSearchRepository> _searches = new();
    private readonly Mock<IListingRepository> _listings = new();
    private readonly List<Notification> _added = [];

    public NotificationServiceTests()
    {
        _searches.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new SavedSearch { Id = 1, UserId = Seeker.Id, Name = "Kalamaja" });
        _notifications.Setup(n => n.AddAsync(It.IsAny<Notification>())).Callback<Notification>(_added.Add);
        _notifications.Setup(n => n.GetNotifiedListingIdsAsync(1, It.IsAny<IReadOnlyCollection<int>>())).ReturnsAsync([]);
    }

    private NotificationService CreateService()
    {
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Returns(Now);
        return new NotificationService(_notifications.Object, _searches.Object, _listings.Object, time.Object,
            NullLogger<NotificationService>.Instance);
    }

    private void ListingsExist(params int[] ids) =>
        _listings.Setup(l => l.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>()))
            .ReturnsAsync(ids.Select(id => new Listing { Id = id, Title = $"Kuulutus {id}" }).ToList());

    private static CreateNotificationsRequestDto Request(params int[] listingIds) =>
        new() { SavedSearchId = 1, ListingIds = [.. listingIds] };

    [Fact]
    public async Task CreateForSavedSearchAsync_CreatesNotificationForSearchOwner()
    {
        ListingsExist(3, 4);

        var result = await CreateService().CreateForSavedSearchAsync(Request(3, 4));

        Assert.Equal(new CreateNotificationsResultDto(2, 0), result);
        Assert.All(_added, n =>
        {
            Assert.Equal(Seeker.Id, n.UserId);
            Assert.Equal(1, n.SavedSearchId);
            Assert.Equal(Now.UtcDateTime, n.CreatedAt);
        });
        Assert.Equal("Uus kuulutus otsingule \"Kalamaja\": Kuulutus 3", _added[0].Message);
        _notifications.Verify(n => n.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateForSavedSearchAsync_SkipsMissingAndAlreadyNotifiedListings()
    {
        ListingsExist(3, 4);   // 99 ei ole olemas (või on kustutatud)
        _notifications.Setup(n => n.GetNotifiedListingIdsAsync(1, It.IsAny<IReadOnlyCollection<int>>())).ReturnsAsync([3]);

        var result = await CreateService().CreateForSavedSearchAsync(Request(3, 4, 99, 4));

        Assert.Equal(new CreateNotificationsResultDto(1, 2), result);
        Assert.Equal(4, Assert.Single(_added).ListingId);
    }

    [Fact]
    public async Task CreateForSavedSearchAsync_NothingNew_DoesNotSave()
    {
        ListingsExist();

        var result = await CreateService().CreateForSavedSearchAsync(Request(99));

        Assert.Equal(0, result.CreatedCount);
        _notifications.Verify(n => n.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateForSavedSearchAsync_MissingSavedSearch_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().CreateForSavedSearchAsync(new CreateNotificationsRequestDto { SavedSearchId = 42, ListingIds = [3] }));
    }

    [Fact]
    public async Task MarkAsReadAsync_OtherUsersNotification_ThrowsForbidden()
    {
        var notification = new Notification { Id = 8, UserId = Seeker.Id };
        _notifications.Setup(n => n.GetByIdAsync(8)).ReturnsAsync(notification);

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateService().MarkAsReadAsync(8, OtherSeeker));
        Assert.False(notification.IsRead);

        await CreateService().MarkAsReadAsync(8, Seeker);
        Assert.True(notification.IsRead);
    }
}

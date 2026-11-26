using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Mapping;
using Kodu24.Api.Repositories;

namespace Kodu24.Api.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;
    private readonly ISavedSearchRepository _searches;
    private readonly IListingRepository _listings;
    private readonly TimeProvider _time;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notifications,
        ISavedSearchRepository searches,
        IListingRepository listings,
        TimeProvider time,
        ILogger<NotificationService> logger)
    {
        _notifications = notifications;
        _searches = searches;
        _listings = listings;
        _time = time;
        _logger = logger;
    }

    public async Task<CreateNotificationsResultDto> CreateForSavedSearchAsync(CreateNotificationsRequestDto request)
    {
        var search = await _searches.GetByIdAsync(request.SavedSearchId)
                     ?? throw new NotFoundException($"Salvestatud otsingut {request.SavedSearchId} ei leitud.");

        var requestedIds = request.ListingIds.Distinct().ToList();
        var listings = await _listings.GetByIdsAsync(requestedIds);
        // Kui taustateenus saadab sama kuulutuse uuesti (nt pärast katkestust), teist teavitust ei teki.
        var alreadyNotified = (await _notifications.GetNotifiedListingIdsAsync(search.Id, requestedIds)).ToHashSet();

        var now = _time.GetUtcNow().UtcDateTime;
        var created = 0;
        foreach (var listing in listings.Where(l => !alreadyNotified.Contains(l.Id)).OrderBy(l => l.CreatedAt))
        {
            await _notifications.AddAsync(new Notification
            {
                UserId = search.UserId,
                SavedSearchId = search.Id,
                ListingId = listing.Id,
                Message = $"Uus kuulutus otsingule \"{search.Name}\": {listing.Title}",
                CreatedAt = now,
            });
            created++;
        }

        if (created > 0)
            await _notifications.SaveChangesAsync();

        _logger.LogInformation("Otsingule {SavedSearchId} loodi {Created} teavitust ({Skipped} jäeti vahele)",
            search.Id, created, requestedIds.Count - created);
        return new CreateNotificationsResultDto(created, requestedIds.Count - created);
    }

    public async Task<List<NotificationDto>> GetMineAsync(CurrentUser user)
    {
        var notifications = await _notifications.GetByUserAsync(user.Id);
        return notifications.Select(n => n.ToDto()).ToList();
    }

    public async Task MarkAsReadAsync(int id, CurrentUser user)
    {
        var notification = await _notifications.GetByIdAsync(id)
                           ?? throw new NotFoundException($"Teavitust {id} ei leitud.");
        user.EnsureOwner(notification.UserId);

        if (notification.IsRead)
            return;

        notification.IsRead = true;
        await _notifications.SaveChangesAsync();
    }
}

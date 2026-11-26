using Kodu24.Api.Data;
using Kodu24.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Api.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext db) : base(db) { }

    public Task<List<Notification>> GetByUserAsync(int userId) =>
        Set.AsNoTracking()
           .Where(n => n.UserId == userId)
           .OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id)
           .ToListAsync();

    public Task<List<int>> GetNotifiedListingIdsAsync(int savedSearchId, IReadOnlyCollection<int> listingIds) =>
        Set.Where(n => n.SavedSearchId == savedSearchId && listingIds.Contains(n.ListingId))
           .Select(n => n.ListingId)
           .ToListAsync();
}

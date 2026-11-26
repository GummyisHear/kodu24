using Kodu24.Api.Data;
using Kodu24.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Api.Repositories;

public class FavoriteRepository : Repository<Favorite>, IFavoriteRepository
{
    public FavoriteRepository(AppDbContext db) : base(db) { }

    public Task<Favorite?> FindAsync(int userId, int listingId) =>
        Set.FirstOrDefaultAsync(f => f.UserId == userId && f.ListingId == listingId);

    public Task<List<Favorite>> GetByUserAsync(int userId) =>
        Set.AsNoTracking()
           .Include(f => f.Listing).ThenInclude(l => l.District)
           .Include(f => f.Listing).ThenInclude(l => l.Images)
           .Where(f => f.UserId == userId)
           .OrderByDescending(f => f.CreatedAt).ThenByDescending(f => f.Id)
           .AsSplitQuery()
           .ToListAsync();

    public Task<List<int>> GetListingIdsAsync(int userId) =>
        Set.Where(f => f.UserId == userId).Select(f => f.ListingId).ToListAsync();
}

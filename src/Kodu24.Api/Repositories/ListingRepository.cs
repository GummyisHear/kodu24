using Kodu24.Api.Data;
using Kodu24.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Api.Repositories;

public class ListingRepository : Repository<Listing>, IListingRepository
{
    public ListingRepository(AppDbContext db) : base(db) { }

    public Task<Listing?> GetWithDetailsAsync(int id) =>
        Set.Include(l => l.District)
           .Include(l => l.Landlord)
           .Include(l => l.Images)
           .FirstOrDefaultAsync(l => l.Id == id);

    // Kustutatud kuulutused on globaalse päringufiltriga juba välja jäetud.
    public Task<List<Listing>> GetExpiredAsync(DateTime now) =>
        Set.Where(l => l.ExpiresAt <= now).ToListAsync();
}

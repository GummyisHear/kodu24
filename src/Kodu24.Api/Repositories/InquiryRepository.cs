using Kodu24.Api.Data;
using Kodu24.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Api.Repositories;

public class InquiryRepository : Repository<Inquiry>, IInquiryRepository
{
    public InquiryRepository(AppDbContext db) : base(db) { }

    public Task<Inquiry?> GetWithDetailsAsync(int id) =>
        Set.Include(i => i.Listing)
           .Include(i => i.Seeker)
           .FirstOrDefaultAsync(i => i.Id == id);

    public Task<List<Inquiry>> GetReceivedAsync(int? landlordId)
    {
        var query = Set.AsNoTracking().Include(i => i.Listing).Include(i => i.Seeker).AsQueryable();
        if (landlordId is int id)
            query = query.Where(i => i.Listing.LandlordId == id);

        return query.OrderByDescending(i => i.CreatedAt).ThenByDescending(i => i.Id).ToListAsync();
    }
}

using Kodu24.Api.Data;
using Kodu24.Api.Dtos;
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

    public async Task<(List<Listing> Items, int TotalCount)> SearchAsync(ListingQueryDto q)
    {
        // Kustutatud kuulutused jäävad globaalse päringufiltri tõttu automaatselt välja.
        var query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            // LIKE on SQLite'is tõstutundetu (ladina tähtede puhul); % ja _ tuleb varjestada.
            var pattern = "%" + EscapeLike(q.Search.Trim()) + "%";
            query = query.Where(l =>
                EF.Functions.Like(l.Title, pattern, "\\") ||
                EF.Functions.Like(l.Address, pattern, "\\") ||
                EF.Functions.Like(l.Description, pattern, "\\"));
        }

        if (q.MinPrice is decimal min)
            query = query.Where(l => l.Price >= min);
        if (q.MaxPrice is decimal max)
            query = query.Where(l => l.Price <= max);
        if (q.Rooms is int rooms)
            query = query.Where(l => l.Rooms == rooms);
        if (q.DistrictId is int districtId)
            query = query.Where(l => l.DistrictId == districtId);
        if (q.CreatedAfter is DateTime after)
        {
            var afterUtc = ToUtc(after);
            query = query.Where(l => l.CreatedAt > afterUtc);
        }

        var total = await query.CountAsync();

        // SQLite ei oska decimal väärtusi sortida, seepärast sorditakse double'iks teisendatult.
        // ThenBy(Id) hoiab järjekorra stabiilsena, et lehekülgedel ei korduks kuulutusi.
        query = q.Sort switch
        {
            ListingSort.Oldest => query.OrderBy(l => l.CreatedAt).ThenBy(l => l.Id),
            ListingSort.PriceAsc => query.OrderBy(l => (double)l.Price).ThenBy(l => l.Id),
            ListingSort.PriceDesc => query.OrderByDescending(l => (double)l.Price).ThenBy(l => l.Id),
            ListingSort.AreaDesc => query.OrderByDescending(l => (double)l.AreaM2).ThenBy(l => l.Id),
            ListingSort.PricePerM2Asc => query.OrderBy(l => (double)l.Price / (double)l.AreaM2).ThenBy(l => l.Id),
            _ => query.OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id),
        };

        var items = await query
            .Include(l => l.District)
            .Include(l => l.Images)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .AsSplitQuery()
            .ToListAsync();

        return (items, total);
    }

    public Task<List<Listing>> GetByLandlordAsync(int landlordId) =>
        Set.AsNoTracking()
           .Where(l => l.LandlordId == landlordId)
           .Include(l => l.District)
           .Include(l => l.Images)
           .Include(l => l.Inquiries)
           .OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id)
           .AsSplitQuery()
           .ToListAsync();

    // Kustutatud kuulutused on globaalse päringufiltriga juba välja jäetud.
    public Task<List<Listing>> GetExpiredAsync(DateTime now) =>
        Set.Where(l => l.ExpiresAt <= now).ToListAsync();

    public Task<List<Listing>> GetByIdsAsync(IReadOnlyCollection<int> ids) =>
        Set.AsNoTracking().Where(l => ids.Contains(l.Id)).ToListAsync();

    // Keskmine €/m² = kuulutuste €/m² keskmine (iga kuulutus loeb võrdselt).
    // SQLite ei oska decimal'iga arvutada (AVG), seega double'i teisendused nagu sortimiselgi.
    public Task<List<DistrictPriceStats>> GetDistrictPriceStatsAsync(DateTime now) =>
        Set.Where(l => l.ExpiresAt > now)
           .GroupBy(l => l.DistrictId)
           .Select(g => new DistrictPriceStats(
               g.Key,
               g.Count(),
               g.Average(l => (double)l.Price),
               g.Average(l => (double)l.Price / (double)l.AreaM2)))
           .ToListAsync();

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    // Päringustringi aeg võib tulla kohaliku ajana (nt "...Z" teisendatakse Local'iks); andmebaasis on UTC.
    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Local => value.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        _ => value,
    };
}

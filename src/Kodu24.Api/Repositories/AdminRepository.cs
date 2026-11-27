using Kodu24.Api.Data;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Api.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _db;

    public AdminRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<(User User, int ActiveListingCount)> Items, int TotalCount)> SearchUsersAsync(AdminUserQueryDto q)
    {
        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var pattern = "%" + EscapeLike(q.Search.Trim()) + "%";
            query = query.Where(u => EF.Functions.Like(u.FullName, pattern, "\\") || EF.Functions.Like(u.Email, pattern, "\\"));
        }
        if (q.Role is not null)
        {
            var role = Enum.Parse<UserRole>(q.Role);
            query = query.Where(u => u.Role == role);
        }

        var total = await query.CountAsync();
        var rows = await query
            .OrderByDescending(u => u.CreatedAt).ThenByDescending(u => u.Id)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            // Päringufilter kehtib ka siin: loetakse ainult kustutamata kuulutused.
            .Select(u => new { User = u, Count = u.Listings.Count() })
            .ToListAsync();

        return (rows.Select(r => (r.User, r.Count)).ToList(), total);
    }

    public Task<User?> GetUserAsync(int id) => _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<int> CountActiveListingsAsync(int userId) => _db.Listings.CountAsync(l => l.LandlordId == userId);

    public async Task<(List<Listing> Items, int TotalCount)> SearchListingsAsync(AdminListingQueryDto q)
    {
        // Admin peab nägema ka kustutatud kuulutusi, seega globaalne filter lülitatakse välja.
        var query = _db.Listings.IgnoreQueryFilters().AsNoTracking();

        query = q.Status switch
        {
            AdminListingQueryDto.Active => query.Where(l => !l.IsDeleted),
            AdminListingQueryDto.Deleted => query.Where(l => l.IsDeleted),
            _ => query,
        };

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var pattern = "%" + EscapeLike(q.Search.Trim()) + "%";
            query = query.Where(l =>
                EF.Functions.Like(l.Title, pattern, "\\") ||
                EF.Functions.Like(l.Address, pattern, "\\") ||
                EF.Functions.Like(l.Landlord.Email, pattern, "\\"));
        }

        var total = await query.CountAsync();
        var items = await query
            .Include(l => l.District)
            .Include(l => l.Landlord)
            .OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<Listing?> GetListingIncludingDeletedAsync(int id) =>
        _db.Listings.IgnoreQueryFilters()
           .Include(l => l.District)
           .Include(l => l.Landlord)
           .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<AdminStatsDto> GetStatsAsync()
    {
        // Iga arv on eraldi lihtne COUNT päring. Päringute, lemmikute ja teavituste arvus on ainult
        // kustutamata kuulutustega seotud kirjed (päringufiltrid).
        var usersByRole = await _db.Users.GroupBy(u => u.Role)
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Role.ToString(), x => x.Count);
        foreach (var role in Enum.GetNames<UserRole>())
            usersByRole.TryAdd(role, 0);

        return new AdminStatsDto(
            usersByRole,
            ActiveListings: await _db.Listings.CountAsync(),
            DeletedListings: await _db.Listings.IgnoreQueryFilters().CountAsync(l => l.IsDeleted),
            Inquiries: await _db.Inquiries.CountAsync(),
            UnreadInquiries: await _db.Inquiries.CountAsync(i => !i.IsRead),
            Favorites: await _db.Favorites.CountAsync(),
            SavedSearches: await _db.SavedSearches.CountAsync(),
            Notifications: await _db.Notifications.CountAsync());
    }

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}

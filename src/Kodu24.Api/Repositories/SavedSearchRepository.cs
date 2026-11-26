using Kodu24.Api.Data;
using Kodu24.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Api.Repositories;

public class SavedSearchRepository : Repository<SavedSearch>, ISavedSearchRepository
{
    public SavedSearchRepository(AppDbContext db) : base(db) { }

    public Task<SavedSearch?> GetWithDistrictAsync(int id) =>
        Set.Include(s => s.District).FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<SavedSearch>> GetByUserAsync(int userId) =>
        Set.AsNoTracking()
           .Include(s => s.District)
           .Where(s => s.UserId == userId)
           .OrderByDescending(s => s.CreatedAt).ThenByDescending(s => s.Id)
           .ToListAsync();

    public Task<int> CountByUserAsync(int userId) => Set.CountAsync(s => s.UserId == userId);

    public Task<List<SavedSearch>> GetAllWithDistrictAsync() =>
        Set.AsNoTracking().Include(s => s.District).OrderBy(s => s.Id).ToListAsync();
}

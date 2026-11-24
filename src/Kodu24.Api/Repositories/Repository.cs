using Kodu24.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Api.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> Set;

    public Repository(AppDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id) => await Set.FindAsync(id);

    public virtual Task<List<T>> GetAllAsync() => Set.ToListAsync();

    public async Task AddAsync(T entity) => await Set.AddAsync(entity);

    public void Update(T entity) => Set.Update(entity);

    public void Remove(T entity) => Set.Remove(entity);

    public Task<int> SaveChangesAsync() => Db.SaveChangesAsync();
}

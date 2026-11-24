using Kodu24.Api.Data;
using Kodu24.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Api.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    // E-postid salvestatakse väiketähtedega (vt AuthService), seega piisab võrdlusest.
    public Task<User?> GetByEmailAsync(string email) =>
        Set.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLowerInvariant());
}

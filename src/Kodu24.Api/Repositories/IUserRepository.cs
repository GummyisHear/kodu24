using Kodu24.Api.Entities;

namespace Kodu24.Api.Repositories;

public interface IUserRepository : IRepository<User>
{
    /// <summary>Otsib kasutaja e-posti järgi (tõstutundetu).</summary>
    Task<User?> GetByEmailAsync(string email);
}

using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;

namespace Kodu24.Api.Repositories;

/// <summary>Andmed halduse (admin) vaadete jaoks. Erinevalt teistest repositooriumitest näeb ka kustutatud kuulutusi.</summary>
public interface IAdminRepository
{
    /// <summary>Kasutajad koos kehtivate kuulutuste arvuga, uuemad eespool.</summary>
    Task<(List<(User User, int ActiveListingCount)> Items, int TotalCount)> SearchUsersAsync(AdminUserQueryDto query);

    Task<User?> GetUserAsync(int id);

    /// <summary>Kasutaja kustutamata kuulutuste arv.</summary>
    Task<int> CountActiveListingsAsync(int userId);

    /// <summary>Kuulutused koos linnaosa ja üürileandjaga, <b>ka kustutatud</b> (IgnoreQueryFilters), uuemad eespool.</summary>
    Task<(List<Listing> Items, int TotalCount)> SearchListingsAsync(AdminListingQueryDto query);

    /// <summary>Kuulutus ID järgi, ka kustutatud.</summary>
    Task<Listing?> GetListingIncludingDeletedAsync(int id);

    Task<AdminStatsDto> GetStatsAsync();

    Task<int> SaveChangesAsync();
}

using Kodu24.Api.Entities;

namespace Kodu24.Api.Repositories;

public interface IFavoriteRepository : IRepository<Favorite>
{
    /// <summary>Kasutaja lemmik konkreetse kuulutuse kohta või null.</summary>
    Task<Favorite?> FindAsync(int userId, int listingId);

    /// <summary>Kasutaja lemmikud koos kuulutuse, linnaosa ja piltidega, viimati lisatud eespool.</summary>
    Task<List<Favorite>> GetByUserAsync(int userId);

    /// <summary>Lemmikuks märgitud kuulutuste ID-d (nimekirjas südamete näitamiseks).</summary>
    Task<List<int>> GetListingIdsAsync(int userId);
}

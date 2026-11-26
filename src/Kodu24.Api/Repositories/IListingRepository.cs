using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;

namespace Kodu24.Api.Repositories;

public interface IListingRepository : IRepository<Listing>
{
    /// <summary>Kuulutus koos linnaosa, üürileandja ja piltidega.</summary>
    Task<Listing?> GetWithDetailsAsync(int id);

    /// <summary>
    /// Aktiivsed kuulutused filtrite, sortimise ja lehekülgedega (koos linnaosa ja piltidega).
    /// Tagastab ühe lehekülje ja kõigi filtrile vastavate kuulutuste arvu.
    /// </summary>
    Task<(List<Listing> Items, int TotalCount)> SearchAsync(ListingQueryDto query);

    /// <summary>Üürileandja kuulutused (koos linnaosa, piltide ja päringutega), uuemad eespool.</summary>
    Task<List<Listing>> GetByLandlordAsync(int landlordId);

    /// <summary>Aktiivsed kuulutused, mille <c>ExpiresAt</c> on möödas.</summary>
    Task<List<Listing>> GetExpiredAsync(DateTime now);

    /// <summary>Aktiivsed kuulutused antud ID-dega (puuduvad ja kustutatud jäetakse välja).</summary>
    Task<List<Listing>> GetByIdsAsync(IReadOnlyCollection<int> ids);

    /// <summary>
    /// Kehtivate kuulutuste arv, keskmine hind ja keskmine €/m² linnaosade kaupa (arvutatakse SQL-is).
    /// Linnaosad, kus kuulutusi pole, tulemusse ei tule.
    /// </summary>
    Task<List<DistrictPriceStats>> GetDistrictPriceStatsAsync(DateTime now);
}

/// <summary>Ühe linnaosa hinnastatistika (vt <see cref="IListingRepository.GetDistrictPriceStatsAsync"/>).</summary>
public record DistrictPriceStats(int DistrictId, int ListingCount, double AveragePrice, double AveragePricePerM2);

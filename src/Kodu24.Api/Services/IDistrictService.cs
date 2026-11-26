using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

public interface IDistrictService
{
    /// <summary>Kõik linnaosad tähestikulises järjekorras.</summary>
    Task<List<DistrictDto>> GetAllAsync();

    /// <summary>
    /// Kõigi linnaosade keskmine hind ja €/m², kallimad eespool;
    /// linnaosad ilma kuulutusteta on lõpus tähestiku järjekorras.
    /// </summary>
    Task<List<DistrictStatsDto>> GetStatsAsync();
}

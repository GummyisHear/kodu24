using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

public interface IDistrictService
{
    /// <summary>Kõik linnaosad tähestikulises järjekorras.</summary>
    Task<List<DistrictDto>> GetAllAsync();
}

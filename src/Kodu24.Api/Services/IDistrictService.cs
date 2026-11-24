using Kodu24.Api.Entities;

namespace Kodu24.Api.Services;

public interface IDistrictService
{
    /// <summary>Kõik linnaosad tähestikulises järjekorras.</summary>
    Task<List<District>> GetAllAsync();
}

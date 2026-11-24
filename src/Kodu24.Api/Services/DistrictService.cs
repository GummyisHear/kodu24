using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Mapping;
using Kodu24.Api.Repositories;

namespace Kodu24.Api.Services;

public class DistrictService : IDistrictService
{
    private readonly IRepository<District> _districts;

    public DistrictService(IRepository<District> districts)
    {
        _districts = districts;
    }

    public async Task<List<DistrictDto>> GetAllAsync()
    {
        var districts = await _districts.GetAllAsync();
        return districts.OrderBy(d => d.Name).Select(d => d.ToDto()).ToList();
    }
}

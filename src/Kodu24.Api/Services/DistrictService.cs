using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Mapping;
using Kodu24.Api.Repositories;

namespace Kodu24.Api.Services;

public class DistrictService : IDistrictService
{
    private readonly IRepository<District> _districts;
    private readonly IListingRepository _listings;
    private readonly TimeProvider _time;

    public DistrictService(IRepository<District> districts, IListingRepository listings, TimeProvider time)
    {
        _districts = districts;
        _listings = listings;
        _time = time;
    }

    public async Task<List<DistrictDto>> GetAllAsync()
    {
        var districts = await _districts.GetAllAsync();
        return districts.OrderBy(d => d.Name).Select(d => d.ToDto()).ToList();
    }

    public async Task<List<DistrictStatsDto>> GetStatsAsync()
    {
        var districts = await _districts.GetAllAsync();
        // Aegunud kuulutused, mida taustateenus pole veel kustutanud, statistikat ei moonuta.
        var stats = (await _listings.GetDistrictPriceStatsAsync(_time.GetUtcNow().UtcDateTime))
            .ToDictionary(s => s.DistrictId);

        return districts
            .Select(d => stats.TryGetValue(d.Id, out var s)
                ? new DistrictStatsDto(d.Id, d.Name, s.ListingCount, Round(s.AveragePrice), Round(s.AveragePricePerM2))
                : new DistrictStatsDto(d.Id, d.Name, 0, null, null))
            .OrderBy(d => d.AveragePricePerM2 is null)
            .ThenByDescending(d => d.AveragePricePerM2)
            .ThenBy(d => d.DistrictName)
            .ToList();
    }

    private static decimal Round(double value) => Math.Round((decimal)value, 2, MidpointRounding.AwayFromZero);
}

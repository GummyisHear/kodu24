using Kodu24.Api.Entities;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Moq;

namespace Kodu24.Tests;

public class DistrictServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IRepository<District>> _districts = new();
    private readonly Mock<IListingRepository> _listings = new();

    public DistrictServiceTests()
    {
        _districts.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<District>
        {
            new() { Id = 1, Name = "Pirita" },
            new() { Id = 2, Name = "Kesklinn" },
            new() { Id = 3, Name = "Mustamäe" },
            new() { Id = 4, Name = "Haabersti" },
        });
    }

    private DistrictService CreateService()
    {
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Returns(Now);
        return new DistrictService(_districts.Object, _listings.Object, time.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDistrictsSortedByName()
    {
        var result = await CreateService().GetAllAsync();

        Assert.Equal(new[] { "Haabersti", "Kesklinn", "Mustamäe", "Pirita" }, result.Select(d => d.Name));
        Assert.Equal(new[] { 4, 2, 3, 1 }, result.Select(d => d.Id));
    }

    [Fact]
    public async Task GetStatsAsync_RoundsAndSortsByPricePerM2_EmptyDistrictsLast()
    {
        _listings.Setup(l => l.GetDistrictPriceStatsAsync(Now.UtcDateTime)).ReturnsAsync(
        [
            new DistrictPriceStats(3, 2, 455.5, 9.876543),
            new DistrictPriceStats(2, 3, 800, 15.005),
        ]);

        var result = await CreateService().GetStatsAsync();

        Assert.Equal(new[] { "Kesklinn", "Mustamäe", "Haabersti", "Pirita" }, result.Select(d => d.DistrictName));
        Assert.Equal(15.01m, result[0].AveragePricePerM2);
        Assert.Equal(455.50m, result[1].AveragePrice);
        Assert.Equal(9.88m, result[1].AveragePricePerM2);
        // Ilma kuulutusteta linnaosad on samuti nimekirjas, aga ilma keskmisteta.
        Assert.All(result.Skip(2), d =>
        {
            Assert.Equal(0, d.ListingCount);
            Assert.Null(d.AveragePrice);
            Assert.Null(d.AveragePricePerM2);
        });
    }
}

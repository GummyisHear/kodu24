using Kodu24.Api.Entities;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Moq;

namespace Kodu24.Tests;

public class DistrictServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsDistrictsSortedByName()
    {
        var repo = new Mock<IRepository<District>>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<District>
        {
            new() { Id = 1, Name = "Pirita" },
            new() { Id = 2, Name = "Kesklinn" },
            new() { Id = 3, Name = "Mustamäe" },
        });

        var service = new DistrictService(repo.Object);

        var result = await service.GetAllAsync();

        Assert.Equal(new[] { "Kesklinn", "Mustamäe", "Pirita" }, result.Select(d => d.Name));
    }
}

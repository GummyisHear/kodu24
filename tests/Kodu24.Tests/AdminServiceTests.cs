using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kodu24.Tests;

public class AdminServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 27, 12, 0, 0, TimeSpan.Zero);
    private static readonly CurrentUser Admin = new(1, UserRole.Admin);

    private readonly Mock<IAdminRepository> _repo = new();

    private AdminService CreateService()
    {
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Returns(Now);
        return new AdminService(_repo.Object, time.Object, NullLogger<AdminService>.Instance);
    }

    private User UserWithRole(int id, UserRole role)
    {
        var user = new User { Id = id, Email = $"u{id}@kodu24.ee", FullName = $"Kasutaja {id}", Role = role };
        _repo.Setup(r => r.GetUserAsync(id)).ReturnsAsync(user);
        return user;
    }

    private Listing DeletedListing(DateTime expiresAt)
    {
        var listing = new Listing
        {
            Id = 10,
            Title = "Kustutatud",
            IsDeleted = true,
            DeletedAt = Now.UtcDateTime.AddDays(-1),
            ExpiresAt = expiresAt,
            District = new District { Name = "Kesklinn" },
            Landlord = new User { FullName = "Mari", Email = "mari@kodu24.ee" },
        };
        _repo.Setup(r => r.GetListingIncludingDeletedAsync(10)).ReturnsAsync(listing);
        return listing;
    }

    [Fact]
    public async Task ChangeRoleAsync_ChangesRoleAndSaves()
    {
        var user = UserWithRole(5, UserRole.Seeker);
        _repo.Setup(r => r.CountActiveListingsAsync(5)).ReturnsAsync(0);

        var dto = await CreateService().ChangeRoleAsync(5, new ChangeRoleRequestDto { Role = "Landlord" }, Admin);

        Assert.Equal(UserRole.Landlord, user.Role);
        Assert.Equal("Landlord", dto.Role);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ChangeRoleAsync_OwnRole_ThrowsValidationError()
    {
        // Muidu võiks ainus admin end kogemata välja lukustada.
        UserWithRole(Admin.Id, UserRole.Admin);

        await Assert.ThrowsAsync<ValidationFailedException>(() =>
            CreateService().ChangeRoleAsync(Admin.Id, new ChangeRoleRequestDto { Role = "Seeker" }, Admin));

        _repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ChangeRoleAsync_ServiceAccount_ThrowsValidationError()
    {
        var service = UserWithRole(6, UserRole.Service);

        await Assert.ThrowsAsync<ValidationFailedException>(() =>
            CreateService().ChangeRoleAsync(6, new ChangeRoleRequestDto { Role = "Admin" }, Admin));

        Assert.Equal(UserRole.Service, service.Role);
    }

    [Fact]
    public async Task RestoreListingAsync_ExpiredListing_IsExtendedSoWorkerDoesNotDeleteItAgain()
    {
        var listing = DeletedListing(expiresAt: Now.UtcDateTime.AddDays(-2));

        var dto = await CreateService().RestoreListingAsync(10);

        Assert.False(listing.IsDeleted);
        Assert.Null(listing.DeletedAt);
        Assert.Equal(Now.UtcDateTime.AddDays(ListingService.ListingLifetimeDays), listing.ExpiresAt);
        Assert.False(dto.IsDeleted);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RestoreListingAsync_StillValidListing_KeepsExpiry()
    {
        var expiresAt = Now.UtcDateTime.AddDays(5);
        var listing = DeletedListing(expiresAt);

        await CreateService().RestoreListingAsync(10);

        Assert.Equal(expiresAt, listing.ExpiresAt);
    }

    [Fact]
    public async Task RestoreListingAsync_NotDeleted_ThrowsConflict()
    {
        var listing = DeletedListing(Now.UtcDateTime.AddDays(5));
        listing.IsDeleted = false;

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().RestoreListingAsync(10));
    }
}

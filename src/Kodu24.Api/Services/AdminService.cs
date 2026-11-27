using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Repositories;

namespace Kodu24.Api.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _admin;
    private readonly TimeProvider _time;
    private readonly ILogger<AdminService> _logger;

    public AdminService(IAdminRepository admin, TimeProvider time, ILogger<AdminService> logger)
    {
        _admin = admin;
        _time = time;
        _logger = logger;
    }

    public async Task<PagedResultDto<AdminUserDto>> GetUsersAsync(AdminUserQueryDto query)
    {
        var (items, total) = await _admin.SearchUsersAsync(query);
        return new PagedResultDto<AdminUserDto>(
            items.Select(x => ToDto(x.User, x.ActiveListingCount)).ToList(), query.Page, query.PageSize, total);
    }

    public async Task<AdminUserDto> ChangeRoleAsync(int userId, ChangeRoleRequestDto request, CurrentUser admin)
    {
        var user = await _admin.GetUserAsync(userId)
                   ?? throw new NotFoundException($"Kasutajat {userId} ei leitud.");

        if (user.Id == admin.Id)
            throw new ValidationFailedException(nameof(ChangeRoleRequestDto.Role), "Enda rolli ei saa muuta.");
        if (user.Role == UserRole.Service)
            throw new ValidationFailedException(nameof(ChangeRoleRequestDto.Role), "Teenusekonto rolli ei saa muuta.");

        var newRole = Enum.Parse<UserRole>(request.Role);
        if (user.Role != newRole)
        {
            _logger.LogWarning("Admin {AdminId} muutis kasutaja {UserId} rolli: {OldRole} → {NewRole}",
                admin.Id, user.Id, user.Role, newRole);
            user.Role = newRole;
            await _admin.SaveChangesAsync();
        }

        // Kasutaja vana token kehtib kuni aegumiseni (kuni 2 h); uus roll hakkab kehtima pärast uut sisselogimist.
        return ToDto(user, await _admin.CountActiveListingsAsync(user.Id));
    }

    public async Task<PagedResultDto<AdminListingDto>> GetListingsAsync(AdminListingQueryDto query)
    {
        var (items, total) = await _admin.SearchListingsAsync(query);
        return new PagedResultDto<AdminListingDto>(items.Select(ToDto).ToList(), query.Page, query.PageSize, total);
    }

    public async Task<AdminListingDto> RestoreListingAsync(int listingId)
    {
        var listing = await _admin.GetListingIncludingDeletedAsync(listingId)
                      ?? throw new NotFoundException($"Kuulutust {listingId} ei leitud.");

        if (!listing.IsDeleted)
            throw new ConflictException("Kuulutus ei ole kustutatud.");

        var now = _time.GetUtcNow().UtcDateTime;
        listing.IsDeleted = false;
        listing.DeletedAt = null;
        if (listing.ExpiresAt <= now)
            listing.ExpiresAt = now.AddDays(ListingService.ListingLifetimeDays);

        await _admin.SaveChangesAsync();
        _logger.LogInformation("Kuulutus {ListingId} taastati, kehtib kuni {ExpiresAt:u}", listing.Id, listing.ExpiresAt);
        return ToDto(listing);
    }

    public Task<AdminStatsDto> GetStatsAsync() => _admin.GetStatsAsync();

    private static AdminUserDto ToDto(User u, int activeListingCount) =>
        new(u.Id, u.Email, u.FullName, u.Phone, u.Role.ToString(), u.CreatedAt, activeListingCount);

    private static AdminListingDto ToDto(Listing l) => new(
        l.Id, l.Title, l.Address, l.District.Name, l.Price,
        l.LandlordId, l.Landlord.FullName, l.Landlord.Email,
        l.CreatedAt, l.ExpiresAt, l.IsDeleted, l.DeletedAt);
}

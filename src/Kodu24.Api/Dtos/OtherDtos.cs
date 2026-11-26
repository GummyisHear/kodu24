namespace Kodu24.Api.Dtos;

public record DistrictDto(int Id, string Name);

/// <summary>
/// Linnaosa hinnastatistika kehtivate kuulutuste põhjal. Kui linnaosas kuulutusi pole,
/// on arv 0 ja keskmised <c>null</c>.
/// </summary>
public record DistrictStatsDto(
    int DistrictId,
    string DistrictName,
    int ListingCount,
    decimal? AveragePrice,
    decimal? AveragePricePerM2);

/// <summary>Kasutaja andmed. Parooli räsi ei lahku kunagi API-st.</summary>
public record UserDto(int Id, string Email, string FullName, string? Phone, string Role, DateTime CreatedAt);

public record FavoriteDto(int ListingId, DateTime CreatedAt, ListingListItemDto Listing);

public record InquiryDto(
    int Id,
    int ListingId,
    string ListingTitle,
    string SeekerName,
    string SeekerEmail,
    string Message,
    bool IsRead,
    DateTime CreatedAt);

public record SavedSearchDto(
    int Id,
    string Name,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? Rooms,
    int? DistrictId,
    string? DistrictName,
    DateTime CreatedAt,
    DateTime LastNotifiedAt);

public record NotificationDto(
    int Id,
    int SavedSearchId,
    int ListingId,
    string Message,
    bool IsRead,
    DateTime CreatedAt);

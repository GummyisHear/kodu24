namespace Kodu24.Api.Dtos;

/// <summary>Kuulutus nimekirjas (otsingutulemused, lemmikud).</summary>
public record ListingListItemDto(
    int Id,
    string Title,
    string Address,
    string DistrictName,
    decimal Price,
    int Rooms,
    decimal AreaM2,
    decimal PricePerM2,
    string? PrimaryImageUrl,
    DateTime CreatedAt);

/// <summary>Kuulutuse detailvaade.</summary>
public record ListingDetailDto(
    int Id,
    string Title,
    string Address,
    int DistrictId,
    string DistrictName,
    decimal Price,
    int Rooms,
    decimal AreaM2,
    decimal PricePerM2,
    string Description,
    double Latitude,
    double Longitude,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    LandlordDto Landlord,
    List<ListingImageDto> Images);

/// <summary>Üürileandja avalik info kuulutusel.</summary>
public record LandlordDto(int Id, string FullName, string? Phone);

public record ListingImageDto(int Id, string Url, bool IsPrimary, int SortOrder);

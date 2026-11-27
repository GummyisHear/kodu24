using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;

namespace Kodu24.Api.Mapping;

/// <summary>
/// Käsitsi mapping olemitelt DTO-deks. Navigatsiooniomadused (nt
/// <c>Listing.District</c>) peavad olema päringus laaditud (<c>Include</c>).
/// </summary>
public static class DtoMapper
{
    /// <summary>Avalik tee, kust üleslaaditud pilte serveeritakse.</summary>
    public const string ImageBasePath = "/uploads/";

    public static decimal PricePerM2(decimal price, decimal areaM2) =>
        areaM2 > 0 ? Math.Round(price / areaM2, 2) : 0;

    public static ListingListItemDto ToListItemDto(this Listing l) => new(
        l.Id,
        l.Title,
        l.Address,
        l.District.Name,
        l.Price,
        l.Rooms,
        l.AreaM2,
        PricePerM2(l.Price, l.AreaM2),
        PrimaryImage(l) is { } img ? ImageUrl(img) : null,
        l.CreatedAt,
        l.Latitude,
        l.Longitude);

    public static ListingDetailDto ToDetailDto(this Listing l) => new(
        l.Id,
        l.Title,
        l.Address,
        l.DistrictId,
        l.District.Name,
        l.Price,
        l.Rooms,
        l.AreaM2,
        PricePerM2(l.Price, l.AreaM2),
        l.Description,
        l.Latitude,
        l.Longitude,
        l.CreatedAt,
        l.ExpiresAt,
        new LandlordDto(l.Landlord.Id, l.Landlord.FullName, l.Landlord.Phone),
        l.Images.OrderBy(i => i.SortOrder).Select(i => i.ToDto()).ToList());

    /// <summary>Vajab laaditud <c>District</c>, <c>Images</c> ja <c>Inquiries</c> navigatsioone.</summary>
    public static MyListingDto ToMyListingDto(this Listing l) => new(
        l.Id,
        l.Title,
        l.Address,
        l.District.Name,
        l.Price,
        l.Rooms,
        l.AreaM2,
        PrimaryImage(l) is { } img ? ImageUrl(img) : null,
        l.Images.Count,
        l.Inquiries.Count,
        l.Inquiries.Count(i => !i.IsRead),
        l.CreatedAt,
        l.ExpiresAt);

    /// <summary>Kopeerib sisendi väljad olemile (lisamisel ja muutmisel).</summary>
    public static void ApplyTo(this ListingRequestDto r, Listing l)
    {
        l.Title = r.Title.Trim();
        l.Address = r.Address.Trim();
        l.DistrictId = r.DistrictId;
        l.Price = r.Price;
        l.Rooms = r.Rooms;
        l.AreaM2 = r.AreaM2;
        l.Description = r.Description?.Trim() ?? "";
        l.Latitude = r.Latitude;
        l.Longitude = r.Longitude;
    }

    public static ListingImageDto ToDto(this ListingImage i) =>
        new(i.Id, ImageUrl(i), i.IsPrimary, i.SortOrder);

    public static DistrictDto ToDto(this District d) => new(d.Id, d.Name);

    public static UserDto ToDto(this User u) =>
        new(u.Id, u.Email, u.FullName, u.Phone, u.Role.ToString(), u.CreatedAt);

    public static FavoriteDto ToDto(this Favorite f) =>
        new(f.ListingId, f.CreatedAt, f.Listing.ToListItemDto());

    public static InquiryDto ToDto(this Inquiry i) => new(
        i.Id,
        i.ListingId,
        i.Listing.Title,
        i.Seeker.FullName,
        i.Seeker.Email,
        i.Message,
        i.IsRead,
        i.CreatedAt);

    public static SavedSearchDto ToDto(this SavedSearch s) => new(
        s.Id,
        s.UserId,
        s.Name,
        s.MinPrice,
        s.MaxPrice,
        s.Rooms,
        s.DistrictId,
        s.District?.Name,
        s.CreatedAt,
        s.LastNotifiedAt);

    public static NotificationDto ToDto(this Notification n) =>
        new(n.Id, n.SavedSearchId, n.ListingId, n.Message, n.IsRead, n.CreatedAt);

    private static ListingImage? PrimaryImage(Listing l) =>
        l.Images.FirstOrDefault(i => i.IsPrimary)
        ?? l.Images.OrderBy(i => i.SortOrder).FirstOrDefault();

    private static string ImageUrl(ListingImage i) => ImageBasePath + i.FileName;
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Dtos;

/// <summary>Kasutajate nimekirja parameetrid (admin).</summary>
public class AdminUserQueryDto
{
    /// <summary>Otsingusõna nimest või e-postist.</summary>
    /// <example>mari</example>
    [FromQuery(Name = "q")]
    [MaxLength(100, ErrorMessage = "Otsingusõna võib olla kuni 100 tähemärki.")]
    public string? Search { get; init; }

    /// <summary>Ainult selle rolliga kasutajad: Admin, Landlord, Seeker või Service.</summary>
    /// <example>Landlord</example>
    [RegularExpression("^(Admin|Landlord|Seeker|Service)$",
        ErrorMessage = "Tundmatu roll. Lubatud: Admin, Landlord, Seeker, Service.")]
    public string? Role { get; init; }

    /// <summary>Lehekülje number (algab 1-st).</summary>
    /// <example>1</example>
    [Range(1, 10_000, ErrorMessage = "Lehekülje number peab olema vähemalt 1.")]
    public int Page { get; init; } = 1;

    /// <summary>Kasutajaid leheküljel (1–100).</summary>
    /// <example>20</example>
    [Range(1, 100, ErrorMessage = "Lehekülje suurus peab olema vahemikus 1–100.")]
    public int PageSize { get; init; } = 20;
}

/// <summary>Kuulutuste nimekirja parameetrid (admin), sh kustutatud kuulutused.</summary>
public class AdminListingQueryDto
{
    public const string Active = "active";
    public const string Deleted = "deleted";
    public const string All = "all";

    /// <summary>Otsingusõna pealkirjast, aadressist või üürileandja e-postist.</summary>
    /// <example>kesklinn</example>
    [FromQuery(Name = "q")]
    [MaxLength(100, ErrorMessage = "Otsingusõna võib olla kuni 100 tähemärki.")]
    public string? Search { get; init; }

    /// <summary>active = kehtivad, deleted = kustutatud või aegunud, all = kõik.</summary>
    /// <example>deleted</example>
    [RegularExpression("^(active|deleted|all)$", ErrorMessage = "Tundmatu olek. Lubatud: active, deleted, all.")]
    public string Status { get; init; } = All;

    /// <summary>Lehekülje number (algab 1-st).</summary>
    /// <example>1</example>
    [Range(1, 10_000, ErrorMessage = "Lehekülje number peab olema vähemalt 1.")]
    public int Page { get; init; } = 1;

    /// <summary>Kuulutusi leheküljel (1–100).</summary>
    /// <example>20</example>
    [Range(1, 100, ErrorMessage = "Lehekülje suurus peab olema vahemikus 1–100.")]
    public int PageSize { get; init; } = 20;
}

/// <summary>Kasutaja rolli muutmine.</summary>
public class ChangeRoleRequestDto
{
    /// <summary>Uus roll: Admin, Landlord või Seeker. Teenusekonto rolli siit anda ega muuta ei saa.</summary>
    /// <example>Landlord</example>
    [Required(ErrorMessage = "Roll on kohustuslik.")]
    [RegularExpression("^(Admin|Landlord|Seeker)$", ErrorMessage = "Lubatud rollid: Admin, Landlord, Seeker.")]
    public string Role { get; init; } = "";
}

/// <summary>Kasutaja halduse vaates.</summary>
public record AdminUserDto(
    int Id,
    string Email,
    string FullName,
    string? Phone,
    string Role,
    DateTime CreatedAt,
    int ActiveListingCount);

/// <summary>Kuulutus halduse vaates (ka kustutatud).</summary>
public record AdminListingDto(
    int Id,
    string Title,
    string Address,
    string DistrictName,
    decimal Price,
    int LandlordId,
    string LandlordName,
    string LandlordEmail,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsDeleted,
    DateTime? DeletedAt);

/// <summary>Portaali ülevaade halduse lehele.</summary>
public record AdminStatsDto(
    Dictionary<string, int> UsersByRole,
    int ActiveListings,
    int DeletedListings,
    int Inquiries,
    int UnreadInquiries,
    int Favorites,
    int SavedSearches,
    int Notifications);

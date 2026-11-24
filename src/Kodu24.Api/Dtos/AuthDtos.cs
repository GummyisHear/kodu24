using System.ComponentModel.DataAnnotations;

namespace Kodu24.Api.Dtos;

/// <summary>Uue kasutaja registreerimine.</summary>
public class RegisterRequestDto
{
    /// <summary>E-posti aadress, millega hiljem sisse logitakse.</summary>
    /// <example>uus.kasutaja@kodu24.ee</example>
    [Required(ErrorMessage = "E-post on kohustuslik.")]
    [EmailAddress(ErrorMessage = "E-posti aadress ei ole korrektne.")]
    [MaxLength(200, ErrorMessage = "E-post võib olla kuni 200 tähemärki.")]
    public string Email { get; init; } = "";

    /// <summary>Parool, vähemalt 8 tähemärki.</summary>
    /// <example>Parool123!</example>
    [Required(ErrorMessage = "Parool on kohustuslik.")]
    [MinLength(8, ErrorMessage = "Parool peab olema vähemalt 8 tähemärki.")]
    [MaxLength(100, ErrorMessage = "Parool võib olla kuni 100 tähemärki.")]
    public string Password { get; init; } = "";

    /// <summary>Ees- ja perekonnanimi.</summary>
    /// <example>Kati Karu</example>
    [Required(ErrorMessage = "Nimi on kohustuslik.")]
    [MaxLength(100, ErrorMessage = "Nimi võib olla kuni 100 tähemärki.")]
    public string FullName { get; init; } = "";

    /// <summary>Telefoninumber (valikuline).</summary>
    /// <example>+3725551234</example>
    [Phone(ErrorMessage = "Telefoninumber ei ole korrektne.")]
    [MaxLength(30, ErrorMessage = "Telefoninumber võib olla kuni 30 tähemärki.")]
    public string? Phone { get; init; }

    /// <summary>Roll: <c>Landlord</c> (üürileandja) või <c>Seeker</c> (üürija).</summary>
    /// <example>Seeker</example>
    [Required(ErrorMessage = "Roll on kohustuslik.")]
    [RegularExpression("^(Landlord|Seeker)$", ErrorMessage = "Roll peab olema Landlord või Seeker.")]
    public string Role { get; init; } = "";
}

/// <summary>Sisselogimine e-posti ja parooliga.</summary>
public class LoginRequestDto
{
    /// <example>liis@kodu24.ee</example>
    [Required(ErrorMessage = "E-post on kohustuslik.")]
    public string Email { get; init; } = "";

    /// <example>Parool123!</example>
    [Required(ErrorMessage = "Parool on kohustuslik.")]
    public string Password { get; init; } = "";
}

/// <summary>Edukas registreerimine või sisselogimine.</summary>
/// <param name="Token">JWT, mis saadetakse päises <c>Authorization: Bearer {token}</c>.</param>
/// <param name="ExpiresAt">Tokeni aegumisaeg (UTC).</param>
/// <param name="User">Sisselogitud kasutaja andmed.</param>
public record AuthResponseDto(string Token, DateTime ExpiresAt, UserDto User);

namespace Kodu24.Api.Auth;

/// <summary>JWT seaded konfiguratsiooni sektsioonist "Jwt".</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Kodu24";
    public string Audience { get; set; } = "Kodu24";

    /// <summary>
    /// Allkirjastamise võti (vähemalt 32 baiti). Arenduses appsettings.Development.json-is,
    /// päris keskkonnas keskkonnamuutujana <c>Jwt__Key</c>. Ei tohi minna repositooriumisse.
    /// </summary>
    public string Key { get; set; } = "";

    public int ExpiresMinutes { get; set; } = 120;
}

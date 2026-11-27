namespace Kodu24.Worker;

/// <summary>Seaded jaotisest "Api": kuhu ja millise kontoga taustateenus ühendub.</summary>
public class ApiOptions
{
    public const string Section = "Api";

    /// <summary>API aadress, nt http://localhost:5134/.</summary>
    public string BaseUrl { get; set; } = "http://localhost:5134/";

    /// <summary>Teenusekonto (roll Service) e-post.</summary>
    public string ServiceEmail { get; set; } = "service@kodu24.ee";

    /// <summary>
    /// Teenusekonto parool. Ei ole repos (v.a arenduse algandmete parool appsettings.Development.json-is):
    /// päris keskkonnas user-secrets või keskkonnamuutuja <c>Api__ServicePassword</c>.
    /// </summary>
    public string ServicePassword { get; set; } = "";

    /// <summary>Kui tihti tööd tehakse (minutites). Vaikimisi kord tunnis.</summary>
    public int IntervalMinutes { get; set; } = 60;
}

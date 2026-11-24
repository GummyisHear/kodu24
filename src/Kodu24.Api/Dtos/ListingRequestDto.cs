using System.ComponentModel.DataAnnotations;

namespace Kodu24.Api.Dtos;

/// <summary>
/// Kuulutuse lisamine või muutmine. Üürileandja võetakse tokenist,
/// loomise ja aegumise aja määrab server.
/// </summary>
public class ListingRequestDto
{
    /// <summary>Kuulutuse pealkiri.</summary>
    /// <example>Valoisa kahetoaline Kalamajas</example>
    [Required(ErrorMessage = "Pealkiri on kohustuslik.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "Pealkiri peab olema 5–150 tähemärki.")]
    public string Title { get; init; } = "";

    /// <summary>Tänav ja majanumber.</summary>
    /// <example>Kotzebue 12-4</example>
    [Required(ErrorMessage = "Aadress on kohustuslik.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Aadress peab olema 3–200 tähemärki.")]
    public string Address { get; init; } = "";

    /// <summary>Linnaosa ID (vt <c>GET /api/districts</c>).</summary>
    /// <example>2</example>
    [Range(1, int.MaxValue, ErrorMessage = "Vali linnaosa.")]
    public int DistrictId { get; init; }

    /// <summary>Kuu üür eurodes.</summary>
    /// <example>720</example>
    [Range(1, 100_000, ErrorMessage = "Hind peab olema vahemikus 1–100 000 €.")]
    public decimal Price { get; init; }

    /// <summary>Tubade arv.</summary>
    /// <example>2</example>
    [Range(1, 20, ErrorMessage = "Tubade arv peab olema vahemikus 1–20.")]
    public int Rooms { get; init; }

    /// <summary>Üldpind ruutmeetrites.</summary>
    /// <example>52.5</example>
    [Range(5, 1000, ErrorMessage = "Pind peab olema vahemikus 5–1000 m².")]
    public decimal AreaM2 { get; init; }

    /// <summary>Vabas vormis kirjeldus (valikuline).</summary>
    /// <example>Renoveeritud korter, rõdu hoovi poole, lähedal Balti jaam.</example>
    [MaxLength(4000, ErrorMessage = "Kirjeldus võib olla kuni 4000 tähemärki.")]
    public string? Description { get; init; }

    /// <summary>Laiuskraad (kaardi jaoks), Eesti piires.</summary>
    /// <example>59.4442</example>
    [Range(57.5, 59.9, ErrorMessage = "Asukoht peab olema Eestis (laiuskraad 57,5–59,9).")]
    public double Latitude { get; init; }

    /// <summary>Pikkuskraad (kaardi jaoks), Eesti piires.</summary>
    /// <example>24.7339</example>
    [Range(21.5, 28.3, ErrorMessage = "Asukoht peab olema Eestis (pikkuskraad 21,5–28,3).")]
    public double Longitude { get; init; }
}

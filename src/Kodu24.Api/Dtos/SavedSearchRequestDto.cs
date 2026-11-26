using System.ComponentModel.DataAnnotations;

namespace Kodu24.Api.Dtos;

/// <summary>
/// Salvestatud otsingu lisamine või muutmine. Kõik filtrid on valikulised ja neid saab vabalt
/// kombineerida; kasutaja ja ajad määrab server.
/// </summary>
public class SavedSearchRequestDto
{
    /// <summary>Otsingu nimi, mida kasutaja oma nimekirjas näeb.</summary>
    /// <example>Kahetoaline Kalamajas</example>
    [Required(ErrorMessage = "Nimi on kohustuslik.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nimi peab olema 2–100 tähemärki.")]
    public string Name { get; init; } = "";

    /// <summary>Minimaalne kuu üür (€).</summary>
    /// <example>400</example>
    [Range(0, 100_000, ErrorMessage = "Minimaalne hind peab olema vahemikus 0–100 000 €.")]
    public decimal? MinPrice { get; init; }

    /// <summary>Maksimaalne kuu üür (€).</summary>
    /// <example>800</example>
    [Range(0, 100_000, ErrorMessage = "Maksimaalne hind peab olema vahemikus 0–100 000 €.")]
    public decimal? MaxPrice { get; init; }

    /// <summary>Täpne tubade arv.</summary>
    /// <example>2</example>
    [Range(1, 20, ErrorMessage = "Tubade arv peab olema vahemikus 1–20.")]
    public int? Rooms { get; init; }

    /// <summary>Linnaosa ID (vt <c>GET /api/districts</c>).</summary>
    /// <example>3</example>
    [Range(1, int.MaxValue, ErrorMessage = "Linnaosa ID peab olema positiivne.")]
    public int? DistrictId { get; init; }
}

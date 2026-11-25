using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Dtos;

/// <summary>
/// Kuulutuste otsingu parameetrid (päringustring). Kõik filtrid on valikulised ja kombineeritavad.
/// </summary>
public class ListingQueryDto
{
    /// <summary>Otsingusõna pealkirjast, aadressist või kirjeldusest.</summary>
    /// <example>rõdu</example>
    [FromQuery(Name = "q")]
    [MaxLength(100, ErrorMessage = "Otsingusõna võib olla kuni 100 tähemärki.")]
    public string? Search { get; init; }

    /// <summary>Minimaalne kuu üür (€).</summary>
    /// <example>400</example>
    [Range(0, 100_000, ErrorMessage = "Minimaalne hind peab olema vahemikus 0–100 000 €.")]
    public decimal? MinPrice { get; init; }

    /// <summary>Maksimaalne kuu üür (€).</summary>
    /// <example>900</example>
    [Range(0, 100_000, ErrorMessage = "Maksimaalne hind peab olema vahemikus 0–100 000 €.")]
    public decimal? MaxPrice { get; init; }

    /// <summary>Täpne tubade arv.</summary>
    /// <example>2</example>
    [Range(1, 20, ErrorMessage = "Tubade arv peab olema vahemikus 1–20.")]
    public int? Rooms { get; init; }

    /// <summary>Linnaosa ID (vt <c>GET /api/districts</c>).</summary>
    /// <example>2</example>
    [FromQuery(Name = "district")]
    [Range(1, int.MaxValue, ErrorMessage = "Linnaosa ID peab olema positiivne.")]
    public int? DistrictId { get; init; }

    /// <summary>Ainult pärast seda aega (UTC) lisatud kuulutused. Kasutab taustateenus uute kuulutuste leidmiseks.</summary>
    /// <example>2026-11-20T00:00:00Z</example>
    public DateTime? CreatedAfter { get; init; }

    /// <summary>
    /// Sortimine: <c>newest</c> (vaikimisi), <c>oldest</c>, <c>price_asc</c>, <c>price_desc</c>,
    /// <c>area_desc</c>, <c>price_m2_asc</c>.
    /// </summary>
    /// <example>price_asc</example>
    [RegularExpression("^(newest|oldest|price_asc|price_desc|area_desc|price_m2_asc)$",
        ErrorMessage = "Tundmatu sortimine. Lubatud: newest, oldest, price_asc, price_desc, area_desc, price_m2_asc.")]
    public string Sort { get; init; } = ListingSort.Newest;

    /// <summary>Lehekülje number (algab 1-st).</summary>
    /// <example>1</example>
    [Range(1, 10_000, ErrorMessage = "Lehekülje number peab olema vähemalt 1.")]
    public int Page { get; init; } = 1;

    /// <summary>Kuulutusi leheküljel (1–50).</summary>
    /// <example>12</example>
    [Range(1, 50, ErrorMessage = "Lehekülje suurus peab olema vahemikus 1–50.")]
    public int PageSize { get; init; } = 12;
}

public static class ListingSort
{
    public const string Newest = "newest";
    public const string Oldest = "oldest";
    public const string PriceAsc = "price_asc";
    public const string PriceDesc = "price_desc";
    public const string AreaDesc = "area_desc";
    public const string PricePerM2Asc = "price_m2_asc";
}

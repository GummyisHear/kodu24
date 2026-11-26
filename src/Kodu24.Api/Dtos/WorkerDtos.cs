using System.ComponentModel.DataAnnotations;

namespace Kodu24.Api.Dtos;

/// <summary>Taustateenus: salvestatud otsingu teavituste kursor.</summary>
public class MarkNotifiedRequestDto
{
    /// <summary>Viimase teavitatud kuulutuse loomise aeg (UTC).</summary>
    /// <example>2026-11-26T12:00:00Z</example>
    [Required(ErrorMessage = "Teavitamise aeg on kohustuslik.")]
    public DateTime? NotifiedAt { get; init; }
}

/// <summary>Taustateenus: teavitused ühe salvestatud otsingu uute kuulutuste kohta.</summary>
public class CreateNotificationsRequestDto
{
    /// <summary>Salvestatud otsingu ID. Teavituse saab otsingu omanik.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Salvestatud otsingu ID peab olema positiivne.")]
    public int SavedSearchId { get; init; }

    /// <summary>Uute kuulutuste ID-d (1–50 korraga).</summary>
    /// <example>[12, 15]</example>
    [Required(ErrorMessage = "Kuulutuste ID-d on kohustuslikud.")]
    [Length(1, 50, ErrorMessage = "Korraga saab teavitada 1–50 kuulutusest.")]
    public List<int> ListingIds { get; init; } = [];
}

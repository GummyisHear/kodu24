using System.ComponentModel.DataAnnotations;

namespace Kodu24.Api.Dtos;

/// <summary>Üürija päring üürileandjale. Saatja võetakse tokenist, kuulutus aadressist.</summary>
public class InquiryRequestDto
{
    /// <summary>Sõnum üürileandjale.</summary>
    /// <example>Tere! Kas korter on veel vaba ja millal saaks seda vaatama tulla?</example>
    [Required(ErrorMessage = "Sõnum on kohustuslik.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Sõnum peab olema 10–2000 tähemärki.")]
    public string Message { get; init; } = "";
}

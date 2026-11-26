using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

public interface IInquiryService
{
    /// <summary>Üürija saadab päringu aktiivse kuulutuse kohta.</summary>
    Task<InquiryDto> CreateAsync(int listingId, InquiryRequestDto request, CurrentUser user);

    /// <summary>Üürileandja enda kuulutustele saabunud päringud; admin näeb kõiki.</summary>
    Task<List<InquiryDto>> GetReceivedAsync(CurrentUser user);

    /// <summary>Üks päring. Näevad saatja, kuulutuse omanik ja admin.</summary>
    Task<InquiryDto> GetByIdAsync(int id, CurrentUser user);

    /// <summary>Märgib päringu loetuks. Lubatud kuulutuse omanikul ja adminil.</summary>
    Task MarkAsReadAsync(int id, CurrentUser user);
}

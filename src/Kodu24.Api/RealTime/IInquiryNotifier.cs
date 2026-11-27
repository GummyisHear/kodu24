using Kodu24.Api.Dtos;

namespace Kodu24.Api.RealTime;

/// <summary>
/// Reaalajas teated üürileandjale. Teenuskiht kutsub seda pärast päringu salvestamist;
/// teenus ei tea, kas teade läheb WebSocketi kaudu või mujale.
/// </summary>
public interface IInquiryNotifier
{
    /// <summary>
    /// Saadab teate <c>InquiryReceived</c> grupile <c>landlord-{landlordId}</c>.
    /// Ei viska erindit: kui keegi pole ühendatud või saatmine ebaõnnestub, päring on ikkagi salvestatud.
    /// </summary>
    Task InquiryReceivedAsync(int landlordId, InquiryDto inquiry);
}

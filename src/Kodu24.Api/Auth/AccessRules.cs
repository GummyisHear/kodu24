using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;

namespace Kodu24.Api.Auth;

/// <summary>
/// Omanikupõhised ligipääsureeglid, mida rolli kontroll üksi ei kata
/// (nt üürileandja tohib muuta ainult enda kuulutusi). Rikkumisel visatakse
/// <see cref="ForbiddenException"/> → 403. Täielik tabel: docs/rollid.md.
/// </summary>
public static class AccessRules
{
    /// <summary>Kuulutust (ja selle pilte) tohib muuta või kustutada omanik või admin.</summary>
    public static void EnsureCanManageListing(this CurrentUser user, Listing listing)
    {
        if (listing.LandlordId != user.Id && !user.IsAdmin)
            throw new ForbiddenException("Saad muuta ainult enda kuulutusi.");
    }

    /// <summary>Päringut näevad selle saatja, kuulutuse omanik ja admin.</summary>
    public static void EnsureCanReadInquiry(this CurrentUser user, Inquiry inquiry)
    {
        var allowed = inquiry.SeekerId == user.Id
                      || inquiry.Listing.LandlordId == user.Id
                      || user.IsAdmin;
        if (!allowed)
            throw new ForbiddenException("Sul ei ole ligipääsu sellele päringule.");
    }

    /// <summary>Isiklikke andmeid (salvestatud otsingud, teavitused) näeb ainult omanik.</summary>
    public static void EnsureOwner(this CurrentUser user, int ownerId)
    {
        if (ownerId != user.Id)
            throw new ForbiddenException("Need andmed kuuluvad teisele kasutajale.");
    }
}

using System.Security.Claims;

namespace Kodu24.Api.Auth;

/// <summary>
/// Tokenis kasutatavad lühikesed JWT standardnimed. Sissetulevaid nimesid ei teisendata
/// (<c>MapInboundClaims = false</c>), seega kehtivad samad nimed ka kontrollerites.
/// </summary>
public static class AuthClaims
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string Name = "name";
    public const string Role = "role";

    /// <summary>Sisselogitud kasutaja ID tokenist.</summary>
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirst(UserId)?.Value
                  ?? throw new InvalidOperationException("Tokenis puudub kasutaja ID."));
}

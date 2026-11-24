using System.Security.Claims;
using Kodu24.Api.Entities;

namespace Kodu24.Api.Auth;

/// <summary>
/// Sisselogitud kasutaja tokenist. Kontroller annab selle teenusele edasi,
/// et teenus saaks kontrollida omanikuõigusi (vt <see cref="AccessRules"/>).
/// </summary>
public record CurrentUser(int Id, UserRole Role)
{
    public bool IsAdmin => Role == UserRole.Admin;
}

public static class CurrentUserExtensions
{
    public static CurrentUser GetCurrentUser(this ClaimsPrincipal principal)
    {
        var role = principal.FindFirst(AuthClaims.Role)?.Value
                   ?? throw new InvalidOperationException("Tokenis puudub roll.");
        return new CurrentUser(principal.GetUserId(), Enum.Parse<UserRole>(role));
    }
}

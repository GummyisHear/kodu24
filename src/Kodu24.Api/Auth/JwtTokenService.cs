using System.Security.Claims;
using System.Text;
using Kodu24.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Kodu24.Api.Auth;

public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _time;

    public JwtTokenService(IOptions<JwtOptions> options, TimeProvider time)
    {
        _options = options.Value;
        _time = time;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var now = _time.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(_options.ExpiresMinutes);

        var claims = new List<Claim>
        {
            new(AuthClaims.UserId, user.Id.ToString()),
            new(AuthClaims.Email, user.Email),
            new(AuthClaims.Name, user.FullName),
            new(AuthClaims.Role, user.Role.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }
}

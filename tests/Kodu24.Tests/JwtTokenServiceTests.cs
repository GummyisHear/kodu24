using Kodu24.Api.Auth;
using Kodu24.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace Kodu24.Tests;

public class JwtTokenServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);

    private static JwtTokenService CreateService()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "Kodu24",
            Audience = "Kodu24",
            Key = "test-key-that-is-at-least-32-bytes-long!!",
            ExpiresMinutes = 60,
        });
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Returns(Now);
        return new JwtTokenService(options, time.Object);
    }

    [Fact]
    public void CreateToken_ContainsUserClaims()
    {
        var user = new User { Id = 42, Email = "mari@kodu24.ee", FullName = "Mari Tamm", Role = UserRole.Landlord };

        var (token, _) = CreateService().CreateToken(user);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);

        Assert.Equal("42", jwt.GetClaim(AuthClaims.UserId).Value);
        Assert.Equal("mari@kodu24.ee", jwt.GetClaim(AuthClaims.Email).Value);
        Assert.Equal("Landlord", jwt.GetClaim(AuthClaims.Role).Value);
        Assert.Equal("Kodu24", jwt.Issuer);
    }

    [Fact]
    public void CreateToken_ExpiresAfterConfiguredMinutes()
    {
        var user = new User { Id = 1, Email = "a@b.ee", FullName = "A", Role = UserRole.Seeker };

        var (token, expiresAt) = CreateService().CreateToken(user);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);

        Assert.Equal(Now.UtcDateTime.AddMinutes(60), expiresAt);
        Assert.Equal(expiresAt, jwt.ValidTo);
    }
}

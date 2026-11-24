using System.Security.Claims;
using Kodu24.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Kodu24.Tests;

public class PoliciesTests
{
    private static readonly IAuthorizationService Authorization = new ServiceCollection()
        .AddLogging()
        .AddAuthorization(options => options.AddKodu24Policies())
        .BuildServiceProvider()
        .GetRequiredService<IAuthorizationService>();

    private static ClaimsPrincipal UserWithRole(string role) => new(new ClaimsIdentity(
        [new Claim(AuthClaims.UserId, "1"), new Claim(AuthClaims.Role, role)],
        authenticationType: "Test",
        nameType: AuthClaims.Name,
        roleType: AuthClaims.Role));

    [Theory]
    [InlineData(Policies.Landlord, Roles.Landlord, true)]
    [InlineData(Policies.Landlord, Roles.Admin, true)]
    [InlineData(Policies.Landlord, Roles.Seeker, false)]
    [InlineData(Policies.Seeker, Roles.Seeker, true)]
    [InlineData(Policies.Seeker, Roles.Landlord, false)]
    [InlineData(Policies.Admin, Roles.Admin, true)]
    [InlineData(Policies.Admin, Roles.Landlord, false)]
    [InlineData(Policies.Service, Roles.Service, true)]
    [InlineData(Policies.Service, Roles.Admin, false)]
    public async Task Policy_AllowsOnlyExpectedRoles(string policy, string role, bool expected)
    {
        var result = await Authorization.AuthorizeAsync(UserWithRole(role), policy);

        Assert.Equal(expected, result.Succeeded);
    }
}

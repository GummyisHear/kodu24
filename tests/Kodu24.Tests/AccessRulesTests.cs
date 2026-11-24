using System.Security.Claims;
using Kodu24.Api.Auth;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;

namespace Kodu24.Tests;

public class AccessRulesTests
{
    private static readonly CurrentUser Owner = new(1, UserRole.Landlord);
    private static readonly CurrentUser OtherLandlord = new(2, UserRole.Landlord);
    private static readonly CurrentUser Admin = new(3, UserRole.Admin);
    private static readonly CurrentUser Seeker = new(4, UserRole.Seeker);

    private static readonly Listing Listing = new() { Id = 10, LandlordId = 1 };

    [Fact]
    public void EnsureCanManageListing_OwnerAndAdminAllowed()
    {
        Owner.EnsureCanManageListing(Listing);
        Admin.EnsureCanManageListing(Listing);
    }

    [Fact]
    public void EnsureCanManageListing_OtherLandlordForbidden()
    {
        Assert.Throws<ForbiddenException>(() => OtherLandlord.EnsureCanManageListing(Listing));
    }

    [Fact]
    public void EnsureCanReadInquiry_SenderLandlordAndAdminAllowed_OthersForbidden()
    {
        var inquiry = new Inquiry { SeekerId = Seeker.Id, Listing = Listing, ListingId = Listing.Id };

        Seeker.EnsureCanReadInquiry(inquiry);
        Owner.EnsureCanReadInquiry(inquiry);
        Admin.EnsureCanReadInquiry(inquiry);
        Assert.Throws<ForbiddenException>(() => OtherLandlord.EnsureCanReadInquiry(inquiry));
    }

    [Fact]
    public void EnsureOwner_EvenAdminCannotReadOthersData()
    {
        Seeker.EnsureOwner(Seeker.Id);
        Assert.Throws<ForbiddenException>(() => Admin.EnsureOwner(Seeker.Id));
    }

    [Fact]
    public void GetCurrentUser_ReadsIdAndRoleFromToken()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(AuthClaims.UserId, "7"), new Claim(AuthClaims.Role, "Seeker")], "Test"));

        var user = principal.GetCurrentUser();

        Assert.Equal(new CurrentUser(7, UserRole.Seeker), user);
    }
}

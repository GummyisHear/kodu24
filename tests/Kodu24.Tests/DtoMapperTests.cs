using Kodu24.Api.Entities;
using Kodu24.Api.Mapping;

namespace Kodu24.Tests;

public class DtoMapperTests
{
    private static Listing SampleListing() => new()
    {
        Id = 7,
        Title = "Hubane kahetoaline",
        Address = "Tartu mnt 1",
        DistrictId = 1,
        District = new District { Id = 1, Name = "Kesklinn" },
        Landlord = new User { Id = 3, FullName = "Mari Maasikas", Phone = "5551234", Email = "mari@example.com", PasswordHash = "x" },
        Price = 650m,
        AreaM2 = 48m,
        Rooms = 2,
        Latitude = 59.4370,
        Longitude = 24.7536,
        Images =
        {
            new ListingImage { Id = 1, FileName = "b.jpg", SortOrder = 2 },
            new ListingImage { Id = 2, FileName = "a.jpg", SortOrder = 1 },
        },
    };

    [Theory]
    [InlineData(650, 48, 13.54)]
    [InlineData(500, 0, 0)]
    public void PricePerM2_IsRoundedAndSafeForZeroArea(decimal price, decimal area, decimal expected)
    {
        Assert.Equal(expected, DtoMapper.PricePerM2(price, area));
    }

    [Fact]
    public void ToMyListingDto_CountsImagesAndUnreadInquiries()
    {
        var listing = SampleListing();
        listing.Inquiries.Add(new Inquiry { Id = 1, IsRead = true });
        listing.Inquiries.Add(new Inquiry { Id = 2, IsRead = false });
        listing.Inquiries.Add(new Inquiry { Id = 3, IsRead = false });

        var dto = listing.ToMyListingDto();

        Assert.Equal(2, dto.ImageCount);
        Assert.Equal(3, dto.InquiryCount);
        Assert.Equal(2, dto.UnreadInquiryCount);
        Assert.Equal("/uploads/a.jpg", dto.PrimaryImageUrl);
    }

    [Fact]
    public void ToListItemDto_UsesPrimaryImage_OrFirstBySortOrder()
    {
        var listing = SampleListing();

        Assert.Equal("/uploads/a.jpg", listing.ToListItemDto().PrimaryImageUrl);

        listing.Images[0].IsPrimary = true;
        Assert.Equal("/uploads/b.jpg", listing.ToListItemDto().PrimaryImageUrl);
    }

    [Fact]
    public void ToListItemDto_NoImages_PrimaryImageIsNull()
    {
        var listing = SampleListing();
        listing.Images.Clear();

        Assert.Null(listing.ToListItemDto().PrimaryImageUrl);
    }

    [Fact]
    public void ToListItemDto_IncludesCoordinatesForMap()
    {
        var dto = SampleListing().ToListItemDto();

        Assert.Equal(59.4370, dto.Latitude);
        Assert.Equal(24.7536, dto.Longitude);
    }

    [Fact]
    public void ToDetailDto_MapsDistrictLandlordAndOrdersImages()
    {
        var dto = SampleListing().ToDetailDto();

        Assert.Equal("Kesklinn", dto.DistrictName);
        Assert.Equal(13.54m, dto.PricePerM2);
        Assert.Equal("Mari Maasikas", dto.Landlord.FullName);
        Assert.Equal(new[] { "/uploads/a.jpg", "/uploads/b.jpg" }, dto.Images.Select(i => i.Url));
    }

    [Fact]
    public void UserToDto_ExposesRoleAsString()
    {
        var user = new User { Id = 1, Email = "a@b.ee", FullName = "A", PasswordHash = "secret", Role = UserRole.Landlord };

        var dto = user.ToDto();

        Assert.Equal("Landlord", dto.Role);
        Assert.DoesNotContain(typeof(Kodu24.Api.Dtos.UserDto).GetProperties(), p => p.Name.Contains("Password"));
    }
}

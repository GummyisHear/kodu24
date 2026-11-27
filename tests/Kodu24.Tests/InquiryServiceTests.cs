using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.RealTime;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kodu24.Tests;

public class InquiryServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly CurrentUser Seeker = new(5, UserRole.Seeker);
    private static readonly CurrentUser Landlord = new(1, UserRole.Landlord);
    private static readonly CurrentUser OtherLandlord = new(2, UserRole.Landlord);
    private static readonly CurrentUser Admin = new(9, UserRole.Admin);

    private readonly Mock<IInquiryRepository> _inquiries = new();
    private readonly Mock<IListingRepository> _listings = new();
    private readonly Mock<IInquiryNotifier> _notifier = new();

    private InquiryService CreateService()
    {
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Returns(Now);
        return new InquiryService(_inquiries.Object, _listings.Object, _notifier.Object, time.Object, NullLogger<InquiryService>.Instance);
    }

    private static Listing ListingOf(int landlordId) => new() { Id = 10, LandlordId = landlordId, Title = "Kahetoaline" };

    private static Inquiry InquiryFor(Listing listing) => new()
    {
        Id = 3,
        ListingId = listing.Id,
        Listing = listing,
        SeekerId = Seeker.Id,
        Seeker = new User { Id = Seeker.Id, FullName = "Liis Mägi", Email = "liis@kodu24.ee" },
        Message = "Kas korter on vaba?",
    };

    private static InquiryRequestDto Request(string message = "  Tere, kas korter on veel vaba?  ") => new() { Message = message };

    [Fact]
    public async Task CreateAsync_SavesInquiryFromTokenUserWithServerTime()
    {
        var listing = ListingOf(Landlord.Id);
        _listings.Setup(l => l.GetByIdAsync(10)).ReturnsAsync(listing);
        Inquiry? saved = null;
        _inquiries.Setup(i => i.AddAsync(It.IsAny<Inquiry>())).Callback<Inquiry>(i =>
        {
            saved = i;
            i.Listing = listing;
            i.Seeker = new User { Id = Seeker.Id, FullName = "Liis Mägi", Email = "liis@kodu24.ee" };
        });
        _inquiries.Setup(i => i.GetWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(() => saved);

        var dto = await CreateService().CreateAsync(10, Request(), Seeker);

        Assert.NotNull(saved);
        Assert.Equal(Seeker.Id, saved.SeekerId);
        Assert.Equal(10, saved.ListingId);
        Assert.Equal(Now.UtcDateTime, saved.CreatedAt);
        Assert.False(saved.IsRead);
        Assert.Equal("Tere, kas korter on veel vaba?", dto.Message);
        _inquiries.Verify(i => i.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NotifiesListingOwnerAfterSaving()
    {
        var listing = ListingOf(Landlord.Id);
        _listings.Setup(l => l.GetByIdAsync(10)).ReturnsAsync(listing);
        var calls = new List<string>();
        _inquiries.Setup(i => i.SaveChangesAsync()).Callback(() => calls.Add("save")).ReturnsAsync(1);
        _inquiries.Setup(i => i.GetWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(InquiryFor(listing));
        _notifier.Setup(n => n.InquiryReceivedAsync(It.IsAny<int>(), It.IsAny<InquiryDto>()))
            .Callback(() => calls.Add("notify")).Returns(Task.CompletedTask);

        await CreateService().CreateAsync(10, Request(), Seeker);

        // Teade läheb kuulutuse omanikule ja alles siis, kui päring on andmebaasis.
        Assert.Equal(["save", "notify"], calls);
        _notifier.Verify(n => n.InquiryReceivedAsync(Landlord.Id, It.Is<InquiryDto>(d => d.ListingId == 10)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_MissingListing_ThrowsNotFoundAndSavesNothing()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().CreateAsync(99, Request(), Seeker));

        _inquiries.Verify(i => i.AddAsync(It.IsAny<Inquiry>()), Times.Never);
        _notifier.Verify(n => n.InquiryReceivedAsync(It.IsAny<int>(), It.IsAny<InquiryDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_OwnListing_ThrowsValidationError()
    {
        // Seeker ei saa kuulutust omada, aga teenus ei tohi sellele lootma jääda.
        _listings.Setup(l => l.GetByIdAsync(10)).ReturnsAsync(ListingOf(Seeker.Id));

        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() => CreateService().CreateAsync(10, Request(), Seeker));

        Assert.True(ex.Errors.ContainsKey("Message"));
    }

    [Fact]
    public async Task GetReceivedAsync_LandlordSeesOnlyOwn_AdminSeesAll()
    {
        _inquiries.Setup(i => i.GetReceivedAsync(It.IsAny<int?>())).ReturnsAsync([]);

        await CreateService().GetReceivedAsync(Landlord);
        await CreateService().GetReceivedAsync(Admin);

        _inquiries.Verify(i => i.GetReceivedAsync(Landlord.Id), Times.Once);
        _inquiries.Verify(i => i.GetReceivedAsync(null), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_ByOwner_SetsIsRead()
    {
        var inquiry = InquiryFor(ListingOf(Landlord.Id));
        _inquiries.Setup(i => i.GetWithDetailsAsync(3)).ReturnsAsync(inquiry);

        await CreateService().MarkAsReadAsync(3, Landlord);

        Assert.True(inquiry.IsRead);
        _inquiries.Verify(i => i.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_ByOtherLandlord_ThrowsForbidden()
    {
        var inquiry = InquiryFor(ListingOf(Landlord.Id));
        _inquiries.Setup(i => i.GetWithDetailsAsync(3)).ReturnsAsync(inquiry);

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateService().MarkAsReadAsync(3, OtherLandlord));

        Assert.False(inquiry.IsRead);
        _inquiries.Verify(i => i.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_BySender_ReturnsInquiry()
    {
        _inquiries.Setup(i => i.GetWithDetailsAsync(3)).ReturnsAsync(InquiryFor(ListingOf(Landlord.Id)));

        var dto = await CreateService().GetByIdAsync(3, Seeker);

        Assert.Equal("Liis Mägi", dto.SeekerName);
    }
}

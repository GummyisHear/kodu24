using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Mapping;
using Kodu24.Api.Repositories;

namespace Kodu24.Api.Services;

public class InquiryService : IInquiryService
{
    private readonly IInquiryRepository _inquiries;
    private readonly IListingRepository _listings;
    private readonly TimeProvider _time;
    private readonly ILogger<InquiryService> _logger;

    public InquiryService(
        IInquiryRepository inquiries,
        IListingRepository listings,
        TimeProvider time,
        ILogger<InquiryService> logger)
    {
        _inquiries = inquiries;
        _listings = listings;
        _time = time;
        _logger = logger;
    }

    public async Task<InquiryDto> CreateAsync(int listingId, InquiryRequestDto request, CurrentUser user)
    {
        // Kustutatud ja aegunud kuulutused on päringufiltriga peidetud → 404.
        var listing = await _listings.GetByIdAsync(listingId)
                      ?? throw new NotFoundException($"Kuulutust {listingId} ei leitud.");

        if (listing.LandlordId == user.Id)
            throw new ValidationFailedException(nameof(InquiryRequestDto.Message), "Enda kuulutusele ei saa päringut saata.");

        var inquiry = new Inquiry
        {
            ListingId = listing.Id,
            SeekerId = user.Id,
            Message = request.Message.Trim(),
            CreatedAt = _time.GetUtcNow().UtcDateTime,
        };

        await _inquiries.AddAsync(inquiry);
        await _inquiries.SaveChangesAsync();
        _logger.LogInformation("Kasutaja {UserId} saatis päringu {InquiryId} kuulutusele {ListingId}",
            user.Id, inquiry.Id, listing.Id);

        // #20: siin (pärast salvestamist) saadetakse üürileandjale WebSocketi teade InquiryReceived.

        return (await FindAsync(inquiry.Id)).ToDto();
    }

    public async Task<List<InquiryDto>> GetReceivedAsync(CurrentUser user)
    {
        var inquiries = await _inquiries.GetReceivedAsync(user.IsAdmin ? null : user.Id);
        return inquiries.Select(i => i.ToDto()).ToList();
    }

    public async Task<InquiryDto> GetByIdAsync(int id, CurrentUser user)
    {
        var inquiry = await FindAsync(id);
        user.EnsureCanReadInquiry(inquiry);
        return inquiry.ToDto();
    }

    public async Task MarkAsReadAsync(int id, CurrentUser user)
    {
        var inquiry = await FindAsync(id);
        // Loetuks märkida saab ainult see, kes päringu sai (kuulutuse omanik) või admin.
        user.EnsureCanManageListing(inquiry.Listing);

        if (inquiry.IsRead)
            return;

        inquiry.IsRead = true;
        await _inquiries.SaveChangesAsync();
    }

    private async Task<Inquiry> FindAsync(int id) =>
        await _inquiries.GetWithDetailsAsync(id)
        ?? throw new NotFoundException($"Päringut {id} ei leitud.");
}

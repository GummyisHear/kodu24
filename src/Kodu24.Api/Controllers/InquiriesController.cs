using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Controllers;

/// <summary>Üürijate päringud üürileandjatele.</summary>
[ApiController]
[Produces("application/json")]
public class InquiriesController : ControllerBase
{
    private readonly IInquiryService _inquiries;

    public InquiriesController(IInquiryService inquiries)
    {
        _inquiries = inquiries;
    }

    /// <summary>Saadab kuulutuse üürileandjale päringu.</summary>
    /// <remarks>Saatja võetakse tokenist. Üürileandja näeb päringut lehel "Saabunud päringud".</remarks>
    /// <param name="listingId">Kuulutuse ID.</param>
    /// <param name="request">Sõnum üürileandjale.</param>
    /// <response code="201">Päring saadeti.</response>
    /// <response code="400">Sõnum on vigane.</response>
    /// <response code="404">Kuulutust ei ole või see on kustutatud/aegunud.</response>
    [HttpPost("api/listings/{listingId:int}/inquiries")]
    [Authorize(Policy = Policies.Seeker)]
    [ProducesResponseType<InquiryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InquiryDto>> Create(int listingId, InquiryRequestDto request)
    {
        var inquiry = await _inquiries.CreateAsync(listingId, request, User.GetCurrentUser());
        return CreatedAtAction(nameof(GetById), new { id = inquiry.Id }, inquiry);
    }

    /// <summary>Sisselogitud üürileandja kuulutustele saabunud päringud, uuemad eespool. Admin näeb kõiki.</summary>
    /// <response code="200">Päringute nimekiri.</response>
    [HttpGet("api/inquiries")]
    [Authorize(Policy = Policies.Landlord)]
    [ProducesResponseType<List<InquiryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<InquiryDto>>> GetReceived() =>
        Ok(await _inquiries.GetReceivedAsync(User.GetCurrentUser()));

    /// <summary>Üks päring. Näevad saatja, kuulutuse omanik ja admin.</summary>
    /// <param name="id">Päringu ID.</param>
    /// <response code="200">Päring.</response>
    /// <response code="403">Päring ei puuduta sind.</response>
    /// <response code="404">Päringut ei leitud.</response>
    [HttpGet("api/inquiries/{id:int}")]
    [Authorize]
    [ProducesResponseType<InquiryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InquiryDto>> GetById(int id) =>
        Ok(await _inquiries.GetByIdAsync(id, User.GetCurrentUser()));

    /// <summary>Märgib päringu loetuks.</summary>
    /// <param name="id">Päringu ID.</param>
    /// <response code="204">Päring on loetud.</response>
    /// <response code="403">Päring on saadetud teise üürileandja kuulutusele.</response>
    /// <response code="404">Päringut ei leitud.</response>
    [HttpPatch("api/inquiries/{id:int}/read")]
    [Authorize(Policy = Policies.Landlord)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _inquiries.MarkAsReadAsync(id, User.GetCurrentUser());
        return NoContent();
    }
}

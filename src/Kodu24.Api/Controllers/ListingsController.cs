using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Controllers;

/// <summary>Üürikuulutused.</summary>
[ApiController]
[Route("api/listings")]
[Produces("application/json")]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listings;

    public ListingsController(IListingService listings)
    {
        _listings = listings;
    }

    /// <summary>Tagastab ühe aktiivse kuulutuse koos piltide ja üürileandja kontaktiga.</summary>
    /// <param name="id">Kuulutuse ID.</param>
    /// <response code="200">Kuulutus.</response>
    /// <response code="404">Kuulutust ei ole või see on kustutatud/aegunud.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ListingDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingDetailDto>> GetById(int id) =>
        Ok(await _listings.GetByIdAsync(id));

    /// <summary>Lisab uue kuulutuse sisselogitud üürileandjale.</summary>
    /// <remarks>Kuulutus on aktiivne 30 päeva, seejärel märgib taustateenus selle aegunuks.</remarks>
    /// <response code="201">Kuulutus loodi; <c>Location</c> päis viitab uuele kuulutusele.</response>
    /// <response code="400">Sisendandmed on vigased.</response>
    [HttpPost]
    [Authorize(Policy = Policies.Landlord)]
    [ProducesResponseType<ListingDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ListingDetailDto>> Create(ListingRequestDto request)
    {
        var listing = await _listings.CreateAsync(request, User.GetCurrentUser());
        return CreatedAtAction(nameof(GetById), new { id = listing.Id }, listing);
    }

    /// <summary>Muudab kuulutust. Üürileandja saab muuta ainult enda kuulutusi, admin kõiki.</summary>
    /// <param name="id">Kuulutuse ID.</param>
    /// <param name="request">Kuulutuse uued andmed (kõik väljad).</param>
    /// <response code="200">Muudetud kuulutus.</response>
    /// <response code="400">Sisendandmed on vigased.</response>
    /// <response code="403">Kuulutus kuulub teisele üürileandjale.</response>
    /// <response code="404">Kuulutust ei leitud.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.Landlord)]
    [ProducesResponseType<ListingDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingDetailDto>> Update(int id, ListingRequestDto request) =>
        Ok(await _listings.UpdateAsync(id, request, User.GetCurrentUser()));

    /// <summary>Kustutab kuulutuse (soft delete: kaob otsingust, aga ajalugu säilib).</summary>
    /// <param name="id">Kuulutuse ID.</param>
    /// <response code="204">Kuulutus kustutati.</response>
    /// <response code="403">Kuulutus kuulub teisele üürileandjale.</response>
    /// <response code="404">Kuulutust ei leitud.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.Landlord)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _listings.DeleteAsync(id, User.GetCurrentUser());
        return NoContent();
    }
}

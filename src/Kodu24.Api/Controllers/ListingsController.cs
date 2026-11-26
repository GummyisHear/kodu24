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

    /// <summary>Otsib aktiivseid kuulutusi. Kõik filtrid on valikulised ja kombineeritavad.</summary>
    /// <remarks>Näide: <c>GET /api/listings?minPrice=400&amp;maxPrice=900&amp;rooms=2&amp;district=2&amp;sort=price_asc&amp;page=1</c></remarks>
    /// <response code="200">Üks lehekülg kuulutusi koos kogusummaga.</response>
    /// <response code="400">Parameetrid on vigased (nt minimaalne hind on suurem kui maksimaalne).</response>
    [HttpGet]
    [ProducesResponseType<PagedResultDto<ListingListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<ListingListItemDto>>> Search([FromQuery] ListingQueryDto query) =>
        Ok(await _listings.SearchAsync(query));

    /// <summary>Sisselogitud üürileandja enda kuulutused koos piltide ja päringute arvuga, uuemad eespool.</summary>
    /// <remarks>Leht "Minu kuulutused". Kustutatud kuulutusi siin ei ole; aegunud on seni, kuni taustateenus need eemaldab.</remarks>
    /// <response code="200">Kuulutuste nimekiri (võib olla tühi).</response>
    [HttpGet("mine")]
    [Authorize(Policy = Policies.Landlord)]
    [ProducesResponseType<List<MyListingDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MyListingDto>>> GetMine() =>
        Ok(await _listings.GetMineAsync(User.GetCurrentUser()));

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

    /// <summary>Taustateenus: kustutab (soft delete) kõik kuulutused, mille kehtivus on lõppenud.</summary>
    /// <remarks>Taustateenus kutsub seda kord tunnis. Korduv kutse on ohutu: juba kustutatud kuulutusi ei puudutata.</remarks>
    /// <response code="200">Mitu kuulutust märgiti aegunuks.</response>
    [HttpPost("expire")]
    [Authorize(Policy = Policies.Service)]
    [ProducesResponseType<ExpireResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ExpireResultDto>> Expire() =>
        Ok(new ExpireResultDto(await _listings.ExpireListingsAsync()));
}

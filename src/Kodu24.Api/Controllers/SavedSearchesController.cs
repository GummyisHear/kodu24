using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Controllers;

/// <summary>
/// Üürija salvestatud otsingud. Taustateenus otsib nende järgi uusi kuulutusi ja saadab teavitusi.
/// Igaüks näeb ja muudab ainult enda otsinguid.
/// </summary>
[ApiController]
[Route("api/saved-searches")]
[Authorize(Policy = Policies.Seeker)]
[Produces("application/json")]
public class SavedSearchesController : ControllerBase
{
    private readonly ISavedSearchService _searches;

    public SavedSearchesController(ISavedSearchService searches)
    {
        _searches = searches;
    }

    /// <summary>Sisselogitud kasutaja salvestatud otsingud, uuemad eespool.</summary>
    /// <response code="200">Otsingute nimekiri.</response>
    [HttpGet]
    [ProducesResponseType<List<SavedSearchDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SavedSearchDto>>> GetMine() =>
        Ok(await _searches.GetMineAsync(User.GetCurrentUser()));

    /// <summary>Üks salvestatud otsing.</summary>
    /// <param name="id">Otsingu ID.</param>
    /// <response code="200">Otsing.</response>
    /// <response code="403">Otsing kuulub teisele kasutajale.</response>
    /// <response code="404">Otsingut ei leitud.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<SavedSearchDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SavedSearchDto>> GetById(int id) =>
        Ok(await _searches.GetByIdAsync(id, User.GetCurrentUser()));

    /// <summary>Salvestab otsingu.</summary>
    /// <remarks>
    /// Kõik filtrid on valikulised. Teavitused tulevad ainult kuulutustest, mis lisatakse pärast salvestamist.
    /// Ühel kasutajal võib olla kuni 20 otsingut.
    /// </remarks>
    /// <param name="request">Otsingu nimi ja filtrid.</param>
    /// <response code="201">Otsing salvestati.</response>
    /// <response code="400">Andmed on vigased (nt min hind > max hind, tundmatu linnaosa, liiga palju otsinguid).</response>
    [HttpPost]
    [ProducesResponseType<SavedSearchDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SavedSearchDto>> Create(SavedSearchRequestDto request)
    {
        var search = await _searches.CreateAsync(request, User.GetCurrentUser());
        return CreatedAtAction(nameof(GetById), new { id = search.Id }, search);
    }

    /// <summary>Muudab otsingut (asendab nime ja kõik filtrid).</summary>
    /// <param name="id">Otsingu ID.</param>
    /// <param name="request">Uus nimi ja filtrid. Puuduv filter tähendab "ükskõik".</param>
    /// <response code="200">Muudetud otsing.</response>
    /// <response code="400">Andmed on vigased.</response>
    /// <response code="403">Otsing kuulub teisele kasutajale.</response>
    /// <response code="404">Otsingut ei leitud.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType<SavedSearchDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SavedSearchDto>> Update(int id, SavedSearchRequestDto request) =>
        Ok(await _searches.UpdateAsync(id, request, User.GetCurrentUser()));

    /// <summary>Kustutab otsingu koos selle teavitustega.</summary>
    /// <param name="id">Otsingu ID.</param>
    /// <response code="204">Otsing kustutati.</response>
    /// <response code="403">Otsing kuulub teisele kasutajale.</response>
    /// <response code="404">Otsingut ei leitud.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _searches.DeleteAsync(id, User.GetCurrentUser());
        return NoContent();
    }
}

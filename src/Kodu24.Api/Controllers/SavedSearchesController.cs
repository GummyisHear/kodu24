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
[Produces("application/json")]
public class SavedSearchesController : ControllerBase
{
    private readonly ISavedSearchService _searches;

    public SavedSearchesController(ISavedSearchService searches)
    {
        _searches = searches;
    }

    /// <summary>Salvestatud otsingud: üürijale tema enda omad (uuemad eespool), taustateenusele kõigi kasutajate omad.</summary>
    /// <remarks>Taustateenus kasutab nimekirja, et iga otsingu kohta uusi kuulutusi leida (<c>createdAfter = lastNotifiedAt</c>).</remarks>
    /// <response code="200">Otsingute nimekiri.</response>
    [HttpGet]
    [Authorize(Policy = Policies.SeekerOrService)]
    [ProducesResponseType<List<SavedSearchDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SavedSearchDto>>> GetList() =>
        Ok(await _searches.GetListAsync(User.GetCurrentUser()));

    /// <summary>Üks salvestatud otsing.</summary>
    /// <param name="id">Otsingu ID.</param>
    /// <response code="200">Otsing.</response>
    /// <response code="403">Otsing kuulub teisele kasutajale.</response>
    /// <response code="404">Otsingut ei leitud.</response>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.Seeker)]
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
    [Authorize(Policy = Policies.Seeker)]
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
    [Authorize(Policy = Policies.Seeker)]
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
    [Authorize(Policy = Policies.Seeker)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _searches.DeleteAsync(id, User.GetCurrentUser());
        return NoContent();
    }

    /// <summary>Taustateenus: märgib, milleni otsingu kohta on teavitused saadetud.</summary>
    /// <remarks>
    /// <c>notifiedAt</c> on viimase teavitatud kuulutuse loomise aeg. Järgmisel korral otsitakse ainult
    /// sellest hilisemaid kuulutusi. Kursor liigub ainult edasi: varasem aeg jäetakse tähelepanuta.
    /// </remarks>
    /// <param name="id">Otsingu ID.</param>
    /// <param name="request">Uus kursori väärtus (UTC).</param>
    /// <response code="204">Kursor uuendati (või oli juba hilisem).</response>
    /// <response code="400">Aeg puudub või on tulevikus.</response>
    /// <response code="404">Otsingut ei leitud.</response>
    [HttpPatch("{id:int}/notified")]
    [Authorize(Policy = Policies.Service)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkNotified(int id, MarkNotifiedRequestDto request)
    {
        await _searches.MarkNotifiedAsync(id, request.NotifiedAt!.Value);
        return NoContent();
    }
}

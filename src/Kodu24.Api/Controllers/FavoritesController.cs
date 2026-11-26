using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Controllers;

/// <summary>Üürija lemmikkuulutused. Igaüks näeb ja muudab ainult enda lemmikuid.</summary>
[ApiController]
[Route("api/favorites")]
[Authorize(Policy = Policies.Seeker)]
[Produces("application/json")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favorites;

    public FavoritesController(IFavoriteService favorites)
    {
        _favorites = favorites;
    }

    /// <summary>Sisselogitud kasutaja lemmikud, viimati lisatud eespool.</summary>
    /// <remarks>Kustutatud kuulutused lemmikute hulka ei tule.</remarks>
    /// <response code="200">Lemmikute nimekiri.</response>
    [HttpGet]
    [ProducesResponseType<List<FavoriteDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FavoriteDto>>> GetMine() =>
        Ok(await _favorites.GetMineAsync(User.GetCurrentUser()));

    /// <summary>Lemmikuks märgitud kuulutuste ID-d.</summary>
    /// <remarks>Kuulutuste nimekiri kasutab seda, et näidata, millised kuulutused on juba lemmikud.</remarks>
    /// <response code="200">Kuulutuste ID-d.</response>
    [HttpGet("ids")]
    [ProducesResponseType<List<int>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<int>>> GetMyListingIds() =>
        Ok(await _favorites.GetMyListingIdsAsync(User.GetCurrentUser()));

    /// <summary>Lisab kuulutuse lemmikuks.</summary>
    /// <remarks>Kui kuulutus on juba lemmik, vastatakse 200 olemasoleva lemmikuga (korduv päring ei tekita viga ega duplikaati).</remarks>
    /// <param name="listingId">Kuulutuse ID.</param>
    /// <response code="201">Kuulutus lisati lemmikuks.</response>
    /// <response code="200">Kuulutus oli juba lemmik.</response>
    /// <response code="404">Kuulutust ei ole või see on kustutatud.</response>
    [HttpPost("{listingId:int}")]
    [ProducesResponseType<FavoriteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<FavoriteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FavoriteDto>> Add(int listingId)
    {
        var (favorite, created) = await _favorites.AddAsync(listingId, User.GetCurrentUser());
        return created ? CreatedAtAction(nameof(GetMine), null, favorite) : Ok(favorite);
    }

    /// <summary>Eemaldab kuulutuse lemmikutest.</summary>
    /// <param name="listingId">Kuulutuse ID.</param>
    /// <response code="204">Kuulutus eemaldati lemmikutest.</response>
    /// <response code="404">Kuulutus ei ole sinu lemmikute hulgas.</response>
    [HttpDelete("{listingId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(int listingId)
    {
        await _favorites.RemoveAsync(listingId, User.GetCurrentUser());
        return NoContent();
    }
}

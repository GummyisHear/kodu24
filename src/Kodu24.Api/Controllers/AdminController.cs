using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Controllers;

/// <summary>
/// Haldus: kasutajad, kõik kuulutused (ka kustutatud) ja portaali ülevaade. Ainult admin.
/// Kuulutuste muutmine ja kustutamine käib tavaliste otspunktidega (<c>PUT/DELETE /api/listings/{id}</c>), kus admin võib muuta kõiki.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Policy = Policies.Admin)]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin)
    {
        _admin = admin;
    }

    /// <summary>Kasutajad (otsing nime või e-posti järgi, rolli filter), uuemad eespool.</summary>
    /// <param name="query">Otsingusõna, roll ja lehekülg.</param>
    /// <response code="200">Üks lehekülg kasutajaid.</response>
    /// <response code="400">Vigane parameeter (nt tundmatu roll).</response>
    [HttpGet("users")]
    [ProducesResponseType<PagedResultDto<AdminUserDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<AdminUserDto>>> GetUsers([FromQuery] AdminUserQueryDto query) =>
        Ok(await _admin.GetUsersAsync(query));

    /// <summary>Muudab kasutaja rolli.</summary>
    /// <remarks>
    /// Enda rolli ja teenusekonto rolli muuta ei saa. Uus roll hakkab kehtima, kui kasutaja uuesti sisse logib
    /// (vana token kehtib kuni aegumiseni).
    /// </remarks>
    /// <param name="id">Kasutaja ID.</param>
    /// <param name="request">Uus roll.</param>
    /// <response code="200">Muudetud kasutaja.</response>
    /// <response code="400">Rolli ei saa nii muuta.</response>
    /// <response code="404">Kasutajat ei leitud.</response>
    [HttpPatch("users/{id:int}/role")]
    [ProducesResponseType<AdminUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDto>> ChangeRole(int id, ChangeRoleRequestDto request) =>
        Ok(await _admin.ChangeRoleAsync(id, request, User.GetCurrentUser()));

    /// <summary>Kõik kuulutused, ka kustutatud ja aegunud, uuemad eespool.</summary>
    /// <param name="query">Otsingusõna, olek (active/deleted/all) ja lehekülg.</param>
    /// <response code="200">Üks lehekülg kuulutusi.</response>
    /// <response code="400">Vigane parameeter.</response>
    [HttpGet("listings")]
    [ProducesResponseType<PagedResultDto<AdminListingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<AdminListingDto>>> GetListings([FromQuery] AdminListingQueryDto query) =>
        Ok(await _admin.GetListingsAsync(query));

    /// <summary>Taastab kustutatud (või aegunud) kuulutuse.</summary>
    /// <remarks>Kui kuulutuse kehtivus on läbi, pikendatakse seda 30 päeva, muidu kustutaks taustateenus selle kohe uuesti.</remarks>
    /// <param name="id">Kuulutuse ID.</param>
    /// <response code="200">Taastatud kuulutus.</response>
    /// <response code="404">Kuulutust ei leitud.</response>
    /// <response code="409">Kuulutus ei ole kustutatud.</response>
    [HttpPost("listings/{id:int}/restore")]
    [ProducesResponseType<AdminListingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminListingDto>> RestoreListing(int id) =>
        Ok(await _admin.RestoreListingAsync(id));

    /// <summary>Portaali ülevaade: kasutajad rollide kaupa, kuulutused, päringud, lemmikud, otsingud, teavitused.</summary>
    /// <response code="200">Arvud.</response>
    [HttpGet("stats")]
    [ProducesResponseType<AdminStatsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminStatsDto>> GetStats() => Ok(await _admin.GetStatsAsync());
}

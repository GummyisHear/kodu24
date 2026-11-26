using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Controllers;

/// <summary>
/// Teavitused salvestatud otsingutele vastavatest uutest kuulutustest. Teavitusi loob taustateenus,
/// kasutaja näeb ainult enda omi.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications)
    {
        _notifications = notifications;
    }

    /// <summary>Sisselogitud kasutaja teavitused, uuemad eespool.</summary>
    /// <response code="200">Teavituste nimekiri.</response>
    [HttpGet]
    [Authorize(Policy = Policies.Seeker)]
    [ProducesResponseType<List<NotificationDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NotificationDto>>> GetMine() =>
        Ok(await _notifications.GetMineAsync(User.GetCurrentUser()));

    /// <summary>Taustateenus: loob teavitused ühe salvestatud otsingu uute kuulutuste kohta.</summary>
    /// <remarks>
    /// Teavituse saab otsingu omanik. Kuulutused, mida pole (või mis on kustutatud), ja kuulutused,
    /// mille kohta selle otsingu teavitus juba on, jäetakse vahele, nii et päringut võib turvaliselt korrata.
    /// </remarks>
    /// <param name="request">Otsingu ID ja uute kuulutuste ID-d.</param>
    /// <response code="200">Loodud ja vahele jäetud teavituste arv.</response>
    /// <response code="400">Andmed on vigased.</response>
    /// <response code="404">Salvestatud otsingut ei leitud.</response>
    [HttpPost]
    [Authorize(Policy = Policies.Service)]
    [ProducesResponseType<CreateNotificationsResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateNotificationsResultDto>> Create(CreateNotificationsRequestDto request) =>
        Ok(await _notifications.CreateForSavedSearchAsync(request));

    /// <summary>Märgib teavituse loetuks.</summary>
    /// <param name="id">Teavituse ID.</param>
    /// <response code="204">Teavitus on loetud.</response>
    /// <response code="403">Teavitus kuulub teisele kasutajale.</response>
    /// <response code="404">Teavitust ei leitud.</response>
    [HttpPatch("{id:int}/read")]
    [Authorize(Policy = Policies.Seeker)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notifications.MarkAsReadAsync(id, User.GetCurrentUser());
        return NoContent();
    }
}

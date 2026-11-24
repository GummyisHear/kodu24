using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Controllers;

/// <summary>Registreerimine, sisselogimine ja sisselogitud kasutaja andmed.</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Registreerib uue üürileandja või üürija ja logib ta kohe sisse.</summary>
    /// <response code="201">Kasutaja loodi; vastuses on JWT.</response>
    /// <response code="400">Sisendandmed on vigased.</response>
    /// <response code="409">Selle e-postiga kasutaja on juba olemas.</response>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
    {
        var result = await _auth.RegisterAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Logib sisse e-posti ja parooliga ning tagastab JWT.</summary>
    /// <remarks>
    /// Saadud tokeni saab Swaggeris sisestada nupu <b>Authorize</b> alt.
    /// Testkontode parool on <c>Parool123!</c> (nt <c>mari@kodu24.ee</c> üürileandja, <c>liis@kodu24.ee</c> üürija).
    /// </remarks>
    /// <response code="200">Sisselogimine õnnestus.</response>
    /// <response code="400">Sisendandmed on vigased.</response>
    /// <response code="401">Vale e-post või parool.</response>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request) =>
        Ok(await _auth.LoginAsync(request));

    /// <summary>Tagastab sisselogitud kasutaja andmed.</summary>
    /// <response code="200">Kasutaja andmed.</response>
    /// <response code="401">Token puudub või on aegunud.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Me() =>
        Ok(await _auth.GetUserAsync(User.GetUserId()));
}

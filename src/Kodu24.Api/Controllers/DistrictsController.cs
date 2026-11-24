using Kodu24.Api.Dtos;
using Kodu24.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Controllers;

/// <summary>Linnaosad, mille järgi kuulutusi filtreeritakse.</summary>
[ApiController]
[Route("api/districts")]
[Produces("application/json")]
public class DistrictsController : ControllerBase
{
    private readonly IDistrictService _districts;

    public DistrictsController(IDistrictService districts)
    {
        _districts = districts;
    }

    /// <summary>Tagastab kõik linnaosad tähestikulises järjekorras.</summary>
    /// <response code="200">Linnaosade nimekiri.</response>
    [HttpGet]
    [ProducesResponseType<List<DistrictDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DistrictDto>>> GetAll() =>
        Ok(await _districts.GetAllAsync());
}

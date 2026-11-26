using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Controllers;

/// <summary>Kuulutuse pildid. Haldab kuulutuse omanik või admin.</summary>
[ApiController]
[Route("api/listings/{listingId:int}/images")]
[Authorize(Policy = Policies.Landlord)]
[Produces("application/json")]
public class ListingImagesController : ControllerBase
{
    // 10 pilti × 5 MB + vormi ülejäänud osa.
    private const long MaxRequestBytes = 60 * 1024 * 1024;

    private readonly IListingImageService _images;

    public ListingImagesController(IListingImageService images)
    {
        _images = images;
    }

    /// <summary>Laeb kuulutusele üles ühe või mitu pilti (multipart/form-data, väli <c>files</c>).</summary>
    /// <remarks>
    /// Lubatud JPEG, PNG ja WebP, kuni 5 MB pilt, kokku kuni 10 pilti kuulutuse kohta.
    /// Tüüp kontrollitakse faili sisu järgi. Esimene pilt muutub põhipildiks, kui seda veel pole.
    /// </remarks>
    /// <param name="listingId">Kuulutuse ID.</param>
    /// <param name="files">Pildifailid.</param>
    /// <response code="201">Pildid lisati; vastuses on kuulutuse kõik pildid.</response>
    /// <response code="400">Fail puudub, ei ole pilt, on liiga suur või pilte on liiga palju.</response>
    /// <response code="403">Kuulutus kuulub teisele üürileandjale.</response>
    /// <response code="404">Kuulutust ei leitud.</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    [ProducesResponseType<List<ListingImageDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ListingImageDto>>> Upload(int listingId, [FromForm] List<IFormFile> files)
    {
        var images = await _images.UploadAsync(listingId, files, User.GetCurrentUser());
        return Created($"/api/listings/{listingId}", images);
    }

    /// <summary>Kustutab kuulutuse pildi.</summary>
    /// <param name="listingId">Kuulutuse ID.</param>
    /// <param name="imageId">Pildi ID.</param>
    /// <response code="204">Pilt kustutati.</response>
    /// <response code="403">Kuulutus kuulub teisele üürileandjale.</response>
    /// <response code="404">Kuulutust või pilti ei leitud.</response>
    [HttpDelete("{imageId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int listingId, int imageId)
    {
        await _images.DeleteAsync(listingId, imageId, User.GetCurrentUser());
        return NoContent();
    }

    /// <summary>Teeb pildi kuulutuse põhipildiks (näidatakse kuulutuste nimekirjas).</summary>
    /// <param name="listingId">Kuulutuse ID.</param>
    /// <param name="imageId">Pildi ID.</param>
    /// <response code="204">Põhipilt muudeti.</response>
    /// <response code="403">Kuulutus kuulub teisele üürileandjale.</response>
    /// <response code="404">Kuulutust või pilti ei leitud.</response>
    [HttpPut("{imageId:int}/primary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrimary(int listingId, int imageId)
    {
        await _images.SetPrimaryAsync(listingId, imageId, User.GetCurrentUser());
        return NoContent();
    }
}

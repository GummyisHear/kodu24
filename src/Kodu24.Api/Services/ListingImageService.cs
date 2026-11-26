using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Mapping;
using Kodu24.Api.Repositories;
using Kodu24.Api.Storage;

namespace Kodu24.Api.Services;

public class ListingImageService : IListingImageService
{
    public const int MaxImagesPerListing = 10;
    public const long MaxFileBytes = 5 * 1024 * 1024;

    /// <summary>Vormivälja nimi, millega pildid saadetakse (multipart/form-data).</summary>
    public const string FilesField = "files";

    private readonly IListingRepository _listings;
    private readonly IRepository<ListingImage> _images;
    private readonly IImageStorage _storage;
    private readonly TimeProvider _time;
    private readonly ILogger<ListingImageService> _logger;

    public ListingImageService(
        IListingRepository listings,
        IRepository<ListingImage> images,
        IImageStorage storage,
        TimeProvider time,
        ILogger<ListingImageService> logger)
    {
        _listings = listings;
        _images = images;
        _storage = storage;
        _time = time;
        _logger = logger;
    }

    public async Task<List<ListingImageDto>> UploadAsync(int listingId, IReadOnlyList<IFormFile> files, CurrentUser user)
    {
        var listing = await FindListingAsync(listingId);
        user.EnsureCanManageListing(listing);

        if (files.Count == 0)
            throw Invalid("Vali vähemalt üks pilt.");
        if (listing.Images.Count + files.Count > MaxImagesPerListing)
            throw Invalid($"Kuulutusel võib olla kuni {MaxImagesPerListing} pilti (praegu {listing.Images.Count}).");

        // Kõik failid kontrollitakse enne salvestamist: kas kõik lähevad läbi või mitte ükski.
        var extensions = new List<string>();
        foreach (var file in files)
            extensions.Add(await ValidateAsync(file));

        var savedFiles = new List<string>();
        try
        {
            var now = _time.GetUtcNow().UtcDateTime;
            var nextOrder = listing.Images.Count == 0 ? 0 : listing.Images.Max(i => i.SortOrder) + 1;
            var hasPrimary = listing.Images.Any(i => i.IsPrimary);

            for (var i = 0; i < files.Count; i++)
            {
                await using var stream = files[i].OpenReadStream();
                var fileName = await _storage.SaveAsync(stream, extensions[i]);
                savedFiles.Add(fileName);

                listing.Images.Add(new ListingImage
                {
                    FileName = fileName,
                    SortOrder = nextOrder++,
                    IsPrimary = !hasPrimary && i == 0,
                    UploadedAt = now,
                });
            }

            await _listings.SaveChangesAsync();
        }
        catch
        {
            // Andmebaasi salvestamine ebaõnnestus → ära jäta kettale faile, millele keegi ei viita.
            foreach (var fileName in savedFiles)
                _storage.Delete(fileName);
            throw;
        }

        _logger.LogInformation("Kasutaja {UserId} lisas kuulutusele {ListingId} {Count} pilti",
            user.Id, listingId, files.Count);
        return listing.Images.OrderBy(i => i.SortOrder).Select(i => i.ToDto()).ToList();
    }

    public async Task DeleteAsync(int listingId, int imageId, CurrentUser user)
    {
        var listing = await FindListingAsync(listingId);
        user.EnsureCanManageListing(listing);
        var image = FindImage(listing, imageId);

        _images.Remove(image);
        listing.Images.Remove(image);
        if (image.IsPrimary && listing.Images.OrderBy(i => i.SortOrder).FirstOrDefault() is { } next)
            next.IsPrimary = true;

        await _listings.SaveChangesAsync();
        // Fail kustutatakse alles pärast edukat salvestamist.
        _storage.Delete(image.FileName);
        _logger.LogInformation("Kasutaja {UserId} kustutas kuulutuse {ListingId} pildi {ImageId}", user.Id, listingId, imageId);
    }

    public async Task SetPrimaryAsync(int listingId, int imageId, CurrentUser user)
    {
        var listing = await FindListingAsync(listingId);
        user.EnsureCanManageListing(listing);
        var image = FindImage(listing, imageId);

        foreach (var other in listing.Images)
            other.IsPrimary = other == image;
        await _listings.SaveChangesAsync();
    }

    /// <summary>
    /// Kontrollib faili suurust ja sisu. Tüüp tuvastatakse faili esimeste baitide järgi,
    /// mitte nime või Content-Type päise järgi, sest neid saab saatja vabalt valida.
    /// Tagastab laiendi, millega fail salvestatakse.
    /// </summary>
    private static async Task<string> ValidateAsync(IFormFile file)
    {
        if (file.Length == 0)
            throw Invalid($"Fail \"{file.FileName}\" on tühi.");
        if (file.Length > MaxFileBytes)
            throw Invalid($"Fail \"{file.FileName}\" on liiga suur (lubatud kuni {MaxFileBytes / 1024 / 1024} MB).");

        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);

        return DetectExtension(header.AsSpan(0, read))
               ?? throw Invalid($"Fail \"{file.FileName}\" ei ole pilt. Lubatud on JPEG, PNG ja WebP.");
    }

    public static string? DetectExtension(ReadOnlySpan<byte> h)
    {
        if (h.Length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF)
            return ".jpg";
        if (h.Length >= 8 && h[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return ".png";
        if (h.Length >= 12 && h[..4].SequenceEqual("RIFF"u8) && h[8..12].SequenceEqual("WEBP"u8))
            return ".webp";
        return null;
    }

    private async Task<Listing> FindListingAsync(int id) =>
        await _listings.GetWithDetailsAsync(id)
        ?? throw new NotFoundException($"Kuulutust {id} ei leitud.");

    private static ListingImage FindImage(Listing listing, int imageId) =>
        listing.Images.FirstOrDefault(i => i.Id == imageId)
        ?? throw new NotFoundException($"Kuulutusel {listing.Id} ei ole pilti {imageId}.");

    private static ValidationFailedException Invalid(string message) => new(FilesField, message);
}

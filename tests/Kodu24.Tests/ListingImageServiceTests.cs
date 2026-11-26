using Kodu24.Api.Auth;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Kodu24.Api.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kodu24.Tests;

public class ListingImageServiceTests
{
    private static readonly CurrentUser Owner = new(1, UserRole.Landlord);
    private static readonly CurrentUser OtherLandlord = new(2, UserRole.Landlord);

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0, 0, 0, 0, 0];

    private readonly Mock<IListingRepository> _listings = new();
    private readonly Mock<IRepository<ListingImage>> _images = new();
    private readonly Mock<IImageStorage> _storage = new();
    private readonly Listing _listing = new() { Id = 10, LandlordId = Owner.Id };
    private int _savedCount;

    public ListingImageServiceTests()
    {
        _listings.Setup(l => l.GetWithDetailsAsync(10)).ReturnsAsync(_listing);
        _storage.Setup(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync((Stream _, string ext) => $"file{++_savedCount}{ext}");
    }

    private ListingImageService CreateService() => new(
        _listings.Object, _images.Object, _storage.Object, TimeProvider.System, NullLogger<ListingImageService>.Instance);

    private static IFormFile File(byte[] content, string name = "pilt.png") =>
        new FormFile(new MemoryStream(content), 0, content.Length, ListingImageService.FilesField, name);

    private void AddExistingImages(int count)
    {
        for (var i = 0; i < count; i++)
            _listing.Images.Add(new ListingImage { Id = i + 1, FileName = $"old{i}.jpg", SortOrder = i, IsPrimary = i == 0 });
    }

    [Fact]
    public async Task UploadAsync_SavesFilesWithDetectedExtension_FirstBecomesPrimary()
    {
        var result = await CreateService().UploadAsync(10, [File(Png), File(Jpeg, "foto.png")], Owner);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsPrimary);
        Assert.False(result[1].IsPrimary);
        Assert.Equal("/uploads/file1.png", result[0].Url);
        // Laiend tuleb faili sisust, mitte nimest: "foto.png" on tegelikult JPEG.
        Assert.Equal("/uploads/file2.jpg", result[1].Url);
        _listings.Verify(l => l.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UploadAsync_FileThatIsNotImage_ThrowsAndSavesNothing()
    {
        var text = "<script>alert(1)</script>"u8.ToArray();

        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() =>
            CreateService().UploadAsync(10, [File(Png), File(text, "pilt.jpg")], Owner));

        Assert.True(ex.Errors.ContainsKey(ListingImageService.FilesField));
        _storage.Verify(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        _listings.Verify(l => l.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UploadAsync_OverImageLimit_ThrowsValidationError()
    {
        AddExistingImages(ListingImageService.MaxImagesPerListing);

        await Assert.ThrowsAsync<ValidationFailedException>(() => CreateService().UploadAsync(10, [File(Png)], Owner));
    }

    [Fact]
    public async Task UploadAsync_OtherLandlordsListing_ThrowsForbidden()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() => CreateService().UploadAsync(10, [File(Png)], OtherLandlord));

        _storage.Verify(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UploadAsync_DatabaseFails_DeletesSavedFiles()
    {
        _listings.Setup(l => l.SaveChangesAsync()).ThrowsAsync(new InvalidOperationException("DB maas"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().UploadAsync(10, [File(Png)], Owner));

        _storage.Verify(s => s.Delete("file1.png"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_PrimaryImage_NextBecomesPrimaryAndFileIsDeleted()
    {
        AddExistingImages(3);

        await CreateService().DeleteAsync(10, 1, Owner);

        Assert.Equal(2, _listing.Images.Count);
        Assert.True(_listing.Images.Single(i => i.Id == 2).IsPrimary);
        _storage.Verify(s => s.Delete("old0.jpg"), Times.Once);
    }

    [Fact]
    public async Task SetPrimaryAsync_LeavesExactlyOnePrimary()
    {
        AddExistingImages(3);

        await CreateService().SetPrimaryAsync(10, 3, Owner);

        Assert.Equal(3, _listing.Images.Single(i => i.IsPrimary).Id);
    }

    [Theory]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, ".jpg")]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ".png")]
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 1, 2, 3, 4, 0x57, 0x45, 0x42, 0x50 }, ".webp")]
    [InlineData(new byte[] { 0x47, 0x49, 0x46, 0x38 }, null)] // GIF ei ole lubatud
    [InlineData(new byte[] { }, null)]
    public void DetectExtension_RecognisesAllowedFormats(byte[] header, string? expected)
    {
        Assert.Equal(expected, ListingImageService.DetectExtension(header));
    }
}

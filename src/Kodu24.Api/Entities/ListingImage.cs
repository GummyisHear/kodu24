namespace Kodu24.Api.Entities;
public class ListingImage
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public Listing Listing { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
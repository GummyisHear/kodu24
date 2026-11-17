namespace Kodu24.Api.Entities;
public class Listing
{
    public int Id { get; set; }
    public int LandlordId { get; set; }
    public User Landlord { get; set; } = null!;
    public int DistrictId { get; set; }
    public District District { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Address { get; set; } = null!;
    public decimal Price { get; set; }
    public int Rooms { get; set; }
    public decimal AreaM2 { get; set; }
    public string Description { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public List<ListingImage> Images { get; set; } = new();
    public List<Favorite> Favorites { get; set; } = new();
    public List<Inquiry> Inquiries { get; set; } = new();
}
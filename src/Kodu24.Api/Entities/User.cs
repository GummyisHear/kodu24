namespace Kodu24.Api.Entities;
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Listing> Listings { get; set; } = new();
    public List<Favorite> Favorites { get; set; } = new();
    public List<Inquiry> Inquiries { get; set; } = new();
    public List<SavedSearch> SavedSearches { get; set; } = new();
}
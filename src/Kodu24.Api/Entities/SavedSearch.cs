namespace Kodu24.Api.Entities;
public class SavedSearch
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Rooms { get; set; }
    public int? DistrictId { get; set; }
    public District? District { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastNotifiedAt { get; set; } = DateTime.UtcNow;

    public List<Notification> Notifications { get; set; } = new();
}
namespace Kodu24.Api.Entities;
public class District
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<Listing> Listings { get; set; } = new();
}
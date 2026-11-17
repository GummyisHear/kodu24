using Kodu24.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kodu24.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<SavedSearch> SavedSearches => Set<SavedSearch>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.FullName).HasMaxLength(100);
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<District>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100);
        });

        b.Entity<Listing>(e =>
        {
            e.Property(x => x.Price).HasPrecision(10, 2);
            e.Property(x => x.AreaM2).HasPrecision(8, 2);
            e.Property(x => x.Title).HasMaxLength(150);
            e.Property(x => x.Address).HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(4000);
            e.HasIndex(x => new { x.DistrictId, x.Price });
            e.HasIndex(x => x.CreatedAt);
            e.HasQueryFilter(x => !x.IsDeleted);

            e.HasOne(x => x.Landlord).WithMany(u => u.Listings)
                .HasForeignKey(x => x.LandlordId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.District).WithMany(d => d.Listings)
                .HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ListingImage>(e =>
            e.HasOne(x => x.Listing).WithMany(l => l.Images)
                .HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Cascade));

        b.Entity<Favorite>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.ListingId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.Favorites)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Listing).WithMany(l => l.Favorites)
                .HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Inquiry>(e =>
        {
            e.Property(x => x.Message).HasMaxLength(2000);
            e.HasOne(x => x.Seeker).WithMany(u => u.Inquiries)
                .HasForeignKey(x => x.SeekerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Listing).WithMany(l => l.Inquiries)
                .HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<SavedSearch>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.MinPrice).HasPrecision(10, 2);
            e.Property(x => x.MaxPrice).HasPrecision(10, 2);
            e.HasOne(x => x.User).WithMany(u => u.SavedSearches)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.District).WithMany()
                .HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Notification>(e =>
        {
            e.Property(x => x.Message).HasMaxLength(500);
            e.HasOne(x => x.User).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SavedSearch).WithMany(s => s.Notifications)
                .HasForeignKey(x => x.SavedSearchId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Listing).WithMany()
                .HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
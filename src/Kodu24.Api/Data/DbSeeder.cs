using BCrypt.Net;
using Kodu24.Api.Entities;

namespace Kodu24.Api.Data;

public static class DbSeeder
{
    // Ainult arenduse jaoks. Päris keskkonnas tuleb paroolid konfiguratsioonist.
    private const string DevPassword = "Parool123!";

    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var hash = BCrypt.Net.BCrypt.HashPassword(DevPassword);

        var admin = new User { Email = "admin@kodu24.ee", FullName = "Admin Kasutaja", Role = UserRole.Admin, PasswordHash = hash };
        var landlord1 = new User { Email = "mari@kodu24.ee", FullName = "Mari Tamm", Phone = "+3725551001", Role = UserRole.Landlord, PasswordHash = hash };
        var landlord2 = new User { Email = "jaan@kodu24.ee", FullName = "Jaan Kask", Phone = "+3725551002", Role = UserRole.Landlord, PasswordHash = hash };
        var seeker1 = new User { Email = "liis@kodu24.ee", FullName = "Liis Mägi", Role = UserRole.Seeker, PasswordHash = hash };
        var seeker2 = new User { Email = "toomas@kodu24.ee", FullName = "Toomas Saar", Role = UserRole.Seeker, PasswordHash = hash };
        var service = new User { Email = "service@kodu24.ee", FullName = "Teenusekonto", Role = UserRole.Service, PasswordHash = hash };
        db.Users.AddRange(admin, landlord1, landlord2, seeker1, seeker2, service);

        var districtNames = new[]
        {
            "Kesklinn", "Kalamaja", "Pirita", "Mustamäe", "Lasnamäe",
            "Haabersti", "Nõmme", "Põhja-Tallinn", "Kristiine", "Pelgulinn"
        };
        var districts = districtNames.ToDictionary(n => n, n => new District { Name = n });
        db.Districts.AddRange(districts.Values);

        var now = DateTime.UtcNow;

        Listing L(User owner, string district, string title, string address,
                  decimal price, int rooms, decimal area, double lat, double lng,
                  string description, int daysAgo, int expiresInDays) => new()
        {
            Landlord = owner,
            District = districts[district],
            Title = title,
            Address = address,
            Price = price,
            Rooms = rooms,
            AreaM2 = area,
            Latitude = lat,
            Longitude = lng,
            Description = description,
            CreatedAt = now.AddDays(-daysAgo),
            ExpiresAt = now.AddDays(expiresInDays)
        };

        db.Listings.AddRange(
            L(landlord1, "Kesklinn", "Valgusküllane kolmetoaline kesklinnas", "Tõnismägi 5", 950, 3, 72, 59.4332, 24.7414, "Renoveeritud korter vanalinna lähedal, mööbliga.", 2, 30),
            L(landlord1, "Kesklinn", "Stuudio Viru keskuse lähedal", "Viru 12", 520, 1, 28, 59.4370, 24.7536, "Kompaktne stuudio, sobib üksikule inimesele.", 5, 30),
            L(landlord1, "Kalamaja", "Hubane kahetoaline Kalamajas", "Vana-Kalamaja 20", 780, 2, 48, 59.4440, 24.7340, "Puitmaja korter, rahulik hoov.", 1, 30),
            L(landlord1, "Kalamaja", "Rõdu ja merevaatega korter", "Kalaranna 8", 890, 2, 55, 59.4458, 24.7392, "Uus arendus, parkimiskoht olemas.", 8, 30),
            L(landlord2, "Pirita", "Ruumikas nelja toaga maja ots", "Merivälja tee 15", 1400, 4, 110, 59.4701, 24.8312, "Sobib perele, aed ja saun.", 3, 30),
            L(landlord2, "Mustamäe", "Odav ühetoaline Mustamäel", "Ehitajate tee 40", 380, 1, 31, 59.4006, 24.6930, "Renoveeritud, lähedal ühistransport.", 4, 30),
            L(landlord2, "Mustamäe", "Kahetoaline Mustamäe keskuses", "Sõpruse pst 160", 540, 2, 46, 59.3951, 24.6735, "Heas seisukorras, mööbliga.", 10, 30),
            L(landlord2, "Lasnamäe", "Kolmetoaline Lasnamäel", "Mustakivi tee 17", 560, 3, 64, 59.4388, 24.8301, "Suur rõdu, lasteaed lähedal.", 6, 30),
            L(landlord2, "Lasnamäe", "Stuudio Lasnamäe tipus", "Pae 24", 400, 1, 30, 59.4350, 24.8010, "Uus ehitus, soodne hind.", 12, 30),
            L(landlord1, "Haabersti", "Kahetoaline Haabersti ostukeskuse juures", "Haabersti 1", 610, 2, 50, 59.4330, 24.6429, "Vaikne piirkond, parkimine hoovis.", 7, 30),
            L(landlord2, "Nõmme", "Maja osa Nõmmel", "Jaama 10", 1100, 3, 85, 59.3821, 24.6918, "Männimetsa lähedal, kamin.", 9, 30),
            L(landlord1, "Põhja-Tallinn", "Loft Põhja-Tallinnas", "Kopli 25", 700, 2, 58, 59.4521, 24.7352, "Kõrged laed, tööstusstiilis.", 15, 30),
            L(landlord1, "Kristiine", "Kolmetoaline Kristiines", "Endla 30", 820, 3, 68, 59.4259, 24.7245, "Renoveeritud köök, kesklinn 10 min.", 11, 30),
            L(landlord2, "Pelgulinn", "Armas ühetoaline Pelgulinnas", "Kolde pst 50", 450, 1, 35, 59.4425, 24.7382, "Puitmaja, hubane hoov.", 14, 30),
            // Aegunud kuulutus, et Workeri expire-otspunkti oleks millega testida
            L(landlord1, "Kesklinn", "Aegunud kuulutus (test)", "Pärnu mnt 100", 600, 2, 45, 59.4290, 24.7360, "See kuulutus on aegunud.", 40, -5)
        );

        db.SaveChanges();
    }
}
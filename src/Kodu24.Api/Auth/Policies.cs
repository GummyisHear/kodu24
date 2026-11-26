using Kodu24.Api.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Kodu24.Api.Auth;

/// <summary>Rollide nimed tokenis (vastavad <see cref="UserRole"/> väärtustele).</summary>
public static class Roles
{
    public const string Admin = nameof(UserRole.Admin);
    public const string Landlord = nameof(UserRole.Landlord);
    public const string Seeker = nameof(UserRole.Seeker);
    public const string Service = nameof(UserRole.Service);
}

/// <summary>
/// Nimelised ligipääsureeglid kontrolleritele: <c>[Authorize(Policy = Policies.Landlord)]</c>.
/// Täielik tabel: docs/rollid.md.
/// </summary>
public static class Policies
{
    /// <summary>Kuulutuste lisamine/muutmine, piltide haldus, saadud päringud. Admin haldab kõiki.</summary>
    public const string Landlord = "LandlordOrAdmin";

    /// <summary>Lemmikud, päringu saatmine, salvestatud otsingud.</summary>
    public const string Seeker = "Seeker";

    /// <summary>Halduse otspunktid.</summary>
    public const string Admin = "Admin";

    /// <summary>Taustateenuse (Worker) otspunktid.</summary>
    public const string Service = "Service";

    /// <summary>Salvestatud otsingute nimekiri: üürija näeb enda omi, taustateenus kõiki.</summary>
    public const string SeekerOrService = "SeekerOrService";

    public static void AddKodu24Policies(this AuthorizationOptions options)
    {
        options.AddPolicy(Landlord, p => p.RequireRole(Roles.Landlord, Roles.Admin));
        options.AddPolicy(Seeker, p => p.RequireRole(Roles.Seeker));
        options.AddPolicy(Admin, p => p.RequireRole(Roles.Admin));
        options.AddPolicy(Service, p => p.RequireRole(Roles.Service));
        options.AddPolicy(SeekerOrService, p => p.RequireRole(Roles.Seeker, Roles.Service));
    }
}

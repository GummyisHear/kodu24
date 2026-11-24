using System.Security.Cryptography;
using System.Text;
using Kodu24.Api.Auth;
using Kodu24.Api.Data;
using Kodu24.Api.Errors;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog loeb seaded appsettings.json "Serilog" sektsioonist (konsool + päevane logifail).
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kodu24 API",
        Version = "v1",
        Description = "Üürikorterite portaali Kodu24 REST API: kuulutused, pildid, lemmikud, " +
                      "päringud üürileandjale, salvestatud otsingud ja teavitused.",
    });

    // Kontrollerite ja DTO-de XML kommentaarid kuvatakse Swaggeris kirjeldusena.
    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

// Ühtne veakäsitlus: kõik vead tagastatakse ProblemDetails (RFC 9457) kujul.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// JWT autentimine. Võti peab olema vähemalt 32 baiti (HMAC-SHA256) ega tohi olla repositooriumis:
// see tuleb user-secrets'ist või keskkonnamuutujast Jwt__Key.
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException(
            "JWT võti puudub või on liiga lühike. Määra Jwt:Key (nt keskkonnamuutuja Jwt__Key), vähemalt 32 tähemärki.");

    // Arenduses genereeritakse igal käivitusel juhuslik võti: värske kloon töötab kohe,
    // kuid tokenid kaotavad kehtivuse pärast API taaskäivitust.
    jwt.Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    Console.WriteLine("HOIATUS: Jwt:Key puudub, kasutan ajutist arendusvõtit (tokenid kehtivad kuni taaskäivituseni).");
}

builder.Services.Configure<JwtOptions>(options =>
{
    options.Issuer = jwt.Issuer;
    options.Audience = jwt.Audience;
    options.Key = jwt.Key;
    options.ExpiresMinutes = jwt.ExpiresMinutes;
});
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = AuthClaims.Name,
            RoleClaimType = AuthClaims.Role,
        };
    });
builder.Services.AddAuthorization();

// CORS: brauseriklient (React/Vite) tohib API-t kutsuda ainult lubatud aadressidelt.
const string WebClientCors = "WebClient";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy(WebClientCors, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Kellaaeg teenusena, et teste saaks kindla ajaga käivitada.
builder.Services.AddSingleton(TimeProvider.System);

// Repositooriumid
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IListingRepository, ListingRepository>();

// Teenused (äriloogika)
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();

var app = builder.Build();

app.UseExceptionHandler();
// Tühjad 4xx/5xx vastused (nt tundmatu tee) saavad samuti ProblemDetails keha.
app.UseStatusCodePages();

// Üks logirida iga HTTP päringu kohta (meetod, tee, staatus, kestus).
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

app.UseHttpsRedirection();

app.UseCors(WebClientCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

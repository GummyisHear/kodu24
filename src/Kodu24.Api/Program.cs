using Kodu24.Api.Data;
using Kodu24.Api.Errors;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Microsoft.EntityFrameworkCore;
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

app.UseAuthorization();

app.MapControllers();

app.Run();

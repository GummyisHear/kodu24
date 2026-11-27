using Kodu24.Worker;
using Kodu24.Worker.Api;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<ApiOptions>()
    .Bind(builder.Configuration.GetSection(ApiOptions.Section))
    .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _), "Api:BaseUrl peab olema täielik aadress.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.ServicePassword),
        "Api:ServicePassword puudub. Määra see user-secrets'iga või keskkonnamuutujaga Api__ServicePassword.")
    .Validate(o => o.IntervalMinutes >= 1, "Api:IntervalMinutes peab olema vähemalt 1.")
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);

// Tüübitud HttpClient: ainult API aadress. Worker ei viita API projektile ega andmebaasile.
builder.Services.AddHttpClient<IKodu24ApiClient, Kodu24ApiClient>((sp, http) =>
{
    var options = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
    http.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    http.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<HourlyJob>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

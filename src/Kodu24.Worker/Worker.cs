using Microsoft.Extensions.Options;

namespace Kodu24.Worker;

/// <summary>
/// Käivitab <see cref="HourlyJob"/>'i kohe pärast starti ja seejärel iga <see cref="ApiOptions.IntervalMinutes"/> järel.
/// Viga ühes käivituses logitakse ja teenus jätkab (nt kui API on parasjagu maas).
/// </summary>
public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ApiOptions _options;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceScopeFactory scopes, IOptions<ApiOptions> options, ILogger<Worker> logger)
    {
        _scopes = scopes;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(_options.IntervalMinutes);
        _logger.LogInformation("Taustateenus käivitus: API {BaseUrl}, intervall {Interval}", _options.BaseUrl, interval);

        using var timer = new PeriodicTimer(interval);
        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<HourlyJob>().RunAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Teenus pannakse kinni.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Taustatöö ebaõnnestus, proovin uuesti järgmisel korral");
        }
    }
}

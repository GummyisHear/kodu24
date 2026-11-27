using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Kodu24.Worker.Api;

/// <summary>
/// HttpClient'il põhinev API klient. Logib teenusekontoga sisse ja hoiab tokenit meeles; kui token on
/// aegumas või API vastab 401 (nt API taaskäivitus vahetas arenduses võtme), logib uuesti sisse.
/// </summary>
public class Kodu24ApiClient : IKodu24ApiClient
{
    private static readonly TimeSpan RenewBefore = TimeSpan.FromMinutes(5);

    private readonly HttpClient _http;
    private readonly ApiOptions _options;
    private readonly TimeProvider _time;
    private readonly ILogger<Kodu24ApiClient> _logger;

    // Tüübitud HttpClient luuakse iga käivituse jaoks uuesti, seega token kehtib ühe käivituse jooksul
    // (üks sisselogimine tunnis).
    private string? _token;
    private DateTime _tokenExpiresAt;

    public Kodu24ApiClient(HttpClient http, IOptions<ApiOptions> options, TimeProvider time, ILogger<Kodu24ApiClient> logger)
    {
        _http = http;
        _options = options.Value;
        _time = time;
        _logger = logger;
    }

    public async Task<ExpireResult> ExpireListingsAsync(CancellationToken ct)
    {
        using var response = await SendAsync(() => new HttpRequestMessage(HttpMethod.Post, "api/listings/expire"), ct);
        return await ReadAsync<ExpireResult>(response, ct);
    }

    public async Task<List<SavedSearch>> GetSavedSearchesAsync(CancellationToken ct)
    {
        using var response = await SendAsync(() => new HttpRequestMessage(HttpMethod.Get, "api/saved-searches"), ct);
        return await ReadAsync<List<SavedSearch>>(response, ct);
    }

    public async Task<ListingPage> GetNewListingsAsync(SavedSearch search, int page, int pageSize, CancellationToken ct)
    {
        var url = BuildListingsUrl(search, page, pageSize);
        using var response = await SendAsync(() => new HttpRequestMessage(HttpMethod.Get, url), ct);
        return await ReadAsync<ListingPage>(response, ct);
    }

    public async Task<CreateNotificationsResult> CreateNotificationsAsync(
        int savedSearchId, IReadOnlyCollection<int> listingIds, CancellationToken ct)
    {
        using var response = await SendAsync(() => new HttpRequestMessage(HttpMethod.Post, "api/notifications")
        {
            Content = JsonContent.Create(new { savedSearchId, listingIds }),
        }, ct);
        return await ReadAsync<CreateNotificationsResult>(response, ct);
    }

    public async Task MarkNotifiedAsync(int savedSearchId, DateTime notifiedAt, CancellationToken ct)
    {
        using var response = await SendAsync(() => new HttpRequestMessage(HttpMethod.Patch, $"api/saved-searches/{savedSearchId}/notified")
        {
            Content = JsonContent.Create(new { notifiedAt }),
        }, ct);
        await EnsureSuccessAsync(response, ct);
    }

    /// <summary>Otsingu filtrid samade nimedega, mis API päringus; puuduv filter jäetakse välja.</summary>
    public static string BuildListingsUrl(SavedSearch search, int page, int pageSize)
    {
        var query = new List<string>();
        void Add(string name, object? value)
        {
            if (value is null) return;
            var text = value switch
            {
                DateTime d => d.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString(),
            };
            query.Add($"{name}={Uri.EscapeDataString(text!)}");
        }

        Add("minPrice", search.MinPrice);
        Add("maxPrice", search.MaxPrice);
        Add("rooms", search.Rooms);
        Add("district", search.DistrictId);
        Add("createdAfter", search.LastNotifiedAt);
        Add("sort", "oldest");
        Add("page", page);
        Add("pageSize", pageSize);
        return "api/listings?" + string.Join('&', query);
    }

    private async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage> createRequest, CancellationToken ct)
    {
        await EnsureTokenAsync(forceLogin: false, ct);
        var response = await SendWithTokenAsync(createRequest(), ct);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        // HttpRequestMessage'it ei saa uuesti saata, seega luuakse päring uuesti.
        response.Dispose();
        _logger.LogInformation("API vastas 401, logime uuesti sisse");
        await EnsureTokenAsync(forceLogin: true, ct);
        return await SendWithTokenAsync(createRequest(), ct);
    }

    private Task<HttpResponseMessage> SendWithTokenAsync(HttpRequestMessage request, CancellationToken ct)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return _http.SendAsync(request, ct);
    }

    private async Task EnsureTokenAsync(bool forceLogin, CancellationToken ct)
    {
        if (!forceLogin && _token is not null && _tokenExpiresAt - _time.GetUtcNow().UtcDateTime > RenewBefore)
            return;

        using var response = await _http.PostAsJsonAsync("api/auth/login",
            new { email = _options.ServiceEmail, password = _options.ServicePassword }, ct);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
            throw new InvalidOperationException(
                $"Teenusekontoga {_options.ServiceEmail} sisselogimine ebaõnnestus. Kontrolli seadet Api:ServicePassword.");

        var login = await ReadAsync<LoginResponse>(response, ct);
        _token = login.Token;
        _tokenExpiresAt = login.ExpiresAt;
        _logger.LogInformation("Teenusekonto logis sisse, token kehtib kuni {ExpiresAt:u}", login.ExpiresAt);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(ct)
               ?? throw new InvalidOperationException($"API vastus oli tühi ({response.RequestMessage?.RequestUri}).");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        // ProblemDetails'i sisu logitakse kaasa, et viga oleks logist näha.
        var body = await response.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException(
            $"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri} vastas {(int)response.StatusCode}: {body}",
            null, response.StatusCode);
    }
}

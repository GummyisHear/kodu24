using System.Net;
using System.Text;
using Kodu24.Worker;
using Kodu24.Worker.Api;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Kodu24.Tests;

/// <summary>Taustateenuse HTTP klient võlts-HTTP-käsitlejaga (päris võrku ei minda).</summary>
public class Kodu24ApiClientTests
{
    /// <summary>Vastab päringutele järjekorras ja jätab meelde, mis saadeti.</summary>
    private sealed class FakeHandler(params Func<HttpRequestMessage, HttpResponseMessage>[] responses) : HttpMessageHandler
    {
        private int _next;
        public List<(string Path, string? Token)> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Requests.Add((request.RequestUri!.PathAndQuery, request.Headers.Authorization?.Parameter));
            return Task.FromResult(responses[_next++](request));
        }
    }

    private static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static Func<HttpRequestMessage, HttpResponseMessage> Login(string token) =>
        _ => Json("{\"token\":\"" + token + "\",\"expiresAt\":\"2099-01-01T00:00:00Z\",\"user\":null}");

    private static Kodu24ApiClient CreateClient(FakeHandler handler) => new(
        new HttpClient(handler) { BaseAddress = new Uri("http://api.test/") },
        Options.Create(new ApiOptions { ServicePassword = "salasona" }),
        TimeProvider.System,
        NullLogger<Kodu24ApiClient>.Instance);

    [Fact]
    public async Task Unauthorized_LogsInAgainAndRetriesOnce()
    {
        // Nt API taaskäivitus: arenduses vahetub JWT võti ja vana token ei kehti enam.
        var handler = new FakeHandler(
            Login("vana"),
            _ => new HttpResponseMessage(HttpStatusCode.Unauthorized),
            Login("uus"),
            _ => Json("""{"expiredCount":2}"""));

        var result = await CreateClient(handler).ExpireListingsAsync(CancellationToken.None);

        Assert.Equal(2, result.ExpiredCount);
        Assert.Equal(
            [("/api/auth/login", null), ("/api/listings/expire", "vana"), ("/api/auth/login", null), ("/api/listings/expire", "uus")],
            handler.Requests);
    }

    [Fact]
    public async Task TokenIsReusedWithinOneRun()
    {
        var handler = new FakeHandler(Login("t"), _ => Json("[]"), _ => Json("""{"expiredCount":0}"""));
        var client = CreateClient(handler);

        await client.GetSavedSearchesAsync(CancellationToken.None);
        await client.ExpireListingsAsync(CancellationToken.None);

        Assert.Single(handler.Requests, r => r.Path == "/api/auth/login");
    }

    [Fact]
    public void BuildListingsUrl_UsesApiQueryNames_AndSkipsEmptyFilters()
    {
        var search = new SavedSearch(1, 4, "Kalamaja", null, 850.5m, null, 2,
            new DateTime(2026, 11, 27, 10, 53, 28, DateTimeKind.Utc).AddTicks(5363704));

        var url = Kodu24ApiClient.BuildListingsUrl(search, page: 2, pageSize: 50);

        // Kümnenderaldaja on punkt (mitte koma, nagu eesti keeles) ja aeg on täpne UTC, sest createdAfter võrdleb rangelt.
        Assert.Equal(
            "api/listings?maxPrice=850.5&district=2&createdAfter=2026-11-27T10%3A53%3A28.5363704Z&sort=oldest&page=2&pageSize=50",
            url);
    }
}

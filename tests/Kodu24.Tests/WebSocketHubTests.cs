using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Kodu24.Api.Dtos;
using Kodu24.Api.RealTime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kodu24.Tests;

/// <summary>
/// WebSocketi hub päris WebSocketi ühendustega mälus töötava testserveri (TestServer) kaudu.
/// JWT-d siin ei kontrollita (see on Program.cs-i seadistus); grupp tuleb päringuparameetrist.
/// </summary>
public class WebSocketHubTests : IAsyncLifetime
{
    private readonly WebSocketHub _hub = new(NullLogger<WebSocketHub>.Instance);
    private IHost _host = null!;
    private TestServer _server = null!;

    private static readonly InquiryDto Inquiry =
        new(7, 1, "Kahetoaline", "Liis Mägi", "liis@kodu24.ee", "Kas korter on vaba?", false, DateTime.UtcNow);

    public async Task InitializeAsync()
    {
        _host = await new HostBuilder()
            .ConfigureWebHost(web => web
                .UseTestServer()
                .ConfigureServices(s => s.AddRouting())
                .Configure(app =>
                {
                    app.UseWebSockets();
                    app.Run(async context =>
                    {
                        using var socket = await context.WebSockets.AcceptWebSocketAsync();
                        var group = WebSocketHub.LandlordGroup(int.Parse(context.Request.Query["landlord"]!));
                        await _hub.HandleConnectionAsync(socket, group, context.RequestAborted);
                    });
                }))
            .StartAsync();
        _server = _host.GetTestServer();
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();
    }

    private async Task<WebSocket> ConnectAsync(int landlordId)
    {
        var socket = await _server.CreateWebSocketClient()
            .ConnectAsync(new Uri($"ws://localhost/ws?landlord={landlordId}"), CancellationToken.None);
        // Ootame, kuni server on ühenduse grupiga sidunud.
        await WaitUntilAsync(() => _hub.ConnectionCount(WebSocketHub.LandlordGroup(landlordId)) > 0);
        return socket;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (var i = 0; i < 100 && !condition(); i++)
            await Task.Delay(20);
        Assert.True(condition(), "Tingimus ei täitunud õigeks ajaks.");
    }

    private static async Task<string?> ReceiveAsync(WebSocket socket, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var buffer = new byte[4096];
        try
        {
            var result = await socket.ReceiveAsync(buffer, cts.Token);
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    [Fact]
    public async Task InquiryReceived_GoesOnlyToListingOwnersGroup()
    {
        using var owner = await ConnectAsync(1);
        using var otherLandlord = await ConnectAsync(2);

        await _hub.InquiryReceivedAsync(1, Inquiry);

        var json = await ReceiveAsync(owner, TimeSpan.FromSeconds(2));
        Assert.NotNull(json);
        using var message = JsonDocument.Parse(json);
        Assert.Equal("InquiryReceived", message.RootElement.GetProperty("type").GetString());
        Assert.Equal(7, message.RootElement.GetProperty("data").GetProperty("id").GetInt32());
        Assert.Equal("Kas korter on vaba?", message.RootElement.GetProperty("data").GetProperty("message").GetString());

        Assert.Null(await ReceiveAsync(otherLandlord, TimeSpan.FromMilliseconds(300)));
    }

    [Fact]
    public async Task SameLandlordInTwoTabs_BothReceive()
    {
        using var tab1 = await ConnectAsync(1);
        using var tab2 = await ConnectAsync(1);
        await WaitUntilAsync(() => _hub.ConnectionCount(WebSocketHub.LandlordGroup(1)) == 2);

        await _hub.InquiryReceivedAsync(1, Inquiry);

        Assert.NotNull(await ReceiveAsync(tab1, TimeSpan.FromSeconds(2)));
        Assert.NotNull(await ReceiveAsync(tab2, TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public async Task ClosedConnection_IsRemovedFromGroup()
    {
        var socket = await ConnectAsync(3);

        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Lahkun", CancellationToken.None);

        await WaitUntilAsync(() => _hub.ConnectionCount(WebSocketHub.LandlordGroup(3)) == 0);
    }

    [Fact]
    public async Task NobodyConnected_DoesNotThrow()
    {
        // Üürileandjal pole lehte lahti: päring on salvestatud, teade lihtsalt jääb saatmata.
        await _hub.InquiryReceivedAsync(99, Inquiry);
    }
}

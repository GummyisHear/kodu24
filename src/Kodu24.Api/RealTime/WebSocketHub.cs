using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using Kodu24.Api.Dtos;

namespace Kodu24.Api.RealTime;

/// <summary>Teade, mis saadetakse WebSocketi kaudu JSON-ina: <c>{ "type": "...", "data": { ... } }</c>.</summary>
public record RealTimeMessage(string Type, object Data);

/// <summary>
/// Hoiab avatud WebSocketi ühendusi gruppide kaupa (nt <c>landlord-2</c>) ja saadab neile teateid.
/// Singleton: sama objekt teenindab kõiki ühendusi. Kliendi saadetud sõnumeid ei töödelda, ühendus on ainult teadete jaoks.
/// </summary>
public class WebSocketHub : IInquiryNotifier
{
    public const string InquiryReceived = "InquiryReceived";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    // grupp → ühenduse ID → ühendus. Üks kasutaja võib olla ühendatud mitmest brauseri vahelehest.
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>> _groups = new();
    private readonly ILogger<WebSocketHub> _logger;

    public WebSocketHub(ILogger<WebSocketHub> logger)
    {
        _logger = logger;
    }

    public static string LandlordGroup(int landlordId) => $"landlord-{landlordId}";

    /// <summary>Mitu ühendust grupis on (testide ja logi jaoks).</summary>
    public int ConnectionCount(string group) => _groups.TryGetValue(group, out var c) ? c.Count : 0;

    /// <summary>Registreerib ühenduse grupis ja ootab, kuni klient selle sulgeb.</summary>
    public async Task HandleConnectionAsync(WebSocket socket, string group, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        var connection = new Connection(socket);
        _groups.GetOrAdd(group, _ => new()).TryAdd(id, connection);
        _logger.LogInformation("WebSocket ühendus {ConnectionId} liitus grupiga {Group}", id, group);

        try
        {
            var buffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Head aega", ct);
                    break;
                }
            }
        }
        catch (Exception ex) when (ex is WebSocketException or OperationCanceledException)
        {
            // Brauser suleti või server pannakse kinni: ühendus lihtsalt eemaldatakse.
        }
        finally
        {
            Remove(group, id);
            _logger.LogInformation("WebSocket ühendus {ConnectionId} lahkus grupist {Group}", id, group);
        }
    }

    public Task InquiryReceivedAsync(int landlordId, InquiryDto inquiry) =>
        SendToGroupAsync(LandlordGroup(landlordId), new RealTimeMessage(InquiryReceived, inquiry));

    public async Task SendToGroupAsync(string group, RealTimeMessage message)
    {
        if (!_groups.TryGetValue(group, out var connections) || connections.IsEmpty)
            return;

        var bytes = JsonSerializer.SerializeToUtf8Bytes(message, Json);
        foreach (var (id, connection) in connections)
        {
            try
            {
                await connection.SendAsync(bytes);
            }
            catch (Exception ex) when (ex is WebSocketException or ObjectDisposedException or InvalidOperationException)
            {
                _logger.LogWarning(ex, "Teadet ei õnnestunud saata ühendusele {ConnectionId}, eemaldan selle", id);
                Remove(group, id);
            }
        }
    }

    private void Remove(string group, Guid id)
    {
        if (_groups.TryGetValue(group, out var connections))
            connections.TryRemove(id, out _);
    }

    /// <summary>WebSocket ei luba kahte samaaegset saatmist, seega iga ühendus saadab lukuga.</summary>
    private sealed class Connection(WebSocket socket)
    {
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        public async Task SendAsync(byte[] bytes)
        {
            await _sendLock.WaitAsync();
            try
            {
                if (socket.State == WebSocketState.Open)
                    await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
            }
            finally
            {
                _sendLock.Release();
            }
        }
    }
}

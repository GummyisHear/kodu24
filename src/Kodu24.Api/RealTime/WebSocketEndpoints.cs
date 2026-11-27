using Kodu24.Api.Auth;

namespace Kodu24.Api.RealTime;

public static class WebSocketEndpoints
{
    public const string InquiriesPath = "/ws/inquiries";

    /// <summary>
    /// <c>GET /ws/inquiries?access_token={jwt}</c>: üürileandja (või admin) reaalajas päringuteated.
    /// Brauseri WebSocket ei saa saata Authorization päist, seega token tuleb päringuparameetrist
    /// (vt <c>OnMessageReceived</c> Program.cs-is).
    /// </summary>
    public static void MapInquiryWebSocket(this WebApplication app)
    {
        app.MapGet(InquiriesPath, async (HttpContext context, WebSocketHub hub) =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                    return Results.Problem("Siia saab ühenduda ainult WebSocketiga.", statusCode: StatusCodes.Status400BadRequest);

                var group = WebSocketHub.LandlordGroup(context.User.GetUserId());
                using var socket = await context.WebSockets.AcceptWebSocketAsync();
                await hub.HandleConnectionAsync(socket, group, context.RequestAborted);
                return Results.Empty;
            })
            .RequireAuthorization(Policies.Landlord)
            .ExcludeFromDescription();
    }
}

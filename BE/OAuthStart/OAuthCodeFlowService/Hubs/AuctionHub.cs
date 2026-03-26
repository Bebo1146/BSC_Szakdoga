using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using OAuthCodeFlowService.Services;

namespace OAuthCodeFlowService.Hubs
{
    /// <summary>
    /// Client-facing hub. Each Angular client connects with session cookie.
    /// On connect: resolves token from session, opens per-client upstream connection to ServicesHoster.
    /// Relays upstream messages back to that specific Angular client.
    /// </summary>
    public class AuctionHub : Hub
    {
        private const string SessionCookieName = "session_id";
        private const string UpstreamHubUrl = "http://localhost:5124/hubs/auction";

        private readonly ISessionRepository _sessions;
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly ILogger<AuctionHub> _logger;

        // Track upstream connections per client
        private static readonly ConcurrentDictionary<string, HubConnection> _upstreamConnections = new();

        public AuctionHub(
            ISessionRepository sessions,
            IHubContext<AuctionHub> hubContext,
            ILogger<AuctionHub> logger)
        {
            _sessions = sessions;
            _hubContext = hubContext;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            HttpContext? httpContext = Context.GetHttpContext();
            if (httpContext is null ||
                !httpContext.Request.Cookies.TryGetValue(SessionCookieName, out string? sessionId) ||
                string.IsNullOrEmpty(sessionId))
            {
                Context.Abort();
                return;
            }

            SessionInfo? session = _sessions.Get(sessionId);
            if (session is null)
            {
                Context.Abort();
                return;
            }

            string connectionId = Context.ConnectionId;
            string accessToken = session.AccessToken;

            // Create upstream connection using THIS client's token
            HubConnection upstream = new HubConnectionBuilder()
                .WithUrl(UpstreamHubUrl, options =>
                {
                    options.AccessTokenProvider = () =>
                    {
                        // Re-read session on each reconnect to get refreshed token
                        SessionInfo? current = _sessions.Get(sessionId);
                        return Task.FromResult(current?.AccessToken);
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            // Relay upstream messages to THIS specific Angular client
            upstream.On<List<AuctionTimeUpdate>>("AuctionTimeUpdate", async updates =>
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("AuctionTimeUpdate", updates);
            });

            try
            {
                await upstream.StartAsync();
                _upstreamConnections[connectionId] = upstream;
                _logger.LogInformation("Upstream connection opened for client {ConnectionId}", connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to open upstream connection for client {ConnectionId}", connectionId);
                Context.Abort();
                return;
            }

            _logger.LogInformation("Attempting upstream connection with token: {TokenPreview}...", accessToken[..Math.Min(20, accessToken.Length)]);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;

            if (_upstreamConnections.TryRemove(connectionId, out HubConnection? upstream))
            {
                await upstream.DisposeAsync();
                _logger.LogInformation("Upstream connection closed for client {ConnectionId}", connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }

    public record AuctionTimeUpdate(
        string ProductId,
        int TimeRemainingSeconds,
        bool IsActive,
        bool HasEnded,
        string Status
    );
}
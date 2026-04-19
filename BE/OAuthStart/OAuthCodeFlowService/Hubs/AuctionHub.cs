using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using OAuthCodeFlowService.Services;

namespace OAuthCodeFlowService.Hubs
{
    public class AuctionHub : Hub
    {
        private const string SessionCookieName = "session_id";
        private readonly string _upstreamHubUrl;

        private readonly ISessionRepository _sessions;
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly ILogger<AuctionHub> _logger;

        private static readonly ConcurrentDictionary<string, HubConnection> _upstreamConnections = new();

        public AuctionHub(
            ISessionRepository sessions,
            IHubContext<AuctionHub> hubContext,
            ILogger<AuctionHub> logger,
            IConfiguration configuration)
        {
            _sessions = sessions;
            _hubContext = hubContext;
            _logger = logger;
            _upstreamHubUrl = configuration.GetValue<string>("Services:AuctionHubUrl")
                ?? "https://localhost:7093/hubs/auction";
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

            HubConnection upstream = new HubConnectionBuilder()
                .WithUrl(_upstreamHubUrl, options =>
                {
                    options.AccessTokenProvider = () =>
                    {
                        SessionInfo? current = _sessions.Get(sessionId);
                        return Task.FromResult(current?.AccessToken);
                    };
                    options.HttpMessageHandlerFactory = _ => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                })
                .WithAutomaticReconnect()
                .Build();

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
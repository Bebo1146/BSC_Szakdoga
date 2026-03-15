using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OAuthCodeFlowService.Services;

namespace OAuthCodeFlowService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BffProxyController : ControllerBase
    {
        private const string SessionCookieName = "session_id";
        private readonly ISessionRepository _sessions;
        private readonly ITokenService _tokenService;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<BffProxyController> _logger;

        // Base URL of the internal Products service (adjust if different)
        private const string ProductsServiceBase = "http://localhost:5124/api/products";

        public BffProxyController(
            ISessionRepository sessions,
            ITokenService tokenService,
            IHttpClientFactory httpFactory,
            ILogger<BffProxyController> logger)
        {
            _sessions = sessions;
            _tokenService = tokenService;
            _httpFactory = httpFactory;
            _logger = logger;
        }

        // Helper: ensure session exists and access token is fresh (refresh if needed)
        private async Task<(bool ok, SessionInfo? session, string? sessionId)> EnsureSessionAsync()
        {
            if (!Request.Cookies.TryGetValue(SessionCookieName, out string? sessionId) || string.IsNullOrEmpty(sessionId))
            {
                return (false, null, null);
            }

            SessionInfo? session = _sessions.Get(sessionId);
            if (session is null) return (false, null, sessionId);

            // Refresh when within 60s of expiry
            if (session.ExpiresAt <= DateTimeOffset.UtcNow.AddSeconds(60))
            {
                if (string.IsNullOrEmpty(session.RefreshToken))
                {
                    _logger.LogInformation("No refresh token available for session {SessionIdPreview}", sessionId[..8]);
                    _sessions.Remove(sessionId);
                    return (false, null, sessionId);
                }

                try
                {
                    TokenValidation.Jwt.TokenResponse refreshed = await _tokenService.RefreshTokenAsync(session.RefreshToken);
                    DateTimeOffset newExpires = DateTimeOffset.UtcNow.AddSeconds(refreshed.ExpiresIn > 0 ? refreshed.ExpiresIn : 3600);
                    SessionInfo updated = new SessionInfo(refreshed.AccessToken, refreshed.RefreshToken, refreshed.IdToken, newExpires, session.PreferredName);

                    _sessions.Update(sessionId, updated);

                    CookieOptions cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                        Expires = updated.ExpiresAt.UtcDateTime,
                        IsEssential = true
                    };
                    Response.Cookies.Append(SessionCookieName, sessionId, cookieOptions);

                    session = updated;
                    _logger.LogInformation("Refreshed access token for session {SessionIdPreview}", sessionId[..8]);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Refresh token failed for session {SessionIdPreview}", sessionId[..8]);
                    _sessions.Remove(sessionId);
                    return (false, null, sessionId);
                }
            }

            return (true, session, sessionId);
        }

        // GET api/bff/products/getall
        [HttpGet("products/getall")]
        public async Task<IActionResult> GetAll()
        {
            (bool ok, SessionInfo session, string sid) = await EnsureSessionAsync();
            if (!ok || session == null) return Unauthorized();

            HttpClient client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

            HttpResponseMessage resp = await client.GetAsync($"{ProductsServiceBase}/getall");
            string content = await resp.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = content,
                ContentType = "application/json",
                StatusCode = (int)resp.StatusCode
            };
        }

        // GET api/bff/products/my-products
        [HttpGet("products/my-products")]
        public async Task<IActionResult> GetMyProducts()
        {
            (bool ok, SessionInfo session, string sid) = await EnsureSessionAsync();
            if (!ok || session == null) return Unauthorized();

            HttpClient client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

            HttpResponseMessage resp = await client.GetAsync($"{ProductsServiceBase}/my-products");
            string content = await resp.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = content,
                ContentType = "application/json",
                StatusCode = (int)resp.StatusCode
            };
        }

        // NEW: GET api/bff/products/my-bids -> proxies to ProductsService /my-bids
        [HttpGet("products/my-bids")]
        public async Task<IActionResult> GetMyBids()
        {
            (bool ok, SessionInfo session, string sid) = await EnsureSessionAsync();
            if (!ok || session == null) return Unauthorized();

            HttpClient client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

            HttpResponseMessage resp = await client.GetAsync($"{ProductsServiceBase}/my-bids");
            string content = await resp.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = content,
                ContentType = "application/json",
                StatusCode = (int)resp.StatusCode
            };
        }

        // POST api/bff/products/addMultiple
        [HttpPost("products/addMultiple")]
        public async Task<IActionResult> AddMultiple([FromBody] JsonElement body)
        {
            (bool ok, SessionInfo session, string sid) = await EnsureSessionAsync();
            if (!ok || session == null) return Unauthorized();

            HttpClient client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage resp = await client.PostAsync($"{ProductsServiceBase}/addMultiple", content);
            string respContent = await resp.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = respContent,
                ContentType = "application/json",
                StatusCode = (int)resp.StatusCode
            };
        }

        // POST api/bff/products/{id}/bid
        [HttpPost("products/{id}/bid")]
        public async Task<IActionResult> PlaceBid(string id, [FromBody] JsonElement body)
        {
            (bool ok, SessionInfo session, string sid) = await EnsureSessionAsync();
            if (!ok || session == null) return Unauthorized();

            HttpClient client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage resp = await client.PostAsync($"{ProductsServiceBase}/{Uri.EscapeDataString(id)}/bid", content);
            string respContent = await resp.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = respContent,
                ContentType = "application/json",
                StatusCode = (int)resp.StatusCode
            };
        }

        // GET api/bff/products/{id}
        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            (bool ok, SessionInfo session, string sid) = await EnsureSessionAsync();
            if (!ok || session == null) return Unauthorized();

            HttpClient client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

            HttpResponseMessage resp = await client.GetAsync($"{ProductsServiceBase}/{Uri.EscapeDataString(id)}");
            string content = await resp.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = content,
                ContentType = "application/json",
                StatusCode = (int)resp.StatusCode
            };
        }

        // GET api/bff/products/user-info
        [HttpGet("products/user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            (bool ok, SessionInfo session, string sid) = await EnsureSessionAsync();
            if (!ok || session == null) return Unauthorized();

            HttpClient client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

            HttpResponseMessage resp = await client.GetAsync($"{ProductsServiceBase}/user-info");
            string content = await resp.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = content,
                ContentType = "application/json",
                StatusCode = (int)resp.StatusCode
            };
        }
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OAuthCodeFlowService.Configuration;
using OAuthCodeFlowService.Models;
using OAuthCodeFlowService.Services;
using TokenValidation.Jwt;
using TokenValidation.TokenValidation;

namespace OAuthCodeFlowService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string SessionCookieName = "session_id";

        private readonly IPkceService _pkceService;
        private readonly IAuthorizationStateStore _stateStore;
        private readonly ITokenService _tokenService;
        private readonly OAuthSettings _settings;
        private readonly ILogger<AuthController> _logger;
        private readonly ISessionRepository _sessionRepository;

        public AuthController(
            IPkceService pkceService,
            IAuthorizationStateStore stateStore,
            ITokenService tokenService,
            IOptions<OAuthSettings> settings,
            ILogger<AuthController> logger,
            ISessionRepository sessionRepository)
        {
            _pkceService = pkceService;
            _stateStore = stateStore;
            _tokenService = tokenService;
            _settings = settings.Value;
            _logger = logger;
            _sessionRepository = sessionRepository;
        }

        /// <summary>
        /// Initiates the OAuth authorization flow by generating an authorization URL
        /// </summary>
        [HttpPost("authorize")]
        public async Task<ActionResult<AuthorizationUrlResponse>> Authorize([FromBody] AuthorizationRequest? request)
        {
            string codeVerifier = _pkceService.GenerateCodeVerifier();
            string codeChallenge = _pkceService.GenerateCodeChallenge(codeVerifier);
            string state = _pkceService.GenerateState();

            string redirectUri = _settings.RedirectUri;
            string scope = request?.Scope ?? _settings.Scope;

            AuthorizationState authState = new AuthorizationState
            {
                State = state,
                CodeVerifier = codeVerifier,
                CodeChallenge = codeChallenge,
                RedirectUri = redirectUri,
                OriginalRedirectUri = request?.RedirectUri
            };

            _stateStore.Store(authState);

            string authEndpoint = await _tokenService.GetAuthorizationEndpointAsync();
            System.Collections.Specialized.NameValueCollection queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["response_type"] = "code";
            queryParams["client_id"] = _settings.ClientId;
            queryParams["redirect_uri"] = redirectUri;
            queryParams["scope"] = scope;
            queryParams["state"] = state;
            queryParams["code_challenge"] = codeChallenge;
            queryParams["code_challenge_method"] = "S256";

            string authUrl = $"{authEndpoint}?{queryParams}";

            _logger.LogInformation("Authorize called; state={State}, redirect_uri={RedirectUri}", state[..10], redirectUri);
            _logger.LogInformation("Authorization URL: {AuthUrl}", authUrl);

            return Ok(new AuthorizationUrlResponse
            {
                AuthorizationUrl = authUrl,
                State = state
            });
        }

        /// <summary>
        /// Handles the OAuth callback and exchanges the authorization code for tokens
        /// Backend creates a server-side session and sets a secure, HttpOnly session cookie.
        /// </summary>
        [HttpPost("callback")]
        public async Task<ActionResult> Callback([FromBody] CallbackRequest request)
        {
            _logger.LogInformation("Callback POST invoked for state {State}", request.State);

            AuthorizationState? authState = _stateStore.Retrieve(request.State);

            if (authState == null)
            {
                _logger.LogWarning("Invalid state parameter received in POST callback: {State}", request.State);
                return BadRequest(new { error = "invalid_state", message = "Invalid or expired state parameter" });
            }

            if (authState.IsExpired(TimeSpan.FromMinutes(_settings.StateExpirationMinutes)))
            {
                _stateStore.Remove(request.State);
                _logger.LogWarning("Expired state parameter received in POST callback: {State}", request.State);
                return BadRequest(new { error = "expired_state", message = "Authorization state has expired" });
            }

            _stateStore.Remove(request.State);

            try
            {
                TokenResponse tokenResponse = await _tokenService.ExchangeCodeAsync(
                    request.Code,
                    authState.CodeVerifier,
                    authState.RedirectUri);

                _logger.LogInformation("Successfully exchanged code for tokens for state {State}", request.State);

                string? preferredName = JwtClaimReader.GetPreferredNameFromJwt(tokenResponse);
                string? userid = JwtClaimReader.GetNameFromJwt(tokenResponse);

                DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : 3600);
                SessionInfo sessionInfo = new SessionInfo(
                    tokenResponse.AccessToken,
                    tokenResponse.RefreshToken,
                    tokenResponse.IdToken,
                    expiresAt,
                    preferredName,
                    userid);

                string sessionId = _sessionRepository.Create(sessionInfo);

                Response.Cookies.Append(SessionCookieName, sessionId, BuildSessionCookieOptions(sessionInfo.ExpiresAt));

                _logger.LogInformation("Session created: {SessionId} (expires {Expires}) pref={Preferred}", sessionId, sessionInfo.ExpiresAt, preferredName);

                return Ok(new { message = "authenticated" });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Token exchange failed in POST callback");
                return BadRequest(new { error = "token_exchange_failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Handles the OAuth callback via GET (browser redirect)
        /// Backend will create session cookie and then redirect to original frontend URL without tokens in the URL.
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> CallbackGet(
            [FromQuery] string? code,
            [FromQuery] string? state,
            [FromQuery] string? error,
            [FromQuery(Name = "error_description")] string? errorDescription)
        {
            _logger.LogInformation("Callback GET invoked. code present: {HasCode}, state present: {HasState}, error: {Error}",
                !string.IsNullOrEmpty(code), !string.IsNullOrEmpty(state), error);

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("OAuth error received in GET callback: {Error} - {Description}", error, errorDescription);
                return BadRequest(new { error, message = errorDescription });
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogWarning("Missing code or state in GET callback");
                return BadRequest(new { error = "missing_parameters", message = "Code and state are required" });
            }

            AuthorizationState? authState = _stateStore.Retrieve(state);

            if (authState == null)
            {
                _logger.LogWarning("Invalid/expired state in GET callback: {State}", state);
                return BadRequest(new { error = "invalid_state", message = "Invalid or expired state parameter" });
            }

            if (authState.IsExpired(TimeSpan.FromMinutes(_settings.StateExpirationMinutes)))
            {
                _stateStore.Remove(state);
                _logger.LogWarning("Expired state in GET callback: {State}", state);
                return BadRequest(new { error = "expired_state", message = "Authorization state has expired" });
            }

            _stateStore.Remove(state);

            try
            {
                TokenResponse tokenResponse = await _tokenService.ExchangeCodeAsync(
                    code,
                    authState.CodeVerifier,
                    authState.RedirectUri);

                _logger.LogInformation("Successfully exchanged code for tokens (GET callback) for state {State}", state);

                string? preferredName = JwtClaimReader.GetPreferredNameFromJwt(tokenResponse);
                string? userid = JwtClaimReader.GetNameFromJwt(tokenResponse);

                DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : 3600);
                SessionInfo sessionInfo = new SessionInfo(
                    tokenResponse.AccessToken,
                    tokenResponse.RefreshToken,
                    tokenResponse.IdToken,
                    expiresAt,
                    preferredName,
                    userid);
                string sessionId = _sessionRepository.Create(sessionInfo);

                Response.Cookies.Append(SessionCookieName, sessionId, BuildSessionCookieOptions(sessionInfo.ExpiresAt));

                _logger.LogInformation("Session created: {SessionId} (expires {Expires}) pref={Preferred}", sessionId, sessionInfo.ExpiresAt, preferredName);

                if (!string.IsNullOrEmpty(authState.OriginalRedirectUri))
                {
                    _logger.LogInformation("Redirecting to original frontend URI: {Original}", authState.OriginalRedirectUri);
                    return Redirect(authState.OriginalRedirectUri);
                }

                return Ok(new { message = "authenticated" });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Token exchange failed in GET callback");
                return BadRequest(new { error = "token_exchange_failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Returns basic session information for the frontend. Backend will use stored tokens for protected calls.
        /// </summary>
        [HttpGet("me")]
        public ActionResult<object> Me()
        {
            if (Request.Cookies.TryGetValue(SessionCookieName, out string? sessionId))
            {
                _logger.LogInformation("Me called. session_id cookie present: {SessionIdPreview}", sessionId is null ? "null" : (sessionId.Length > 8 ? sessionId[..8] : sessionId));
            }
            else
            {
                _logger.LogInformation("Me called. session_id cookie NOT present.");
            }

            if (!Request.Cookies.TryGetValue(SessionCookieName, out sessionId) || string.IsNullOrEmpty(sessionId))
            {
                return Unauthorized(new { error = "no_session" });
            }

            SessionInfo? session = _sessionRepository.Get(sessionId);
            if (session == null)
            {
                return Unauthorized(new { error = "invalid_session" });
            }

            return Ok(new
            {
                expiresAt = session.ExpiresAt,
                hasRefreshToken = !string.IsNullOrEmpty(session.RefreshToken),
                preferredName = session.PreferredName,
                userId = session.UserId
            });
        }

        /// <summary>
        /// Refreshes an access token using a refresh token (server-side).
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponse>> Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                TokenResponse tokenResponse = await _tokenService.RefreshTokenAsync(request.RefreshToken);
                return Ok(tokenResponse);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return BadRequest(new { error = "refresh_failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Returns the logout URL for ending the session
        /// </summary>
        [HttpGet("logout-url")]
        public async Task<ActionResult> GetLogoutUrl([FromQuery] string? idTokenHint)
        {
            string endSessionEndpoint = await _tokenService.GetEndSessionEndpointAsync();
            System.Collections.Specialized.NameValueCollection queryParams = HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrEmpty(idTokenHint))
            {
                queryParams["id_token_hint"] = idTokenHint;
            }

            if (!string.IsNullOrEmpty(_settings.PostLogoutRedirectUri))
            {
                queryParams["post_logout_redirect_uri"] = _settings.PostLogoutRedirectUri;
            }

            string logoutUrl = queryParams.Count > 0
                ? $"{endSessionEndpoint}?{queryParams}"
                : endSessionEndpoint;

            return Ok(new { logoutUrl });
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });

        /// <summary>
        /// Builds consistent cookie options for session cookies.
        /// Sets Domain when CookieDomain is configured (to share across subdomains).
        /// </summary>
        private CookieOptions BuildSessionCookieOptions(DateTimeOffset expiresAt)
        {
            CookieOptions options = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiresAt.UtcDateTime,
                IsEssential = true
            };

            if (!string.IsNullOrEmpty(_settings.CookieDomain))
            {
                options.Domain = _settings.CookieDomain;
                options.SameSite = SameSiteMode.None;
                options.Secure = true;
            }
            else
            {
                options.Secure = Request.IsHttps;
                options.SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax;
            }

            return options;
        }
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using OAuthCodeFlowService.Configuration;
using OAuthCodeFlowService.Controllers;
using OAuthCodeFlowService.Models;
using OAuthCodeFlowService.Services;
using TokenValidation.Jwt;

namespace Tests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<IPkceService> _mockPkce = null!;
        private Mock<IAuthorizationStateStore> _mockStateStore = null!;
        private Mock<ITokenService> _mockTokenService = null!;
        private Mock<ISessionRepository> _mockSessionRepo = null!;
        private Mock<ILogger<AuthController>> _mockLogger = null!;
        private OAuthSettings _settings = null!;
        private AuthController _controller = null!;

        private static string CreateTestJwt(string name, string preferredUsername)
        {
            JwtSecurityTokenHandler handler = new();
            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("name", name),
                    new Claim("preferred_username", preferredUsername)
                }),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(new byte[32]),
                    SecurityAlgorithms.HmacSha256)
            };
            SecurityToken token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        private static DefaultHttpContext CreateHttpContextWithCookie(string? sessionId)
        {
            DefaultHttpContext ctx = new();
            if (sessionId != null)
            {
                Mock<IRequestCookieCollection> mockCookies = new();
                string? outVal = sessionId;
                mockCookies.Setup(c => c.TryGetValue("session_id", out outVal)).Returns(true);
                mockCookies.Setup(c => c.ContainsKey("session_id")).Returns(true);
                ctx.Request.Cookies = mockCookies.Object;
            }
            return ctx;
        }

        [SetUp]
        public void SetUp()
        {
            _mockPkce = new Mock<IPkceService>();
            _mockStateStore = new Mock<IAuthorizationStateStore>();
            _mockTokenService = new Mock<ITokenService>();
            _mockSessionRepo = new Mock<ISessionRepository>();
            _mockLogger = new Mock<ILogger<AuthController>>();

            _settings = new OAuthSettings
            {
                Issuer = "http://keycloak:8080/realms/dev-realm",
                ClientId = "test-client",
                ClientSecret = "secret",
                RedirectUri = "https://localhost/api/auth/callback",
                PostLogoutRedirectUri = "https://localhost",
                Scope = "openid profile email",
                StateExpirationMinutes = 10
            };

            _controller = new AuthController(
                _mockPkce.Object,
                _mockStateStore.Object,
                _mockTokenService.Object,
                Options.Create(_settings),
                _mockLogger.Object,
                _mockSessionRepo.Object);

            DefaultHttpContext httpContext = new();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Test]
        public async Task Authorize_ReturnsOkWithAuthorizationUrl()
        {
            _mockPkce.Setup(p => p.GenerateCodeVerifier()).Returns("verifier");
            _mockPkce.Setup(p => p.GenerateCodeChallenge("verifier")).Returns("challenge");
            _mockPkce.Setup(p => p.GenerateState()).Returns("random-state-value");
            _mockTokenService.Setup(t => t.GetAuthorizationEndpointAsync())
                .ReturnsAsync("http://keycloak:8080/realms/dev-realm/protocol/openid-connect/auth");

            ActionResult<AuthorizationUrlResponse> result = await _controller.Authorize(null);

            OkObjectResult ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);

            AuthorizationUrlResponse response = ok!.Value as AuthorizationUrlResponse;
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.State, Is.EqualTo("random-state-value"));
            Assert.That(response.AuthorizationUrl, Does.Contain("response_type=code"));
            Assert.That(response.AuthorizationUrl, Does.Contain("client_id=test-client"));
            Assert.That(response.AuthorizationUrl, Does.Contain("code_challenge=challenge"));
            Assert.That(response.AuthorizationUrl, Does.Contain("code_challenge_method=S256"));
        }

        [Test]
        public async Task Authorize_WithCustomScope_UsesProvidedScope()
        {
            _mockPkce.Setup(p => p.GenerateCodeVerifier()).Returns("v");
            _mockPkce.Setup(p => p.GenerateCodeChallenge("v")).Returns("c");
            _mockPkce.Setup(p => p.GenerateState()).Returns("state12345");
            _mockTokenService.Setup(t => t.GetAuthorizationEndpointAsync()).ReturnsAsync("http://auth/authorize");

            AuthorizationRequest request = new() { Scope = "openid custom-scope" };

            ActionResult<AuthorizationUrlResponse> result = await _controller.Authorize(request);

            OkObjectResult ok = result.Result as OkObjectResult;
            AuthorizationUrlResponse response = ok!.Value as AuthorizationUrlResponse;
            Assert.That(response!.AuthorizationUrl, Does.Contain("openid+custom-scope").Or.Contain("openid%20custom-scope"));
        }

        [Test]
        public async Task Authorize_StoresAuthorizationState()
        {
            _mockPkce.Setup(p => p.GenerateCodeVerifier()).Returns("v");
            _mockPkce.Setup(p => p.GenerateCodeChallenge("v")).Returns("c");
            _mockPkce.Setup(p => p.GenerateState()).Returns("state12345");
            _mockTokenService.Setup(t => t.GetAuthorizationEndpointAsync()).ReturnsAsync("http://auth/authorize");

            await _controller.Authorize(null);

            _mockStateStore.Verify(s => s.Store(It.Is<AuthorizationState>(
                a => a.State == "state12345" &&
                     a.CodeVerifier == "v" &&
                     a.CodeChallenge == "c")),
                Times.Once);
        }

        [Test]
        public async Task Callback_InvalidState_ReturnsBadRequest()
        {
            _mockStateStore.Setup(s => s.Retrieve("bad-state")).Returns((AuthorizationState?)null);

            CallbackRequest request = new() { Code = "code", State = "bad-state" };
            ActionResult result = await _controller.Callback(request);

            BadRequestObjectResult bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            string json = JsonSerializer.Serialize(bad!.Value);
            Assert.That(json, Does.Contain("invalid_state"));
        }

        [Test]
        public async Task Callback_ExpiredState_ReturnsBadRequest()
        {
            AuthorizationState expiredState = new()
            {
                State = "expired",
                CodeVerifier = "v",
                CodeChallenge = "c",
                RedirectUri = "https://localhost/callback",
                CreatedAt = DateTime.UtcNow.AddMinutes(-20)
            };
            _mockStateStore.Setup(s => s.Retrieve("expired")).Returns(expiredState);

            CallbackRequest request = new() { Code = "code", State = "expired" };
            ActionResult result = await _controller.Callback(request);

            BadRequestObjectResult bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            string json = JsonSerializer.Serialize(bad!.Value);
            Assert.That(json, Does.Contain("expired_state"));
            _mockStateStore.Verify(s => s.Remove("expired"), Times.Once);
        }

        [Test]
        public async Task Callback_ValidState_ExchangesCodeAndCreatesSession()
        {
            string testJwt = CreateTestJwt("John Doe", "johndoe");
            AuthorizationState authState = new()
            {
                State = "valid-state",
                CodeVerifier = "verifier",
                CodeChallenge = "challenge",
                RedirectUri = "https://localhost/callback"
            };
            _mockStateStore.Setup(s => s.Retrieve("valid-state")).Returns(authState);

            TokenResponse tokenResponse = new()
            {
                AccessToken = testJwt,
                RefreshToken = "refresh-token",
                IdToken = testJwt,
                ExpiresIn = 3600
            };
            _mockTokenService.Setup(t => t.ExchangeCodeAsync("auth-code", "verifier", "https://localhost/callback"))
                .ReturnsAsync(tokenResponse);
            _mockSessionRepo.Setup(r => r.Create(It.IsAny<SessionInfo>())).Returns("session-123");

            CallbackRequest request = new() { Code = "auth-code", State = "valid-state" };
            ActionResult result = await _controller.Callback(request);

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);

            _mockStateStore.Verify(s => s.Remove("valid-state"), Times.Once);
            _mockSessionRepo.Verify(r => r.Create(It.Is<SessionInfo>(
                si => si.AccessToken == testJwt && si.RefreshToken == "refresh-token")),
                Times.Once);
        }

        [Test]
        public async Task Callback_TokenExchangeFails_ReturnsBadRequest()
        {
            AuthorizationState authState = new()
            {
                State = "state",
                CodeVerifier = "v",
                CodeChallenge = "c",
                RedirectUri = "https://localhost/callback"
            };
            _mockStateStore.Setup(s => s.Retrieve("state")).Returns(authState);
            _mockTokenService.Setup(t => t.ExchangeCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Keycloak unreachable"));

            CallbackRequest request = new() { Code = "code", State = "state" };
            ActionResult result = await _controller.Callback(request);

            BadRequestObjectResult bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            string json = JsonSerializer.Serialize(bad!.Value);
            Assert.That(json, Does.Contain("token_exchange_failed"));
        }

        [Test]
        public async Task CallbackGet_OAuthError_ReturnsBadRequest()
        {
            IActionResult result = await _controller.CallbackGet(null, null, "access_denied", "User denied");

            BadRequestObjectResult bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
        }

        [Test]
        public async Task CallbackGet_MissingCodeOrState_ReturnsBadRequest()
        {
            IActionResult result = await _controller.CallbackGet(null, null, null, null);

            BadRequestObjectResult bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            string json = JsonSerializer.Serialize(bad!.Value);
            Assert.That(json, Does.Contain("missing_parameters"));
        }

        [Test]
        public async Task CallbackGet_InvalidState_ReturnsBadRequest()
        {
            _mockStateStore.Setup(s => s.Retrieve("bad")).Returns((AuthorizationState?)null);

            IActionResult result = await _controller.CallbackGet("code", "bad", null, null);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CallbackGet_ValidWithOriginalRedirectUri_Redirects()
        {
            string testJwt = CreateTestJwt("User", "user1");
            AuthorizationState authState = new()
            {
                State = "state",
                CodeVerifier = "v",
                CodeChallenge = "c",
                RedirectUri = "https://localhost/callback",
                OriginalRedirectUri = "https://auction.local:9443/dashboard"
            };
            _mockStateStore.Setup(s => s.Retrieve("state")).Returns(authState);
            _mockTokenService.Setup(t => t.ExchangeCodeAsync("code", "v", "https://localhost/callback"))
                .ReturnsAsync(new TokenResponse { AccessToken = testJwt, IdToken = testJwt, ExpiresIn = 3600 });
            _mockSessionRepo.Setup(r => r.Create(It.IsAny<SessionInfo>())).Returns("sid");

            IActionResult result = await _controller.CallbackGet("code", "state", null, null);

            RedirectResult redirect = result as RedirectResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.Url, Is.EqualTo("https://auction.local:9443/dashboard"));
        }

        [Test]
        public async Task CallbackGet_ValidWithoutOriginalRedirect_ReturnsOk()
        {
            string testJwt = CreateTestJwt("User", "user1");
            AuthorizationState authState = new()
            {
                State = "state",
                CodeVerifier = "v",
                CodeChallenge = "c",
                RedirectUri = "https://localhost/callback",
                OriginalRedirectUri = null
            };
            _mockStateStore.Setup(s => s.Retrieve("state")).Returns(authState);
            _mockTokenService.Setup(t => t.ExchangeCodeAsync("code", "v", "https://localhost/callback"))
                .ReturnsAsync(new TokenResponse { AccessToken = testJwt, IdToken = testJwt, ExpiresIn = 3600 });
            _mockSessionRepo.Setup(r => r.Create(It.IsAny<SessionInfo>())).Returns("sid");

            IActionResult result = await _controller.CallbackGet("code", "state", null, null);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public void Me_NoCookie_ReturnsUnauthorized()
        {
            ActionResult<object> result = _controller.Me();

            Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public void Me_InvalidSessionId_ReturnsUnauthorized()
        {
            InMemorySessionRepository realRepo = new();
            AuthController controller = new AuthController(
                _mockPkce.Object, _mockStateStore.Object, _mockTokenService.Object,
                Options.Create(_settings), _mockLogger.Object, realRepo);

            DefaultHttpContext ctx = CreateHttpContextWithCookie("invalid-session");
            controller.ControllerContext = new ControllerContext { HttpContext = ctx };

            ActionResult<object> result = controller.Me();

            Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public void Me_ValidSession_ReturnsSessionInfo()
        {
            InMemorySessionRepository realRepo = new();
            SessionInfo session = new("access", "refresh", "id", DateTimeOffset.UtcNow.AddHours(1), "JohnDoe", "user-1");
            string sessionId = realRepo.Create(session);

            AuthController controller = new AuthController(
                _mockPkce.Object, _mockStateStore.Object, _mockTokenService.Object,
                Options.Create(_settings), _mockLogger.Object, realRepo);

            DefaultHttpContext ctx = CreateHttpContextWithCookie(sessionId);
            controller.ControllerContext = new ControllerContext { HttpContext = ctx };

            ActionResult<object> result = controller.Me();

            OkObjectResult ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            string json = JsonSerializer.Serialize(ok!.Value);
            Assert.That(json, Does.Contain("JohnDoe"));
        }    

        [Test]
        public async Task Refresh_Success_ReturnsOkWithTokens()
        {
            TokenResponse tokenResponse = new() { AccessToken = "new-access", RefreshToken = "new-refresh", ExpiresIn = 3600 };
            _mockTokenService.Setup(t => t.RefreshTokenAsync("old-refresh")).ReturnsAsync(tokenResponse);

            RefreshRequest request = new() { RefreshToken = "old-refresh" };
            ActionResult<TokenResponse> result = await _controller.Refresh(request);

            OkObjectResult ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            TokenResponse response = ok!.Value as TokenResponse;
            Assert.That(response!.AccessToken, Is.EqualTo("new-access"));
        }

        [Test]
        public async Task Refresh_Failure_ReturnsBadRequest()
        {
            _mockTokenService.Setup(t => t.RefreshTokenAsync("bad-token"))
                .ThrowsAsync(new HttpRequestException("Token expired"));

            RefreshRequest request = new() { RefreshToken = "bad-token" };
            ActionResult<TokenResponse> result = await _controller.Refresh(request);

            BadRequestObjectResult bad = result.Result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            string json = JsonSerializer.Serialize(bad!.Value);
            Assert.That(json, Does.Contain("refresh_failed"));
        }

        [Test]
        public async Task GetLogoutUrl_WithIdTokenHint_IncludesItInUrl()
        {
            _mockTokenService.Setup(t => t.GetEndSessionEndpointAsync())
                .ReturnsAsync("http://keycloak/logout");

            ActionResult result = await _controller.GetLogoutUrl("my-id-token");

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            string json = JsonSerializer.Serialize(ok!.Value);
            Assert.That(json, Does.Contain("my-id-token"));
            Assert.That(json, Does.Contain("keycloak"));
        }

        [Test]
        public async Task GetLogoutUrl_WithPostLogoutRedirect_IncludesIt()
        {
            _mockTokenService.Setup(t => t.GetEndSessionEndpointAsync())
                .ReturnsAsync("http://keycloak/logout");

            ActionResult result = await _controller.GetLogoutUrl(null);

            OkObjectResult ok = result as OkObjectResult;
            string json = JsonSerializer.Serialize(ok!.Value);
            Assert.That(json, Does.Contain("https://localhost"));
        }

        [Test]
        public async Task GetLogoutUrl_NoPostLogoutUri_ReturnsEndpointOnly()
        {
            _settings.PostLogoutRedirectUri = "";
            _mockTokenService.Setup(t => t.GetEndSessionEndpointAsync())
                .ReturnsAsync("http://keycloak/logout");

            ActionResult result = await _controller.GetLogoutUrl(null);

            OkObjectResult ok = result as OkObjectResult;
            string json = JsonSerializer.Serialize(ok!.Value);
            Assert.That(json, Does.Contain("http://keycloak/logout"));
        }

        [Test]
        public void Health_ReturnsOk()
        {
            IActionResult result = _controller.Health();

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            string json = JsonSerializer.Serialize(ok!.Value);
            Assert.That(json, Does.Contain("healthy"));
        }
    }
}
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using OAuthCodeFlowService.Controllers;
using OAuthCodeFlowService.Services;

namespace Tests
{
    [TestFixture]
    public class BffProxyControllerTests
    {
        private Mock<ISessionRepository> _mockSessions = null!;
        private Mock<ITokenService> _mockTokenService = null!;
        private Mock<ILogger<BffProxyController>> _mockLogger = null!;
        private Mock<IHttpClientFactory> _mockHttpFactory = null!;
        private Mock<HttpMessageHandler> _mockHandler = null!;
        private IConfiguration _configuration = null!;

        private BffProxyController CreateController(string? sessionId = null, bool isAdmin = false)
        {
            BffProxyController controller = new BffProxyController(
                _mockSessions.Object,
                _mockTokenService.Object,
                _mockHttpFactory.Object,
                _mockLogger.Object,
                _configuration);

            DefaultHttpContext ctx = new();

            if (isAdmin)
            {
                ctx.Request.Headers["ssl-client-verify"] = "SUCCESS";
            }

            if (sessionId != null)
            {
                Mock<IRequestCookieCollection> mockCookies = new();
                string? outVal = sessionId;
                mockCookies.Setup(c => c.TryGetValue("session_id", out outVal)).Returns(true);
                mockCookies.Setup(c => c.ContainsKey("session_id")).Returns(true);
                ctx.Request.Cookies = mockCookies.Object;
            }

            controller.ControllerContext = new ControllerContext { HttpContext = ctx };
            return controller;
        }

        [SetUp]
        public void SetUp()
        {
            _mockSessions = new Mock<ISessionRepository>();
            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<BffProxyController>>();
            _mockHandler = new Mock<HttpMessageHandler>();
            _mockHttpFactory = new Mock<IHttpClientFactory>();

            Dictionary<string, string?> configData = new()
            {
                ["Services:ProductsApiBase"] = "http://services-hoster:8080/api/products",
                ["Services:PaymentsApiBase"] = "http://payments:8080"
            };
            _configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();
        }

        private void SetupHttpResponse(HttpStatusCode statusCode, string content)
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });

            HttpClient client = new HttpClient(_mockHandler.Object);
            _mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        }

        private void SetupValidSession(string sessionId)
        {
            SessionInfo session = new("access-token", "refresh", "id", DateTimeOffset.UtcNow.AddHours(1), "User", "user-1");
            _mockSessions.Setup(s => s.Get(sessionId)).Returns(session);
        }

        [Test]
        public async Task GetAll_NoCookie_ReturnsUnauthorized()
        {
            BffProxyController controller = CreateController(sessionId: null);

            IActionResult result = await controller.GetAll();

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task GetAll_InvalidSession_ReturnsUnauthorized()
        {
            _mockSessions.Setup(s => s.Get("bad-session")).Returns((SessionInfo?)null);
            BffProxyController controller = CreateController(sessionId: "bad-session");

            IActionResult result = await controller.GetAll();

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task GetAll_ValidSession_ProxiesRequestAndReturnsContent()
        {
            string expectedJson = """[{"id":"p-1","name":"Camera"}]""";
            SetupValidSession("session-1");
            SetupHttpResponse(HttpStatusCode.OK, expectedJson);

            BffProxyController controller = CreateController(sessionId: "session-1");

            IActionResult result = await controller.GetAll();

            ContentResult content = result as ContentResult;
            Assert.That(content, Is.Not.Null);
            Assert.That(content!.StatusCode, Is.EqualTo(200));
            Assert.That(content.Content, Is.EqualTo(expectedJson));
            Assert.That(content.ContentType, Is.EqualTo("application/json"));
        }

        [Test]
        public async Task GetAll_UpstreamReturns500_ProxiesStatusCode()
        {
            SetupValidSession("session-1");
            SetupHttpResponse(HttpStatusCode.InternalServerError, """{"error":"fail"}""");

            BffProxyController controller = CreateController(sessionId: "session-1");

            IActionResult result = await controller.GetAll();

            ContentResult content = result as ContentResult;
            Assert.That(content!.StatusCode, Is.EqualTo(500));
        }

        [Test]
        public async Task GetMyProducts_ValidSession_ProxiesRequest()
        {
            SetupValidSession("session-1");
            SetupHttpResponse(HttpStatusCode.OK, "[]");

            BffProxyController controller = CreateController(sessionId: "session-1");

            IActionResult result = await controller.GetMyProducts();

            ContentResult content = result as ContentResult;
            Assert.That(content, Is.Not.Null);
            Assert.That(content!.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task GetMyBids_NoCookie_ReturnsUnauthorized()
        {
            BffProxyController controller = CreateController(sessionId: null);

            IActionResult result = await controller.GetMyBids();

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task MarkProductsAsSold_NullIds_ReturnsBadRequest()
        {
            BffProxyController controller = CreateController(sessionId: null);

            IActionResult result = await controller.MarkProductsAsSold(null);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MarkProductsAsSold_EmptyIds_ReturnsBadRequest()
        {
            BffProxyController controller = CreateController(sessionId: null);

            IActionResult result = await controller.MarkProductsAsSold(new List<string>());

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MarkProductsAsSold_ValidSession_ProxiesRequest()
        {
            SetupValidSession("session-1");
            SetupHttpResponse(HttpStatusCode.OK, """{"updated":true}""");

            BffProxyController controller = CreateController(sessionId: "session-1");

            IActionResult result = await controller.MarkProductsAsSold(new List<string> { "p-1" });

            ContentResult content = result as ContentResult;
            Assert.That(content, Is.Not.Null);
            Assert.That(content!.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task AddFeedback_NullBody_ReturnsBadRequest()
        {
            BffProxyController controller = CreateController(sessionId: "session-1");

            IActionResult result = await controller.AddFeedback("p-1", null);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AddFeedback_ValidSession_ProxiesRequest()
        {
            SetupValidSession("session-1");
            SetupHttpResponse(HttpStatusCode.OK, """{"ok":true}""");

            BffProxyController controller = CreateController(sessionId: "session-1");
            JsonElement body = JsonSerializer.Deserialize<JsonElement>("""{"rating":5,"comment":"great"}""");

            IActionResult result = await controller.AddFeedback("p-1", body);

            ContentResult content = result as ContentResult;
            Assert.That(content, Is.Not.Null);
            Assert.That(content!.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task MarkProductsAsAccepted_NullIds_ReturnsBadRequest()
        {
            BffProxyController controller = CreateController(sessionId: null);

            IActionResult result = await controller.MarkProductsAsAccepted(null);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MarkProductsAsAccepted_EmptyIds_ReturnsBadRequest()
        {
            BffProxyController controller = CreateController(sessionId: null);

            IActionResult result = await controller.MarkProductsAsAccepted(new List<string>());

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MarkProductsAsAccepted_NotAdmin_ReturnsForbid()
        {
            SetupValidSession("session-1");
            BffProxyController controller = CreateController(sessionId: "session-1", isAdmin: false);

            IActionResult result = await controller.MarkProductsAsAccepted(new List<string> { "p-1" });

            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task MarkProductsAsAccepted_Admin_ProxiesRequest()
        {
            SetupValidSession("session-1");
            SetupHttpResponse(HttpStatusCode.OK, """{"accepted":true}""");

            BffProxyController controller = CreateController(sessionId: "session-1", isAdmin: true);

            IActionResult result = await controller.MarkProductsAsAccepted(new List<string> { "p-1" });

            ContentResult content = result as ContentResult;
            Assert.That(content, Is.Not.Null);
            Assert.That(content!.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task MarkProductsAsRejected_NotAdmin_ReturnsForbid()
        {
            SetupValidSession("session-1");
            BffProxyController controller = CreateController(sessionId: "session-1", isAdmin: false);

            JsonElement body = JsonSerializer.Deserialize<JsonElement>("""[{"id":"p-1","reason":"spam"}]""");
            IActionResult result = await controller.MarkProductsAsRejected(body);

            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task MarkProductsAsRejected_Admin_ProxiesRequest()
        {
            SetupValidSession("session-1");
            SetupHttpResponse(HttpStatusCode.OK, """{"rejected":true}""");

            BffProxyController controller = CreateController(sessionId: "session-1", isAdmin: true);

            JsonElement body = JsonSerializer.Deserialize<JsonElement>("""[{"id":"p-1","reason":"spam"}]""");
            IActionResult result = await controller.MarkProductsAsRejected(body);

            ContentResult content = result as ContentResult;
            Assert.That(content!.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task CreatePayment_NoCookie_ReturnsUnauthorized()
        {
            BffProxyController controller = CreateController(sessionId: null);
            JsonElement body = JsonSerializer.Deserialize<JsonElement>("""{"bidId":"b-1","amount":100}""");

            IActionResult result = await controller.CreatePayment(body);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task CreatePayment_ValidSession_ProxiesRequest()
        {
            SetupValidSession("session-1");
            SetupHttpResponse(HttpStatusCode.OK, """{"id":"pi_123","status":"requires_confirmation"}""");

            BffProxyController controller = CreateController(sessionId: "session-1");
            JsonElement body = JsonSerializer.Deserialize<JsonElement>("""{"bidId":"b-1","amount":100}""");

            IActionResult result = await controller.CreatePayment(body);

            ContentResult content = result as ContentResult;
            Assert.That(content, Is.Not.Null);
            Assert.That(content!.StatusCode, Is.EqualTo(200));
            Assert.That(content.Content, Does.Contain("pi_123"));
        }

        [Test]
        public async Task ConfirmPayment_ValidSession_ProxiesRequest()
        {
            SetupValidSession("session-1");
            SetupHttpResponse(HttpStatusCode.OK, """{"id":"pi_123","status":"succeeded"}""");

            BffProxyController controller = CreateController(sessionId: "session-1");

            IActionResult result = await controller.ConfirmPayment("pi_123");

            ContentResult content = result as ContentResult;
            Assert.That(content!.StatusCode, Is.EqualTo(200));
            Assert.That(content.Content, Does.Contain("succeeded"));
        }

        [Test]
        public async Task GetPayment_ValidSession_ProxiesRequest()
        {
            SetupValidSession("session-1");
            SetupHttpResponse(HttpStatusCode.OK, """{"id":"pi_123"}""");

            BffProxyController controller = CreateController(sessionId: "session-1");

            IActionResult result = await controller.GetPayment("pi_123");

            ContentResult content = result as ContentResult;
            Assert.That(content!.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task GetAll_ForwardsBearerTokenToUpstream()
        {
            SessionInfo session = new("my-access-token", "refresh", "id", DateTimeOffset.UtcNow.AddHours(1), "User", "user-1");
            _mockSessions.Setup(s => s.Get("session-1")).Returns(session);

            string? capturedAuthHeader = null;
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
                {
                    capturedAuthHeader = req.Headers.Authorization?.ToString();
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });

            HttpClient client = new HttpClient(_mockHandler.Object);
            _mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            BffProxyController controller = CreateController(sessionId: "session-1");
            await controller.GetAll();

            Assert.That(capturedAuthHeader, Is.EqualTo("Bearer my-access-token"));
        }

        [Test]
        public async Task GetAll_ExpiredSession_WithRefreshToken_RefreshesAndProxies()
        {
            SessionInfo expiredSession = new("old-token", "refresh-token", "id",
                DateTimeOffset.UtcNow.AddSeconds(-10), "User", "user-1");
            _mockSessions.Setup(s => s.Get("session-1")).Returns(expiredSession);

            TokenValidation.Jwt.TokenResponse refreshed = new()
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh",
                IdToken = "new-id",
                ExpiresIn = 3600
            };
            _mockTokenService.Setup(t => t.RefreshTokenAsync("refresh-token")).ReturnsAsync(refreshed);

            SetupHttpResponse(HttpStatusCode.OK, "[]");

            BffProxyController controller = CreateController(sessionId: "session-1");
            IActionResult result = await controller.GetAll();

            _mockSessions.Verify(s => s.Update("session-1", It.Is<SessionInfo>(
                si => si.AccessToken == "new-access-token")), Times.Once);

            ContentResult content = result as ContentResult;
            Assert.That(content!.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task GetAll_ExpiredSession_NoRefreshToken_ReturnsUnauthorized()
        {
            SessionInfo expiredNoRefresh = new("old-token", null, "id",
                DateTimeOffset.UtcNow.AddSeconds(-10), "User", "user-1");
            _mockSessions.Setup(s => s.Get("session-1")).Returns(expiredNoRefresh);

            BffProxyController controller = CreateController(sessionId: "session-1");
            IActionResult result = await controller.GetAll();

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
            _mockSessions.Verify(s => s.Remove("session-1"), Times.Once);
        }

        [Test]
        public async Task GetAll_ExpiredSession_RefreshFails_ReturnsUnauthorized()
        {
            SessionInfo expiredSession = new("old-token", "refresh-token", "id",
                DateTimeOffset.UtcNow.AddSeconds(-10), "User", "user-1");
            _mockSessions.Setup(s => s.Get("session-1")).Returns(expiredSession);
            _mockTokenService.Setup(t => t.RefreshTokenAsync("refresh-token"))
                .ThrowsAsync(new HttpRequestException("Keycloak down"));

            BffProxyController controller = CreateController(sessionId: "session-1");
            IActionResult result = await controller.GetAll();

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
            _mockSessions.Verify(s => s.Remove("session-1"), Times.Once);
        }
    }
}
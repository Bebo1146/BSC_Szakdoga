using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OAuthCodeFlowService.Configuration;
using TokenValidation.Jwt;

namespace OAuthCodeFlowService.Services
{
    public class TokenService : ITokenService
    {
        private readonly HttpClient _httpClient;
        private readonly OAuthSettings _settings;
        private readonly ILogger<TokenService> _logger;
        private OpenIdConnectConfiguration? _discoveryDocument;
        private readonly SemaphoreSlim _discoveryLock = new(1, 1);

        public TokenService(
            HttpClient httpClient,
            IOptions<OAuthSettings> settings,
            ILogger<TokenService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        private async Task<OpenIdConnectConfiguration> GetDiscoveryDocumentAsync()
        {
            if (_discoveryDocument != null)
                return _discoveryDocument;

            await _discoveryLock.WaitAsync();
            try
            {
                if (_discoveryDocument != null)
                    return _discoveryDocument;

                // Use the internal issuer for server-to-server discovery
                string internalIssuer = _settings.EffectiveInternalIssuer;

                // Allow HTTP for discovery in development only.
                // In production you MUST use HTTPS and keep RequireHttps = true.
                HttpDocumentRetriever httpDocRetriever = new HttpDocumentRetriever(_httpClient)
                {
                    RequireHttps = false
                };

                ConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{internalIssuer.TrimEnd('/')}/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever(),
                    httpDocRetriever);

                _discoveryDocument = await configurationManager.GetConfigurationAsync();
                _logger.LogInformation("Discovery document loaded from {Issuer}", internalIssuer);
                return _discoveryDocument;
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is IOException || ex is HttpRequestException || ex is TaskCanceledException)
            {
                _logger.LogError(ex, "Failed to load discovery document from {Issuer}", _settings.EffectiveInternalIssuer);
                throw new InvalidOperationException($"Failed to load discovery document from {_settings.EffectiveInternalIssuer}", ex);
            }
            finally
            {
                _discoveryLock.Release();
            }
        }

        /// <summary>
        /// Returns the token endpoint using the internal Keycloak URL (for server-to-server calls).
        /// </summary>
        public async Task<string> GetTokenEndpointAsync()
        {
            OpenIdConnectConfiguration doc = await GetDiscoveryDocumentAsync();
            return doc.TokenEndpoint;
        }

        /// <summary>
        /// Returns the authorization endpoint using the public Issuer URL (for browser redirects).
        /// </summary>
        public async Task<string> GetAuthorizationEndpointAsync()
        {
            OpenIdConnectConfiguration doc = await GetDiscoveryDocumentAsync();
            string endpoint = doc.AuthorizationEndpoint;

            // If using an internal issuer, replace the internal host with the public issuer
            // so the browser can reach Keycloak
            if (!string.IsNullOrEmpty(_settings.InternalIssuer) &&
                endpoint.StartsWith(_settings.InternalIssuer.TrimEnd('/')))
            {
                endpoint = _settings.Issuer.TrimEnd('/') +
                    endpoint[_settings.InternalIssuer.TrimEnd('/').Length..];
            }

            return endpoint;
        }

        /// <summary>
        /// Returns the end-session endpoint using the public Issuer URL (for browser redirects).
        /// </summary>
        public async Task<string> GetEndSessionEndpointAsync()
        {
            OpenIdConnectConfiguration doc = await GetDiscoveryDocumentAsync();
            string endpoint = doc.EndSessionEndpoint;

            // Replace internal host with public issuer for browser access
            if (!string.IsNullOrEmpty(_settings.InternalIssuer) &&
                endpoint.StartsWith(_settings.InternalIssuer.TrimEnd('/')))
            {
                endpoint = _settings.Issuer.TrimEnd('/') +
                    endpoint[_settings.InternalIssuer.TrimEnd('/').Length..];
            }

            return endpoint;
        }

        public async Task<TokenResponse> ExchangeCodeAsync(string code, string codeVerifier, string redirectUri)
        {
            string tokenEndpoint = await GetTokenEndpointAsync();

            Dictionary<string, string> postData = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["client_id"] = _settings.ClientId,
                ["code_verifier"] = codeVerifier
            };

            if (!string.IsNullOrEmpty(_settings.ClientSecret))
            {
                postData["client_secret"] = _settings.ClientSecret;
            }

            _logger.LogDebug("Exchanging code for token at {Endpoint}", tokenEndpoint);

            HttpResponseMessage response = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(postData));
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token exchange failed: {StatusCode} - {Content}", response.StatusCode, content);
                throw new HttpRequestException($"Token exchange failed: {response.StatusCode}");
            }

            TokenResponse tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content)
                ?? throw new InvalidOperationException("Failed to deserialize token response");

            _logger.LogInformation("Token exchange successful");
            return tokenResponse;
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            string tokenEndpoint = await GetTokenEndpointAsync();

            Dictionary<string, string> postData = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = _settings.ClientId
            };

            if (!string.IsNullOrEmpty(_settings.ClientSecret))
            {
                postData["client_secret"] = _settings.ClientSecret;
            }

            HttpResponseMessage response = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(postData));
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token refresh failed: {StatusCode} - {Content}", response.StatusCode, content);
                throw new HttpRequestException($"Token refresh failed: {response.StatusCode}");
            }

            return JsonSerializer.Deserialize<TokenResponse>(content)
                ?? throw new InvalidOperationException("Failed to deserialize token response");
        }
    }
}
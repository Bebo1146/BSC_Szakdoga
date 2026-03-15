using System.Collections.Concurrent;
using System.Text.Json;
using System.Web;

namespace OAuthStart.OAuth2.AuthorizationCodeFlow
{
    internal class AuthorizationCodeFlowService
    {
        private readonly Uri _authorizationEndpoint;
        private readonly Uri _tokenEndpoint;
        private readonly string _clientId;
        private readonly string? _clientSecret;
        private readonly string _redirectUri;
        private readonly string _scope;
        private readonly ConcurrentDictionary<string, AuthorizationState> _pendingAuthorizations;
        private readonly TimeSpan _stateExpiration;

        internal AuthorizationCodeFlowService(
            Uri authorizationEndpoint,
            Uri tokenEndpoint,
            string clientId,
            string redirectUri,
            string scope = "openid profile email",
            string? clientSecret = null,
            TimeSpan? stateExpiration = null)
        {
            _authorizationEndpoint = authorizationEndpoint;
            _tokenEndpoint = tokenEndpoint;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
            _scope = scope;
            _pendingAuthorizations = new ConcurrentDictionary<string, AuthorizationState>();
            _stateExpiration = stateExpiration ?? TimeSpan.FromMinutes(10);
        }

        internal string BuildAuthorizationUrl()
        {
            AuthorizationState authState = new AuthorizationState(_redirectUri);
            _pendingAuthorizations[authState.State] = authState;

            UriBuilder uriBuilder = new UriBuilder(_authorizationEndpoint);
            System.Collections.Specialized.NameValueCollection queryParams = HttpUtility.ParseQueryString(string.Empty);

            queryParams["response_type"] = "code";
            queryParams["client_id"] = _clientId;
            queryParams["redirect_uri"] = _redirectUri;
            queryParams["scope"] = _scope;
            queryParams["state"] = authState.State;
            queryParams["code_challenge"] = authState.CodeChallenge;
            queryParams["code_challenge_method"] = "S256";

            uriBuilder.Query = queryParams.ToString();

            Console.WriteLine($"DEBUG --- AuthorizationCodeFlowService>BuildAuthorizationUrl ---");
            Console.WriteLine($"DEBUG --- Authorization URL: {uriBuilder.Uri}");

            return uriBuilder.Uri.ToString();
        }

        internal async Task<TokenResponse> HandleCallbackAsync(string code, string state)
        {
            Console.WriteLine($"DEBUG --- AuthorizationCodeFlowService>HandleCallbackAsync ---");
            Console.WriteLine($"DEBUG --- Received code: {code[..Math.Min(10, code.Length)]}..., state: {state[..Math.Min(10, state.Length)]}...");

            if (!_pendingAuthorizations.TryRemove(state, out AuthorizationState? authState))
            {
                throw new InvalidOperationException("Invalid or expired state parameter. Possible CSRF attack.");
            }

            if (authState.IsExpired(_stateExpiration))
            {
                throw new InvalidOperationException("Authorization state has expired. Please try again.");
            }

            return await ExchangeCodeForTokenAsync(code, authState.CodeVerifier);
        }

        private async Task<TokenResponse> ExchangeCodeForTokenAsync(string code, string codeVerifier)
        {
            Dictionary<string, string> postData = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _redirectUri },
                { "client_id", _clientId },
                { "code_verifier", codeVerifier }
            };

            if (!string.IsNullOrEmpty(_clientSecret))
            {
                postData["client_secret"] = _clientSecret;
            }

            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine($"DEBUG --- Token exchange request to: {_tokenEndpoint}");

                FormUrlEncodedContent requestContent = new FormUrlEncodedContent(postData);
                HttpResponseMessage response = await client.PostAsync(_tokenEndpoint, requestContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"DEBUG --- Token response status: {response.StatusCode}");
                Console.WriteLine($"DEBUG --- Token response: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Token exchange failed: {response.StatusCode} - {responseContent}");
                }

                TokenResponse? tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    throw new InvalidOperationException("Failed to parse token response.");
                }

                return tokenResponse;
            }
        }

        internal async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            Dictionary<string, string> postData = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "client_id", _clientId }
            };

            if (!string.IsNullOrEmpty(_clientSecret))
            {
                postData["client_secret"] = _clientSecret;
            }

            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine($"DEBUG --- Refresh token request to: {_tokenEndpoint}");

                FormUrlEncodedContent requestContent = new FormUrlEncodedContent(postData);
                HttpResponseMessage response = await client.PostAsync(_tokenEndpoint, requestContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"DEBUG --- Refresh response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Token refresh failed: {response.StatusCode} - {responseContent}");
                }

                TokenResponse? tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    throw new InvalidOperationException("Failed to parse refresh token response.");
                }

                return tokenResponse;
            }
        }

        internal void CleanupExpiredStates()
        {
            foreach (KeyValuePair<string, AuthorizationState> kvp in _pendingAuthorizations)
            {
                if (kvp.Value.IsExpired(_stateExpiration))
                {
                    _pendingAuthorizations.TryRemove(kvp.Key, out _);
                }
            }
        }
    }
}
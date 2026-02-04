using System.Text.Json;

namespace OAuthStart.OAuth2HttpCommunication.FlowClients
{
    internal class AuthorizationCodeFlowClient
    {
        private Dictionary<string, string> _postData { get; }
        private Uri _tokenEndpoint { get; }

        internal AuthorizationCodeFlowClient(Uri tokenEndpoint, string code, string codeVerifier, string clientId, string secret = null)
        {
            _postData = new Dictionary<string, string>
            {
                { "code", code },
                { "code_verifier", codeVerifier },
                { "client_id", clientId },
                { "grant_type", GrantTypes.AuthorizationCode },
                { "redirect_uri", "" }
            };

            if (!string.IsNullOrEmpty(secret))
            {
                _postData.Add("client_secret", secret);
            }

            _tokenEndpoint = tokenEndpoint;
        }

        internal async Task<string> GetTokenAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine("DEBUG --- AuthorizationCodeFlowClient>GetTokenAsync  ---");
                Console.WriteLine($"DEBUG --- request content:\r\n{string.Join(Environment.NewLine, _postData)}");

                FormUrlEncodedContent requestContent = new FormUrlEncodedContent(_postData);

                HttpResponseMessage response = await client.PostAsync(_tokenEndpoint, requestContent);
                string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Console.WriteLine("DEBUG --- response status code: " + response.StatusCode);

                JsonDocument data = JsonDocument.Parse(responseContent);

                Console.WriteLine($"DEBUG --- response json content: {responseContent}");

                if (data.RootElement.TryGetProperty("access_token", out JsonElement accessTokenElement) && accessTokenElement.ValueKind == JsonValueKind.String)
                {
                    return accessTokenElement.GetString()!;
                }

                throw new InvalidOperationException("The access_token property was not found in the response.");
            }
        }
    }
}

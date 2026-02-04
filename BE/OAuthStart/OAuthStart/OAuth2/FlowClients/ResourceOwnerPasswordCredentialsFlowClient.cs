using System.Text.Json;

namespace OAuthStart.OAuth2HttpCommunication.FlowClients
{
    internal class ResourceOwnerPasswordCredentialsFlowClient
    {
        private Dictionary<string, string> _postData { get; }
        private Uri _tokenEndpoint { get; }

        internal ResourceOwnerPasswordCredentialsFlowClient(Uri tokenEndpoint, string clientId, string username, string password)
        {
            _postData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "password" },
                { "username", username },
                { "password", password }
            };

            _tokenEndpoint = tokenEndpoint;
        }

        internal ResourceOwnerPasswordCredentialsFlowClient(Uri tokenEndpoint, string clientId, string scope, string username, string password)
        {
            _postData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "password" },
                { "scope", scope },
                { "username", username },
                { "password", password }
            };

            _tokenEndpoint = tokenEndpoint;
        }

        internal async Task<string> GetTokenAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine("DEBUG --- ResourceOwnerPasswordCredentialsFlowClient>GetTokenAsync  ---");
                var output = _postData.Select(kvp => $"{kvp.Key}={(kvp.Key == "password" ? "**********" : kvp.Value)}");

                Console.WriteLine($"DEBUG --- token request content:\r\n{string.Join(Environment.NewLine, output)}");

                HttpResponseMessage response = await client.PostAsync(_tokenEndpoint, new FormUrlEncodedContent(_postData));
                string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                Console.WriteLine("DEBUG --- token response status code: " + response.StatusCode);

                JsonDocument data = JsonDocument.Parse(responseContent);

                Console.WriteLine($"DEBUG --- token response json content: {responseContent}");

                if (data.RootElement.TryGetProperty("access_token", out JsonElement accessTokenElement) && accessTokenElement.ValueKind == JsonValueKind.String)
                {
                    string? accessToken = accessTokenElement.GetString();
                    if (accessToken is not null)
                    {
                        return accessToken;
                    }
                }

                throw new InvalidOperationException("The access_token property was not found or is null in the token response.");
            }
        }
    }
}

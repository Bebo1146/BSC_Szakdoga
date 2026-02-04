using System.Text.Json;

namespace OAuthStart.OAuth2HttpCommunication.FlowClients
{
    internal class ClientCredentialsFlowClient
    {
        private Dictionary<string, string> _postData { get; }
        private Uri _tokenEndpoint { get; }

        internal ClientCredentialsFlowClient(Uri tokenEndpoint, string clientId, string secret, string scope)
        {
            _postData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "client_credentials" },
                { "client_secret", secret },
                { "scope", scope }
            };

            _tokenEndpoint = tokenEndpoint;
        }

        internal ClientCredentialsFlowClient(Uri tokenEndpoint, string clientId, string secret)
        {
            _postData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "client_credentials" },
                { "client_secret", secret },
            };

            _tokenEndpoint = tokenEndpoint;
        }

        internal async Task<string> GetTokenAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine("DEBUG --- ClientCredentialsFlowClient>GetTokenAsync  ---");
                Console.WriteLine($"DEBUG --- request content:\r\n{string.Join(Environment.NewLine, _postData)}");

                HttpResponseMessage response = await client.PostAsync(_tokenEndpoint, new FormUrlEncodedContent(_postData));
                string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Console.WriteLine("DEBUG --- response status code: " + response.StatusCode);

                JsonDocument data = JsonDocument.Parse(responseContent);

                Console.WriteLine($"DEBUG --- response json content: {responseContent}");

                if (data.RootElement.TryGetProperty("access_token", out JsonElement accessTokenElement) &&
                    accessTokenElement.ValueKind == JsonValueKind.String)
                {
                    return accessTokenElement.GetString()!;
                }

                throw new InvalidOperationException("The access_token property was not found in the response.");
            }
        }
    }
}

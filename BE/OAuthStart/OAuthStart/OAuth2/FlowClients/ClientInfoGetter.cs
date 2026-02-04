using System.Text.Json;

namespace OAuthStart.OAuth2.FlowClients
{
    internal class ClientInfoGetter
    {
        internal async Task<JsonDocument> GetClientInfoAsync(Uri clientInfoUrl, string accessToken, CancellationToken cancellationToken = default)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.GetAsync(clientInfoUrl, cancellationToken);

                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                JsonDocument clientInfo = JsonDocument.Parse(responseContent);

                return clientInfo;
            }
        }
    }
}
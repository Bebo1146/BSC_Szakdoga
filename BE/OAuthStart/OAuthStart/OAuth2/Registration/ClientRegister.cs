using System.Text;
using System.Text.Json;

namespace OAuthStart.OAuth2.Registration
{
    internal class ClientRegister
    {
        public async Task RegisterAsync(JsonDocument clientToRegister, Uri clientRegistrationUrl, string accessToken, CancellationToken cancellationToken = default)
        {
            using (HttpClient client = new HttpClient())
            {
                accessToken = accessToken.Trim().Trim('"');

                var json = clientToRegister.RootElement.GetRawText();
                using var req = new HttpRequestMessage(HttpMethod.Post, clientRegistrationUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                // Some environments are picky; this forces the header to be exactly what Postman sends.
                req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                req.Headers.TryAddWithoutValidation("Accept", "application/json");

                using var resp = await client.SendAsync(req, cancellationToken);
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);

                Console.WriteLine($"POST {clientRegistrationUrl}");
                Console.WriteLine($"Status: {(int)resp.StatusCode} {resp.StatusCode}");
                Console.WriteLine(body);

                resp.EnsureSuccessStatusCode();
            }
        }
    }
}

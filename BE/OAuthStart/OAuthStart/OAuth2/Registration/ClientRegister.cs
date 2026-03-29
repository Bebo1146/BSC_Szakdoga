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

                string json = clientToRegister.RootElement.GetRawText();
                using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, clientRegistrationUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                req.Headers.TryAddWithoutValidation("Accept", "application/json");

                using HttpResponseMessage resp = await client.SendAsync(req, cancellationToken);
                string body = await resp.Content.ReadAsStringAsync(cancellationToken);

                Console.WriteLine($"POST {clientRegistrationUrl}");
                Console.WriteLine($"Status: {(int)resp.StatusCode} {resp.StatusCode}");
                Console.WriteLine(body);

                resp.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Updates an existing client via PUT /admin/realms/{realm}/clients/{id}.
        /// Merges the internal Keycloak UUID into the payload before sending.
        /// </summary>
        public async Task UpdateAsync(JsonDocument clientToUpdate, Uri clientUrl, string internalId, string accessToken, CancellationToken cancellationToken = default)
        {
            using (HttpClient client = new HttpClient())
            {
                accessToken = accessToken.Trim().Trim('"');

                // Keycloak PUT requires the "id" field in the JSON body
                Dictionary<string, JsonElement> props = new();
                foreach (JsonProperty prop in clientToUpdate.RootElement.EnumerateObject())
                {
                    props[prop.Name] = prop.Value;
                }
                props["id"] = JsonDocument.Parse($"\"{internalId}\"").RootElement;

                string json = JsonSerializer.Serialize(props);

                using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, clientUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                req.Headers.TryAddWithoutValidation("Accept", "application/json");

                using HttpResponseMessage resp = await client.SendAsync(req, cancellationToken);
                string body = await resp.Content.ReadAsStringAsync(cancellationToken);

                Console.WriteLine($"PUT {clientUrl}");
                Console.WriteLine($"Status: {(int)resp.StatusCode} {resp.StatusCode}");
                if (!string.IsNullOrWhiteSpace(body)) Console.WriteLine(body);

                resp.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Registers or updates a client. Tries POST first; if 409 Conflict, looks up the client ID and PUTs.
        /// </summary>
        public async Task RegisterOrUpdateAsync(JsonDocument clientToRegister, Uri clientRegistrationUrl, string accessToken, CancellationToken cancellationToken = default)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                accessToken = accessToken.Trim().Trim('"');

                string json = clientToRegister.RootElement.GetRawText();
                string clientId = clientToRegister.RootElement.GetProperty("clientId").GetString()!;

                // Try to register (POST)
                using HttpRequestMessage postReq = new HttpRequestMessage(HttpMethod.Post, clientRegistrationUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                postReq.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                postReq.Headers.TryAddWithoutValidation("Accept", "application/json");

                using HttpResponseMessage postResp = await httpClient.SendAsync(postReq, cancellationToken);

                if (postResp.IsSuccessStatusCode)
                {
                    string body = await postResp.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine($"POST {clientRegistrationUrl} → {(int)postResp.StatusCode} (created)");
                    Console.WriteLine(body);
                    return;
                }

                if ((int)postResp.StatusCode != 409)
                {
                    string errorBody = await postResp.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine($"POST {clientRegistrationUrl} → {(int)postResp.StatusCode}");
                    Console.WriteLine(errorBody);
                    postResp.EnsureSuccessStatusCode(); // throws
                }

                // 409 Conflict — client already exists. Look up its internal ID.
                Console.WriteLine($"Client '{clientId}' already exists. Updating...");

                Uri lookupUrl = new Uri($"{clientRegistrationUrl}?clientId={Uri.EscapeDataString(clientId)}");
                using HttpRequestMessage getReq = new HttpRequestMessage(HttpMethod.Get, lookupUrl);
                getReq.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                getReq.Headers.TryAddWithoutValidation("Accept", "application/json");

                using HttpResponseMessage getResp = await httpClient.SendAsync(getReq, cancellationToken);
                getResp.EnsureSuccessStatusCode();

                string getBody = await getResp.Content.ReadAsStringAsync(cancellationToken);
                JsonDocument clients = JsonDocument.Parse(getBody);
                string internalId = clients.RootElement[0].GetProperty("id").GetString()!;

                // PUT to update (with internal ID in URL and body)
                Uri updateUrl = new Uri($"{clientRegistrationUrl}/{internalId}");
                await UpdateAsync(clientToRegister, updateUrl, internalId, accessToken, cancellationToken);
                Console.WriteLine($"Client '{clientId}' updated successfully.");
            }
        }
    }
}

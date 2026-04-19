using System.Text.Json;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OAuthStart.OAuth2.Registration;
using OAuthStart.OAuth2HttpCommunication;
using OAuthStart.OAuth2HttpCommunication.FlowClients;

namespace OAuthStart
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string devRealmIssuer = "http://localhost:8080/realms/dev-realm/";
            string masterIssuer = "http://localhost:8080/realms/master/";

            DiscoveryClient devRealmDiscoveryClient = new DiscoveryClient(devRealmIssuer);
            DiscoveryClient masterDiscoveryClient = new DiscoveryClient(masterIssuer);

            OpenIdConnectConfiguration masterDiscoveryDocument = masterDiscoveryClient.GetDiscoveryDocumentAsync().Result;
            OpenIdConnectConfiguration devDiscoveryDocument = devRealmDiscoveryClient.GetDiscoveryDocumentAsync().Result;

            Console.WriteLine(masterDiscoveryDocument.TokenEndpoint);

            ResourceOwnerPasswordCredentialsFlowClient ROPCFlowClient =
                new ResourceOwnerPasswordCredentialsFlowClient(new Uri(masterDiscoveryDocument.TokenEndpoint), "admin-cli", "admin", "admin");

            string token = await ROPCFlowClient.GetTokenAsync();

            string myBackendClient = """
            {
              "clientId": "my-backend-client",
              "name": "Backend OAuth Code Flow Client",
              "enabled": true,
              "protocol": "openid-connect",
              "publicClient": false,
              "standardFlowEnabled": true,
              "directAccessGrantsEnabled": false,
              "serviceAccountsEnabled": false,
              "redirectUris": [
                "https://localhost:4443/api/auth/callback",
                "https://localhost:4443/auth-callback",
                "https://localhost:4443/*",
                "http://localhost:4200/api/auth/callback",
                "http://localhost:4200/auth-callback",
                "https://localhost:7037/api/auth/callback",
                "http://localhost:5215/api/auth/callback",
                "https://oauth.pstmn.io/v1/callback"
              ],
              "webOrigins": [
                "https://localhost:4443",
                "http://localhost:4200",
                "http://localhost:3000",
                "http://localhost:5173"
              ],
              "attributes": {
                "pkce.code.challenge.method": "S256",
                "post.logout.redirect.uris": "https://localhost:4443##http://localhost:4200"
              }
            }
            """;

            JsonDocument myBackendClientToRegister = JsonDocument.Parse(
    myBackendClient,
    new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });

            ClientRegister clientRegister = new ClientRegister();

            await clientRegister.RegisterOrUpdateAsync(myBackendClientToRegister, new Uri("http://localhost:8080/admin/realms/dev-realm/clients"), token);

            string oidcJson = """
                {
                  "client_name": "svc-client2",
                  "application_type": "confidential",
                  "grant_types": ["client_credentials"],
                  "token_endpoint_auth_method": "client_secret_basic"
                }
                """;

            JsonDocument oidcClientToRegister = JsonDocument.Parse(oidcJson);

            RealmRegister realmRegister = new RealmRegister();

            await realmRegister.RegisterAsync(
                JsonDocument.Parse("""
                {
                  "realm": "dev-realm",
                  "enabled": true
                }
                """),
                new Uri("http://localhost:8080/admin/realms"),
                token);
        }
    }
}

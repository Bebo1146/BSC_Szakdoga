using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OAuthStart.OAuth2.AuthorizationCodeFlow;
using OAuthStart.OAuth2HttpCommunication;
using System.Diagnostics;

namespace OAuthStart
{
    internal class AuthCodeFlowExample
    {
        internal static async Task RunAsync()
        {
            string devRealmIssuer = "http://localhost:8080/realms/dev-realm/";
            DiscoveryClient discoveryClient = new DiscoveryClient(devRealmIssuer);
            OpenIdConnectConfiguration discoveryDocument = await discoveryClient.GetDiscoveryDocumentAsync();

            AuthorizationCodeFlowService authService = new AuthorizationCodeFlowService(
                authorizationEndpoint: new Uri(discoveryDocument.AuthorizationEndpoint),
                tokenEndpoint: new Uri(discoveryDocument.TokenEndpoint),
                clientId: "my-backend-client6",
                redirectUri: "http://localhost:5000/callback",
                scope: "openid profile email",
                clientSecret: "your-client-secret"
            );

            string authUrl = authService.BuildAuthorizationUrl();
            Console.WriteLine($"Opening browser for authorization: {authUrl}");
            Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });

            using (OAuthCallbackHandler callbackHandler = new OAuthCallbackHandler(authService, "http://localhost:5000/"))
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
                {
                    try
                    {
                        TokenResponse tokenResponse = await callbackHandler.StartAndWaitForCallbackAsync(cts.Token);
                        
                        Console.WriteLine("=== Token Response ===");
                        Console.WriteLine($"Access Token: {tokenResponse.AccessToken[..50]}...");
                        Console.WriteLine($"Token Type: {tokenResponse.TokenType}");
                        Console.WriteLine($"Expires In: {tokenResponse.ExpiresIn} seconds");
                        
                        if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                        {
                            Console.WriteLine($"Refresh Token: {tokenResponse.RefreshToken[..20]}...");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Authorization failed: {ex.Message}");
                    }
                }
            }
        }
    }
}
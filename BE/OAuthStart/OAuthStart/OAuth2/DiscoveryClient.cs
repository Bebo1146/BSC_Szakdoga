using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace OAuthStart.OAuth2HttpCommunication
{
    internal class DiscoveryClient
    {
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configManager;
        private string _authority { get; }

        internal DiscoveryClient(string authority)
        {
            _authority = authority;

            HttpDocumentRetriever documentRetriever = new HttpDocumentRetriever() { RequireHttps = _authority.StartsWith("https://") };
            _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                new Uri(new Uri(_authority), ".well-known/openid-configuration").AbsoluteUri,
                new OpenIdConnectConfigurationRetriever(),
                documentRetriever
            );
        }

        internal async Task<OpenIdConnectConfiguration> GetDiscoveryDocumentAsync()
        {
            try
            {
                OpenIdConnectConfiguration config = await _configManager
                    .GetConfigurationAsync(CancellationToken.None)
                    .ConfigureAwait(false);

                return config;
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is IOException || ex is HttpRequestException || ex is TaskCanceledException)
            {
                string message = $"OpenID Connect discovery failed for issuer '{_authority}'. " +
                    $"The authority may be unreachable or not expose a valid '.well-known/openid-configuration'." +
                    Environment.NewLine +
                    $"Exception: {ex}";
                Console.WriteLine($"ERROR --- {message}");
                throw new InvalidOperationException(message, ex);
            }
        }
    }
}
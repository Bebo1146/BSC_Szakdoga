namespace OAuthCodeFlowService.Configuration
{
    public class OAuthSettings
    {
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Internal (Docker-network) URL for server-to-server calls (discovery, token exchange).
        /// Falls back to Issuer if not set.
        /// </summary>
        public string? InternalIssuer { get; set; }

        /// <summary>
        /// Returns InternalIssuer if set, otherwise Issuer.
        /// Use this for all server-side HTTP calls to Keycloak.
        /// </summary>
        public string EffectiveInternalIssuer => !string.IsNullOrEmpty(InternalIssuer) ? InternalIssuer : Issuer;

        public string ClientId { get; set; } = string.Empty;
        public string? ClientSecret { get; set; }
        public string RedirectUri { get; set; } = string.Empty;
        public string PostLogoutRedirectUri { get; set; } = string.Empty;
        public string Scope { get; set; } = "openid profile email";
        public int StateExpirationMinutes { get; set; } = 10;
    }
}
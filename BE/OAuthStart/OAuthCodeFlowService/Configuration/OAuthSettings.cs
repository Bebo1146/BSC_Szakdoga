namespace OAuthCodeFlowService.Configuration
{
    public class OAuthSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string? InternalIssuer { get; set; }
        public string EffectiveInternalIssuer => !string.IsNullOrEmpty(InternalIssuer) ? InternalIssuer : Issuer;
        public string ClientId { get; set; } = string.Empty;
        public string? ClientSecret { get; set; }
        public string RedirectUri { get; set; } = string.Empty;
        public string PostLogoutRedirectUri { get; set; } = string.Empty;
        public string Scope { get; set; } = "openid profile email";
        public int StateExpirationMinutes { get; set; } = 10;

        /// <summary>
        /// Optional cookie domain for sharing session across subdomains (e.g. ".auction.local").
        /// </summary>
        public string? CookieDomain { get; set; }
    }
}
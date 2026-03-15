namespace OAuthCodeFlowService.Configuration
{
    public class OAuthSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string? ClientSecret { get; set; }
        public string RedirectUri { get; set; } = string.Empty;
        public string PostLogoutRedirectUri { get; set; } = string.Empty;
        public string Scope { get; set; } = "openid profile email";
        public int StateExpirationMinutes { get; set; } = 10;
    }
}
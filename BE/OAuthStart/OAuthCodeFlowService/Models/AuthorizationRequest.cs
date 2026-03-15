namespace OAuthCodeFlowService.Models
{
    public class AuthorizationRequest
    {
        public string? RedirectUri { get; set; }
        public string? Scope { get; set; }
    }

    public class CallbackRequest
    {
        public required string Code { get; set; }
        public required string State { get; set; }
    }

    public class RefreshRequest
    {
        public required string RefreshToken { get; set; }
    }

    public class AuthorizationUrlResponse
    {
        public required string AuthorizationUrl { get; set; }
        public required string State { get; set; }
    }
}
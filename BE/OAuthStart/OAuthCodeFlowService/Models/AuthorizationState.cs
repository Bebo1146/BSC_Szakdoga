namespace OAuthCodeFlowService.Models
{
    public class AuthorizationState
    {
        public required string State { get; init; }
        public required string CodeVerifier { get; init; }
        public required string CodeChallenge { get; init; }
        public required string RedirectUri { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public string? OriginalRedirectUri { get; init; }

        public bool IsExpired(TimeSpan maxAge) => DateTime.UtcNow - CreatedAt > maxAge;
    }
}
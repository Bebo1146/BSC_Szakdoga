using System.Security.Cryptography;

namespace OAuthStart.OAuth2.AuthorizationCodeFlow
{
    internal class AuthorizationState
    {
        internal string State { get; }
        internal string CodeVerifier { get; }
        internal string CodeChallenge { get; }
        internal DateTime CreatedAt { get; }
        internal string? RedirectUri { get; }

        internal AuthorizationState(string redirectUri)
        {
            PkceGenerator pkce = new PkceGenerator();
            State = GenerateState();
            CodeVerifier = pkce.CodeVerifier;
            CodeChallenge = pkce.CodeChallenge;
            CreatedAt = DateTime.UtcNow;
            RedirectUri = redirectUri;
        }

        private static string GenerateState()
        {
            byte[] randomBytes = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        internal bool IsExpired(TimeSpan maxAge)
        {
            return DateTime.UtcNow - CreatedAt > maxAge;
        }
    }
}
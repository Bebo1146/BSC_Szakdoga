namespace TokenValidation.TokenValidation
{
    public sealed class TokenValidationOptions
    {
        public string Authority { get; init; } = "";
        public string Audience { get; init; } = "";

        /// <summary>
        /// Override the issuer used for token validation.
        /// Use when the token's "iss" claim differs from Authority (e.g., Docker internal vs external URLs).
        /// Falls back to Authority if not set.
        /// </summary>
        public string? ValidIssuer { get; init; }

        // Optional knobs you may want
        public bool RequireHttpsMetadata { get; init; } = true;
        public int ClockSkewSeconds { get; init; } = 60;

        // Optional: required scope enforcement at policy level
        public string[] RequiredScopes { get; init; } = Array.Empty<string>();
    }
}

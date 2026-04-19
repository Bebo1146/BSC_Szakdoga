namespace TokenValidation.TokenValidation
{
    public sealed class TokenValidationOptions
    {
        public string Authority { get; init; } = "";
        public string Audience { get; init; } = "";
        public string? ValidIssuer { get; init; }
        public bool RequireHttpsMetadata { get; init; } = true;
        public int ClockSkewSeconds { get; init; } = 60;
        public string[] RequiredScopes { get; init; } = Array.Empty<string>();
    }
}

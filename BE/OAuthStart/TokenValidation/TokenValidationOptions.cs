namespace TokenValidation
{
    public sealed class TokenValidationOptions
    {
        public string Authority { get; init; } = "";
        public string Audience { get; init; } = "";

        // Optional knobs you may want
        public bool RequireHttpsMetadata { get; init; } = true;
        public int ClockSkewSeconds { get; init; } = 60;

        // Optional: required scope enforcement at policy level
        public string[] RequiredScopes { get; init; } = Array.Empty<string>();
    }
}

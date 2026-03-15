using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using TokenValidation.Jwt;

namespace TokenValidation.TokenValidation 
{
    public static class JwtClaimReader
    {
        public static string? GetPreferredNameFromJwt(TokenResponse tokenResponse)
        {
            string jwt = tokenResponse.IdToken ?? tokenResponse.AccessToken;
            return GetPreferredNameFromJwtString(jwt);
        }

        public static string? GetNameFromJwt(TokenResponse tokenResponse)
        {
            string jwt = tokenResponse.IdToken ?? tokenResponse.AccessToken;
            return GetNameFromJwtString(jwt);
        }

        // New helper: extract token from Authorization header value (e.g. "Bearer <token>")
        public static string? GetTokenFromAuthorizationHeader(string? authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader)) return null;

            const string bearerPrefix = "Bearer ";
            if (authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string token = authorizationHeader.Substring(bearerPrefix.Length).Trim();
                return string.IsNullOrEmpty(token) ? null : token;
            }

            // If header doesn't use "Bearer " prefix, assume header itself might be the token
            return string.IsNullOrWhiteSpace(authorizationHeader) ? null : authorizationHeader.Trim();
        }

        // Modified: return a TokenResponse built from the bearer token on the request
        public static TokenResponse? GetTokenFromRequest(HttpRequest? request)
        {
            if (request == null) return null;

            if (request.Headers.TryGetValue("Authorization", out StringValues values))
            {
                string? header = values.FirstOrDefault();
                string? token = GetTokenFromAuthorizationHeader(header);
                if (string.IsNullOrEmpty(token)) return null;

                // Construct a minimal TokenResponse using the extracted access token.
                return new TokenResponse
                {
                    AccessToken = token,
                    TokenType = "Bearer",
                    RefreshToken = null,
                    IdToken = null,
                    ExpiresIn = 0,
                    Scope = null
                };
            }

            return null;
        }

        // Internal helpers that operate on raw JWT string
        private static string? GetPreferredNameFromJwtString(string? jwt)
        {
            if (string.IsNullOrEmpty(jwt)) return null;

            try
            {
                // If someone passed a header containing "Bearer ...", strip it defensively
                if (jwt.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    jwt = jwt.Substring("Bearer ".Length).Trim();

                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(jwt))
                    return null;

                JwtSecurityToken token = handler.ReadJwtToken(jwt);

                string? preferred = token.Claims.FirstOrDefault(c => c.Type == "preferred_username" || c.Type == ClaimTypes.Name)?.Value;
                return string.IsNullOrWhiteSpace(preferred) ? null : preferred;
            }
            catch
            {
                return null;
            }
        }

        private static string? GetNameFromJwtString(string? jwt)
        {
            if (string.IsNullOrEmpty(jwt)) return null;

            try
            {
                if (jwt.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    jwt = jwt.Substring("Bearer ".Length).Trim();

                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(jwt))
                    return null;

                JwtSecurityToken token = handler.ReadJwtToken(jwt);

                string? name = token.Claims.FirstOrDefault(c => c.Type == "name" || c.Type == ClaimTypes.Name)?.Value;
                return string.IsNullOrWhiteSpace(name) ? null : name;
            }
            catch
            {
                return null;
            }
        }
    }
}

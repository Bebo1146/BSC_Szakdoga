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

        public static string? GetTokenFromAuthorizationHeader(string? authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader)) return null;

            const string bearerPrefix = "Bearer ";
            if (authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string token = authorizationHeader.Substring(bearerPrefix.Length).Trim();
                return string.IsNullOrEmpty(token) ? null : token;
            }

            return string.IsNullOrWhiteSpace(authorizationHeader) ? null : authorizationHeader.Trim();
        }

        public static TokenResponse? GetTokenFromRequest(HttpRequest? request)
        {
            if (request == null) return null;

            if (request.Headers.TryGetValue("Authorization", out StringValues values))
            {
                string? header = values.FirstOrDefault();
                string? token = GetTokenFromAuthorizationHeader(header);
                if (string.IsNullOrEmpty(token)) return null;

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

        private static string? GetPreferredNameFromJwtString(string? jwt)
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

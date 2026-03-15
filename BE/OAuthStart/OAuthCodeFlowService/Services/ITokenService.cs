using OAuthCodeFlowService.Models;
using TokenValidation.Jwt;

namespace OAuthCodeFlowService.Services
{
    public interface ITokenService
    {
        Task<string> GetTokenEndpointAsync();
        Task<string> GetAuthorizationEndpointAsync();
        Task<string> GetEndSessionEndpointAsync();
        Task<TokenResponse> ExchangeCodeAsync(string code, string codeVerifier, string redirectUri);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    }
}
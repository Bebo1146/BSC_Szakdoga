using OAuthCodeFlowService.Models;

namespace OAuthCodeFlowService.Services
{
    public interface IAuthorizationStateStore
    {
        void Store(AuthorizationState state);
        AuthorizationState? Retrieve(string stateKey);
        void Remove(string stateKey);
        void CleanupExpired(TimeSpan maxAge);
    }
}
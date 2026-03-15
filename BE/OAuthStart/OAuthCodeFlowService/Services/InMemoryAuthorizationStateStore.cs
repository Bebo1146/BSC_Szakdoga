using System.Collections.Concurrent;
using OAuthCodeFlowService.Models;

namespace OAuthCodeFlowService.Services
{
    public class InMemoryAuthorizationStateStore : IAuthorizationStateStore
    {
        private readonly ConcurrentDictionary<string, AuthorizationState> _states = new();

        public void Store(AuthorizationState state)
        {
            _states[state.State] = state;
        }

        public AuthorizationState? Retrieve(string stateKey)
        {
            _states.TryGetValue(stateKey, out AuthorizationState? state);
            return state;
        }

        public void Remove(string stateKey)
        {
            _states.TryRemove(stateKey, out _);
        }

        public void CleanupExpired(TimeSpan maxAge)
        {
            foreach (KeyValuePair<string, AuthorizationState> kvp in _states)
            {
                if (kvp.Value.IsExpired(maxAge))
                {
                    _states.TryRemove(kvp.Key, out _);
                }
            }
        }
    }
}
using System;
using System.Collections.Concurrent;

namespace OAuthCodeFlowService.Services
{
    public sealed record SessionInfo(
        string AccessToken,
        string? RefreshToken,
        string? IdToken,
        DateTimeOffset ExpiresAt,
        string? PreferredName);

    public interface ISessionRepository
    {
        string Create(SessionInfo info);
        SessionInfo? Get(string id);
        void Update(string id, SessionInfo info);
        void Remove(string id);
    }

    public sealed class InMemorySessionRepository : ISessionRepository
    {
        private readonly ConcurrentDictionary<string, SessionInfo> _store = new();

        public string Create(SessionInfo info)
        {
            string id = Guid.NewGuid().ToString("N");
            _store[id] = info;
            return id;
        }

        public SessionInfo? Get(string id) =>
            _store.TryGetValue(id, out SessionInfo? value) ? value : null;

        // Replace session contents for an existing id
        public void Update(string id, SessionInfo info)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            _store.AddOrUpdate(id, info, (_, __) => info);
        }

        public void Remove(string id) => _store.TryRemove(id, out _);
    }
}
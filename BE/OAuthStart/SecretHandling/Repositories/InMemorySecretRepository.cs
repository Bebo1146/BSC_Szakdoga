using System;
using System.Collections.Concurrent;
using System.Text;
using SecretHandling.Interfaces;
using SecretHandling.Models;

namespace SecretHandling.Repositories
{
    internal sealed class InMemorySecretRepository : ISecretRepository
    {
        private readonly ConcurrentDictionary<string, Secret> _store = new();

        public InMemorySecretRepository()
        {
            // Seed the in-memory store with the client secret for local/dev use
            _store["my-backend-client3"] = new Secret(Encoding.UTF8.GetBytes("guSeztSLShenJQnkCtjukabXm1HWdYKM"));
        }

        public Secret Read(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("clientId is required", nameof(clientId));

            if (_store.TryGetValue(clientId, out Secret? secret))
            {
                return secret;
            }

            throw new KeyNotFoundException($"Secret not found for clientId '{clientId}'.");
        }

        public void Write(string clientId, Secret secret)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("clientId is required", nameof(clientId));
            if (secret is null)
                throw new ArgumentNullException(nameof(secret));

            _store[clientId] = secret;
        }
    }
}
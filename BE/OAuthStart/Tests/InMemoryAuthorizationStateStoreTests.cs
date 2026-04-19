using NUnit.Framework;
using OAuthCodeFlowService.Models;
using OAuthCodeFlowService.Services;

namespace Tests
{
    [TestFixture]
    public class InMemoryAuthorizationStateStoreTests
    {
        private InMemoryAuthorizationStateStore _store = null!;

        [SetUp]
        public void SetUp()
        {
            _store = new InMemoryAuthorizationStateStore();
        }

        private static AuthorizationState CreateState(string key, DateTime? createdAt = null)
        {
            return new AuthorizationState
            {
                State = key,
                CodeVerifier = "verifier",
                CodeChallenge = "challenge",
                RedirectUri = "https://localhost/callback",
                CreatedAt = createdAt ?? DateTime.UtcNow
            };
        }

        [Test]
        public void Store_And_Retrieve_ReturnsStoredState()
        {
            AuthorizationState state = CreateState("test-key");

            _store.Store(state);
            AuthorizationState? retrieved = _store.Retrieve("test-key");

            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved!.CodeVerifier, Is.EqualTo("verifier"));
        }

        [Test]
        public void Retrieve_NonExistentKey_ReturnsNull()
        {
            AuthorizationState? retrieved = _store.Retrieve("non-existent");

            Assert.That(retrieved, Is.Null);
        }

        [Test]
        public void Remove_DeletesState()
        {
            AuthorizationState state = CreateState("to-remove");
            _store.Store(state);

            _store.Remove("to-remove");

            Assert.That(_store.Retrieve("to-remove"), Is.Null);
        }

        [Test]
        public void Store_OverwritesExistingState()
        {
            _store.Store(CreateState("dup-key"));

            AuthorizationState replacement = new()
            {
                State = "dup-key",
                CodeVerifier = "new-verifier",
                CodeChallenge = "new-challenge",
                RedirectUri = "https://other/callback"
            };
            _store.Store(replacement);

            AuthorizationState? retrieved = _store.Retrieve("dup-key");
            Assert.That(retrieved!.CodeVerifier, Is.EqualTo("new-verifier"));
        }

        [Test]
        public void CleanupExpired_RemovesOnlyExpiredStates()
        {
            _store.Store(CreateState("fresh", DateTime.UtcNow));
            _store.Store(CreateState("expired", DateTime.UtcNow.AddMinutes(-20)));

            _store.CleanupExpired(TimeSpan.FromMinutes(10));

            Assert.That(_store.Retrieve("fresh"), Is.Not.Null);
            Assert.That(_store.Retrieve("expired"), Is.Null);
        }
    }
}
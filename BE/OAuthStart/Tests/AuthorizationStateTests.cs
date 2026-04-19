using NUnit.Framework;
using OAuthCodeFlowService.Models;

namespace Tests
{
    [TestFixture]
    public class AuthorizationStateTests
    {
        [Test]
        public void IsExpired_WithinMaxAge_ReturnsFalse()
        {
            AuthorizationState state = new()
            {
                State = "key",
                CodeVerifier = "v",
                CodeChallenge = "c",
                RedirectUri = "https://localhost",
                CreatedAt = DateTime.UtcNow
            };

            Assert.That(state.IsExpired(TimeSpan.FromMinutes(10)), Is.False);
        }

        [Test]
        public void IsExpired_PastMaxAge_ReturnsTrue()
        {
            AuthorizationState state = new()
            {
                State = "key",
                CodeVerifier = "v",
                CodeChallenge = "c",
                RedirectUri = "https://localhost",
                CreatedAt = DateTime.UtcNow.AddMinutes(-15)
            };

            Assert.That(state.IsExpired(TimeSpan.FromMinutes(10)), Is.True);
        }
    }
}
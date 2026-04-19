using NUnit.Framework;
using OAuthCodeFlowService.Services;

namespace Tests
{
    [TestFixture]
    public class PkceServiceTests
    {
        private PkceService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new PkceService();
        }

        [Test]
        public void GenerateCodeVerifier_ReturnsBase64UrlEncodedString()
        {
            string verifier = _service.GenerateCodeVerifier();

            Assert.That(verifier, Is.Not.Null.And.Not.Empty);
            Assert.That(verifier, Does.Not.Contain('+'));
            Assert.That(verifier, Does.Not.Contain('/'));
            Assert.That(verifier, Does.Not.Contain('='));
        }

        [Test]
        public void GenerateCodeVerifier_ProducesUniqueValues()
        {
            string verifier1 = _service.GenerateCodeVerifier();
            string verifier2 = _service.GenerateCodeVerifier();

            Assert.That(verifier1, Is.Not.EqualTo(verifier2));
        }

        [Test]
        public void GenerateCodeChallenge_ReturnsDeterministicHash()
        {
            string verifier = _service.GenerateCodeVerifier();

            string challenge1 = _service.GenerateCodeChallenge(verifier);
            string challenge2 = _service.GenerateCodeChallenge(verifier);

            Assert.That(challenge1, Is.EqualTo(challenge2));
        }

        [Test]
        public void GenerateCodeChallenge_DifferentVerifiersProduceDifferentChallenges()
        {
            string challenge1 = _service.GenerateCodeChallenge("verifier-one");
            string challenge2 = _service.GenerateCodeChallenge("verifier-two");

            Assert.That(challenge1, Is.Not.EqualTo(challenge2));
        }

        [Test]
        public void GenerateCodeChallenge_OutputIsBase64UrlEncoded()
        {
            string challenge = _service.GenerateCodeChallenge("test-verifier");

            Assert.That(challenge, Does.Not.Contain('+'));
            Assert.That(challenge, Does.Not.Contain('/'));
            Assert.That(challenge, Does.Not.Contain('='));
        }

        [Test]
        public void GenerateState_ReturnsNonEmptyUniqueValues()
        {
            string state1 = _service.GenerateState();
            string state2 = _service.GenerateState();

            Assert.That(state1, Is.Not.Null.And.Not.Empty);
            Assert.That(state1, Is.Not.EqualTo(state2));
        }
    }
}
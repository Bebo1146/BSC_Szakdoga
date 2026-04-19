using NUnit.Framework;
using OAuthCodeFlowService.Services;

namespace Tests
{
    [TestFixture]
    public class InMemorySessionRepositoryTests
    {
        private InMemorySessionRepository _repo = null!;

        [SetUp]
        public void SetUp()
        {
            _repo = new InMemorySessionRepository();
        }

        private static SessionInfo CreateSession(string token = "access-token") =>
            new(token, "refresh-token", "id-token", DateTimeOffset.UtcNow.AddHours(1), "TestUser", "user-1");

        [Test]
        public void Create_ReturnsNonEmptyId()
        {
            string id = _repo.Create(CreateSession());

            Assert.That(id, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void Get_AfterCreate_ReturnsSession()
        {
            SessionInfo session = CreateSession();
            string id = _repo.Create(session);

            SessionInfo? retrieved = _repo.Get(id);

            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved!.AccessToken, Is.EqualTo("access-token"));
            Assert.That(retrieved.PreferredName, Is.EqualTo("TestUser"));
        }

        [Test]
        public void Get_NonExistentId_ReturnsNull()
        {
            Assert.That(_repo.Get("no-such-id"), Is.Null);
        }

        [Test]
        public void Update_ReplacesSession()
        {
            string id = _repo.Create(CreateSession("old-token"));

            SessionInfo updated = CreateSession("new-token");
            _repo.Update(id, updated);

            SessionInfo? retrieved = _repo.Get(id);
            Assert.That(retrieved!.AccessToken, Is.EqualTo("new-token"));
        }

        [Test]
        public void Update_EmptyId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _repo.Update("", CreateSession()));
        }

        [Test]
        public void Remove_DeletesSession()
        {
            string id = _repo.Create(CreateSession());

            _repo.Remove(id);

            Assert.That(_repo.Get(id), Is.Null);
        }

        [Test]
        public void Create_ProducesUniqueIds()
        {
            string id1 = _repo.Create(CreateSession());
            string id2 = _repo.Create(CreateSession());

            Assert.That(id1, Is.Not.EqualTo(id2));
        }
    }
}
using NUnit.Framework;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class UserSessionTest
    {
        [SetUp]
        public void Setup()
        {
            // Reset before each test
            MarketMinds.Shared.Services.UserSession.CurrentUserId = null;
            MarketMinds.Shared.Services.UserSession.CurrentUserRole = null;
        }

        [Test]
        public void SetAndGet_CurrentUserId_Works()
        {
            MarketMinds.Shared.Services.UserSession.CurrentUserId = 42;
            Assert.That(MarketMinds.Shared.Services.UserSession.CurrentUserId, Is.EqualTo(42));
        }

        [Test]
        public void SetAndGet_CurrentUserRole_Works()
        {
            MarketMinds.Shared.Services.UserSession.CurrentUserRole = "Admin";
            Assert.That(MarketMinds.Shared.Services.UserSession.CurrentUserRole, Is.EqualTo("Admin"));
        }

        [Test]
        public void DefaultValues_AreNull()
        {
            Assert.That(MarketMinds.Shared.Services.UserSession.CurrentUserId, Is.Null);
            Assert.That(MarketMinds.Shared.Services.UserSession.CurrentUserRole, Is.Null);
        }
    }
} 
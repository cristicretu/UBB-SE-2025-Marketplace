using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.IRepository;
using Moq;
using NUnit.Framework;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class WaitlistServiceTest
    {
        private Mock<IWaitListRepository> _mockRepo;
        private WaitlistService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IWaitListRepository>();
            _service = new WaitlistService();
            typeof(WaitlistService)
                .GetField("_waitListRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_service, _mockRepo.Object);
        }

        [Test]
        public async Task AddUserToWaitlist_CallsRepository()
        {
            await _service.AddUserToWaitlist(1, 2);
            _mockRepo.Verify(r => r.AddUserToWaitlist(1, 2), Times.Once);
        }

        [Test]
        public async Task GetUserWaitlistPosition_ReturnsPosition()
        {
            _mockRepo.Setup(r => r.GetUserWaitlistPosition(1, 2)).ReturnsAsync(3);
            var result = await _service.GetUserWaitlistPosition(1, 2);
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public async Task IsUserInWaitlist_ReturnsTrue()
        {
            _mockRepo.Setup(r => r.IsUserInWaitlist(1, 2)).ReturnsAsync(true);
            var result = await _service.IsUserInWaitlist(1, 2);
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task RemoveUserFromWaitlist_CallsRepository()
        {
            await _service.RemoveUserFromWaitlist(1, 2);
            _mockRepo.Verify(r => r.RemoveUserFromWaitlist(1, 2), Times.Once);
        }

        [Test]
        public async Task GetUsersInWaitlist_ReturnsList()
        {
            var list = new List<UserWaitList> { new UserWaitList { UserID = 1 } };
            _mockRepo.Setup(r => r.GetUsersInWaitlist(2)).ReturnsAsync(list);
            var result = await _service.GetUsersInWaitlist(2);
            Assert.That(result, Is.EqualTo(list));
        }

        [Test]
        public async Task GetUserWaitlists_ReturnsList()
        {
            var list = new List<UserWaitList> { new UserWaitList { UserID = 1 } };
            _mockRepo.Setup(r => r.GetUserWaitlists(1)).ReturnsAsync(list);
            var result = await _service.GetUserWaitlists(1);
            Assert.That(result, Is.EqualTo(list));
        }

        [Test]
        public async Task GetWaitlistSize_ReturnsSize()
        {
            _mockRepo.Setup(r => r.GetWaitlistSize(2)).ReturnsAsync(5);
            var result = await _service.GetWaitlistSize(2);
            Assert.That(result, Is.EqualTo(5));
        }
    }
} 
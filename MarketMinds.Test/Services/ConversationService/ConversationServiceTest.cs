using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository; // <-- Add this
using MarketMinds.Shared.Services.ConversationService; // <-- And this
using MarketMinds.Test.Services.ConversationService;
using MarketMinds.Shared.Services.Interfaces;
namespace MarketMinds.Test.Services.ConversationService
{
    [TestFixture]
    public class ConversationServiceTest
    {
        private IConversationService _service;

        private ConversationRepositoryMock _repo;

        [SetUp]
        public void Setup()
        {
            _repo = new ConversationRepositoryMock();
            _service = new MarketMinds.Shared.Services.ConversationService.ConversationService(_repo);

        }

        [Test]
        public async Task CreateConversationAsync_ValidUserId_ReturnsConversation()
        {
            var result = await _service.CreateConversationAsync(1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(1));
            Assert.That(result.Id, Is.GreaterThan(0));
        }

        [Test]
        public void CreateConversationAsync_InvalidUserId_ThrowsArgumentException()
        {
            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.CreateConversationAsync(0));
            Assert.That(ex.InnerException, Is.TypeOf<ArgumentException>());
        }

        [Test]
        public void CreateConversationAsync_RepositoryReturnsNull_ThrowsInvalidOperationException()
        {
            // Simulate repository returning null (UserId < 0)
            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.CreateConversationAsync(-5));
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void CreateConversationAsync_RepositoryReturnsInvalidId_ThrowsInvalidOperationException()
        {
            // Add a conversation with Id = 0 to simulate failure
            var repo = new ConversationRepositoryMock();
            repo.AddTestConversation(new Conversation { Id = 0, UserId = 2 });
            var service = new MarketMinds.Shared.Services.ConversationService.ConversationService(repo);


            // Patch the repo to return this conversation
            repo.Clear();
            repo.AddTestConversation(new Conversation { Id = 0, UserId = 2 });

            // Use a custom repo that always returns Id = 0
            var customRepo = new ConversationRepositoryMockAlwaysZero();
            var customService = new MarketMinds.Shared.Services.ConversationService.ConversationService(customRepo);


            var ex = Assert.ThrowsAsync<Exception>(async () => await customService.CreateConversationAsync(2));
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public async Task GetConversationByIdAsync_ValidId_ReturnsConversation()
        {
            var conversation = new Conversation { Id = 10, UserId = 1 };
            _repo.AddTestConversation(conversation);

            var result = await _service.GetConversationByIdAsync(10);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(10));
        }

        [Test]
        public void GetConversationByIdAsync_InvalidId_ThrowsArgumentException()
        {
            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.GetConversationByIdAsync(0));
            Assert.That(ex.InnerException, Is.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetConversationByIdAsync_NotFound_ThrowsKeyNotFoundException()
        {
            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.GetConversationByIdAsync(999));
            Assert.That(ex.InnerException, Is.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public async Task GetUserConversationsAsync_ValidUserId_ReturnsList()
        {
            _repo.AddTestConversation(new Conversation { Id = 1, UserId = 5 });
            _repo.AddTestConversation(new Conversation { Id = 2, UserId = 5 });
            _repo.AddTestConversation(new Conversation { Id = 3, UserId = 6 });

            var result = await _service.GetUserConversationsAsync(5);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetUserConversationsAsync_NoConversations_ReturnsEmptyList()
        {
            var result = await _service.GetUserConversationsAsync(12345);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetUserConversationsAsync_InvalidUserId_ThrowsArgumentException()
        {
            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.GetUserConversationsAsync(0));
            Assert.That(ex.InnerException, Is.TypeOf<ArgumentException>());
        }
    }

    // Custom repo to simulate CreateConversationAsync returning Id = 0
    public class ConversationRepositoryMockAlwaysZero : IConversationRepository
    {
        public Task<Conversation> CreateConversationAsync(Conversation conversation)
        {
            return Task.FromResult(new Conversation { Id = 0, UserId = conversation.UserId });
        }

        public Task<Conversation> GetConversationByIdAsync(int conversationId)
        {
            return Task.FromResult<Conversation>(null);
        }

        public Task<List<Conversation>> GetConversationsByUserIdAsync(int userId)
        {
            return Task.FromResult(new List<Conversation>());
        }
    }
}

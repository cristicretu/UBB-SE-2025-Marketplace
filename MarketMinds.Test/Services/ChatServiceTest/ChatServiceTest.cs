using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using MarketMinds.Shared.Models;
using MarketMinds.Test.Services.ChatServiceTest;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services;

namespace MarketMinds.Tests.Services.ChatServiceTest
{
    [TestFixture]
    public class ChatServiceTest
    {
        private IChatService _chatService;
        private ChatRepositoryMock _chatRepositoryMock;

        private const int USER_ID = 1;
        private const int CONVERSATION_ID = 100;

        [SetUp]
        public void Setup()
        {
            _chatRepositoryMock = new ChatRepositoryMock();
            _chatService = new ChatService(_chatRepositoryMock);
        }

        [Test]
        public async Task CreateConversationAsync_ShouldReturnConversation()
        {
            var result = await _chatService.CreateConversationAsync(USER_ID);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(USER_ID));
        }

        [Test]
        public async Task GetConversationAsync_ShouldReturnNull_WhenNotFound()
        {
            var result = await _chatService.GetConversationAsync(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetConversationAsync_ShouldReturn_WhenFound()
        {
            var conversation = new Conversation { Id = CONVERSATION_ID, UserId = USER_ID };
            _chatRepositoryMock.AddTestConversation(conversation);

            var result = await _chatService.GetConversationAsync(CONVERSATION_ID);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(CONVERSATION_ID));
        }

        [Test]
        public async Task SendMessageAsync_ShouldAddMessage()
        {
            var result = await _chatService.SendMessageAsync(CONVERSATION_ID, USER_ID, "hello");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.EqualTo("hello"));
            Assert.That(result.UserId, Is.EqualTo(USER_ID));
        }

        [Test]
        public async Task GetMessagesAsync_ShouldReturnMessages()
        {
            _chatRepositoryMock.AddTestMessages(new List<Message>
            {
                new Message { Id = 1, ConversationId = CONVERSATION_ID, UserId = USER_ID, Content = "Test 1" },
                new Message { Id = 2, ConversationId = CONVERSATION_ID, UserId = USER_ID, Content = "Test 2" }
            });

            var result = await _chatService.GetMessagesAsync(CONVERSATION_ID);

            Assert.That(result.Count, Is.EqualTo(2));
        }
    }
}


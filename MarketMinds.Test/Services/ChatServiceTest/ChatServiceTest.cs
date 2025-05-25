using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.DreamTeam.ChatService;
using MarketMinds.Test.Services.ChatServiceTest;
// Remove this using to avoid ambiguity
// using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

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
            var conversation = new Conversation { Id = CONVERSATION_ID, UserId = USER_ID };
            _chatRepositoryMock.AddTestConversation(conversation);

            var result = await _chatService.SendMessageAsync(CONVERSATION_ID, USER_ID, "hello");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.EqualTo("hello"));
            Assert.That(result.UserId, Is.EqualTo(USER_ID));
        }

        [Test]
        public async Task GetMessagesAsync_ShouldReturnMessages()
        {
            _chatRepositoryMock.AddTestMessages(new List<MarketMinds.Shared.Models.Message>
            {
                new MarketMinds.Shared.Models.Message { Id = 1, ConversationId = CONVERSATION_ID, UserId = USER_ID, Content = "Test 1" },
                new MarketMinds.Shared.Models.Message { Id = 2, ConversationId = CONVERSATION_ID, UserId = USER_ID, Content = "Test 2" }
            });

            var result = await _chatService.GetMessagesAsync(CONVERSATION_ID);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetUserConversationsAsync_ShouldReturnConversations()
        {
            var conversation1 = new Conversation { Id = 1, UserId = USER_ID };
            var conversation2 = new Conversation { Id = 2, UserId = USER_ID };
            _chatRepositoryMock.AddTestConversation(conversation1);
            _chatRepositoryMock.AddTestConversation(conversation2);

            var result = await _chatService.GetUserConversationsAsync(USER_ID);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].UserId, Is.EqualTo(USER_ID));
            Assert.That(result[1].UserId, Is.EqualTo(USER_ID));
        }

        [Test]
        public async Task GetMessagesAsync_ShouldReturnEmptyList_WhenNoMessages()
        {
            var result = await _chatService.GetMessagesAsync(9999);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task SendMessageAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            // Arrange: do not add any conversation, so repository returns null
            // (or you can mock the repository to return null explicitly if using a mock framework)
            var result = await _chatService.SendMessageAsync(9999, 1, "test");

            // Assert
            Assert.That(result, Is.Null);
        }


        // Remove this method, as it is not needed and causes errors
        /*
        public Task<Message> SendMessageAsync(int conversationId, int userId, string content)
        {
            var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conversation == null)
                return Task.FromResult<Message>(null);

            var message = new Message
            {
                Id = _messages.Count + 1,
                ConversationId = conversationId,
                UserId = userId,
                Content = content
            };
            _messages.Add(message);
            return Task.FromResult(message);
        }
        */
    }


}

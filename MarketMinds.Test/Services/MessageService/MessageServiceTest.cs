using System;
using System.Threading.Tasks;
using Xunit;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.MessageService;
using MarketMinds.Tests.Mocks;

namespace MarketMinds.Tests.Services
{
    public class MessageServiceTests
    {
        private readonly MessageService _service;

        public MessageServiceTests()
        {
            var mockRepo = new MockMessageRepository();
            _service = new MessageService(mockRepo);
        }

        [Fact]
        public async Task CreateMessageAsync_ShouldCreateMessageSuccessfully()
        {
            var dto = new CreateMessageDto
            {
                ConversationId = 1,
                UserId = 10,
                Content = "Hello, world!"
            };

            var result = await _service.CreateMessageAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.ConversationId, result.ConversationId);
            Assert.Equal(dto.UserId, result.UserId);
            Assert.Equal(dto.Content, result.Content);
        }

        [Theory]
        [InlineData(0, 1, "Hi")]
        [InlineData(1, 0, "Hi")]
        [InlineData(1, 1, "")]
        [InlineData(1, 1, "   ")]
        public async Task CreateMessageAsync_InvalidInput_ShouldThrow(int convId, int userId, string content)
        {
            var dto = new CreateMessageDto
            {
                ConversationId = convId,
                UserId = userId,
                Content = content
            };

            await Assert.ThrowsAnyAsync<ArgumentException>(() => _service.CreateMessageAsync(dto));
        }

        [Fact]
        public async Task GetMessagesByConversationIdAsync_ShouldReturnMessages()
        {
            var dto1 = new CreateMessageDto { ConversationId = 2, UserId = 1, Content = "Msg 1" };
            var dto2 = new CreateMessageDto { ConversationId = 2, UserId = 1, Content = "Msg 2" };

            await _service.CreateMessageAsync(dto1);
            await _service.CreateMessageAsync(dto2);

            var messages = await _service.GetMessagesByConversationIdAsync(2);

            Assert.Equal(2, messages.Count);
        }

        [Fact]
        public async Task GetMessagesByConversationIdAsync_InvalidId_ShouldThrow()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetMessagesByConversationIdAsync(0));
        }

        [Fact]
        public async Task Legacy_CreateMessageAsync_ShouldReturnMessage()
        {
            var result = await _service.CreateMessageAsync(3, 20, "Legacy call");

            Assert.NotNull(result);
            Assert.Equal(3, result.ConversationId);
            Assert.Equal(20, result.UserId);
            Assert.Equal("Legacy call", result.Content);
        }
        [Fact]
        public async Task Legacy_GetMessagesLegacyAsync_ShouldReturnMessages()
        {
            await _service.CreateMessageAsync(new CreateMessageDto
            {
                ConversationId = 4,
                UserId = 5,
                Content = "Legacy msg 1"
            });

            var messages = await _service.GetMessagesLegacyAsync(4);

            Assert.Single(messages);
            Assert.Equal("Legacy msg 1", messages[0].Content);
        }
    }
}

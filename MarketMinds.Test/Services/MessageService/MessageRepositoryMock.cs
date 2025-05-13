using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Tests.Mocks
{
    public class MockMessageRepository : IMessageRepository
    {
        private readonly List<Message> _messages = new();
        private int _nextId = 1;

        public Task<Message> CreateMessageAsync(CreateMessageDto newMessage)
        {
            var message = new Message
            {
                Id = _nextId++,
                ConversationId = newMessage.ConversationId,
                UserId = newMessage.UserId,
                Content = newMessage.Content
            };

            _messages.Add(message);
            return Task.FromResult(message);
        }

        public Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId)
        {
            var result = _messages
                .Where(m => m.ConversationId == conversationId)
                .ToList();

            return Task.FromResult(result);
        }
    }
}

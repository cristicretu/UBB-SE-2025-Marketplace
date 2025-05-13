using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Test.Services.ChatServiceTest
{
    public class ChatRepositoryMock : IChatRepository
    {
        private readonly List<Conversation> _conversations = new();
        private readonly List<Message> _messages = new();

        public Task<Conversation> CreateConversationAsync(int userId)
        {
            var conversation = new Conversation
            {
                Id = _conversations.Count + 1,
                UserId = userId
            };
            _conversations.Add(conversation);
            return Task.FromResult(conversation);
        }

        public Task<Conversation> GetConversationAsync(int conversationId)
        {
            var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
            return Task.FromResult(conversation);
        }

        public Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            var list = _conversations.Where(c => c.UserId == userId).ToList();
            return Task.FromResult(list);
        }

        public Task<Message> SendMessageAsync(int conversationId, int userId, string content)
        {
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

        public Task<List<Message>> GetMessagesAsync(int conversationId)
        {
            var msgs = _messages.Where(m => m.ConversationId == conversationId).ToList();
            return Task.FromResult(msgs);
        }

        // Helpers for test control
        public void AddTestConversation(Conversation c) => _conversations.Add(c);
        public void AddTestMessages(IEnumerable<Message> m) => _messages.AddRange(m);
    }
}


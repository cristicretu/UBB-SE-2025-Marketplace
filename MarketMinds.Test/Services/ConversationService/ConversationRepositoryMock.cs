using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Test.Services.ConversationService
{
    public class ConversationRepositoryMock : IConversationRepository
    {
        private readonly List<Conversation> _conversations = new();

        public Task<Conversation> CreateConversationAsync(Conversation conversation)
        {
            if (conversation == null)
                return Task.FromResult<Conversation>(null);

            // Simulate failure if UserId is negative
            if (conversation.UserId < 0)
                return Task.FromResult<Conversation>(null);

            // Simulate success
            conversation.Id = _conversations.Count + 1;
            _conversations.Add(conversation);
            return Task.FromResult(conversation);
        }

        public Task<Conversation> GetConversationByIdAsync(int conversationId)
        {
            var conv = _conversations.FirstOrDefault(c => c.Id == conversationId);
            return Task.FromResult(conv);
        }

        public Task<List<Conversation>> GetConversationsByUserIdAsync(int userId)
        {
            var result = _conversations.Where(c => c.UserId == userId).ToList();
            return Task.FromResult(result);
        }

        // Helpers for tests
        public void AddTestConversation(Conversation c)
        {
            _conversations.Add(c);
        }

        public void Clear()
        {
            _conversations.Clear();
        }
        public class ConversationRepositoryMockReturnsNull : IConversationRepository
        {
            public Task<Conversation> CreateConversationAsync(Conversation conversation) => Task.FromResult<Conversation>(null);
            public Task<Conversation> GetConversationByIdAsync(int conversationId) => Task.FromResult<Conversation>(null);
            public Task<List<Conversation>> GetConversationsByUserIdAsync(int userId) => Task.FromResult<List<Conversation>>(null);
        }

    }
}

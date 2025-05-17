using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.ConversationService
{
    public interface IConversationService
    {
        Task<Conversation> CreateConversationAsync(int userId);
        Task<Conversation> GetConversationByIdAsync(int conversationId);
        Task<List<Conversation>> GetUserConversationsAsync(int userId);
    }
}

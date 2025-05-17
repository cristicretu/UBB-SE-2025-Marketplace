using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IConversationRepository
    {
        Task<Conversation> CreateConversationAsync(Conversation conversation);
        Task<Conversation> GetConversationByIdAsync(int conversationId);
        Task<List<Conversation>> GetConversationsByUserIdAsync(int userId);
    }
}

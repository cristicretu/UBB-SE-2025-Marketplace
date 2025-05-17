using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.DreamTeam.ChatService;

public interface IChatService
{
    Task<Conversation> CreateConversationAsync(int userId);
    Task<Conversation> GetConversationAsync(int conversationId);
    Task<List<Conversation>> GetUserConversationsAsync(int userId);
    Task<Message> SendMessageAsync(int conversationId, int userId, string content);
    Task<List<Message>> GetMessagesAsync(int conversationId);
}

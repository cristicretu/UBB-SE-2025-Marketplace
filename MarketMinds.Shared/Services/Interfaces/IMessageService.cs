using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.MessageService
{
    public interface IMessageService
    {
        // New primary methods that handle DTOs
        Task<MessageDto> CreateMessageAsync(CreateMessageDto createMessageDto);
        Task<List<MessageDto>> GetMessagesByConversationIdAsync(int conversationId);

        // Legacy methods - keeping for backward compatibility
        Task<Message> CreateMessageAsync(int conversationId, int userId, string content);
        Task<List<Message>> GetMessagesLegacyAsync(int conversationId);
    }
}

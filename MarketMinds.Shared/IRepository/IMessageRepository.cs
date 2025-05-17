using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IMessageRepository
    {
        Task<Message> CreateMessageAsync(CreateMessageDto newMessage);

        Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId);
    }
}

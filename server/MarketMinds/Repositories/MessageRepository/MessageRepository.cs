using Microsoft.EntityFrameworkCore;
using MarketMinds.Shared.Models;
using Server.DataAccessLayer;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Repositories.MessageRepository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext context;

        public MessageRepository(ApplicationDbContext dbContext)
        {
            this.context = dbContext;
        }

        public async Task<Message> CreateMessageAsync(CreateMessageDto newMessage)
        {
            var message = new Message
            {
                ConversationId = newMessage.ConversationId,
                UserId = newMessage.UserId,
                Content = newMessage.Content,
            };

            await context.Messages.AddAsync(message);
            await context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId)
        {
            return await context.Messages
                .Where(m => m.ConversationId == conversationId)
                .ToListAsync();
        }
    }
}

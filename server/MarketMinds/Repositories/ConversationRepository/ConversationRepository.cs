using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Server.DataAccessLayer;

namespace MarketMinds.Repositories.ConversationRepository
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly ApplicationDbContext context;

        public ConversationRepository(ApplicationDbContext databaseContext)
        {
            this.context = databaseContext;
        }

        public async Task<Conversation> CreateConversationAsync(Conversation conversation)
        {
            await context.Conversations.AddAsync(conversation);
            await context.SaveChangesAsync();
            return conversation;
        }

        public async Task<Conversation> GetConversationByIdAsync(int conversationId)
        {
            return await context.Conversations
                .FirstOrDefaultAsync(conversation => conversation.Id == conversationId);
        }

        public async Task<List<Conversation>> GetConversationsByUserIdAsync(int userId)
        {
            return await context.Conversations
                .Where(conversation => conversation.UserId == userId)
                .ToListAsync();
        }
    }
}

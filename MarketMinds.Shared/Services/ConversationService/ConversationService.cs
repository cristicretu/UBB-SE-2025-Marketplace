using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.ConversationService
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository conversationRepository;

        public ConversationService(IConversationRepository conversationRepository)
        {
            this.conversationRepository = conversationRepository;
        }

        public async Task<Conversation> CreateConversationAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("User ID must be greater than zero");
                }
                
                var conversationModel = new Conversation
                {
                    UserId = userId
                };

                var result = await conversationRepository.CreateConversationAsync(conversationModel);
                
                if (result == null || result.Id <= 0)
                {
                    throw new InvalidOperationException("Failed to create conversation");
                }

                return result;
            }
            catch (Exception exception)
            {
                throw new Exception($"Error creating conversation: {exception.Message}", exception);
            }
        }

        public async Task<Conversation> GetConversationByIdAsync(int conversationId)
        {
            try
            {
                if (conversationId <= 0)
                {
                    throw new ArgumentException("Conversation ID must be greater than zero");
                }
                
                var result = await conversationRepository.GetConversationByIdAsync(conversationId);
                
                if (result == null)
                {
                    throw new KeyNotFoundException($"Conversation with id {conversationId} not found.");
                }
                
                return result;
            }
            catch (Exception exception)
            {
                throw new Exception($"Error retrieving conversation: {exception.Message}", exception);
            }
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("User ID must be greater than zero");
                }
                
                var results = await conversationRepository.GetConversationsByUserIdAsync(userId);
                
                return results ?? new List<Conversation>();
            }
            catch (Exception exception)
            {
                throw new Exception($"Error retrieving user conversations: {exception.Message}", exception);
            }
        }
    }
}

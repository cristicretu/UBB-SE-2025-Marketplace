using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.Interfaces;

namespace MarketMinds.Shared.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        public async Task<Conversation> CreateConversationAsync(int userId)
        {
            var result = await chatRepository.CreateConversationAsync(userId);
            return ConvertToDomainConversation(result);
        }

        public async Task<Conversation> GetConversationAsync(int conversationId)
        {
            var result = await chatRepository.GetConversationAsync(conversationId);
            return ConvertToDomainConversation(result);
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            var results = await chatRepository.GetUserConversationsAsync(userId);
            var domainConversations = new List<Conversation>();

            foreach (var conversation in results)
            {
                domainConversations.Add(ConvertToDomainConversation(conversation));
            }

            return domainConversations;
        }

        public async Task<Message> SendMessageAsync(int conversationId, int userId, string content)
        {
            var result = await chatRepository.SendMessageAsync(conversationId, userId, content);
            return ConvertToDomainMessage(result);
        }

        public async Task<List<Message>> GetMessagesAsync(int conversationId)
        {
            var results = await chatRepository.GetMessagesAsync(conversationId);
            var domainMessages = new List<Message>();

            foreach (var message in results)
            {
                domainMessages.Add(ConvertToDomainMessage(message));
            }

            return domainMessages;
        }

        private Conversation ConvertToDomainConversation(MarketMinds.Shared.Models.Conversation sharedModel)
        {
            if (sharedModel == null)
            {
                return null;
            }
            return new Conversation
            {
                Id = sharedModel.Id,
                UserId = sharedModel.UserId
            };
        }

        private Message ConvertToDomainMessage(MarketMinds.Shared.Models.Message sharedModel)
        {
            if (sharedModel == null)
            {
                return null;
            }

            return new Message
            {
                Id = sharedModel.Id,
                ConversationId = sharedModel.ConversationId,
                UserId = sharedModel.UserId,
                Content = sharedModel.Content
            };
        }
    }
}
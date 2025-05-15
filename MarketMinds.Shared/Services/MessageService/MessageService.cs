using System.Diagnostics;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.MessageService
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository messageRepository;

        public MessageService(IMessageRepository repository)
        {
            messageRepository = repository;
        }

        public async Task<MessageDto> CreateMessageAsync(CreateMessageDto createMessageDto)
        {
            if (createMessageDto == null)
            {
                throw new ArgumentNullException(nameof(createMessageDto), "Message data cannot be null");
            }

            if (createMessageDto.ConversationId <= 0)
            {
                throw new ArgumentException("Conversation ID must be greater than zero.", nameof(createMessageDto.ConversationId));
            }

            if (createMessageDto.UserId <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.", nameof(createMessageDto.UserId));
            }

            if (string.IsNullOrWhiteSpace(createMessageDto.Content))
            {
                throw new ArgumentException("Content cannot be null or empty.", nameof(createMessageDto.Content));
            }

            var sw = Stopwatch.StartNew();

            // Check if this is the first message in the conversation (welcome message)
            bool isWelcomeMessage = false;
            try
            {
                var existingMessages = await messageRepository.GetMessagesByConversationIdAsync(createMessageDto.ConversationId);
                isWelcomeMessage = existingMessages == null || existingMessages.Count == 0;
            }
            catch (Exception)
            {
                // Silently continue if we can't determine if it's a welcome message
            }

            var createdMessage = await messageRepository.CreateMessageAsync(createMessageDto);

            // Convert to DTO for response
            return new MessageDto
            {
                Id = createdMessage.Id,
                ConversationId = createdMessage.ConversationId,
                UserId = createdMessage.UserId,
                Content = createdMessage.Content
            };
        }

        public async Task<List<MessageDto>> GetMessagesByConversationIdAsync(int conversationId)
        {
            if (conversationId <= 0)
            {
                throw new ArgumentException("Conversation ID must be greater than zero.", nameof(conversationId));
            }

            var messages = await messageRepository.GetMessagesByConversationIdAsync(conversationId);

            // Convert to DTOs
            return messages.Select(message => new MessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                UserId = message.UserId,
                Content = message.Content
            }).ToList();
        }

        // Legacy method - keeping for backward compatibility
        public async Task<Message> CreateMessageAsync(int conversationId, int userId, string content)
        {
            var createDto = new CreateMessageDto
            {
                ConversationId = conversationId,
                UserId = userId,
                Content = content
            };

            var messageDto = await CreateMessageAsync(createDto);

            // Convert back to Message for backward compatibility
            return new Message
            {
                Id = messageDto.Id,
                ConversationId = messageDto.ConversationId,
                UserId = messageDto.UserId,
                Content = messageDto.Content
            };
        }

        // Legacy method - keeping for backward compatibility
        public async Task<List<Message>> GetMessagesLegacyAsync(int conversationId)
        {
            return await messageRepository.GetMessagesByConversationIdAsync(conversationId);
        }
    }
}

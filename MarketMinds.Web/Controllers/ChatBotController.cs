using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketMinds.Web.Models;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.ConversationService;
using MarketMinds.Shared.Services.MessageService;
using MarketMinds.Shared.Models;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
    public class ChatBotController : Controller
    {
        private readonly ILogger<ChatBotController> _logger;
        private readonly IConversationService _conversationService;
        private readonly IMessageService _messageService;
        private readonly IChatbotService _chatbotService;
        
        // User ID constants
        private const int DefaultUserId = 1;
        private const int BotUserId = 0; // System bot user ID
        
        // Message constants
        private const string WelcomeMessage = "Hello! I'm your shopping assistant. How can I help you today?";
        private const string ErrorResponseMessage = "I'm sorry, I'm having trouble understanding right now. Please try again later.";
        
        // Index constants
        private const int FirstConversationIndex = 0;

        private const int NoConversation = 0;
        private const int MinimumValidId = 1;

        private const int SingleUnit = 1;

        public ChatBotController(
            ILogger<ChatBotController> logger,
            IConversationService conversationService,
            IMessageService messageService,
            IChatbotService chatbotService)
        {
            _logger = logger;
            _conversationService = conversationService;
            _messageService = messageService;
            _chatbotService = chatbotService;
        }

        public async Task<IActionResult> Index(int? conversationId = null)
        {
            var viewModel = new ChatViewModel();

            try
            {
                // Get current user ID
                int userId = User.Identity.IsAuthenticated ? User.GetCurrentUserId() : DefaultUserId;

                // Set the current user for the chatbot service
                if (User.Identity.IsAuthenticated)
                {
                    _chatbotService.SetCurrentUser(new User { Id = userId });
                }
                else
                {
                    // Set a default user for non-authenticated sessions
                    _chatbotService.SetCurrentUser(new User { Id = DefaultUserId });
                }

                // Get all conversations for the user
                try
                {
                    _logger.LogInformation("Getting conversations for user ID: {UserId}", userId);
                    viewModel.Conversations = await _conversationService.GetUserConversationsAsync(userId);
                    _logger.LogInformation("Retrieved {Count} conversations", viewModel.Conversations.Count);
                    
                    // If no conversations exist, create one automatically
                    if (viewModel.Conversations.Count == NoConversation)
                    {
                        _logger.LogInformation("No conversations found, creating a new conversation");
                        await CreateNewConversationWithWelcomeMessage(userId, viewModel);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting user conversations: {Message}", ex.Message);
                    viewModel.Conversations = new List<Conversation>();
                }

                // Select the requested conversation if specified
                if (conversationId.HasValue && conversationId.Value > MinimumValidId - SingleUnit)
                {
                    try
                    {
                        // Get the specific conversation
                        _logger.LogInformation("Getting conversation ID: {ConversationId}", conversationId.Value);
                        viewModel.CurrentConversation = await _conversationService.GetConversationByIdAsync(conversationId.Value);
                        
                        // Get messages for this conversation
                        viewModel.Messages = await _messageService.GetMessagesLegacyAsync(conversationId.Value);
                        _logger.LogInformation("Retrieved {Count} messages for conversation {ConversationId}", 
                            viewModel.Messages?.Count ?? 0, conversationId.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting conversation {ConversationId}: {Message}", 
                            conversationId.Value, ex.Message);
                    }
                }
                // If no specific conversation requested, select the first one by default
                else if (viewModel.Conversations != null && viewModel.Conversations.Count > NoConversation)
                {
                    try
                    {
                        viewModel.CurrentConversation = viewModel.Conversations[FirstConversationIndex];

                        _logger.LogInformation("Getting messages for conversation {ConversationId}", viewModel.CurrentConversation.Id);
                        viewModel.Messages = await _messageService.GetMessagesLegacyAsync(viewModel.CurrentConversation.Id);
                        _logger.LogInformation("Retrieved {Count} messages for conversation {ConversationId}", 
                            viewModel.Messages?.Count ?? 0, viewModel.CurrentConversation.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting messages for conversation {ConversationId}: {Message}", 
                            viewModel.CurrentConversation.Id, ex.Message);
                        viewModel.Messages = new List<Message>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat data: {Message}", ex.Message);
                // Continue with empty data
            }

            return View(viewModel);
        }

        private async Task CreateNewConversationWithWelcomeMessage(int userId, ChatViewModel viewModel)
        {
            try
            {
                // Create the conversation
                _logger.LogInformation("Creating new conversation for user {UserId}", userId);
                var conversation = await _conversationService.CreateConversationAsync(userId);
                _logger.LogInformation("Created conversation with ID: {ConversationId}", conversation.Id);

                // Add welcome message
                _logger.LogInformation("Adding welcome message to conversation {ConversationId}", conversation.Id);
                var welcomeMessage = await _messageService.CreateMessageAsync(conversation.Id, BotUserId, WelcomeMessage);
                _logger.LogInformation("Added welcome message with ID: {MessageId}", welcomeMessage.Id);

                // Update the view model with the new conversation
                viewModel.Conversations.Add(conversation);
                viewModel.CurrentConversation = conversation;

                viewModel.Messages = new List<Message> { welcomeMessage };
                _logger.LogInformation("View model updated with new conversation and welcome message");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new conversation with welcome message: {Message}", ex.Message);
                throw; // Re-throw to be handled by the calling method
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConversation()
        {
            _logger.LogInformation("CreateConversation action called");

            try
            {
                // Get current user ID
                int userId = User.Identity.IsAuthenticated ? User.GetCurrentUserId() : DefaultUserId;

                // Create a new conversation
                var conversation = await _conversationService.CreateConversationAsync(userId);
                _logger.LogInformation("Created conversation with ID: {ConversationId}", conversation.Id);

                // Add welcome message
                var welcomeMessage = await _messageService.CreateMessageAsync(conversation.Id, BotUserId, WelcomeMessage);
                _logger.LogInformation("Added welcome message with ID: {MessageId}", welcomeMessage.Id);

                return RedirectToAction("Index", new { conversationId = conversation.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation: {Message}", ex.Message);

                // Redirect to index without a specific conversation ID
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            _logger.LogInformation("Sending message in conversation {ConversationId}: {Content}", conversationId, content);

            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return RedirectToAction("Index", new { conversationId });
                }

                // Get current user ID
                int userId = User.Identity.IsAuthenticated ? User.GetCurrentUserId() : DefaultUserId;

                // Send user message
                var userMessage = await _messageService.CreateMessageAsync(conversationId, userId, content);
                _logger.LogInformation("Created user message with ID: {MessageId}", userMessage.Id);

                // Get bot response
                try
                {
                    // Set the current user for the chatbot service if authenticated
                    if (User.Identity.IsAuthenticated)
                    {
                        _chatbotService.SetCurrentUser(new User { Id = userId });
                    }
                    else
                    {
                        // Set a default user for non-authenticated sessions
                        _chatbotService.SetCurrentUser(new User { Id = DefaultUserId });
                    }

                    // Get bot response
                    string botResponse = await _chatbotService.GetBotResponseAsync(content);
                    _logger.LogInformation("Received bot response: {Response}", botResponse);

                    // Send bot response as a message
                    if (!string.IsNullOrEmpty(botResponse))
                    {
                        var botMessage = await _messageService.CreateMessageAsync(conversationId, BotUserId, botResponse);
                        _logger.LogInformation("Created bot message with ID: {MessageId}", botMessage.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting bot response: {Message}", ex.Message);

                    // Send a fallback response if the bot service fails
                    await _messageService.CreateMessageAsync(conversationId, BotUserId, ErrorResponseMessage);
                    _logger.LogInformation("Created error message for failed bot response");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message: {Message}", ex.Message);
            }
            
            return RedirectToAction("Index", new { conversationId });
        }
    }
}

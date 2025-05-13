using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.DreamTeam.ChatbotService
{
    public interface IChatbotService
    {
        Node InitializeChat();
        bool SelectOption(Node selectedNode);
        string GetCurrentResponse();
        IEnumerable<Node> GetCurrentOptions();
        bool IsInteractionActive();
        Task<string> GetBotResponseAsync(string userMessage, bool isWelcomeMessage = false);
        void SetCurrentUser(MarketMinds.Shared.Models.User user);
        Task<string> GetUserContextAsync(int userId);
    }
}
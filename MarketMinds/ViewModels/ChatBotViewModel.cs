using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.Interfaces;

namespace MarketMinds.ViewModels;

public class ChatBotViewModel
{
    private readonly IChatbotService chatBotService;
    private string currentResponse;
    private ObservableCollection<Node> currentOptions;
    private bool isActive;
    private MarketMinds.Shared.Models.User currentUser;
    public ObservableCollection<ChatMessage> Messages { get; private set; }

    public ChatBotViewModel(IChatbotService chatBotService)
    {
        this.chatBotService = chatBotService;
        currentOptions = new ObservableCollection<Node>();
        Messages = new ObservableCollection<ChatMessage>();
    }

    public void SetCurrentUser(MarketMinds.Shared.Models.User user)
    {
        if (user == null)
        {
            System.Diagnostics.Debug.WriteLine("[VIEWMODEL] WARNING: Attempted to set null user in ChatBotViewModel");
            return;
        }
        currentUser = user;
        chatBotService.SetCurrentUser(user);
    }

    public void InitializeChat()
    {
        chatBotService.InitializeChat();
        UpdateState();
        AddBotMessage("Hello! I'm your shopping assistant. How can I help you today?");
    }
    public bool SelectOption(Node selectedNode)
    {
        bool result = chatBotService.SelectOption(selectedNode);
        UpdateState();
        return result;
    }
    public string GetCurrentResponse()
    {
        return currentResponse;
    }
    public IEnumerable<Node> GetCurrentOptions()
    {
        return currentOptions;
    }
    public bool IsChatInteractionActive()
    {
        return isActive;
    }
    private void UpdateState()
    {
        currentResponse = chatBotService.GetCurrentResponse();
        currentOptions.Clear();
        var options = chatBotService.GetCurrentOptions();
        if (options != null)
        {
            foreach (var option in options)
            {
                currentOptions.Add(option);
            }
        }
        isActive = chatBotService.IsInteractionActive();
    }

    public void AddUserMessage(string message)
    {
        Messages.Add(new ChatMessage { Text = message, IsFromUser = true });
    }

    public void AddBotMessage(string message)
    {
        Messages.Add(new ChatMessage { Text = message, IsFromUser = false });
    }

    public async Task<string> SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        AddUserMessage(message);
        string botResponse = await chatBotService.GetBotResponseAsync(message);
        AddBotMessage(botResponse);
        return botResponse;
    }
}

public class ChatMessage
{
    public string Text { get; set; }
    public bool IsFromUser { get; set; }
}

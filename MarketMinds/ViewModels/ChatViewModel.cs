using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;

namespace MarketMinds.ViewModels;

public class ChatViewModel
{
    private readonly IChatService chatService;
    private int currentUserId;
    private int currentConversationId;

    public ChatViewModel(IChatService chatService)
    {
        this.chatService = chatService;
    }

    public async Task InitializeAsync(string userId)
    {
        // Try to parse the string ID to int
        if (int.TryParse(userId, out int userIdInt))
        {
            await InitializeAsync(userIdInt);
        }
        else
        {
            throw new ArgumentException("Invalid user ID format. The user ID must be a valid integer.", nameof(userId));
        }
    }

    public async Task InitializeAsync(int userId)
    {
        currentUserId = userId;
        var conversations = await chatService.GetUserConversationsAsync(userId);

        if (conversations != null && conversations.Count > 0)
        {
            currentConversationId = conversations[0].Id;
        }
        else
        {
            var newConversation = await chatService.CreateConversationAsync(userId);
            currentConversationId = newConversation.Id;
        }
    }

    public async Task<Message> SendMessageAsync(string content)
    {
        return await chatService.SendMessageAsync(currentConversationId, currentUserId, content);
    }

    public async Task<List<Message>> GetMessagesAsync()
    {
        return await chatService.GetMessagesAsync(currentConversationId);
    }
    public async Task<Conversation> GetCurrentConversationAsync()
    {
        return await chatService.GetConversationAsync(currentConversationId);
    }

    public int CurrentConversationId => currentConversationId;
    public int CurrentUserId => currentUserId;
}
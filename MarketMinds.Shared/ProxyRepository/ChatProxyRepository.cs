using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.ProxyRepository
{
    public class ChatProxyRepository : IChatRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;
        public ChatProxyRepository(IConfiguration configuration)
        {
            this.httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
        }

        public async Task<Conversation> CreateConversationAsync(int userId)
        {
            try
            {
                var requestData = new { UserId = userId };
                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/conversation", content);

                if (response.IsSuccessStatusCode)
                {
                    var conversation = await response.Content.ReadFromJsonAsync<Conversation>();
                    return conversation;
                }
                else
                {
                    throw new Exception($"Failed to create conversation: {response.StatusCode}");
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error creating conversation: {exception.Message}", exception);
            }
        }

        public async Task<Conversation> GetConversationAsync(int conversationId)
        {
            try
            {
                var response = await httpClient.GetAsync($"{apiBaseUrl}/conversation/{conversationId}");

                if (response.IsSuccessStatusCode)
                {
                    var conversation = await response.Content.ReadFromJsonAsync<Conversation>();
                    return conversation;
                }
                else
                {
                    throw new Exception($"Failed to get conversation: {response.StatusCode}");
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error getting conversation: {exception.Message}", exception);
            }
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            try
            {
                var response = await httpClient.GetAsync($"{apiBaseUrl}/conversation/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var conversations = await response.Content.ReadFromJsonAsync<List<Conversation>>();
                    return conversations;
                }
                else
                {
                    throw new Exception($"Failed to get conversations: {response.StatusCode}");
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error getting conversations: {exception.Message}", exception);
            }
        }

        public async Task<Message> SendMessageAsync(int conversationId, int userId, string content)
        {
            try
            {
                var requestData = new
                {
                    ConversationId = conversationId,
                    UserId = userId,
                    Content = content
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/message", stringContent);

                if (response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadFromJsonAsync<Message>();
                    return message;
                }
                else
                {
                    throw new Exception($"Failed to send message: {response.StatusCode}");
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error sending message: {exception.Message}", exception);
            }
        }

        public async Task<List<Message>> GetMessagesAsync(int conversationId)
        {
            try
            {
                var response = await httpClient.GetAsync($"{apiBaseUrl}/message/conversation/{conversationId}");

                if (response.IsSuccessStatusCode)
                {
                    var messages = await response.Content.ReadFromJsonAsync<List<Message>>();
                    return messages;
                }
                else
                {
                    throw new Exception($"Failed to get messages: {response.StatusCode}");
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error getting messages: {exception.Message}", exception);
            }
        }
    }
}
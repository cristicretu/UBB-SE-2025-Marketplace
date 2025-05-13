using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;


namespace MarketMinds.Shared.ProxyRepository
{
    public class ConversationProxyRepository : IConversationRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public ConversationProxyRepository(IConfiguration configuration)
        {
            this.httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
        }

        public async Task<Conversation> CreateConversationAsync(Conversation conversation)
        {
            try
            {
                var requestData = new
                {
                    UserId = conversation.UserId
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/Conversation", content);

                if (response.IsSuccessStatusCode)
                {
                    var createdConversation = await response.Content.ReadFromJsonAsync<Conversation>();
                    return createdConversation;
                }
                else
                {
                    throw new Exception($"Failed to create conversation: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Conversation> GetConversationByIdAsync(int conversationId)
        {
            try
            {
                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/Conversation/{conversationId}");

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
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Conversation>> GetConversationsByUserIdAsync(int userId)
        {
            try
            {
                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/Conversation/user/{userId}");

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
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
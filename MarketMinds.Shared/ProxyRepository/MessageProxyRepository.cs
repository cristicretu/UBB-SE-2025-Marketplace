using System;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Newtonsoft.Json;

namespace MarketMinds.Shared.ProxyRepository
{
    public class MessageProxyRepository : IMessageRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public MessageProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
        }

        public async Task<Message> CreateMessageAsync(CreateMessageDto newMessage)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(newMessage);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/Message", stringContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var message = JsonConvert.DeserializeObject<Message>(responseContent);
                    return message;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to create message: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in CreateMessageAsync: {ex.Message}");
                throw;
            }
        }

        // For backward compatibility
        public async Task<Message> CreateMessageAsync(int conversationId, int userId, string content)
        {
            var createDto = new CreateMessageDto
            {
                ConversationId = conversationId,
                UserId = userId,
                Content = content
            };

            return await CreateMessageAsync(createDto);
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId)
        {
            try
            {
                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/Message/conversation/{conversationId}");

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
            catch (Exception)
            {
                throw;
            }
        }
    }
}
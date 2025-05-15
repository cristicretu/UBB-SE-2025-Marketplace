using System.Text;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.ProxyRepository
{    
    public class ChatbotProxyRepository : IChatbotRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;
        public ChatbotProxyRepository(IConfiguration configuration)
        {
            this.httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
        }

        public async Task<string> GetBotResponseAsync(string userMessage, int? userId = null)
        {
            try
            {
                var requestData = new
                {
                    Message = userMessage,
                    UserId = userId
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var fullUrl = $"{apiBaseUrl}/api/Chatbot";
        
                var response = await httpClient.PostAsync(fullUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<ChatbotResponse>(resultJson);
                    return responseObject.Message;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to get bot response: {response.StatusCode}");
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to get bot response: {exception.Message}");
            }
        }

        public async Task<string> GetUserContextAsync(int userId)
        {
            try
            {
                var fullUrl = $"{apiBaseUrl}/api/Chatbot/UserContext/{userId}";
                
                var response = await httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var contextJson = await response.Content.ReadAsStringAsync();
                    var contextObject = JsonConvert.DeserializeObject<UserContextResponse>(contextJson);
                    return contextObject.Context;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to get user context: {response.StatusCode}");
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to get user context: {exception.Message}");
            }
        }

        public async Task<User> GetUserAsync(int userId)
        {
            try
            {
                var fullUrl = $"{apiBaseUrl}/api/Chatbot/User/{userId}";
                var response = await httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<User>(json);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Basket> GetUserBasketAsync(int userId)
        {
            try
            {
                var fullUrl = $"{apiBaseUrl}/api/Chatbot/UserBasket/{userId}";
                var response = await httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Basket>(json);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<BasketItem>> GetBasketItemsAsync(int basketId)
        {
            try
            {
                var fullUrl = $"{apiBaseUrl}/api/Chatbot/BasketItems/{basketId}";
                var response = await httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<BasketItem>>(json);
                }
                else
                {
                    return new List<BasketItem>();
                }
            }
            catch (Exception)
            {
                return new List<BasketItem>();
            }
        }

        public async Task<BuyProduct> GetBuyProductAsync(int productId)
        {
            try
            {
                var fullUrl = $"{apiBaseUrl}/api/Chatbot/Product/{productId}";
                var response = await httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<BuyProduct>(json);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Review>> GetReviewsGivenByUserAsync(int userId)
        {
            try
            {
                var fullUrl = $"{apiBaseUrl}/api/Chatbot/ReviewsGiven/{userId}";
                var response = await httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Review>>(json);
                }
                else
                {
                    return new List<Review>();
                }
            }
            catch (Exception)
            {
                return new List<Review>();
            }
        }

        public async Task<List<Review>> GetReviewsReceivedByUserAsync(int userId)
        {
            try
            {
                var fullUrl = $"{apiBaseUrl}/api/Chatbot/ReviewsReceived/{userId}";
                var response = await httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Review>>(json);
                }
                else
                {
                    return new List<Review>();
                }
            }
            catch (Exception)
            {
                return new List<Review>();
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await GetUserAsync(userId);
        }

        public async Task<List<Order>> GetBuyerOrdersAsync(int userId)
        {
            try
            {
                var fullUrl = $"{apiBaseUrl}/api/Chatbot/BuyerOrders/{userId}";
                var response = await httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Order>>(json);
                }
                else
                {
                    return new List<Order>();
                }
            }
            catch (Exception)
            {
                return new List<Order>();
            }
        }

        public async Task<List<Order>> GetSellerOrdersAsync(int userId)
        {
            try
            {
                var fullUrl = $"{apiBaseUrl}/api/Chatbot/SellerOrders/{userId}";
                var response = await httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Order>>(json);
                }
                else
                {
                    return new List<Order>();
                }
            }
            catch (Exception)
            {
                return new List<Order>();
            }
        }
    }

    public class ChatbotResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class UserContextResponse
    {
        public string Context { get; set; }
        public bool Success { get; set; }
    }
}
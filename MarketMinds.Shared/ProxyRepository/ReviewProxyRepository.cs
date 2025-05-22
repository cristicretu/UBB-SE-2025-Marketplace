using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text.Json;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.ProxyRepository
{
    public class ReviewProxyRepository : IReviewRepository
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public ReviewProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("API base URL is null or empty");
            }

            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }

            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
            Console.WriteLine($"Review Repository initialized with base address: {httpClient.BaseAddress}");

            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Raw data access methods
        public string GetReviewsBySellerRaw(int sellerId)
        {
            var response = httpClient.GetAsync($"review/seller/{sellerId}").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public string GetReviewsByBuyerRaw(int buyerId)
        {
            var response = httpClient.GetAsync($"review/buyer/{buyerId}").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public void CreateReviewRaw(Review review)
        {
            var response = httpClient.PostAsJsonAsync("review", review, jsonOptions).Result;
            response.EnsureSuccessStatusCode();
        }

        public void EditReviewRaw(object updateRequest)
        {
            var response = httpClient.PutAsJsonAsync("review", updateRequest, jsonOptions).Result;
            response.EnsureSuccessStatusCode();
        }

        public void DeleteReviewRaw(object deleteRequest)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{httpClient.BaseAddress}review?reviewId={((dynamic)deleteRequest).ReviewId}&sellerId={((dynamic)deleteRequest).SellerId}&buyerId={((dynamic)deleteRequest).BuyerId}")
            };

            var response = httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
        }

        // Legacy implementations that delegate to service layer
        public ObservableCollection<Review> GetAllReviewsBySeller(User seller)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public ObservableCollection<Review> GetAllReviewsByBuyer(User buyer)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public void CreateReview(Review review)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public void EditReview(Review review, double rating, string description)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public void DeleteReview(Review review)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }
    }
}

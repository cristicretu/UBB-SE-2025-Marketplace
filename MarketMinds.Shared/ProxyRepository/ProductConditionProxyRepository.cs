using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Shared.ProxyRepository
{
    public class ProductConditionProxyRepository : IProductConditionRepository
    {
        private readonly HttpClient httpClient;

        public ProductConditionProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public string GetAllProductConditionsRaw()
        {
            var response = httpClient.GetAsync("ProductCondition").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public string CreateProductConditionRaw(string displayTitle, string description)
        {
            var requestContent = new StringContent(
                $"{{\"displayTitle\":\"{displayTitle}\",\"description\":\"{description}\"}}",
                System.Text.Encoding.UTF8,
                "application/json");
            var response = httpClient.PostAsync("ProductCondition", requestContent).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public void DeleteProductConditionRaw(string displayTitle)
        {
            var response = httpClient.DeleteAsync($"ProductCondition/{displayTitle}").Result;
            response.EnsureSuccessStatusCode();
        }

        // Legacy implementations that delegate to service layer
        public List<Condition> GetAllProductConditions()
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public Condition CreateProductCondition(string displayTitle, string description)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public void DeleteProductCondition(string displayTitle)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }
    }

    public class ProductConditionRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
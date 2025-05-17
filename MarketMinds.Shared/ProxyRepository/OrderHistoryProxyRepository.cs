// <copyright file="OrderHistoryProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.Shared.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.IRepository;
    

    /// <summary>
    /// Proxy repository class for managing order history operations via REST API.
    /// </summary>
    public class OrderHistoryProxyRepository : IOrderHistoryRepository
    {
        private const string ApiBaseRoute = "api/orderhistory";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public OrderHistoryProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
            
        }

        /// <inheritdoc />
        public async Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{orderHistoryId}/dummy-products");
            await this.ThrowOnError(nameof(GetProductsFromOrderHistoryAsync), response);

            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return products ?? new List<Product>();
        }

        /// <inheritdoc />
        public async Task<int> CreateOrderHistoryAsync()
        {
            throw new NotImplementedException("The CreateOrderHistoryAsync method is not implemented.");
        }

        private async Task ThrowOnError(string methodName, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = response.ReasonPhrase;
                }
                throw new Exception($"{methodName}: {errorMessage}");
            }
        }
    }
}
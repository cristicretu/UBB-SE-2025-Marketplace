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
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{orderHistoryId}/products");
            await this.ThrowOnError(nameof(GetProductsFromOrderHistoryAsync), response);

            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return products ?? new List<Product>();
        }

        /// <inheritdoc />
        public async Task<int> CreateOrderHistoryAsync(int buyerId)
        {
            try
            {
                var content = JsonContent.Create(new { BuyerId = buyerId });

                var response = await this.httpClient.PostAsync(ApiBaseRoute, content);

                var responseContent = await response.Content.ReadAsStringAsync();

                await this.ThrowOnError(nameof(CreateOrderHistoryAsync), response);
                var orderHistoryId = await response.Content.ReadFromJsonAsync<int>();
                return orderHistoryId;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<List<OrderHistory>> GetOrderHistoriesByBuyerAsync(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/buyer/{buyerId}");
            await this.ThrowOnError(nameof(GetOrderHistoriesByBuyerAsync), response);

            var orderHistories = await response.Content.ReadFromJsonAsync<List<OrderHistory>>();
            return orderHistories ?? new List<OrderHistory>();
        }

        /// <inheritdoc />
        public async Task<OrderHistory> GetOrderHistoryByIdAsync(int orderHistoryId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{orderHistoryId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            await this.ThrowOnError(nameof(GetOrderHistoryByIdAsync), response);
            var orderHistory = await response.Content.ReadFromJsonAsync<OrderHistory>();
            return orderHistory;
        }

        /// <inheritdoc />
        public async Task UpdateOrderHistoryAsync(int orderHistoryId, string note, string shippingAddress, string paymentMethod)
        {
            var updateRequest = new
            {
                Note = note,
                ShippingAddress = shippingAddress,
                PaymentMethod = paymentMethod
            };

            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/{orderHistoryId}", updateRequest);
            await this.ThrowOnError(nameof(UpdateOrderHistoryAsync), response);
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
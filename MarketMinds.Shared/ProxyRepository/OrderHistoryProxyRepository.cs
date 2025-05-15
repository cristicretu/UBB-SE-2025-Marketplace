// <copyright file="OrderHistoryProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Proxy repository class for managing order history operations via REST API.
    /// </summary>
    public class OrderHistoryProxyRepository : IOrderHistoryRepository
    {
        private const string ApiBaseRoute = "api/orderhistory";
        private readonly CustomHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public OrderHistoryProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
        }

        /// <inheritdoc />
        public async Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{orderHistoryId}/dummy-products");
            await this.ThrowOnError(nameof(GetProductsFromOrderHistoryAsync), response);

            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return products ?? new List<Product>();
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
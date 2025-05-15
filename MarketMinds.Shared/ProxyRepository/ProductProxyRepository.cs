// <copyright file="ProductProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.ProxyRepository
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.DataTransferObjects;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Proxy repository class for managing dummy product operations via REST API.
    /// </summary>
    public class ProductProxyRepository : IProductRepository
    {
        private const string ApiBaseRoute = "api/products";
        private readonly CustomHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public ProductProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
        }

        /// <inheritdoc />
        public async Task AddProductAsync(string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            var requestDto = new ProductRequest
            {
                Name = name,
                Price = price,
                SellerID = sellerId,
                ProductType = productType,
                StartDate = startDate,
                EndDate = endDate,
            };

            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}", requestDto);
            await this.ThrowOnError(nameof(AddProductAsync), response);
        }

        /// <inheritdoc />
        public async Task DeleteProduct(int id)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/{id}");
            await this.ThrowOnError(nameof(DeleteProduct), response);
        }

        /// <inheritdoc />
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{productId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            await this.ThrowOnError(nameof(GetProductByIdAsync), response);
            var product = await response.Content.ReadFromJsonAsync<Product>();
            return product;
        }

        /// <inheritdoc />
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            if (!sellerId.HasValue)
            {
                return null;
            }

            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/seller/{sellerId.Value}/name");
            await this.ThrowOnError(nameof(GetSellerNameAsync), response);
            var sellerName = await response.Content.ReadAsStringAsync();
            return sellerName;
        }

        /// <inheritdoc />
        public async Task<List<Product>> GetBorrowableProductsAsync()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/borrowable");
            await this.ThrowOnError(nameof(GetBorrowableProductsAsync), response);
            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return products ?? new List<Product>();
        }

        /// <inheritdoc />
        public async Task UpdateProductAsync(int id, string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            var requestDto = new ProductRequest
            {
                Name = name,
                Price = price,
                SellerID = sellerId,
                ProductType = productType,
                StartDate = startDate,
                EndDate = endDate,
            };

            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/{id}", requestDto);
            await this.ThrowOnError(nameof(UpdateProductAsync), response);
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
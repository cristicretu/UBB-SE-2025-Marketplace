// <copyright file="ProductProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.Shared.ProxyRepository
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models.DTOs;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.IRepository;


    /// <summary>
    /// Proxy repository class for managing dummy product operations via REST API.
    /// </summary>
    public class ProductProxyRepository : IProductRepository
    {
        private const string ApiBaseRoute = "api/products";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public ProductProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);

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
        public async Task<List<BorrowProduct>> GetBorrowableProductsAsync()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/borrowable");
            await this.ThrowOnError(nameof(GetBorrowableProductsAsync), response);
            var products = await response.Content.ReadFromJsonAsync<List<BorrowProduct>>();
            return products ?? new List<BorrowProduct>();
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

        /// <inheritdoc />
        public List<Product> GetProducts()
        {
            // Synchronous implementation of GetProducts
            // This could be implemented to call the async version and wait for it
            try
            {
                var response = httpClient.GetAsync($"{ApiBaseRoute}").Result;
                ThrowOnError(nameof(GetProducts), response).Wait();
                var products = response.Content.ReadFromJsonAsync<List<Product>>().Result;
                return products ?? new List<Product>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProducts: {ex.Message}");
                return new List<Product>();
            }
        }

        /// <inheritdoc />
        public Product GetProductByID(int id)
        {
            // Synchronous implementation to get a product by ID
            try
            {
                var response = httpClient.GetAsync($"{ApiBaseRoute}/{id}").Result;
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                ThrowOnError(nameof(GetProductByID), response).Wait();
                var product = response.Content.ReadFromJsonAsync<Product>().Result;
                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProductByID: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc />
        public void AddProduct(Product product)
        {
            // Synchronous implementation to add a product
            try
            {
                var response = httpClient.PostAsJsonAsync($"{ApiBaseRoute}", product).Result;
                ThrowOnError(nameof(AddProduct), response).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddProduct: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc />
        public void UpdateProduct(Product product)
        {
            // Synchronous implementation to update a product
            try
            {
                var response = httpClient.PutAsJsonAsync($"{ApiBaseRoute}/{product.Id}", product).Result;
                ThrowOnError(nameof(UpdateProduct), response).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateProduct: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc />
        public void DeleteProduct(Product product)
        {
            // Synchronous implementation to delete a product
            if (product == null || product.Id <= 0)
            {
                throw new ArgumentException("Invalid product for deletion", nameof(product));
            }

            try
            {
                var response = httpClient.DeleteAsync($"{ApiBaseRoute}/{product.Id}").Result;
                ThrowOnError(nameof(DeleteProduct), response).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteProduct: {ex.Message}");
                throw;
            }
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
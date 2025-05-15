// -----------------------------------------------------------------------
// <copyright file="SellerProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Web;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// A repository implementation that acts as a proxy for seller-related operations
    /// via a remote API.
    /// </summary>
    public class SellerProxyRepository : ISellerRepository
    {
        private const string ApiBaseRoute = "api/sellers";
        private readonly CustomHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base URL of the API (e.g., "http://localhost:5000/").</param>
        public SellerProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
        }

        /// <inheritdoc />
        public async Task AddNewFollowerNotification(int sellerId, int currentFollowerCount, string message)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["currentFollowerCount"] = currentFollowerCount.ToString();
            query["message"] = message; // Encoding handled by ParseQueryString/ToString
            string queryString = query.ToString() ?? string.Empty;

            var response = await this.httpClient.PostAsync($"{ApiBaseRoute}/{sellerId}/notifications/add?{queryString}", null); // No body
            await this.ThrowOnError(nameof(AddNewFollowerNotification), response);
        }

        /// <inheritdoc />
        public async Task AddSeller(Seller seller)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/add", seller);
            await this.ThrowOnError(nameof(AddSeller), response);
        }

        /// <inheritdoc />
        public async Task<int> GetLastFollowerCount(int sellerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{sellerId}/last-follower-count");
            await this.ThrowOnError(nameof(GetLastFollowerCount), response);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        /// <inheritdoc />
        public async Task<List<string>> GetNotifications(int sellerId, int maxNotifications)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["maxNotifications"] = maxNotifications.ToString();
            string queryString = query.ToString() ?? string.Empty;

            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{sellerId}/notifications?{queryString}");
            await this.ThrowOnError(nameof(GetNotifications), response);
            var notifications = await response.Content.ReadFromJsonAsync<List<string>>();
            return notifications ?? new List<string>();
        }

        /// <inheritdoc />
        public async Task<List<Product>> GetProducts(int sellerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{sellerId}/products");
            await this.ThrowOnError(nameof(GetProducts), response);
            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return products ?? new List<Product>();
        }

        /// <inheritdoc />
        public async Task<List<Review>> GetReviews(int sellerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{sellerId}/reviews");
            await this.ThrowOnError(nameof(GetReviews), response);
            var reviews = await response.Content.ReadFromJsonAsync<List<Review>>();
            return reviews ?? new List<Review>();
        }

        /// <inheritdoc />
        public async Task<Seller> GetSellerInfo(User user)
        {
            int userId = user.UserId;
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{userId}/info");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Seller(user);
            }

            await this.ThrowOnError(nameof(GetSellerInfo), response);
            var seller = await response.Content.ReadFromJsonAsync<Seller>();

            if (seller == null)
            {
                throw new InvalidOperationException($"API returned null Seller info for User ID: {userId}");
            }

            return seller;
        }

        /// <inheritdoc />
        public async Task UpdateSeller(Seller seller)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/update", seller);
            await this.ThrowOnError(nameof(UpdateSeller), response);
        }

        /// <inheritdoc />
        public async Task UpdateTrustScore(int sellerId, double trustScore)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["score"] = trustScore.ToString();
            string queryString = query.ToString() ?? string.Empty;

            var response = await this.httpClient.PutAsync($"{ApiBaseRoute}/{sellerId}/trust-score?{queryString}", null); // No body
            await this.ThrowOnError(nameof(UpdateTrustScore), response);
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

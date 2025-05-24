// -----------------------------------------------------------------------
// <copyright file="SellerProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MarketMinds.Shared.ProxyRepository
{
    using System.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Web;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.IRepository;
    using Microsoft.Extensions.Configuration;


    /// <summary>
    /// A repository implementation that acts as a proxy for seller-related operations
    /// via a remote API.
    /// </summary>
    public class SellerProxyRepository : ISellerRepository
    {
        private const string ApiBaseRoute = "api/sellers";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base URL of the API (e.g., "http://localhost:5000/").</param>
        public SellerProxyRepository(IConfiguration configuration)
        {
            Debug.WriteLine($"SellerProxyRepository constructor called with baseApiUrl: {configuration["ApiSettings:BaseUrl"]}");

            this.httpClient = new HttpClient();
            var baseApiUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (string.IsNullOrEmpty(baseApiUrl))
            {
                throw new InvalidOperationException("API base URL is null or empty in SellerProxyRepository constructor");
            }

            if (!baseApiUrl.EndsWith("/"))
            {
                baseApiUrl += "/";
            }

            this.httpClient.BaseAddress = new Uri(baseApiUrl);
            Debug.WriteLine($"HttpClient.BaseAddress set to: {this.httpClient.BaseAddress}");
        }

        /// <inheritdoc />
        public async Task AddNewFollowerNotification(int sellerId, int currentFollowerCount, string message)
        {
            Debug.WriteLine($"AddNewFollowerNotification called for seller ID: {sellerId}, follower count: {currentFollowerCount}");

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["currentFollowerCount"] = currentFollowerCount.ToString();
            query["message"] = message;
            string queryString = query.ToString() ?? string.Empty;

            var requestUrl = $"{ApiBaseRoute}/{sellerId}/notifications/add?{queryString}";
            Debug.WriteLine($"Request URL: {requestUrl}");

            var response = await this.httpClient.PostAsync(requestUrl, null);
            Debug.WriteLine($"Response status: {response.StatusCode}");

            await this.ThrowOnError(nameof(AddNewFollowerNotification), response);
        }

        /// <inheritdoc />
        public async Task AddSeller(Seller seller)
        {
            Debug.WriteLine($"AddSeller called for seller ID: {seller?.Id ?? -1}");

            var requestUrl = $"{ApiBaseRoute}/add";
            Debug.WriteLine($"Request URL: {requestUrl}");

            var response = await this.httpClient.PostAsJsonAsync(requestUrl, seller);
            Debug.WriteLine($"Response status: {response.StatusCode}");

            await this.ThrowOnError(nameof(AddSeller), response);
        }

        /// <inheritdoc />
        public async Task<int> GetLastFollowerCount(int sellerId)
        {
            Debug.WriteLine($"GetLastFollowerCount called for seller ID: {sellerId}");

            var requestUrl = $"{ApiBaseRoute}/{sellerId}/last-follower-count";
            Debug.WriteLine($"Request URL: {requestUrl}");

            var response = await this.httpClient.GetAsync(requestUrl);
            Debug.WriteLine($"Response status: {response.StatusCode}");

            await this.ThrowOnError(nameof(GetLastFollowerCount), response);

            var count = await response.Content.ReadFromJsonAsync<int>();
            Debug.WriteLine($"Follower count: {count}");
            return count;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetNotifications(int sellerId, int maxNotifications)
        {
            Debug.WriteLine($"GetNotifications called for seller ID: {sellerId}, max: {maxNotifications}");

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["maxNotifications"] = maxNotifications.ToString();
            string queryString = query.ToString() ?? string.Empty;

            var requestUrl = $"{ApiBaseRoute}/{sellerId}/notifications?{queryString}";
            Debug.WriteLine($"Request URL: {requestUrl}");

            var response = await this.httpClient.GetAsync(requestUrl);
            Debug.WriteLine($"Response status: {response.StatusCode}");

            await this.ThrowOnError(nameof(GetNotifications), response);

            var notifications = await response.Content.ReadFromJsonAsync<List<string>>();
            Debug.WriteLine($"Retrieved {notifications?.Count ?? 0} notifications");
            return notifications ?? new List<string>();
        }

        /// <inheritdoc />
        public async Task<List<BuyProduct>> GetProducts(int sellerId)
        {
            Debug.WriteLine($"GetProducts called for seller ID: {sellerId}");

            var requestUrl = $"{ApiBaseRoute}/{sellerId}/products";
            Debug.WriteLine($"Request URL: {requestUrl}");

            var response = await this.httpClient.GetAsync(requestUrl);
            Debug.WriteLine($"Response status: {response.StatusCode}");

            await this.ThrowOnError(nameof(GetProducts), response);

            var products = await response.Content.ReadFromJsonAsync<List<BuyProduct>>();
            Debug.WriteLine($"Retrieved {products?.Count ?? 0} products");
            return products ?? new List<BuyProduct>();
        }

        /// <inheritdoc />
        public async Task<List<Review>> GetReviews(int sellerId)
        {
            Debug.WriteLine($"GetReviews called for seller ID: {sellerId}");

            var requestUrl = $"{ApiBaseRoute}/{sellerId}/reviews";
            Debug.WriteLine($"Request URL: {requestUrl}");

            var response = await this.httpClient.GetAsync(requestUrl);
            Debug.WriteLine($"Response status: {response.StatusCode}");

            await this.ThrowOnError(nameof(GetReviews), response);

            var reviews = await response.Content.ReadFromJsonAsync<List<Review>>();
            Debug.WriteLine($"Retrieved {reviews?.Count ?? 0} reviews");
            return reviews ?? new List<Review>();
        }

        /// <inheritdoc />
        public async Task<Seller> GetSellerInfo(User user)
        {
            Debug.WriteLine($"GetSellerInfo called for User ID: {user?.Id ?? -1}");

            if (user == null)
            {
                Debug.WriteLine("ERROR: User is null");
                throw new ArgumentNullException(nameof(user));
            }

            int userId = user.Id;
            var requestUrl = $"{ApiBaseRoute}/{userId}/info";
            Debug.WriteLine($"Request URL: {requestUrl}");

            var response = await this.httpClient.GetAsync(requestUrl);
            Debug.WriteLine($"Response status: {response.StatusCode}");

            // Special case for 404 - seller not found
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Debug.WriteLine("Seller not found (404), returning new Seller with User");
                return new Seller(user);
            }

            // For other errors, throw exception
            await this.ThrowOnError(nameof(GetSellerInfo), response);

            // Get response content for debugging
            var contentString = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Response content: {contentString}");

            try
            {
                var seller = await response.Content.ReadFromJsonAsync<Seller>();

                if (seller == null)
                {
                    Debug.WriteLine("Deserialized seller is null, creating new one with User");
                    return new Seller(user);
                }

                // Make sure User is set
                if (seller.User == null)
                {
                    Debug.WriteLine("Setting seller.User with the provided User");
                    seller.User = user;
                }

                Debug.WriteLine($"Successfully deserialized seller: ID={seller.Id}, StoreName='{seller.StoreName}', User.Email='{seller.User?.Email}'");
                return seller;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deserializing seller: {ex.Message}");
                // Return a new seller with the user if deserialization fails
                return new Seller(user);
            }
        }

        /// <inheritdoc />
        public async Task UpdateSeller(Seller seller)
        {
            Debug.WriteLine($"UpdateSeller called for seller ID: {seller?.Id ?? -1}");

            if (seller == null)
            {
                Debug.WriteLine("ERROR: Seller is null");
                throw new ArgumentNullException(nameof(seller));
            }

            var requestUrl = $"{ApiBaseRoute}/update";
            Debug.WriteLine($"Request URL: {requestUrl}");

            var response = await this.httpClient.PutAsJsonAsync(requestUrl, seller);
            Debug.WriteLine($"Response status: {response.StatusCode}");

            await this.ThrowOnError(nameof(UpdateSeller), response);
        }

        /// <inheritdoc />
        public async Task UpdateTrustScore(int sellerId, double trustScore)
        {
            Debug.WriteLine($"UpdateTrustScore called for seller ID: {sellerId}, score: {trustScore}");

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["score"] = trustScore.ToString();
            string queryString = query.ToString() ?? string.Empty;

            var requestUrl = $"{ApiBaseRoute}/{sellerId}/trust-score?{queryString}";
            Debug.WriteLine($"Request URL: {requestUrl}");

            var response = await this.httpClient.PutAsync(requestUrl, null);
            Debug.WriteLine($"Response status: {response.StatusCode}");

            await this.ThrowOnError(nameof(UpdateTrustScore), response);
        }

        private async Task ThrowOnError(string methodName, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API ERROR: {methodName} - Status: {response.StatusCode}, Message: {errorMessage}");

                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = response.ReasonPhrase;
                }

                throw new Exception($"{methodName}: {errorMessage}");
            }
        }
    }
}

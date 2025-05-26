using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;
using MarketMinds.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

namespace MarketMinds.Shared.ProxyRepository
{
    /// <summary>
    /// Proxy repository implementation for managing buyer linkages via HTTP API
    /// </summary>
    public class BuyerLinkageProxyRepository : IBuyerLinkageRepository
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the BuyerLinkageProxyRepository class
        /// </summary>
        /// <param name="httpClient">HTTP client for API calls</param>
        public BuyerLinkageProxyRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public BuyerLinkageProxyRepository(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("API base URL is null or empty");
            }

            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }

            _httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Creates a new pending linkage request between two buyers
        /// </summary>
        /// <param name="requestingBuyerId">Requesting buyer ID</param>
        /// <param name="receivingBuyerId">Receiving buyer ID</param>
        /// <returns>The created buyer linkage request</returns>
        public async Task<BuyerLinkage> CreateLinkageRequestAsync(int requestingBuyerId, int receivingBuyerId)
        {
            var response = await _httpClient.PostAsync(
                $"buyerlinkage/sendrequest?currentBuyerId={requestingBuyerId}&targetBuyerId={receivingBuyerId}", 
                null);

            if (response.IsSuccessStatusCode)
            {
                return new BuyerLinkage(requestingBuyerId, receivingBuyerId);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create linkage request: {errorContent}");
        }

        /// <summary>
        /// Approves a pending linkage request
        /// </summary>
        /// <param name="requestingBuyerId">Requesting buyer ID</param>
        /// <param name="receivingBuyerId">Receiving buyer ID</param>
        /// <returns>True if request was approved, false if not found</returns>
        public async Task<bool> ApproveLinkageRequestAsync(int requestingBuyerId, int receivingBuyerId)
        {
            var response = await _httpClient.PostAsync(
                $"buyerlinkage/acceptrequest?currentBuyerId={receivingBuyerId}&requestingBuyerId={requestingBuyerId}", 
                null);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<bool>(content, _jsonOptions);
            }

            return false;
        }

        /// <summary>
        /// Gets the linkage status between two buyers
        /// </summary>
        /// <param name="currentBuyerId">Current buyer ID</param>
        /// <param name="targetBuyerId">Target buyer ID</param>
        /// <returns>The linkage status</returns>
        public async Task<BuyerLinkageStatus> GetLinkageStatusAsync(int currentBuyerId, int targetBuyerId)
        {
            var response = await _httpClient.GetAsync(
                $"buyerlinkage/status?currentBuyerId={currentBuyerId}&targetBuyerId={targetBuyerId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // The API now returns the enum directly as JSON, not as a string
                return JsonSerializer.Deserialize<BuyerLinkageStatus>(content, _jsonOptions);
            }

            return BuyerLinkageStatus.None;
        }

        /// <summary>
        /// Creates a new linkage between two buyers (DEPRECATED - use CreateLinkageRequestAsync)
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>The created buyer linkage</returns>
        public async Task<BuyerLinkage> CreateLinkageAsync(int buyerId1, int buyerId2)
        {
            // For backward compatibility, redirect to the new request-based system
            return await CreateLinkageRequestAsync(buyerId1, buyerId2);
        }

        /// <summary>
        /// Removes a linkage between two buyers
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>True if linkage was removed, false if it didn't exist</returns>
        public async Task<bool> RemoveLinkageAsync(int buyerId1, int buyerId2)
        {
            var response = await _httpClient.PostAsync(
                $"buyerlinkage/removelink?currentBuyerId={buyerId1}&targetBuyerId={buyerId2}", 
                null);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<bool>(content, _jsonOptions);
            }

            return false;
        }

        /// <summary>
        /// Checks if two buyers are linked
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>True if buyers are linked, false otherwise</returns>
        public async Task<bool> AreBuyersLinkedAsync(int buyerId1, int buyerId2)
        {
            var status = await GetLinkageStatusAsync(buyerId1, buyerId2);
            return status == BuyerLinkageStatus.Linked;
        }

        /// <summary>
        /// Gets a specific linkage between two buyers
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>The buyer linkage if it exists, null otherwise</returns>
        public async Task<BuyerLinkage?> GetLinkageAsync(int buyerId1, int buyerId2)
        {
            var isLinked = await AreBuyersLinkedAsync(buyerId1, buyerId2);
            return isLinked ? new BuyerLinkage(buyerId1, buyerId2) : null;
        }

        /// <summary>
        /// Gets all linkages for a specific buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of buyer linkages involving the specified buyer</returns>
        public async Task<IEnumerable<BuyerLinkage>> GetBuyerLinkagesAsync(int buyerId)
        {
            var linkedBuyerIds = await GetLinkedBuyerIdsAsync(buyerId);
            return linkedBuyerIds.Select(id => new BuyerLinkage(buyerId, id));
        }

        /// <summary>
        /// Gets all linked buyer IDs for a specific buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of buyer IDs that are linked to the specified buyer</returns>
        public async Task<IEnumerable<int>> GetLinkedBuyerIdsAsync(int buyerId)
        {
            var response = await _httpClient.GetAsync($"buyerlinkage/{buyerId}/linked");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var linkedBuyers = JsonSerializer.Deserialize<List<Buyer>>(content, _jsonOptions);
                return linkedBuyers?.Select(b => b.Id) ?? new List<int>();
            }

            return new List<int>();
        }
    }
} 
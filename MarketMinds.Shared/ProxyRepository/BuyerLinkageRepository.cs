using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;
using MarketMinds.Shared.Services;
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

        /// <summary>
        /// Creates a new linkage between two buyers
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>The created buyer linkage</returns>
        public async Task<BuyerLinkage> CreateLinkageAsync(int buyerId1, int buyerId2)
        {
            var response = await _httpClient.PostAsync(
                $"buyerlinkage/link?currentBuyerId={buyerId1}&targetBuyerId={buyerId2}", 
                null);

            if (response.IsSuccessStatusCode)
            {
                return new BuyerLinkage(buyerId1, buyerId2);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create linkage: {errorContent}");
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
                $"buyerlinkage/unlink?currentBuyerId={buyerId1}&targetBuyerId={buyerId2}", 
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
            var response = await _httpClient.GetAsync(
                $"buyerlinkage/check?buyerId1={buyerId1}&buyerId2={buyerId2}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<bool>(content, _jsonOptions);
            }

            return false;
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
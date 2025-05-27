using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;
using MarketMinds.Shared.Services;
using System.Text.Json;

namespace MarketMinds.Shared.ProxyRepository
{
    /// <summary>
    /// Proxy repository implementation for managing buyer-seller follows via HTTP API
    /// </summary>
    public class BuyerSellerFollowProxyRepository : IBuyerSellerFollowRepository
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the BuyerSellerFollowProxyRepository class
        /// </summary>
        /// <param name="httpClient">HTTP client for API calls</param>
        public BuyerSellerFollowProxyRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Creates a new follow relationship where a buyer follows a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>The created buyer-seller follow</returns>
        public async Task<BuyerSellerFollow> CreateFollowAsync(int buyerId, int sellerId)
        {
            var response = await _httpClient.PostAsync(
                $"buyers/{buyerId}/follow/{sellerId}",
                null);

            if (response.IsSuccessStatusCode)
            {
                return new BuyerSellerFollow(buyerId, sellerId);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create follow relationship: {errorContent}");
        }

        /// <summary>
        /// Removes a follow relationship between a buyer and seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if follow was removed, false if it didn't exist</returns>
        public async Task<bool> RemoveFollowAsync(int buyerId, int sellerId)
        {
            var response = await _httpClient.PostAsync(
                $"buyers/{buyerId}/unfollow/{sellerId}", null);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Checks if a buyer is following a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if buyer is following seller, false otherwise</returns>
        public async Task<bool> IsFollowingAsync(int buyerId, int sellerId)
        {
            var response = await _httpClient.GetAsync(
                $"buyers/{buyerId}/following/{sellerId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<bool>(content, _jsonOptions);
            }

            return false;
        }

        /// <summary>
        /// Gets a specific follow relationship between a buyer and seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>The buyer-seller follow if it exists, null otherwise</returns>
        public async Task<BuyerSellerFollow?> GetFollowAsync(int buyerId, int sellerId)
        {
            var isFollowing = await IsFollowingAsync(buyerId, sellerId);
            return isFollowing ? new BuyerSellerFollow(buyerId, sellerId) : null;
        }

        /// <summary>
        /// Gets all follows for a specific buyer (all sellers they follow)
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of buyer-seller follows for the specified buyer</returns>
        public async Task<IEnumerable<BuyerSellerFollow>> GetBuyerFollowsAsync(int buyerId)
        {
            var followedSellerIds = await GetFollowedSellerIdsAsync(buyerId);
            return followedSellerIds.Select(sellerId => new BuyerSellerFollow(buyerId, sellerId));
        }

        /// <summary>
        /// Gets all followers for a specific seller (all buyers following them)
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>List of buyer-seller follows for the specified seller</returns>
        public async Task<IEnumerable<BuyerSellerFollow>> GetSellerFollowersAsync(int sellerId)
        {
            var followerBuyerIds = await GetFollowerBuyerIdsAsync(sellerId);
            return followerBuyerIds.Select(buyerId => new BuyerSellerFollow(buyerId, sellerId));
        }

        /// <summary>
        /// Gets all seller IDs that a buyer is following
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of seller IDs that the buyer is following</returns>
        public async Task<IEnumerable<int>> GetFollowedSellerIdsAsync(int buyerId)
        {
            var response = await _httpClient.GetAsync($"buyers/{buyerId}/following/ids");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var sellerIds = JsonSerializer.Deserialize<List<int>>(content, _jsonOptions);
                return sellerIds ?? new List<int>();
            }

            return new List<int>();
        }

        /// <summary>
        /// Gets all buyer IDs that are following a seller
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>List of buyer IDs that are following the seller</returns>
        public async Task<IEnumerable<int>> GetFollowerBuyerIdsAsync(int sellerId)
        {
            var response = await _httpClient.GetAsync($"sellers/{sellerId}/followers/ids");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var buyerIds = JsonSerializer.Deserialize<List<int>>(content, _jsonOptions);
                return buyerIds ?? new List<int>();
            }

            return new List<int>();
        }
    }
}
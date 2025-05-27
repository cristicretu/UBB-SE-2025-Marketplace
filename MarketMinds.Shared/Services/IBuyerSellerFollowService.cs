using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services
{
    /// <summary>
    /// Service interface for managing buyer-seller follow relationships
    /// </summary>
    public interface IBuyerSellerFollowService
    {
        /// <summary>
        /// Makes a buyer follow a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if follow was successful, false otherwise</returns>
        Task<bool> FollowSellerAsync(int buyerId, int sellerId);

        /// <summary>
        /// Makes a buyer unfollow a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if unfollow was successful, false otherwise</returns>
        Task<bool> UnfollowSellerAsync(int buyerId, int sellerId);

        /// <summary>
        /// Checks if a buyer is following a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if buyer is following seller, false otherwise</returns>
        Task<bool> IsFollowingAsync(int buyerId, int sellerId);

        /// <summary>
        /// Gets all sellers that a buyer is following
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of sellers that the buyer is following</returns>
        Task<IEnumerable<Seller>> GetFollowedSellersAsync(int buyerId);

        /// <summary>
        /// Gets all buyers that are following a seller
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>List of buyers that are following the seller</returns>
        Task<IEnumerable<Buyer>> GetFollowersAsync(int sellerId);

        /// <summary>
        /// Gets the count of sellers that a buyer is following
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>Number of sellers the buyer is following</returns>
        Task<int> GetFollowedSellersCountAsync(int buyerId);

        /// <summary>
        /// Gets the count of buyers following a seller
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>Number of buyers following the seller</returns>
        Task<int> GetFollowersCountAsync(int sellerId);

        /// <summary>
        /// Gets follow status information for display purposes
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>Follow information object</returns>
        Task<BuyerSellerFollowInfo?> GetFollowStatusAsync(int buyerId, int sellerId);
    }
}
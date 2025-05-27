using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Repositories
{
    /// <summary>
    /// Repository interface for managing buyer-seller follows
    /// </summary>
    public interface IBuyerSellerFollowRepository
    {
        /// <summary>
        /// Creates a new follow relationship where a buyer follows a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>The created buyer-seller follow</returns>
        Task<BuyerSellerFollow> CreateFollowAsync(int buyerId, int sellerId);

        /// <summary>
        /// Removes a follow relationship between a buyer and seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if follow was removed, false if it didn't exist</returns>
        Task<bool> RemoveFollowAsync(int buyerId, int sellerId);

        /// <summary>
        /// Checks if a buyer is following a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if buyer is following seller, false otherwise</returns>
        Task<bool> IsFollowingAsync(int buyerId, int sellerId);

        /// <summary>
        /// Gets a specific follow relationship between a buyer and seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>The buyer-seller follow if it exists, null otherwise</returns>
        Task<BuyerSellerFollow?> GetFollowAsync(int buyerId, int sellerId);

        /// <summary>
        /// Gets all follows for a specific buyer (all sellers they follow)
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of buyer-seller follows for the specified buyer</returns>
        Task<IEnumerable<BuyerSellerFollow>> GetBuyerFollowsAsync(int buyerId);

        /// <summary>
        /// Gets all followers for a specific seller (all buyers following them)
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>List of buyer-seller follows for the specified seller</returns>
        Task<IEnumerable<BuyerSellerFollow>> GetSellerFollowersAsync(int sellerId);

        /// <summary>
        /// Gets all seller IDs that a buyer is following
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of seller IDs that the buyer is following</returns>
        Task<IEnumerable<int>> GetFollowedSellerIdsAsync(int buyerId);

        /// <summary>
        /// Gets all buyer IDs that are following a seller
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>List of buyer IDs that are following the seller</returns>
        Task<IEnumerable<int>> GetFollowerBuyerIdsAsync(int sellerId);
    }
}
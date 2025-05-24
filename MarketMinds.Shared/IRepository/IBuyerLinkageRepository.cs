using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Repositories
{
    /// <summary>
    /// Repository interface for managing buyer linkages
    /// </summary>
    public interface IBuyerLinkageRepository
    {
        /// <summary>
        /// Creates a new linkage between two buyers
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>The created buyer linkage</returns>
        Task<BuyerLinkage> CreateLinkageAsync(int buyerId1, int buyerId2);

        /// <summary>
        /// Removes a linkage between two buyers
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>True if linkage was removed, false if it didn't exist</returns>
        Task<bool> RemoveLinkageAsync(int buyerId1, int buyerId2);

        /// <summary>
        /// Checks if two buyers are linked
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>True if buyers are linked, false otherwise</returns>
        Task<bool> AreBuyersLinkedAsync(int buyerId1, int buyerId2);

        /// <summary>
        /// Gets a specific linkage between two buyers
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>The buyer linkage if it exists, null otherwise</returns>
        Task<BuyerLinkage?> GetLinkageAsync(int buyerId1, int buyerId2);

        /// <summary>
        /// Gets all linkages for a specific buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of buyer linkages involving the specified buyer</returns>
        Task<IEnumerable<BuyerLinkage>> GetBuyerLinkagesAsync(int buyerId);

        /// <summary>
        /// Gets all linked buyer IDs for a specific buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of buyer IDs that are linked to the specified buyer</returns>
        Task<IEnumerable<int>> GetLinkedBuyerIdsAsync(int buyerId);
    }
} 
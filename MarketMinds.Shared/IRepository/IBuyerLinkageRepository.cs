using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;

namespace MarketMinds.Shared.Repositories
{
    /// <summary>
    /// Repository interface for managing buyer linkages
    /// </summary>
    public interface IBuyerLinkageRepository
    {
        /// <summary>
        /// Creates a new pending linkage request between two buyers
        /// </summary>
        /// <param name="requestingBuyerId">Requesting buyer ID</param>
        /// <param name="receivingBuyerId">Receiving buyer ID</param>
        /// <returns>The created buyer linkage request</returns>
        Task<BuyerLinkage> CreateLinkageRequestAsync(int requestingBuyerId, int receivingBuyerId);

        /// <summary>
        /// Approves a pending linkage request
        /// </summary>
        /// <param name="requestingBuyerId">Requesting buyer ID</param>
        /// <param name="receivingBuyerId">Receiving buyer ID</param>
        /// <returns>True if request was approved, false if not found</returns>
        Task<bool> ApproveLinkageRequestAsync(int requestingBuyerId, int receivingBuyerId);

        /// <summary>
        /// Removes a linkage or linkage request between two buyers
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>True if linkage was removed, false if it didn't exist</returns>
        Task<bool> RemoveLinkageAsync(int buyerId1, int buyerId2);

        /// <summary>
        /// Checks if two buyers are linked (approved linkage only)
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>True if buyers are linked, false otherwise</returns>
        Task<bool> AreBuyersLinkedAsync(int buyerId1, int buyerId2);

        /// <summary>
        /// Gets the linkage status between two buyers
        /// </summary>
        /// <param name="currentBuyerId">Current buyer ID</param>
        /// <param name="targetBuyerId">Target buyer ID</param>
        /// <returns>The linkage status</returns>
        Task<MarketMinds.Shared.Services.BuyerLinkageStatus> GetLinkageStatusAsync(int currentBuyerId, int targetBuyerId);

        /// <summary>
        /// Gets a specific linkage between two buyers (approved only)
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>The buyer linkage if it exists and is approved, null otherwise</returns>
        Task<BuyerLinkage?> GetLinkageAsync(int buyerId1, int buyerId2);

        /// <summary>
        /// Gets all approved linkages for a specific buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of buyer linkages involving the specified buyer</returns>
        Task<IEnumerable<BuyerLinkage>> GetBuyerLinkagesAsync(int buyerId);

        /// <summary>
        /// Gets all linked buyer IDs for a specific buyer (approved linkages only)
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of buyer IDs that are linked to the specified buyer</returns>
        Task<IEnumerable<int>> GetLinkedBuyerIdsAsync(int buyerId);
    }
} 
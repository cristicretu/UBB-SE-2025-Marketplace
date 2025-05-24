using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services
{
    /// <summary>
    /// Service interface for buyer linkage business logic
    /// </summary>
    public interface IBuyerLinkageService
    {
        /// <summary>
        /// Links two buyers together
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to link with</param>
        /// <returns>True if linking was successful, false otherwise</returns>
        Task<bool> LinkBuyersAsync(int currentBuyerId, int targetBuyerId);

        /// <summary>
        /// Unlinks two buyers
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to unlink from</param>
        /// <returns>True if unlinking was successful, false otherwise</returns>
        Task<bool> UnlinkBuyersAsync(int currentBuyerId, int targetBuyerId);

        /// <summary>
        /// Checks if two buyers are linked
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>True if buyers are linked, false otherwise</returns>
        Task<bool> AreBuyersLinkedAsync(int buyerId1, int buyerId2);

        /// <summary>
        /// Gets the linkage status between the current buyer and target buyer
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID</param>
        /// <returns>Linkage information containing status and details</returns>
        Task<BuyerLinkageInfo> GetLinkageStatusAsync(int currentBuyerId, int targetBuyerId);

        /// <summary>
        /// Gets all linked buyers for a specific buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of linked buyer information</returns>
        Task<IEnumerable<Buyer>> GetLinkedBuyersAsync(int buyerId);

        /// <summary>
        /// Gets the count of linked buyers for a specific buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>Number of linked buyers</returns>
        Task<int> GetLinkedBuyersCountAsync(int buyerId);
    }

    /// <summary>
    /// Contains information about buyer linkage status
    /// </summary>
    public class BuyerLinkageInfo
    {
        /// <summary>
        /// Whether the buyers are linked
        /// </summary>
        public bool IsLinked { get; set; }

        /// <summary>
        /// Whether the current user can link/unlink (not viewing own profile)
        /// </summary>
        public bool CanManageLink { get; set; }

        /// <summary>
        /// The date when the linkage was created (if linked)
        /// </summary>
        public DateTime? LinkedDate { get; set; }

        /// <summary>
        /// Display text for the link button
        /// </summary>
        public string ButtonText => IsLinked ? "Unlink" : "Link";

        /// <summary>
        /// CSS class for the link button
        /// </summary>
        public string ButtonCssClass => IsLinked ? "unlink-btn" : "link-btn";
    }
} 
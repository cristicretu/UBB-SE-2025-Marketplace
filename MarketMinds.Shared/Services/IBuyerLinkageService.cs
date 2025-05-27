using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services
{
    /// <summary>
    /// Service interface for buyer linkage business logic
    /// </summary>
    public interface IBuyerLinkageService
    {
        /// <summary>
        /// Creates a linkage request between two buyers (pending approval)
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to link with</param>
        /// <returns>True if linkage request was created successfully, false otherwise</returns>
        Task<bool> CreateLinkageRequestAsync(int currentBuyerId, int targetBuyerId);

        /// <summary>
        /// Accepts a pending linkage request
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID (the one accepting)</param>
        /// <param name="requestingBuyerId">Requesting buyer's ID (the one who sent the request)</param>
        /// <returns>True if request was accepted successfully, false otherwise</returns>
        Task<bool> AcceptLinkageRequestAsync(int currentBuyerId, int requestingBuyerId);

        /// <summary>
        /// Rejects a pending linkage request
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID (the one rejecting)</param>
        /// <param name="requestingBuyerId">Requesting buyer's ID (the one who sent the request)</param>
        /// <returns>True if request was rejected successfully, false otherwise</returns>
        Task<bool> RejectLinkageRequestAsync(int currentBuyerId, int requestingBuyerId);

        /// <summary>
        /// Cancels a pending linkage request that the current user sent
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID (the one who sent the request)</param>
        /// <param name="targetBuyerId">Target buyer's ID (the one who received the request)</param>
        /// <returns>True if request was cancelled successfully, false otherwise</returns>
        Task<bool> CancelLinkageRequestAsync(int currentBuyerId, int targetBuyerId);

        /// <summary>
        /// Unlinks two buyers (removes an existing approved linkage)
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to unlink from</param>
        /// <returns>True if unlinking was successful, false otherwise</returns>
        Task<bool> UnlinkBuyersAsync(int currentBuyerId, int targetBuyerId);

        /// <summary>
        /// Checks if two buyers are linked (approved linkage)
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
        /// Gets all linked buyers for a specific buyer (approved linkages only)
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of linked buyer information</returns>
        Task<IEnumerable<Buyer>> GetLinkedBuyersAsync(int buyerId);

        /// <summary>
        /// Gets the count of linked buyers for a specific buyer (approved linkages only)
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
        /// The status of the linkage between buyers
        /// </summary>
        public BuyerLinkageStatus Status { get; set; } = BuyerLinkageStatus.None;

        /// <summary>
        /// Whether the current user can manage the link (not viewing own profile)
        /// </summary>
        public bool CanManageLink { get; set; }

        /// <summary>
        /// The date when the linkage was created (if linked)
        /// </summary>
        public DateTime? LinkedDate { get; set; }

        /// <summary>
        /// Whether the buyers are linked (approved linkage)
        /// </summary>
        public bool IsLinked => Status == BuyerLinkageStatus.Linked;

        /// <summary>
        /// Whether there is a pending request sent by the current user
        /// </summary>
        public bool IsPendingSent => Status == BuyerLinkageStatus.PendingSent;

        /// <summary>
        /// Whether there is a pending request received by the current user
        /// </summary>
        public bool IsPendingReceived => Status == BuyerLinkageStatus.PendingReceived;

        /// <summary>
        /// Display text for the primary action button
        /// </summary>
        public string ButtonText => Status switch
        {
            BuyerLinkageStatus.None => "Send Link Request",
            BuyerLinkageStatus.PendingSent => "Request Pending",
            BuyerLinkageStatus.PendingReceived => "Accept Request",
            BuyerLinkageStatus.Linked => "Linked",
            _ => "Link"
        };

        /// <summary>
        /// CSS class for the primary action button
        /// </summary>
        public string ButtonCssClass => Status switch
        {
            BuyerLinkageStatus.None => "bg-blue-600 hover:bg-blue-700",
            BuyerLinkageStatus.PendingSent => "bg-yellow-500 hover:bg-yellow-600",
            BuyerLinkageStatus.PendingReceived => "bg-green-600 hover:bg-green-700",
            BuyerLinkageStatus.Linked => "bg-green-600 hover:bg-red-600",
            _ => "bg-blue-600 hover:bg-blue-700"
        };

        /// <summary>
        /// Whether to show a secondary action button (reject/cancel)
        /// </summary>
        public bool ShowSecondaryButton => Status == BuyerLinkageStatus.PendingReceived || Status == BuyerLinkageStatus.PendingSent;

        /// <summary>
        /// Text for the secondary action button
        /// </summary>
        public string SecondaryButtonText => Status switch
        {
            BuyerLinkageStatus.PendingReceived => "Reject",
            BuyerLinkageStatus.PendingSent => "Cancel Request",
            _ => ""
        };

        /// <summary>
        /// CSS class for the secondary action button
        /// </summary>
        public string SecondaryButtonCssClass => "bg-red-600 hover:bg-red-700";
    }
}
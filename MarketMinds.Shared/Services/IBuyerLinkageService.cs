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
    /// Enumeration for buyer linkage status
    /// </summary>
    public enum BuyerLinkageStatus
    {
        /// <summary>
        /// No linkage exists between the buyers
        /// </summary>
        None,

        /// <summary>
        /// Current user has sent a linkage request that is pending approval
        /// </summary>
        PendingSent,

        /// <summary>
        /// Current user has received a linkage request that is pending their approval
        /// </summary>
        PendingReceived,

        /// <summary>
        /// Buyers are linked (approved linkage)
        /// </summary>
        Linked
    }

    /// <summary>
    /// Contains information about buyer linkage status
    /// </summary>
    public class BuyerLinkageInfo
    {
        /// <summary>
        /// The status of the linkage between buyers
        /// </summary>
        public MarketMinds.Shared.Services.BuyerLinkageStatus Status { get; set; } = MarketMinds.Shared.Services.BuyerLinkageStatus.None;

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
        public bool IsLinked => Status == MarketMinds.Shared.Services.BuyerLinkageStatus.Linked;

        /// <summary>
        /// Whether there is a pending request sent by the current user
        /// </summary>
        public bool IsPendingSent => Status == MarketMinds.Shared.Services.BuyerLinkageStatus.PendingSent;

        /// <summary>
        /// Whether there is a pending request received by the current user
        /// </summary>
        public bool IsPendingReceived => Status == MarketMinds.Shared.Services.BuyerLinkageStatus.PendingReceived;

        /// <summary>
        /// Display text for the primary action button
        /// </summary>
        public string ButtonText => Status switch
        {
            MarketMinds.Shared.Services.BuyerLinkageStatus.None => "Send Link Request",
            MarketMinds.Shared.Services.BuyerLinkageStatus.PendingSent => "Request Pending",
            MarketMinds.Shared.Services.BuyerLinkageStatus.PendingReceived => "Accept Request",
            MarketMinds.Shared.Services.BuyerLinkageStatus.Linked => "Linked",
            _ => "Link"
        };

        /// <summary>
        /// CSS class for the primary action button
        /// </summary>
        public string ButtonCssClass => Status switch
        {
            MarketMinds.Shared.Services.BuyerLinkageStatus.None => "bg-blue-600 hover:bg-blue-700",
            MarketMinds.Shared.Services.BuyerLinkageStatus.PendingSent => "bg-yellow-500 hover:bg-yellow-600",
            MarketMinds.Shared.Services.BuyerLinkageStatus.PendingReceived => "bg-green-600 hover:bg-green-700",
            MarketMinds.Shared.Services.BuyerLinkageStatus.Linked => "bg-green-600 hover:bg-red-600",
            _ => "bg-blue-600 hover:bg-blue-700"
        };

        /// <summary>
        /// Whether to show a secondary action button (reject/cancel)
        /// </summary>
        public bool ShowSecondaryButton => Status == MarketMinds.Shared.Services.BuyerLinkageStatus.PendingReceived || Status == MarketMinds.Shared.Services.BuyerLinkageStatus.PendingSent;

        /// <summary>
        /// Text for the secondary action button
        /// </summary>
        public string SecondaryButtonText => Status switch
        {
            MarketMinds.Shared.Services.BuyerLinkageStatus.PendingReceived => "Reject",
            MarketMinds.Shared.Services.BuyerLinkageStatus.PendingSent => "Cancel Request",
            _ => ""
        };

        /// <summary>
        /// CSS class for the secondary action button
        /// </summary>
        public string SecondaryButtonCssClass => "bg-red-600 hover:bg-red-700";
    }
} 
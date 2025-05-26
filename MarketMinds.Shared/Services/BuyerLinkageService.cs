using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;
using MarketMinds.Shared.Services;
using Microsoft.Extensions.Logging;

namespace MarketMinds.Server.Services
{
    /// <summary>
    /// Service implementation for buyer linkage business logic
    /// </summary>
    public class BuyerLinkageService : IBuyerLinkageService
    {
        private readonly IBuyerLinkageRepository _buyerLinkageRepository;
        private readonly IBuyerService _buyerService;
        private readonly ILogger<BuyerLinkageService> _logger;

        /// <summary>
        /// Initializes a new instance of the BuyerLinkageService class
        /// </summary>
        /// <param name="buyerLinkageRepository">Buyer linkage repository</param>
        /// <param name="buyerService">Buyer service</param>
        /// <param name="logger">Logger</param>
        public BuyerLinkageService(
            IBuyerLinkageRepository buyerLinkageRepository,
            IBuyerService buyerService,
            ILogger<BuyerLinkageService> logger)
        {
            _buyerLinkageRepository = buyerLinkageRepository ?? throw new ArgumentNullException(nameof(buyerLinkageRepository));
            _buyerService = buyerService ?? throw new ArgumentNullException(nameof(buyerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a linkage request between two buyers (pending approval)
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to link with</param>
        /// <returns>True if linkage request was created successfully, false otherwise</returns>
        public async Task<bool> CreateLinkageRequestAsync(int currentBuyerId, int targetBuyerId)
        {
            try
            {
                _logger.LogInformation("Creating linkage request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);

                if (currentBuyerId == targetBuyerId)
                {
                    _logger.LogWarning("Buyer {BuyerId} attempted to link to themselves", currentBuyerId);
                    return false;
                }

                // Verify both buyers exist
                var currentBuyer = await _buyerService.GetBuyerByIdAsync(currentBuyerId);
                var targetBuyer = await _buyerService.GetBuyerByIdAsync(targetBuyerId);

                if (currentBuyer == null)
                {
                    _logger.LogWarning("Current buyer {BuyerId} not found", currentBuyerId);
                    return false;
                }

                if (targetBuyer == null)
                {
                    _logger.LogWarning("Target buyer {BuyerId} not found", targetBuyerId);
                    return false;
                }

                // Check current status
                var currentStatus = await _buyerLinkageRepository.GetLinkageStatusAsync(currentBuyerId, targetBuyerId);
                if (currentStatus != BuyerLinkageStatus.None)
                {
                    _logger.LogInformation("Linkage already exists between buyers {CurrentBuyerId} and {TargetBuyerId} with status {Status}", 
                        currentBuyerId, targetBuyerId, currentStatus);
                    return currentStatus == BuyerLinkageStatus.Linked; // Return true if already linked
                }

                // Create the pending linkage request
                await _buyerLinkageRepository.CreateLinkageRequestAsync(currentBuyerId, targetBuyerId);

                _logger.LogInformation("Successfully created linkage request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating linkage request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);
                return false;
            }
        }

        /// <summary>
        /// Accepts a pending linkage request
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID (the one accepting)</param>
        /// <param name="requestingBuyerId">Requesting buyer's ID (the one who sent the request)</param>
        /// <returns>True if request was accepted successfully, false otherwise</returns>
        public async Task<bool> AcceptLinkageRequestAsync(int currentBuyerId, int requestingBuyerId)
        {
            try
            {
                _logger.LogInformation("Buyer {CurrentBuyerId} accepting linkage request from buyer {RequestingBuyerId}", 
                    currentBuyerId, requestingBuyerId);

                if (currentBuyerId == requestingBuyerId)
                {
                    _logger.LogWarning("Buyer {BuyerId} attempted to accept request from themselves", currentBuyerId);
                    return false;
                }

                // Verify the request exists and is pending
                var currentStatus = await _buyerLinkageRepository.GetLinkageStatusAsync(currentBuyerId, requestingBuyerId);
                if (currentStatus != BuyerLinkageStatus.PendingReceived)
                {
                    _logger.LogWarning("No pending request found from buyer {RequestingBuyerId} to buyer {CurrentBuyerId}. Current status: {Status}", 
                        requestingBuyerId, currentBuyerId, currentStatus);
                    return false;
                }

                // Approve the linkage request
                var result = await _buyerLinkageRepository.ApproveLinkageRequestAsync(requestingBuyerId, currentBuyerId);

                if (result)
                {
                    _logger.LogInformation("Successfully accepted linkage request from buyer {RequestingBuyerId} to buyer {CurrentBuyerId}", 
                        requestingBuyerId, currentBuyerId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting linkage request from buyer {RequestingBuyerId} to buyer {CurrentBuyerId}", 
                    requestingBuyerId, currentBuyerId);
                return false;
            }
        }

        /// <summary>
        /// Rejects a pending linkage request
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID (the one rejecting)</param>
        /// <param name="requestingBuyerId">Requesting buyer's ID (the one who sent the request)</param>
        /// <returns>True if request was rejected successfully, false otherwise</returns>
        public async Task<bool> RejectLinkageRequestAsync(int currentBuyerId, int requestingBuyerId)
        {
            try
            {
                _logger.LogInformation("Buyer {CurrentBuyerId} rejecting linkage request from buyer {RequestingBuyerId}", 
                    currentBuyerId, requestingBuyerId);

                if (currentBuyerId == requestingBuyerId)
                {
                    _logger.LogWarning("Buyer {BuyerId} attempted to reject request from themselves", currentBuyerId);
                    return false;
                }

                // Verify the request exists and is pending
                var currentStatus = await _buyerLinkageRepository.GetLinkageStatusAsync(currentBuyerId, requestingBuyerId);
                if (currentStatus != BuyerLinkageStatus.PendingReceived)
                {
                    _logger.LogWarning("No pending request found from buyer {RequestingBuyerId} to buyer {CurrentBuyerId}. Current status: {Status}", 
                        requestingBuyerId, currentBuyerId, currentStatus);
                    return false;
                }

                // Remove the linkage request
                var result = await _buyerLinkageRepository.RemoveLinkageAsync(requestingBuyerId, currentBuyerId);

                if (result)
                {
                    _logger.LogInformation("Successfully rejected linkage request from buyer {RequestingBuyerId} to buyer {CurrentBuyerId}", 
                        requestingBuyerId, currentBuyerId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting linkage request from buyer {RequestingBuyerId} to buyer {CurrentBuyerId}", 
                    requestingBuyerId, currentBuyerId);
                return false;
            }
        }

        /// <summary>
        /// Cancels a pending linkage request that the current user sent
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID (the one who sent the request)</param>
        /// <param name="targetBuyerId">Target buyer's ID (the one who received the request)</param>
        /// <returns>True if request was cancelled successfully, false otherwise</returns>
        public async Task<bool> CancelLinkageRequestAsync(int currentBuyerId, int targetBuyerId)
        {
            try
            {
                _logger.LogInformation("Buyer {CurrentBuyerId} cancelling linkage request to buyer {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);

                if (currentBuyerId == targetBuyerId)
                {
                    _logger.LogWarning("Buyer {BuyerId} attempted to cancel request to themselves", currentBuyerId);
                    return false;
                }

                // Verify the request exists and was sent by current user
                var currentStatus = await _buyerLinkageRepository.GetLinkageStatusAsync(currentBuyerId, targetBuyerId);
                if (currentStatus != BuyerLinkageStatus.PendingSent)
                {
                    _logger.LogWarning("No pending request found from buyer {CurrentBuyerId} to buyer {TargetBuyerId}. Current status: {Status}", 
                        currentBuyerId, targetBuyerId, currentStatus);
                    return false;
                }

                // Remove the linkage request
                var result = await _buyerLinkageRepository.RemoveLinkageAsync(currentBuyerId, targetBuyerId);

                if (result)
                {
                    _logger.LogInformation("Successfully cancelled linkage request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}", 
                        currentBuyerId, targetBuyerId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling linkage request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);
                return false;
            }
        }

        /// <summary>
        /// Unlinks two buyers (removes an existing approved linkage)
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to unlink from</param>
        /// <returns>True if unlinking was successful, false otherwise</returns>
        public async Task<bool> UnlinkBuyersAsync(int currentBuyerId, int targetBuyerId)
        {
            try
            {
                _logger.LogInformation("Attempting to unlink buyer {CurrentBuyerId} from buyer {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);

                if (currentBuyerId == targetBuyerId)
                {
                    _logger.LogWarning("Buyer {BuyerId} attempted to unlink from themselves", currentBuyerId);
                    return false;
                }

                // Verify they are actually linked
                var currentStatus = await _buyerLinkageRepository.GetLinkageStatusAsync(currentBuyerId, targetBuyerId);
                if (currentStatus != BuyerLinkageStatus.Linked)
                {
                    _logger.LogWarning("Buyers {CurrentBuyerId} and {TargetBuyerId} are not linked. Current status: {Status}", 
                        currentBuyerId, targetBuyerId, currentStatus);
                    return false;
                }

                var result = await _buyerLinkageRepository.RemoveLinkageAsync(currentBuyerId, targetBuyerId);

                if (result)
                {
                    _logger.LogInformation("Successfully unlinked buyer {CurrentBuyerId} from buyer {TargetBuyerId}", 
                        currentBuyerId, targetBuyerId);
                }
                else
                {
                    _logger.LogInformation("No linkage found between buyer {CurrentBuyerId} and buyer {TargetBuyerId}", 
                        currentBuyerId, targetBuyerId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking buyers {CurrentBuyerId} and {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);
                return false;
            }
        }

        /// <summary>
        /// Checks if two buyers are linked (approved linkage)
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>True if buyers are linked, false otherwise</returns>
        public async Task<bool> AreBuyersLinkedAsync(int buyerId1, int buyerId2)
        {
            try
            {
                return await _buyerLinkageRepository.AreBuyersLinkedAsync(buyerId1, buyerId2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if buyers {BuyerId1} and {BuyerId2} are linked", 
                    buyerId1, buyerId2);
                return false;
            }
        }

        /// <summary>
        /// Gets the linkage status between the current buyer and target buyer
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID</param>
        /// <returns>Linkage information containing status and details</returns>
        public async Task<BuyerLinkageInfo> GetLinkageStatusAsync(int currentBuyerId, int targetBuyerId)
        {
            try
            {
                var linkageInfo = new BuyerLinkageInfo
                {
                    CanManageLink = currentBuyerId != targetBuyerId
                };

                if (currentBuyerId == targetBuyerId)
                {
                    // Viewing own profile - no linking allowed
                    linkageInfo.Status = BuyerLinkageStatus.None;
                    return linkageInfo;
                }

                // Get the current status from repository
                var status = await _buyerLinkageRepository.GetLinkageStatusAsync(currentBuyerId, targetBuyerId);
                linkageInfo.Status = status;

                // If linked, get the linkage date
                if (status == BuyerLinkageStatus.Linked)
                {
                    var linkage = await _buyerLinkageRepository.GetLinkageAsync(currentBuyerId, targetBuyerId);
                    linkageInfo.LinkedDate = linkage?.LinkedDate;
                }

                return linkageInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting linkage status between buyers {CurrentBuyerId} and {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);

                return new BuyerLinkageInfo
                {
                    Status = BuyerLinkageStatus.None,
                    CanManageLink = false
                };
            }
        }

        /// <summary>
        /// Gets all linked buyers for a specific buyer (approved linkages only)
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of linked buyer information</returns>
        public async Task<IEnumerable<Buyer>> GetLinkedBuyersAsync(int buyerId)
        {
            try
            {
                var linkedBuyerIds = await _buyerLinkageRepository.GetLinkedBuyerIdsAsync(buyerId);
                var linkedBuyers = new List<Buyer>();

                foreach (var linkedBuyerId in linkedBuyerIds)
                {
                    var buyer = await _buyerService.GetBuyerByIdAsync(linkedBuyerId);
                    if (buyer != null)
                    {
                        linkedBuyers.Add(buyer);
                    }
                }

                return linkedBuyers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting linked buyers for buyer {BuyerId}", buyerId);
                return new List<Buyer>();
            }
        }

        /// <summary>
        /// Gets the count of linked buyers for a specific buyer (approved linkages only)
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>Number of linked buyers</returns>
        public async Task<int> GetLinkedBuyersCountAsync(int buyerId)
        {
            try
            {
                var linkedBuyerIds = await _buyerLinkageRepository.GetLinkedBuyerIdsAsync(buyerId);
                return linkedBuyerIds.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting linked buyers count for buyer {BuyerId}", buyerId);
                return 0;
            }
        }
    }
} 
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
        /// Links two buyers together
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to link with</param>
        /// <returns>True if linking was successful, false otherwise</returns>
        public async Task<bool> LinkBuyersAsync(int currentBuyerId, int targetBuyerId)
        {
            try
            {
                _logger.LogInformation("Attempting to link buyer {CurrentBuyerId} with buyer {TargetBuyerId}", 
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

                // Check if already linked
                var alreadyLinked = await _buyerLinkageRepository.AreBuyersLinkedAsync(currentBuyerId, targetBuyerId);
                if (alreadyLinked)
                {
                    _logger.LogInformation("Buyers {CurrentBuyerId} and {TargetBuyerId} are already linked", 
                        currentBuyerId, targetBuyerId);
                    return true; // Already linked is considered success
                }

                // Create the linkage
                await _buyerLinkageRepository.CreateLinkageAsync(currentBuyerId, targetBuyerId);

                _logger.LogInformation("Successfully linked buyer {CurrentBuyerId} with buyer {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking buyers {CurrentBuyerId} and {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);
                return false;
            }
        }

        /// <summary>
        /// Unlinks two buyers
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
        /// Checks if two buyers are linked
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
                    linkageInfo.IsLinked = false;
                    return linkageInfo;
                }

                var linkage = await _buyerLinkageRepository.GetLinkageAsync(currentBuyerId, targetBuyerId);
                linkageInfo.IsLinked = linkage != null;
                linkageInfo.LinkedDate = linkage?.LinkedDate;

                return linkageInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting linkage status between buyers {CurrentBuyerId} and {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);

                return new BuyerLinkageInfo
                {
                    IsLinked = false,
                    CanManageLink = false
                };
            }
        }

        /// <summary>
        /// Gets all linked buyers for a specific buyer
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
        /// Gets the count of linked buyers for a specific buyer
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
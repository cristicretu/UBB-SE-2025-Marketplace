using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;
using MarketMinds.Shared.Services.UserService;
using Microsoft.Extensions.Logging;

namespace MarketMinds.Shared.Services
{
    /// <summary>
    /// Service implementation for managing buyer-seller follow relationships
    /// </summary>
    public class BuyerSellerFollowService : IBuyerSellerFollowService
    {
        private readonly IBuyerSellerFollowRepository _followRepository;
        private readonly IBuyerService _buyerService;
        private readonly ISellerService _sellerService;
        private readonly ILogger<BuyerSellerFollowService>? _logger;

        /// <summary>
        /// Initializes a new instance of the BuyerSellerFollowService class
        /// </summary>
        /// <param name="followRepository">Follow repository</param>
        /// <param name="buyerService">Buyer service</param>
        /// <param name="sellerService">Seller service</param>
        /// <param name="logger">Logger</param>
        public BuyerSellerFollowService(
            IBuyerSellerFollowRepository followRepository,
            IBuyerService buyerService,
            ISellerService sellerService,
            ILogger<BuyerSellerFollowService>? logger = null)
        {
            _followRepository = followRepository ?? throw new ArgumentNullException(nameof(followRepository));
            _buyerService = buyerService ?? throw new ArgumentNullException(nameof(buyerService));
            _sellerService = sellerService ?? throw new ArgumentNullException(nameof(sellerService));
            _logger = logger;
        }

        /// <summary>
        /// Makes a buyer follow a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if follow was successful, false otherwise</returns>
        public async Task<bool> FollowSellerAsync(int buyerId, int sellerId)
        {
            try
            {
                _logger?.LogInformation("Attempting to create follow relationship: Buyer {BuyerId} -> Seller {SellerId}", buyerId, sellerId);

                // Validate input
                if (buyerId <= 0 || sellerId <= 0)
                {
                    _logger?.LogWarning("Invalid IDs provided: BuyerId={BuyerId}, SellerId={SellerId}", buyerId, sellerId);
                    return false;
                }

                // Check if already following
                var isAlreadyFollowing = await _followRepository.IsFollowingAsync(buyerId, sellerId);
                if (isAlreadyFollowing)
                {
                    _logger?.LogInformation("Buyer {BuyerId} is already following Seller {SellerId}", buyerId, sellerId);
                    return true; // Already following, consider this success
                }

                // Create the follow relationship
                await _followRepository.CreateFollowAsync(buyerId, sellerId);
                _logger?.LogInformation("Successfully created follow relationship: Buyer {BuyerId} -> Seller {SellerId}", buyerId, sellerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating follow relationship: Buyer {BuyerId} -> Seller {SellerId}", buyerId, sellerId);
                return false;
            }
        }

        /// <summary>
        /// Makes a buyer unfollow a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if unfollow was successful, false otherwise</returns>
        public async Task<bool> UnfollowSellerAsync(int buyerId, int sellerId)
        {
            try
            {
                _logger?.LogInformation("Attempting to remove follow relationship: Buyer {BuyerId} -> Seller {SellerId}", buyerId, sellerId);

                // Validate input
                if (buyerId <= 0 || sellerId <= 0)
                {
                    _logger?.LogWarning("Invalid IDs provided: BuyerId={BuyerId}, SellerId={SellerId}", buyerId, sellerId);
                    return false;
                }

                // Remove the follow relationship
                var result = await _followRepository.RemoveFollowAsync(buyerId, sellerId);
                if (result)
                {
                    _logger?.LogInformation("Successfully removed follow relationship: Buyer {BuyerId} -> Seller {SellerId}", buyerId, sellerId);
                }
                else
                {
                    _logger?.LogWarning("Follow relationship not found to remove: Buyer {BuyerId} -> Seller {SellerId}", buyerId, sellerId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error removing follow relationship: Buyer {BuyerId} -> Seller {SellerId}", buyerId, sellerId);
                return false;
            }
        }

        /// <summary>
        /// Checks if a buyer is following a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if buyer is following seller, false otherwise</returns>
        public async Task<bool> IsFollowingAsync(int buyerId, int sellerId)
        {
            try
            {
                if (buyerId <= 0 || sellerId <= 0)
                {
                    return false;
                }

                return await _followRepository.IsFollowingAsync(buyerId, sellerId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking follow status: Buyer {BuyerId} -> Seller {SellerId}", buyerId, sellerId);
                return false;
            }
        }

        /// <summary>
        /// Gets all sellers that a buyer is following
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of sellers that the buyer is following</returns>
        public async Task<IEnumerable<Seller>> GetFollowedSellersAsync(int buyerId)
        {
            try
            {
                if (buyerId <= 0)
                {
                    return new List<Seller>();
                }

                var sellerIds = await _followRepository.GetFollowedSellerIdsAsync(buyerId);
                var sellers = new List<Seller>();

                foreach (var sellerId in sellerIds)
                {
                    try
                    {
                        var seller = await _sellerService.GetSellerByIdAsync(sellerId);
                        if (seller != null)
                        {
                            sellers.Add(seller);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to load seller {SellerId} for buyer {BuyerId}", sellerId, buyerId);
                    }
                }

                return sellers;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting followed sellers for buyer {BuyerId}", buyerId);
                return new List<Seller>();
            }
        }

        /// <summary>
        /// Gets all buyers that are following a seller
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>List of buyers that are following the seller</returns>
        public async Task<IEnumerable<Buyer>> GetFollowersAsync(int sellerId)
        {
            try
            {
                if (sellerId <= 0)
                {
                    return new List<Buyer>();
                }

                var buyerIds = await _followRepository.GetFollowerBuyerIdsAsync(sellerId);
                var buyers = new List<Buyer>();

                foreach (var buyerId in buyerIds)
                {
                    try
                    {
                        var buyer = await _buyerService.GetBuyerByIdAsync(buyerId);
                        if (buyer != null)
                        {
                            buyers.Add(buyer);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to load buyer {BuyerId} for seller {SellerId}", buyerId, sellerId);
                    }
                }

                return buyers;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting followers for seller {SellerId}", sellerId);
                return new List<Buyer>();
            }
        }

        /// <summary>
        /// Gets the count of sellers that a buyer is following
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>Number of sellers the buyer is following</returns>
        public async Task<int> GetFollowedSellersCountAsync(int buyerId)
        {
            try
            {
                if (buyerId <= 0)
                {
                    return 0;
                }

                var sellerIds = await _followRepository.GetFollowedSellerIdsAsync(buyerId);
                return sellerIds.Count();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting followed sellers count for buyer {BuyerId}", buyerId);
                return 0;
            }
        }

        /// <summary>
        /// Gets the count of buyers following a seller
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>Number of buyers following the seller</returns>
        public async Task<int> GetFollowersCountAsync(int sellerId)
        {
            try
            {
                if (sellerId <= 0)
                {
                    return 0;
                }

                var buyerIds = await _followRepository.GetFollowerBuyerIdsAsync(sellerId);
                return buyerIds.Count();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting followers count for seller {SellerId}", sellerId);
                return 0;
            }
        }

        /// <summary>
        /// Gets follow status information for display purposes
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>Follow information object</returns>
        public async Task<BuyerSellerFollowInfo?> GetFollowStatusAsync(int buyerId, int sellerId)
        {
            try
            {
                if (buyerId <= 0 || sellerId <= 0)
                {
                    return null;
                }

                var isFollowing = await _followRepository.IsFollowingAsync(buyerId, sellerId);
                var followInfo = new BuyerSellerFollowInfo
                {
                    IsFollowing = isFollowing,
                    CanManageFollow = true, // Buyers can always manage their follows
                    FollowedDate = isFollowing ? DateTime.UtcNow : null // Simplified for now
                };

                return followInfo;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting follow status: Buyer {BuyerId} -> Seller {SellerId}", buyerId, sellerId);
                return null;
            }
        }
    }
} 
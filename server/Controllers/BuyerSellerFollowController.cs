using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Repositories;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace Server.Controllers
{
    /// <summary>
    /// API controller for managing buyer-seller follows
    /// </summary>
    [ApiController]
    [Route("api/buyers/{buyerId}")]
    public class BuyerSellerFollowController : ControllerBase
    {
        private readonly IBuyerSellerFollowRepository _followRepository;
        private readonly IBuyerRepository _buyerRepository;
        private readonly ILogger<BuyerSellerFollowController> _logger;

        /// <summary>
        /// Initializes a new instance of the BuyerSellerFollowController class
        /// </summary>
        /// <param name="followRepository">The buyer-seller follow repository</param>
        /// <param name="buyerRepository">The buyer repository</param>
        /// <param name="logger">The logger</param>
        public BuyerSellerFollowController(
            IBuyerSellerFollowRepository followRepository,
            IBuyerRepository buyerRepository,
            ILogger<BuyerSellerFollowController> logger)
        {
            _followRepository = followRepository ?? throw new ArgumentNullException(nameof(followRepository));
            _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Makes a buyer follow a seller
        /// </summary>
        /// <param name="buyerId">Buyer's ID</param>
        /// <param name="sellerId">Seller's ID to follow</param>
        /// <returns>True if follow was successful</returns>
        [HttpPost("follow/{sellerId}")]
        public async Task<ActionResult<bool>> FollowSeller(int buyerId, int sellerId)
        {
            try
            {
                _logger.LogInformation("API: Buyer {BuyerId} attempting to follow seller {SellerId}", 
                    buyerId, sellerId);

                // Validate input
                if (buyerId <= 0 || sellerId <= 0)
                {
                    return BadRequest("Valid buyer and seller IDs are required");
                }

                if (buyerId == sellerId)
                {
                    return BadRequest("A buyer cannot follow themselves");
                }

                // Check if buyer exists - seller existence will be checked in repository
                var buyerExists = await _buyerRepository.CheckIfBuyerExists(buyerId);
                if (!buyerExists)
                {
                    return BadRequest($"Buyer {buyerId} not found");
                }

                // Check if already following
                var alreadyFollowing = await _followRepository.IsFollowingAsync(buyerId, sellerId);
                if (alreadyFollowing)
                {
                    _logger.LogInformation("Buyer {BuyerId} is already following seller {SellerId}", 
                        buyerId, sellerId);
                    return Ok(true); // Already following is considered success
                }

                // Create the follow relationship
                await _followRepository.CreateFollowAsync(buyerId, sellerId);

                _logger.LogInformation("Successfully created follow: Buyer {BuyerId} -> Seller {SellerId}", 
                    buyerId, sellerId);

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error creating follow: Buyer {BuyerId} -> Seller {SellerId}", 
                    buyerId, sellerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to follow seller: {ex.Message}");
            }
        }

        /// <summary>
        /// Makes a buyer unfollow a seller
        /// </summary>
        /// <param name="buyerId">Buyer's ID</param>
        /// <param name="sellerId">Seller's ID to unfollow</param>
        /// <returns>True if unfollow was successful</returns>
        [HttpPost("unfollow/{sellerId}")]
        public async Task<ActionResult<bool>> UnfollowSeller(int buyerId, int sellerId)
        {
            try
            {
                _logger.LogInformation("API: Buyer {BuyerId} attempting to unfollow seller {SellerId}", 
                    buyerId, sellerId);

                // Validate input
                if (buyerId <= 0 || sellerId <= 0)
                {
                    return BadRequest("Valid buyer and seller IDs are required");
                }

                var result = await _followRepository.RemoveFollowAsync(buyerId, sellerId);

                if (result)
                {
                    _logger.LogInformation("Successfully removed follow: Buyer {BuyerId} -> Seller {SellerId}", 
                        buyerId, sellerId);
                }
                else
                {
                    _logger.LogInformation("No follow relationship found: Buyer {BuyerId} -> Seller {SellerId}", 
                        buyerId, sellerId);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error removing follow: Buyer {BuyerId} -> Seller {SellerId}", 
                    buyerId, sellerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to unfollow seller: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a buyer is following a seller
        /// </summary>
        /// <param name="buyerId">Buyer's ID</param>
        /// <param name="sellerId">Seller's ID</param>
        /// <returns>True if buyer is following seller</returns>
        [HttpGet("following/{sellerId}")]
        public async Task<ActionResult<bool>> IsFollowing(int buyerId, int sellerId)
        {
            try
            {
                if (buyerId <= 0 || sellerId <= 0)
                {
                    return BadRequest("Valid buyer and seller IDs are required");
                }

                bool result = await _followRepository.IsFollowingAsync(buyerId, sellerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error checking follow status: Buyer {BuyerId} -> Seller {SellerId}", 
                    buyerId, sellerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to check follow status: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the follow status between a buyer and seller
        /// </summary>
        /// <param name="buyerId">Buyer's ID</param>
        /// <param name="sellerId">Seller's ID</param>
        /// <returns>Follow information</returns>
        [HttpGet("status/{sellerId}")]
        public async Task<ActionResult<BuyerSellerFollowInfo>> GetFollowStatus(int buyerId, int sellerId)
        {
            try
            {
                if (buyerId <= 0 || sellerId <= 0)
                {
                    return BadRequest("Valid buyer and seller IDs are required");
                }

                var followInfo = new BuyerSellerFollowInfo
                {
                    CanManageFollow = buyerId != sellerId
                };

                if (buyerId == sellerId)
                {
                    // Cannot follow yourself
                    followInfo.IsFollowing = false;
                    return Ok(followInfo);
                }

                followInfo.IsFollowing = await _followRepository.IsFollowingAsync(buyerId, sellerId);

                return Ok(followInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting follow status: Buyer {BuyerId} -> Seller {SellerId}", 
                    buyerId, sellerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get follow status: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all sellers that a buyer is following
        /// </summary>
        /// <param name="buyerId">Buyer's ID</param>
        /// <returns>List of sellers the buyer is following</returns>
        [HttpGet("following")]
        public async Task<ActionResult<IEnumerable<int>>> GetFollowedSellers(int buyerId)
        {
            try
            {
                if (buyerId <= 0)
                {
                    return BadRequest("Valid buyer ID is required");
                }

                var sellerIds = await _followRepository.GetFollowedSellerIdsAsync(buyerId);
                return Ok(sellerIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting followed sellers for buyer {BuyerId}", buyerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get followed sellers: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the count of sellers a buyer is following
        /// </summary>
        /// <param name="buyerId">Buyer's ID</param>
        /// <returns>Count of followed sellers</returns>
        [HttpGet("following/count")]
        public async Task<ActionResult<int>> GetFollowedSellersCount(int buyerId)
        {
            try
            {
                var sellerIds = await _followRepository.GetFollowedSellerIdsAsync(buyerId);
                return Ok(sellerIds.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting followed sellers count for buyer {BuyerId}", buyerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get follow count: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// API controller for seller followers - separate route for sellers
    /// </summary>
    [ApiController]
    [Route("api/sellers/{sellerId}")]
    public class SellerFollowersController : ControllerBase
    {
        private readonly IBuyerSellerFollowRepository _followRepository;
        private readonly ILogger<SellerFollowersController> _logger;

        /// <summary>
        /// Initializes a new instance of the SellerFollowersController class
        /// </summary>
        /// <param name="followRepository">The buyer-seller follow repository</param>
        /// <param name="logger">The logger</param>
        public SellerFollowersController(
            IBuyerSellerFollowRepository followRepository,
            ILogger<SellerFollowersController> logger)
        {
            _followRepository = followRepository ?? throw new ArgumentNullException(nameof(followRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all buyer IDs that are following a seller
        /// </summary>
        /// <param name="sellerId">Seller's ID</param>
        /// <returns>List of buyer IDs following the seller</returns>
        [HttpGet("followers/ids")]
        public async Task<ActionResult<IEnumerable<int>>> GetFollowerIds(int sellerId)
        {
            try
            {
                if (sellerId <= 0)
                {
                    return BadRequest("Valid seller ID is required");
                }

                var followerIds = await _followRepository.GetFollowerBuyerIdsAsync(sellerId);
                return Ok(followerIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting followers for seller {SellerId}", sellerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get followers: {ex.Message}");
            }
        }
    }
} 
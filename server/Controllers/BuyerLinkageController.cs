using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Repositories;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace Server.Controllers
{
    /// <summary>
    /// API controller for managing buyer linkages
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BuyerLinkageController : ControllerBase
    {
        private readonly IBuyerLinkageRepository _buyerLinkageRepository;
        private readonly IBuyerRepository _buyerRepository;
        private readonly ILogger<BuyerLinkageController> _logger;

        /// <summary>
        /// Initializes a new instance of the BuyerLinkageController class
        /// </summary>
        /// <param name="buyerLinkageRepository">The buyer linkage repository</param>
        /// <param name="buyerRepository">The buyer repository</param>
        /// <param name="logger">The logger</param>
        public BuyerLinkageController(
            IBuyerLinkageRepository buyerLinkageRepository,
            IBuyerRepository buyerRepository,
            ILogger<BuyerLinkageController> logger)
        {
            _buyerLinkageRepository = buyerLinkageRepository ?? throw new ArgumentNullException(nameof(buyerLinkageRepository));
            _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Links two buyers together
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to link with</param>
        /// <returns>True if linking was successful</returns>
        [HttpPost("link")]
        public async Task<ActionResult<bool>> LinkBuyers([FromQuery] int currentBuyerId, [FromQuery] int targetBuyerId)
        {
            try
            {
                _logger.LogInformation("API: Linking buyer {CurrentBuyerId} with buyer {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);

                // Validate input
                if (currentBuyerId <= 0 || targetBuyerId <= 0)
                {
                    return BadRequest("Valid buyer IDs are required");
                }

                if (currentBuyerId == targetBuyerId)
                {
                    return BadRequest("A buyer cannot link to themselves");
                }

                // Check if buyers exist
                var currentBuyerExists = await _buyerRepository.CheckIfBuyerExists(currentBuyerId);
                var targetBuyerExists = await _buyerRepository.CheckIfBuyerExists(targetBuyerId);

                if (!currentBuyerExists)
                {
                    return BadRequest($"Current buyer {currentBuyerId} not found");
                }

                if (!targetBuyerExists)
                {
                    return BadRequest($"Target buyer {targetBuyerId} not found");
                }

                // Check if already linked
                var alreadyLinked = await _buyerLinkageRepository.AreBuyersLinkedAsync(currentBuyerId, targetBuyerId);
                if (alreadyLinked)
                {
                    _logger.LogInformation("Buyers {CurrentBuyerId} and {TargetBuyerId} are already linked", 
                        currentBuyerId, targetBuyerId);
                    return Ok(true); // Already linked is considered success
                }

                // Create the linkage
                await _buyerLinkageRepository.CreateLinkageAsync(currentBuyerId, targetBuyerId);

                _logger.LogInformation("Successfully linked buyer {CurrentBuyerId} with buyer {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error linking buyers {CurrentBuyerId} and {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to link buyers: {ex.Message}");
            }
        }

        /// <summary>
        /// Unlinks two buyers
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to unlink from</param>
        /// <returns>True if unlinking was successful</returns>
        [HttpPost("unlink")]
        public async Task<ActionResult<bool>> UnlinkBuyers([FromQuery] int currentBuyerId, [FromQuery] int targetBuyerId)
        {
            try
            {
                _logger.LogInformation("API: Unlinking buyer {CurrentBuyerId} from buyer {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);

                // Validate input
                if (currentBuyerId <= 0 || targetBuyerId <= 0)
                {
                    return BadRequest("Valid buyer IDs are required");
                }

                if (currentBuyerId == targetBuyerId)
                {
                    return BadRequest("A buyer cannot unlink from themselves");
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error unlinking buyers {CurrentBuyerId} and {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to unlink buyers: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if two buyers are linked
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>True if buyers are linked</returns>
        [HttpGet("check")]
        public async Task<ActionResult<bool>> AreBuyersLinked([FromQuery] int buyerId1, [FromQuery] int buyerId2)
        {
            try
            {
                if (buyerId1 <= 0 || buyerId2 <= 0)
                {
                    return BadRequest("Valid buyer IDs are required");
                }

                bool result = await _buyerLinkageRepository.AreBuyersLinkedAsync(buyerId1, buyerId2);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error checking if buyers {BuyerId1} and {BuyerId2} are linked", 
                    buyerId1, buyerId2);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to check buyer linkage: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the linkage status between two buyers
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID</param>
        /// <returns>Linkage information</returns>
        [HttpGet("status")]
        public async Task<ActionResult<BuyerLinkageInfo>> GetLinkageStatus([FromQuery] int currentBuyerId, [FromQuery] int targetBuyerId)
        {
            try
            {
                if (currentBuyerId <= 0 || targetBuyerId <= 0)
                {
                    return BadRequest("Valid buyer IDs are required");
                }

                var linkageInfo = new BuyerLinkageInfo
                {
                    CanManageLink = currentBuyerId != targetBuyerId
                };

                if (currentBuyerId == targetBuyerId)
                {
                    // Viewing own profile - no linking allowed
                    linkageInfo.IsLinked = false;
                    return Ok(linkageInfo);
                }

                var linkage = await _buyerLinkageRepository.GetLinkageAsync(currentBuyerId, targetBuyerId);
                linkageInfo.IsLinked = linkage != null;
                linkageInfo.LinkedDate = linkage?.LinkedDate;

                return Ok(linkageInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting linkage status between buyers {CurrentBuyerId} and {TargetBuyerId}", 
                    currentBuyerId, targetBuyerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get linkage status: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all linked buyers for a specific buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of linked buyers</returns>
        [HttpGet("{buyerId}/linked")]
        public async Task<ActionResult<IEnumerable<Buyer>>> GetLinkedBuyers(int buyerId)
        {
            try
            {
                if (buyerId <= 0)
                {
                    return BadRequest("Valid buyer ID is required");
                }

                var linkedBuyerIds = await _buyerLinkageRepository.GetLinkedBuyerIdsAsync(buyerId);
                var linkedBuyers = new List<Buyer>();

                foreach (var linkedBuyerId in linkedBuyerIds)
                {
                    var buyer = await _buyerRepository.LoadBuyerInfo(linkedBuyerId);
                    if (buyer != null)
                    {
                        linkedBuyers.Add(buyer);
                    }
                }

                return Ok(linkedBuyers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting linked buyers for buyer {BuyerId}", buyerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get linked buyers: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the count of linked buyers for a specific buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>Number of linked buyers</returns>
        [HttpGet("{buyerId}/count")]
        public async Task<ActionResult<int>> GetLinkedBuyersCount(int buyerId)
        {
            try
            {
                if (buyerId <= 0)
                {
                    return BadRequest("Valid buyer ID is required");
                }

                var linkedBuyerIds = await _buyerLinkageRepository.GetLinkedBuyerIdsAsync(buyerId);
                return Ok(linkedBuyerIds.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting linked buyers count for buyer {BuyerId}", buyerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get linked buyers count: {ex.Message}");
            }
        }
    }
} 
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
        /// Sends a linkage request to another buyer
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to send request to</param>
        /// <returns>True if request was sent successfully</returns>
        [HttpPost("sendrequest")]
        public async Task<ActionResult<bool>> SendLinkageRequest([FromQuery] int currentBuyerId, [FromQuery] int targetBuyerId)
        {
            try
            {
                _logger.LogInformation("API: Sending linkage request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}",
                    currentBuyerId, targetBuyerId);

                // Validate input
                if (currentBuyerId <= 0 || targetBuyerId <= 0)
                {
                    return BadRequest("Valid buyer IDs are required");
                }

                if (currentBuyerId == targetBuyerId)
                {
                    return BadRequest("A buyer cannot send a request to themselves");
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

                // Create the linkage request
                await _buyerLinkageRepository.CreateLinkageRequestAsync(currentBuyerId, targetBuyerId);

                _logger.LogInformation("Successfully sent linkage request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}",
                    currentBuyerId, targetBuyerId);

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error sending linkage request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}",
                    currentBuyerId, targetBuyerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to send linkage request: {ex.Message}");
            }
        }

        /// <summary>
        /// Accepts a pending linkage request
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID (the one accepting)</param>
        /// <param name="requestingBuyerId">Requesting buyer's ID (the one who sent the request)</param>
        /// <returns>True if request was accepted successfully</returns>
        [HttpPost("acceptrequest")]
        public async Task<ActionResult<bool>> AcceptLinkageRequest([FromQuery] int currentBuyerId, [FromQuery] int requestingBuyerId)
        {
            try
            {
                _logger.LogInformation("API: Buyer {CurrentBuyerId} accepting linkage request from buyer {RequestingBuyerId}",
                    currentBuyerId, requestingBuyerId);

                // Validate input
                if (currentBuyerId <= 0 || requestingBuyerId <= 0)
                {
                    return BadRequest("Valid buyer IDs are required");
                }

                if (currentBuyerId == requestingBuyerId)
                {
                    return BadRequest("A buyer cannot accept a request from themselves");
                }

                var result = await _buyerLinkageRepository.ApproveLinkageRequestAsync(requestingBuyerId, currentBuyerId);

                if (result)
                {
                    _logger.LogInformation("Successfully accepted linkage request from buyer {RequestingBuyerId} by buyer {CurrentBuyerId}",
                        requestingBuyerId, currentBuyerId);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error accepting linkage request from buyer {RequestingBuyerId} by buyer {CurrentBuyerId}",
                    requestingBuyerId, currentBuyerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to accept linkage request: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a linkage or rejects/cancels a request
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID</param>
        /// <returns>True if removal was successful</returns>
        [HttpPost("removelink")]
        public async Task<ActionResult<bool>> RemoveLink([FromQuery] int currentBuyerId, [FromQuery] int targetBuyerId)
        {
            try
            {
                _logger.LogInformation("API: Removing link/request between buyer {CurrentBuyerId} and buyer {TargetBuyerId}",
                    currentBuyerId, targetBuyerId);

                // Validate input
                if (currentBuyerId <= 0 || targetBuyerId <= 0)
                {
                    return BadRequest("Valid buyer IDs are required");
                }

                if (currentBuyerId == targetBuyerId)
                {
                    return BadRequest("A buyer cannot remove a link with themselves");
                }

                var result = await _buyerLinkageRepository.RemoveLinkageAsync(currentBuyerId, targetBuyerId);

                if (result)
                {
                    _logger.LogInformation("Successfully removed link/request between buyer {CurrentBuyerId} and buyer {TargetBuyerId}",
                        currentBuyerId, targetBuyerId);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error removing link/request between buyers {CurrentBuyerId} and {TargetBuyerId}",
                    currentBuyerId, targetBuyerId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to remove link/request: {ex.Message}");
            }
        }

        /// <summary>
        /// Links two buyers together (DEPRECATED - use sendrequest instead)
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to link with</param>
        /// <returns>True if linking was successful</returns>
        [HttpPost("link")]
        public async Task<ActionResult<bool>> LinkBuyers([FromQuery] int currentBuyerId, [FromQuery] int targetBuyerId)
        {
            // Redirect to the new request-based system for backward compatibility
            return await SendLinkageRequest(currentBuyerId, targetBuyerId);
        }

        /// <summary>
        /// Unlinks two buyers (DEPRECATED - use removelink instead)
        /// </summary>
        /// <param name="currentBuyerId">Current buyer's ID</param>
        /// <param name="targetBuyerId">Target buyer's ID to unlink from</param>
        /// <returns>True if unlinking was successful</returns>
        [HttpPost("unlink")]
        public async Task<ActionResult<bool>> UnlinkBuyers([FromQuery] int currentBuyerId, [FromQuery] int targetBuyerId)
        {
            // Redirect to the new remove link system for backward compatibility
            return await RemoveLink(currentBuyerId, targetBuyerId);
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
        /// <returns>Linkage status as enum value</returns>
        [HttpGet("status")]
        public async Task<ActionResult<BuyerLinkageStatus>> GetLinkageStatus([FromQuery] int currentBuyerId, [FromQuery] int targetBuyerId)
        {
            try
            {
                if (currentBuyerId <= 0 || targetBuyerId <= 0)
                {
                    return BadRequest("Valid buyer IDs are required");
                }

                var status = await _buyerLinkageRepository.GetLinkageStatusAsync(currentBuyerId, targetBuyerId);
                return Ok(status);
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
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route: /api/account
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository accountRepository; // Added repository field
        private readonly ILogger<AccountController> logger;
        private readonly static int MINIMUM_USER_ID = 0;

        public AccountController(IAccountRepository accountRepository, ILogger<AccountController> logger)
        {
            this.accountRepository = accountRepository;
            this.logger = logger;
        }

        // GET: api/account/{userId}
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> GetUser(int userId)
        {
            logger.LogInformation("GetUser endpoint called for userId: {UserId}", userId);
            if (userId <= MINIMUM_USER_ID)
            {
                logger.LogWarning("GetUser called with invalid userId: {UserId}", userId);
                return BadRequest("User ID must be positive.");
            }

            try
            {
                var user = await accountRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    logger.LogInformation("User not found for userId: {UserId}", userId);
                    return NotFound();
                }
                return Ok(user);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error in GetUser endpoint for userId: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred fetching user data.");
            }
        }

        // GET: api/account/{userId}/orders
        [HttpGet("{userId}/orders")]
        [ProducesResponseType(typeof(List<UserOrder>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserOrder>>> GetUserOrders(int userId)
        {
            logger.LogInformation("GetUserOrders endpoint called for userId: {UserId}", userId);
            if (userId <= MINIMUM_USER_ID)
            {
                logger.LogWarning("GetUserOrders called with invalid userId: {UserId}", userId);
                return BadRequest("User ID must be positive.");
            }

            try
            {
                var orders = await accountRepository.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error in GetUserOrders endpoint for userId: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred fetching user orders.");
            }
        }

        // POST: api/account/{userId}/orders
        [HttpPost("{userId}/orders")]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> CreateOrderFromBasket(int userId, [FromBody] CreateOrderRequest request)
        {
            logger.LogInformation("CreateOrderFromBasket endpoint called for userId: {UserId}", userId);

            if (userId <= MINIMUM_USER_ID)
            {
                logger.LogWarning("CreateOrderFromBasket called with invalid userId: {UserId}", userId);
                return BadRequest("User ID must be positive.");
            }

            if (request == null || request.BasketId <= 0)
            {
                logger.LogWarning("CreateOrderFromBasket called with invalid basketId for userId: {UserId}", userId);
                return BadRequest("Basket ID must be provided and positive.");
            }

            try
            {
                // First get the user to check their balance
                var user = await accountRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    logger.LogWarning("User not found for userId: {UserId}", userId);
                    return NotFound($"User with ID {userId} not found.");
                }

                // Get the basket total cost
                var basketTotal = await accountRepository.GetBasketTotalAsync(userId, request.BasketId);
                double finalTotal = basketTotal;
                if (request.TotalAmount > 0 && request.DiscountAmount > 0)
                {
                    logger.LogInformation("Using provided discount amount: {DiscountAmount}, total amount: {TotalAmount}",
                        request.DiscountAmount, request.TotalAmount);
                    finalTotal = request.TotalAmount;
                }

                // Check if user has enough balance
                if (user.Balance < finalTotal)
                {
                    logger.LogWarning("User {UserId} has insufficient funds. Balance: {Balance}, Required: {Total}",
                        userId, user.Balance, finalTotal);
                    return BadRequest($"Insufficient funds. Your balance is ${user.Balance:F2}, but the total cost is ${finalTotal:F2}.");
                }

                var createdOrders = await accountRepository.CreateOrderFromBasketAsync(userId, request.BasketId, request.DiscountAmount);

                user.Balance -= finalTotal;
                await accountRepository.UpdateUserAsync(user);

                logger.LogInformation("Successfully created {OrderCount} orders for userId: {UserId} from basketId: {BasketId}. New balance: {Balance}",
                    createdOrders.Count, userId, request.BasketId, user.Balance);

                return StatusCode(StatusCodes.Status201Created, createdOrders);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Invalid argument in CreateOrderFromBasket for userId: {UserId}, basketId: {BasketId}",
                    userId, request.BasketId);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Operation not valid in CreateOrderFromBasket for userId: {UserId}, basketId: {BasketId}",
                    userId, request.BasketId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in CreateOrderFromBasket for userId: {UserId}, basketId: {BasketId}",
                    userId, request.BasketId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred creating orders from basket.");
            }
        }
    }

    // DTO for the create order request
    public class CreateOrderRequest
    {
        public int BasketId { get; set; }
        public double DiscountAmount { get; set; } = 0;
        public double TotalAmount { get; set; } = 0;
    }
}
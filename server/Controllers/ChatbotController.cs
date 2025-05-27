using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotRepository chatbotRepository;

        public ChatbotController(
            IChatbotRepository chatbotRepository)
        {
            this.chatbotRepository = chatbotRepository;
        }

        [HttpPost]
        public async Task<IActionResult> GetChatbotResponse([FromBody] ChatbotRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }
            
            try
            {                
                if (string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest("Message cannot be empty");
                }

                string botResponse = await chatbotRepository.GetBotResponseAsync(request.Message, request.UserId);           
                return Ok(new ChatbotResponse
                {
                    Message = botResponse,
                    Success = true
                });
            }
            catch (Exception exception)
            {
                return Ok(new ChatbotResponse 
                {
                    Message = "I'm sorry, I encountered an unexpected error. Please try again later.",
                    Success = true
                });
            }
        }

        [HttpGet("UserContext/{userId}")]
        public async Task<IActionResult> GetUserContext(int userId)
        {
            try
            {
                string userContext = await chatbotRepository.GetUserContextAsync(userId);
                return Ok(new UserContextResponse
                {
                    Context = userContext,
                    Success = true
                });
            }
            catch (Exception exception)
            {
                return Ok(new UserContextResponse
                {
                    Context = "Error retrieving user context.",
                    Success = false
                });
            }
        }

        [HttpGet("User/{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            try
            {
                var user = await chatbotRepository.GetUserAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving user information.");
            }
        }

        [HttpGet("UserBasket/{userId}")]
        public async Task<IActionResult> GetUserBasket(int userId)
        {
            try
            {
                var basket = await chatbotRepository.GetUserBasketAsync(userId);
                if (basket == null)
                {
                    return NotFound();
                }

                return Ok(basket);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving user basket.");
            }
        }

        [HttpGet("BasketItems/{basketId}")]
        public async Task<IActionResult> GetBasketItems(int basketId)
        {
            try
            {
                var items = await chatbotRepository.GetBasketItemsAsync(basketId);
                return Ok(items);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving basket items.");
            }
        }

        [HttpGet("Product/{productId}")]
        public async Task<IActionResult> GetProduct(int productId)
        {
            try
            {
                var product = await chatbotRepository.GetBuyProductAsync(productId);
                if (product == null)
                {
                    return NotFound();
                }

                return Ok(product);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving product information.");
            }
        }

        [HttpGet("ReviewsGiven/{userId}")]
        public async Task<IActionResult> GetReviewsGiven(int userId)
        {
            try
            {
                var reviews = await chatbotRepository.GetReviewsGivenByUserAsync(userId);
                return Ok(reviews);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving reviews.");
            }
        }

        [HttpGet("ReviewsReceived/{userId}")]
        public async Task<IActionResult> GetReviewsReceived(int userId)
        {
            try
            {
                var reviews = await chatbotRepository.GetReviewsReceivedByUserAsync(userId);
                return Ok(reviews);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving reviews.");
            }
        }

        [HttpGet("BuyerOrders/{userId}")]
        public async Task<IActionResult> GetBuyerOrders(int userId)
        {
            try
            {
                var orders = await chatbotRepository.GetBuyerOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving buyer orders.");
            }
        }

        [HttpGet("SellerOrders/{userId}")]
        public async Task<IActionResult> GetSellerOrders(int userId)
        {
            try
            {
                var orders = await chatbotRepository.GetSellerOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving seller orders.");
            }
        }

        [HttpGet("TrackedOrders/{userId}")]
        public async Task<IActionResult> GetTrackedOrders(int userId)
        {
            try
            {
                var trackedOrders = await chatbotRepository.GetTrackedOrdersAsync(userId);
                return Ok(trackedOrders);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving tracked orders.");
            }
        }

        [HttpGet("UserWaitlists/{userId}")]
        public async Task<IActionResult> GetUserWaitlists(int userId)
        {
            try
            {
                var waitlists = await chatbotRepository.GetUserWaitlistsAsync(userId);
                return Ok(waitlists);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving user waitlists.");
            }
        }

        [HttpGet("UserAuctionProducts/{userId}")]
        public async Task<IActionResult> GetUserAuctionProducts(int userId)
        {
            try
            {
                var auctionProducts = await chatbotRepository.GetUserAuctionProductsAsync(userId);
                return Ok(auctionProducts);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving user auction products.");
            }
        }

        [HttpGet("UserBids/{userId}")]
        public async Task<IActionResult> GetUserBids(int userId)
        {
            try
            {
                var bids = await chatbotRepository.GetUserBidsAsync(userId);
                return Ok(bids);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving user bids.");
            }
        }
    }

    public class ChatbotRequest
    {
        public string Message { get; set; }
        public int? UserId { get; set; }
    }
    
    public class ChatbotResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class UserContextResponse
    {
        public string Context { get; set; }
        public bool Success { get; set; }
    }
}

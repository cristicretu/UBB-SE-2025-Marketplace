using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

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
                if (userId <= 0)
                {
                    return BadRequest("Invalid user ID");
                }

                string userContext = await chatbotRepository.GetUserContextAsync(userId);
                
                if (string.IsNullOrWhiteSpace(userContext))
                {
                    return BadRequest($"Empty user context returned for user {userId}");
                }
                
                return Ok(new UserContextResponse
                {
                    Context = userContext ?? string.Empty,
                    Success = !string.IsNullOrWhiteSpace(userContext)
                });
            }
            catch (Exception exception)
            {
                return Ok(new UserContextResponse
                {
                    Context = string.Empty,
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
                    return NotFound($"User with ID {userId} not found");
                }
                return Ok(user);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving user data");
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
                    return NotFound($"Basket for user ID {userId} not found");
                }
                return Ok(basket);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving basket data");
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
                return StatusCode(500, "Error retrieving basket items");
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
                    return NotFound($"Product with ID {productId} not found");
                }
                return Ok(product);
            }
            catch (Exception exception)
            {
                return StatusCode(500, "Error retrieving product data");
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
                return StatusCode(500, "Error retrieving reviews given by user");
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
                return StatusCode(500, "Error retrieving reviews received by user");
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
                return StatusCode(500, "Error retrieving buyer orders");
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
                return StatusCode(500, "Error retrieving seller orders");
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

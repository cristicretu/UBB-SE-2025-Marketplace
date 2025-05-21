using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.Models.DTOs.Mappers;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository basketRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IOrderHistoryRepository orderHistoryRepository;
        private readonly IOrderSummaryRepository orderSummaryRepository;
        private readonly ITrackedOrderRepository trackedOrderRepository;

        // Add JsonSerializerOptions that disables reference handling
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public BasketController(
            IBasketRepository basketRepository,
            IOrderRepository orderRepository,
            IOrderHistoryRepository orderHistoryRepository,
            IOrderSummaryRepository orderSummaryRepository,
            ITrackedOrderRepository trackedOrderRepository)
        {
            this.basketRepository = basketRepository;
            this.orderRepository = orderRepository;
            this.orderHistoryRepository = orderHistoryRepository;
            this.orderSummaryRepository = orderSummaryRepository;
            this.trackedOrderRepository = trackedOrderRepository;
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(BasketDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBasketByUserId(int userId)
        {
            try
            {
                var basket = basketRepository.GetBasketByUserId(userId);
                var basketDto = BasketMapper.ToDTO(basket);
                return new JsonResult(basketDto, jsonOptions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("{basketId}/items")]
        [ProducesResponseType(typeof(List<BasketItemDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBasketItems(int basketId)
        {
            try
            {
                var items = basketRepository.GetBasketItems(basketId);
                var itemDtos = items.Select(item => BasketMapper.ToDTO(item)).ToList();
                return new JsonResult(itemDtos, jsonOptions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost("user/{userId}/product/{productId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddProductToBasket(int userId, int productId, [FromBody] int quantity)
        {
            try
            {
                // Get the user's basket
                Basket basket = basketRepository.GetBasketByUserId(userId);

                // Add the item
                basketRepository.AddItemToBasket(basket.Id, productId, quantity);

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPut("user/{userId}/product/{productId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateProductQuantity(int userId, int productId, [FromBody] int quantity)
        {
            try
            {
                // Get the user's basket
                Basket basket = basketRepository.GetBasketByUserId(userId);

                if (quantity == 0)
                {
                    // If quantity is zero, remove the item
                    basketRepository.RemoveItemByProductId(basket.Id, productId);
                }
                else
                {
                    // Update the quantity
                    basketRepository.UpdateItemQuantityByProductId(basket.Id, productId, quantity);
                }

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPut("user/{userId}/product/{productId}/increase")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult IncreaseProductQuantity(int userId, int productId)
        {
            try
            {
                // Get the user's basket
                Basket basket = basketRepository.GetBasketByUserId(userId);

                // Get the current quantity of the item
                List<BasketItem> items = basketRepository.GetBasketItems(basket.Id);
                BasketItem targetItem = items.FirstOrDefault(item => item.Product.Id == productId);

                if (targetItem == null)
                {
                    return NotFound("Item not found in basket");
                }

                // Update the quantity with one more
                basketRepository.UpdateItemQuantityByProductId(basket.Id, productId, targetItem.Quantity + 1);

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPut("user/{userId}/product/{productId}/decrease")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DecreaseProductQuantity(int userId, int productId)
        {
            try
            {
                // Get the user's basket
                Basket basket = basketRepository.GetBasketByUserId(userId);

                // Get the current quantity of the item
                List<BasketItem> items = basketRepository.GetBasketItems(basket.Id);
                BasketItem targetItem = items.FirstOrDefault(item => item.Product.Id == productId);

                if (targetItem == null)
                {
                    return NotFound("Item not found in basket");
                }

                if (targetItem.Quantity > 1)
                {
                    // Decrease quantity by 1
                    basketRepository.UpdateItemQuantityByProductId(basket.Id, productId, targetItem.Quantity - 1);
                }
                else
                {
                    // Remove item if quantity would be 0
                    basketRepository.RemoveItemByProductId(basket.Id, productId);
                }

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpDelete("user/{userId}/product/{productId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RemoveProductFromBasket(int userId, int productId)
        {
            try
            {
                // Get the user's basket
                Basket basket = basketRepository.GetBasketByUserId(userId);

                // Remove the product
                basketRepository.RemoveItemByProductId(basket.Id, productId);

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpDelete("user/{userId}/clear")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ClearBasket(int userId)
        {
            try
            {
                // Get the user's basket
                Basket basket = basketRepository.GetBasketByUserId(userId);

                // Clear the basket
                basketRepository.ClearBasket(basket.Id);

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost("{basketId}/promocode")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ApplyPromoCode(int basketId, [FromBody] string code)
        {
            try
            {
                // Moved logic to service layer
                return Ok(new { DiscountRate = 0.0 });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("{basketId}/totals")]
        [ProducesResponseType(typeof(BasketTotalsDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CalculateBasketTotals(int basketId, [FromQuery] string promoCode = null)
        {
            try
            {
                // Moved logic to service layer
                return Ok(new { Subtotal = 0.0, Discount = 0.0, TotalAmount = 0.0 });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("{basketId}/validate")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ValidateBasketBeforeCheckout(int basketId)
        {
            try
            {
                // Moved logic to service layer
                List<BasketItem> items = basketRepository.GetBasketItems(basketId);
                bool isValid = items.Count > 0;
                return Ok(isValid);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Processes a checkout operation for a user's basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="checkoutInfo">Information needed for the checkout process.</param>
        /// <returns>Order summary and tracking information.</returns>
        [HttpPost("user/{userId}/checkout")]
        [ProducesResponseType(typeof(CheckoutResultDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckoutBasket(int userId, [FromBody] CheckoutInfoDTO checkoutInfo)
        {
            try
            {
                // Validate the checkout information
                if (checkoutInfo == null)
                {
                    return BadRequest("Checkout information is required.");
                }

                // Get the user's basket
                Basket basket = basketRepository.GetBasketByUserId(userId);
                
                // Get all items in the basket
                List<BasketItem> items = basketRepository.GetBasketItems(basket.Id);
                
                if (items.Count == 0)
                {
                    return BadRequest("Cannot checkout an empty basket.");
                }

                // Create OrderSummary
                OrderSummary orderSummary = new OrderSummary
                {
                    Subtotal = items.Sum(item => item.GetPrice()),
                    WarrantyTax = checkoutInfo.WarrantyTax,
                    DeliveryFee = checkoutInfo.DeliveryFee,
                    FinalTotal = items.Sum(item => item.GetPrice()) + checkoutInfo.WarrantyTax + checkoutInfo.DeliveryFee,
                    FullName = checkoutInfo.FullName,
                    Email = checkoutInfo.Email,
                    PhoneNumber = checkoutInfo.PhoneNumber,
                    Address = checkoutInfo.Address,
                    PostalCode = checkoutInfo.PostalCode,
                    AdditionalInfo = checkoutInfo.AdditionalInfo,
                    ContractDetails = checkoutInfo.ContractDetails
                };
                
                int orderSummaryId = await orderSummaryRepository.AddOrderSummaryAsync(orderSummary);
                
                // Create OrderHistory
                int orderHistoryId = await orderHistoryRepository.CreateOrderHistoryAsync();
                
                // Create order for each basket item
                List<int> orderIds = new List<int>();
                
                foreach (var item in items)
                {
                    // Create order record
                    await orderRepository.AddOrderAsync(
                        item.ProductId,
                        userId,
                        "BuyProduct", // assuming product type is BuyProduct
                        checkoutInfo.PaymentMethod,
                        orderSummaryId,
                        DateTime.Now);
                    
                    // Get the most recently created order's ID
                    var recentOrders = await orderRepository.GetOrdersFromOrderHistoryAsync(orderHistoryId);
                    int orderId = recentOrders.OrderByDescending(o => o.Id).First().Id;
                    orderIds.Add(orderId);
                    
                    // Create tracked order record
                    TrackedOrder trackedOrder = new TrackedOrder
                    {
                        OrderID = orderId,
                        CurrentStatus = OrderStatus.Pending,
                        EstimatedDeliveryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                        DeliveryAddress = checkoutInfo.Address
                    };
                    
                    await trackedOrderRepository.AddTrackedOrderAsync(trackedOrder);
                    
                    // Create initial order checkpoint
                    OrderCheckpoint initialCheckpoint = new OrderCheckpoint
                    {
                        TrackedOrderID = trackedOrder.TrackedOrderID,
                        Timestamp = DateTime.Now,
                        Location = "Warehouse",
                        Description = "Order received and processing started",
                        Status = OrderStatus.Pending
                    };
                    
                    await trackedOrderRepository.AddOrderCheckpointAsync(initialCheckpoint);
                }
                
                // Clear the basket after successful checkout
                basketRepository.ClearBasket(basket.Id);
                
                // Return checkout result
                var result = new CheckoutResultDTO
                {
                    OrderSummaryId = orderSummaryId,
                    OrderHistoryId = orderHistoryId,
                    OrderIds = orderIds,
                    TotalAmount = orderSummary.FinalTotal,
                    EstimatedDeliveryDate = DateTime.Now.AddDays(7)
                };
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"An error occurred during checkout: {ex.Message}");
            }
        }
    }
}

// Add this DTO to represent checkout information
namespace MarketMinds.Shared.Models.DTOs
{
    public class CheckoutInfoDTO
    {
        public string PaymentMethod { get; set; } = "card"; // Default to card
        public double WarrantyTax { get; set; } = 0;
        public double DeliveryFee { get; set; } = 0;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; }
        public string? ContractDetails { get; set; }
    }

    public class CheckoutResultDTO
    {
        public int OrderSummaryId { get; set; }
        public int OrderHistoryId { get; set; }
        public List<int> OrderIds { get; set; } = new List<int>();
        public double TotalAmount { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
    }
}
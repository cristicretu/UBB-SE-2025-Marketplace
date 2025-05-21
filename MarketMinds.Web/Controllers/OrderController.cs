using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using System.Security.Claims;

namespace WebMarketplace.Controllers
{
    public class OrderController : Controller
    {
        private readonly ITrackedOrderService _trackedOrderService;
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderSummaryService _orderSummaryService;

        public OrderController(ITrackedOrderService trackedOrderService, IOrderService orderService, ILogger<OrderController> logger, IOrderSummaryService orderSummaryService)
        {
            _trackedOrderService = trackedOrderService;
            _orderService = orderService;
            _logger = logger;
            _orderSummaryService = orderSummaryService;
        }

        // GET: Order/Test
        public IActionResult Test()
        {
            return Content("Test action works!");
        }

        // GET: Order/OrderHistory
        public async Task<IActionResult> OrderHistory(int userId = 0)
        {
            try
            {
                _logger.LogInformation($"OrderHistory action called with userId parameter: {userId}");
                
                // Get the current user's ID from claims if userId is not provided
                if (userId <= 0)
                {
                    userId = GetCurrentUserId();
                    if (userId <= 0)
                    {
                        _logger.LogWarning("No valid user ID found in claims");
                        return View(Array.Empty<OrderDisplayInfo>());
                    }
                }
                
                var orders = await _orderService.GetOrdersWithProductInfoAsync(userId);
                _logger.LogInformation($"Retrieved {orders?.Count ?? 0} orders for userId: {userId}");
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OrderHistory action");
                return View(Array.Empty<OrderDisplayInfo>());
            }
        }

        // GET: Order/GetFilteredOrders
        [HttpGet]
        public async Task<IActionResult> GetFilteredOrders(string searchText, string timePeriod)
        {
            try
            {
                _logger.LogInformation($"GetFilteredOrders called with searchText: {searchText}, timePeriod: {timePeriod}");
                
                // Get the current user's ID from claims
                int userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    _logger.LogWarning("No valid user ID found in claims for GetFilteredOrders");
                    return Json(new { success = false, message = "Unable to determine your user ID. Please log in again." });
                }

                _logger.LogInformation($"Getting orders for userId: {userId}");
                var orders = await _orderService.GetOrdersWithProductInfoAsync(userId, searchText, timePeriod);
                _logger.LogInformation($"Found {orders?.Count ?? 0} orders");

                return Json(new { success = true, orders = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetFilteredOrders. searchText: {searchText}, timePeriod: {timePeriod}");
                return Json(new { success = false, message = $"Error retrieving orders: {ex.Message}" });
            }
        }

        // GET: Order/Track
        public IActionResult Track()
        {
            return View("TrackOrder");
        }

        // GET: Order/TrackOrder/{orderId}?hasControl={true|false}
        [HttpGet]
        [Route("Order/TrackOrder/{orderId:int}")]
        public async Task<IActionResult> TrackOrder(int orderId, bool hasControl = false)
        {
            _logger.LogInformation($"TrackOrder action called with orderId: {orderId}");
            
            var trackedOrder = await _trackedOrderService.GetTrackedOrderByOrderIdAsync(orderId);
            if (trackedOrder == null)
            {
                _logger.LogWarning($"Tracked order not found for orderId: {orderId}");
                
                // Try to get the order details to get a proper delivery address
                try {
                    // Get the order first and then get its order summary
                    var order = await _orderService.GetOrderByIdAsync(orderId);
                    if (order == null)
                    {
                        _logger.LogError($"Order not found for orderId: {orderId}");
                        return Json(new { success = false, message = "Order not found" });
                    }
                    
                    var orderSummary = await _orderSummaryService.GetOrderSummaryByIdAsync(order.OrderSummaryID);
                    string deliveryAddress = orderSummary?.Address ?? "No delivery address provided";
                    
                    await _trackedOrderService.CreateTrackedOrderForOrderAsync(
                        orderId, 
                        DateOnly.FromDateTime(DateTime.Now.AddDays(7)), // Better estimate for delivery
                        deliveryAddress,
                        OrderStatus.PROCESSING,
                        "Order received"
                    );
                }
                catch (Exception ex) {
                    _logger.LogError(ex, $"Error creating tracked order for orderId: {orderId}");
                    return Json(new { success = false, message = "Error creating tracked order" });
                }
                
                _logger.LogInformation($"Creating new tracked order for orderId: {orderId}");
                trackedOrder = await _trackedOrderService.GetTrackedOrderByOrderIdAsync(orderId);
                if (trackedOrder == null)
                {
                    _logger.LogError($"Failed to create tracked order for orderId: {orderId}");
                    return Json(new { success = false, message = "Failed to create tracked order" });
                }
                _logger.LogInformation($"Created new tracked order for orderId: {orderId}");
                return View("TrackedOrder", trackedOrder);
            }

            if (hasControl)
            {
                _logger.LogInformation($"Rendering TrackedOrderControl view for orderId: {orderId}");
                return View("TrackedOrderControl", trackedOrder);
            }
            else
            {
                _logger.LogInformation($"Rendering TrackedOrder view for orderId: {orderId}");
                return View("TrackedOrder", trackedOrder);
            }
        }

        // POST: Order/RevertCheckpoint
        [HttpPost]
        public async Task<IActionResult> RevertCheckpoint(int trackedOrderId)
        {
            try
            {
                var trackedOrder = await _trackedOrderService.GetTrackedOrderByOrderIdAsync(trackedOrderId);
                if (trackedOrder == null)
                {
                    return Json(new { success = false, message = "Tracked order not found" });
                }

                await _trackedOrderService.RevertToPreviousCheckpointAsync(trackedOrder);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Error reverting checkpoint" });
            }
        }

        // POST: Order/UpdateDeliveryDate
        [HttpPost]
        public async Task<IActionResult> UpdateDeliveryDate(int trackedOrderId, DateOnly newDate)
        {
            try
            {
                var trackedOrder = await _trackedOrderService.GetTrackedOrderByIDAsync(trackedOrderId);
                if (trackedOrder == null)
                {
                    return Json(new { success = false, message = "Tracked order not found" });
                }

                await _trackedOrderService.UpdateTrackedOrderAsync(trackedOrderId, newDate, trackedOrder.CurrentStatus);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Error updating delivery date" });
            }
        }

        // POST: Order/AddCheckpoint
        [HttpPost]
        public async Task<IActionResult> AddCheckpoint(int trackedOrderId, OrderCheckpoint checkpoint)
        {
            try
            {
                checkpoint.TrackedOrderID = trackedOrderId;
                await _trackedOrderService.AddOrderCheckpointAsync(checkpoint);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Error adding checkpoint" });
            }
        }

        // POST: Order/UpdateCheckpoint
        [HttpPost]
        public async Task<IActionResult> UpdateCheckpoint(int trackedOrderId, OrderCheckpoint checkpoint)
        {
            try
            {
                await _trackedOrderService.UpdateOrderCheckpointAsync(
                    checkpoint.CheckpointID,
                    checkpoint.Timestamp,
                    checkpoint.Location,
                    checkpoint.Description,
                    checkpoint.Status);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Error updating checkpoint" });
            }
        }

        // GET: Order/GetOrderDetails
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                var summary = await _orderSummaryService.GetOrderSummaryByIdAsync(orderId);
                if (summary == null)
                {
                    return Json(new { success = false, message = "Order summary not found" });
                }
                return Json(new {
                    success = true,
                    summary = new {
                        id = summary.ID,
                        subtotal = summary.Subtotal,
                        warrantyTax = summary.WarrantyTax,
                        deliveryFee = summary.DeliveryFee,
                        finalTotal = summary.FinalTotal,
                        fullName = summary.FullName,
                        email = summary.Email,
                        phoneNumber = summary.PhoneNumber,
                        address = summary.Address,
                        postalCode = summary.PostalCode,
                        additionalInfo = summary.AdditionalInfo,
                        contractDetails = summary.ContractDetails
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order summary details");
                return Json(new { success = false, message = "Error retrieving order summary details" });
            }
        }

        // Helper method to get the current user ID
        private int GetCurrentUserId()
        {
            int userId = 0;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out userId))
            {
                _logger.LogInformation($"Got userId from claims: {userId}");
                return userId;
            }
            
            var customIdClaim = User.FindFirst("UserId");
            if (customIdClaim != null && int.TryParse(customIdClaim.Value, out userId))
            {
                _logger.LogInformation($"Got userId from custom claim: {userId}");
                return userId;
            }
            
            return 0;
        }
    }
} 
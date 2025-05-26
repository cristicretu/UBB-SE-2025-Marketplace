using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace WebMarketplace.Controllers
{
    public class OrderController : Controller
    {
        private readonly ITrackedOrderService _trackedOrderService;
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderSummaryService _orderSummaryService;
        private readonly IContractService _contractService;
        private readonly IConfiguration _configuration;

        public OrderController(
            ITrackedOrderService trackedOrderService, 
            IOrderService orderService, 
            ILogger<OrderController> logger, 
            IOrderSummaryService orderSummaryService,
            IContractService contractService,
            IConfiguration configuration)
        {
            _trackedOrderService = trackedOrderService;
            _orderService = orderService;
            _logger = logger;
            _orderSummaryService = orderSummaryService;
            _contractService = contractService;
            _configuration = configuration;
        }

        // GET: Order/Test
        public IActionResult Test()
        {
            return Content("Test action works!");
        }

        // GET: Order/OrderHistory
        public async Task<IActionResult> OrderHistory(int userId = 0)
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001";
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
                
                // Pass seller status to view
                ViewBag.IsSeller = IsUserSeller();
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
            
            hasControl &= IsUserSeller();
            
            var trackedOrder = await _trackedOrderService.GetTrackedOrderByOrderIdAsync(orderId);
            if (trackedOrder == null)
            {
                try
                {
                    var order = await _orderService.GetOrderByIdAsync(orderId);
                    if (order == null)
                    {
                        return ReturnErrorView("Order not found. Please check the order ID and try again.");
                    }
                    
                    var orderSummary = await _orderSummaryService.GetOrderSummaryByIdAsync(order.OrderSummaryID);
                    string deliveryAddress = orderSummary?.Address ?? "No delivery address provided";
                    
                    await _trackedOrderService.CreateTrackedOrderForOrderAsync(
                        orderId, 
                        DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                        deliveryAddress,
                        OrderStatus.PROCESSING,
                        "Order received"
                    );
                    
                    trackedOrder = await _trackedOrderService.GetTrackedOrderByOrderIdAsync(orderId);
                    if (trackedOrder == null)
                    {
                        return ReturnErrorView("Failed to create tracked order. Please try again later.");
                    }
                }
                catch (Exception ex)
                {
                    return ReturnErrorView($"Error creating tracked order: {ex.Message}. Please try again later.");
                }
            }

            return hasControl ? View("TrackedOrderControl", trackedOrder) : View("TrackedOrder", trackedOrder);
        }

        private bool IsUserSeller()
        {
            return UserSession.CurrentUserRole == "3" || UserSession.CurrentUserRole == "Seller";
        }

        private IActionResult ReturnErrorView(string errorMessage)
        {
            _logger.LogError(errorMessage);
            ViewBag.ErrorMessage = errorMessage;
            return View("TrackOrder");
        }

        // POST: Order/RevertCheckpoint
        [HttpPost]
        public async Task<IActionResult> RevertCheckpoint(int trackedOrderId)
        {
            try
            {
                // Fixed: Use GetTrackedOrderByIDAsync since we already have the tracked order ID
                var trackedOrder = await _trackedOrderService.GetTrackedOrderByIDAsync(trackedOrderId);
                if (trackedOrder == null)
                {
                    return Json(new { success = false, message = "Tracked order not found" });
                }

                await _trackedOrderService.RevertToPreviousCheckpointAsync(trackedOrder);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reverting checkpoint for tracked order ID {trackedOrderId}");
                return Json(new { success = false, message = $"Error reverting checkpoint: {ex.Message}" });
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
                _logger.LogInformation($"Adding checkpoint with status {checkpoint.Status} to tracked order {trackedOrderId}");
                await _trackedOrderService.AddOrderCheckpointAsync(checkpoint);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding checkpoint to tracked order {trackedOrderId}");
                return Json(new { success = false, message = $"Error adding checkpoint: {ex.Message}" });
            }
        }

        // POST: Order/UpdateCheckpoint
        [HttpPost]
        public async Task<IActionResult> UpdateCheckpoint(int trackedOrderId, OrderCheckpoint checkpoint)
        {
            try
            {
                _logger.LogInformation($"Updating checkpoint {checkpoint.CheckpointID} with status {checkpoint.Status} for tracked order {trackedOrderId}");
                await _trackedOrderService.UpdateOrderCheckpointAsync(
                    checkpoint.CheckpointID,
                    checkpoint.Timestamp,
                    checkpoint.Location,
                    checkpoint.Description,
                    checkpoint.Status);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating checkpoint {checkpoint.CheckpointID} for tracked order {trackedOrderId}");
                return Json(new { success = false, message = $"Error updating checkpoint: {ex.Message}" });
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

        // POST: Order/GenerateContract
        [HttpPost]
        public async Task<IActionResult> GenerateContract(int orderId)
        {
            try
            {
                // First get the order to ensure it exists
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                var orderSummary = await _orderSummaryService.GetOrderSummaryByIdAsync(order.OrderSummaryID);
                if (orderSummary == null)
                {
                    return Json(new { success = false, message = "Order summary not found" });
                }

                // Create a new contract
                var contract = new Contract
                {
                    OrderID = orderId, // Use the actual Order ID
                    ContractStatus = "ACTIVE",
                    ContractContent = orderSummary.ContractDetails ?? "Standard contract terms",
                    RenewalCount = 0,
                    AdditionalTerms = string.Empty
                };

                // Get the predefined contract type (assuming BorrowingContract for now)
                var predefinedContract = await _contractService.GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType.BorrowingContract);

                // Generate PDF content (empty for now, will be filled by the server)
                byte[] pdfContent = new byte[0];

                // Add the contract to the database
                var newContract = await _contractService.AddContractAsync(contract, pdfContent);

                // IMPORTANT: Include the generatedContractId in the response
                if (newContract != null)
                {
                    _logger.LogInformation($"Contract generated successfully with ID: {newContract.ContractID} for OrderID: {orderId}");
                    return Json(new { success = true, message = "Contract generated successfully!", generatedContractId = newContract.ContractID });
                }
                else
                {
                    _logger.LogError($"Contract service returned null after adding contract for OrderID: {orderId}");
                    return Json(new { success = false, message = "Error generating contract: Failed to retrieve new contract details after creation." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating contract");
                return Json(new { success = false, message = $"Error generating contract: {ex.Message}" });
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
using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;

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
        public async Task<IActionResult> OrderHistory(int userId)
        {
            try
            {
                var orders = await _orderService.GetOrdersWithProductInfoAsync(userId);
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
                
                // Get the current user's ID from the session or authentication
                int userId = 1; // TODO: Replace with actual user ID from session/auth

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
            
            var trackedOrder = await _trackedOrderService.GetTrackedOrderByIDAsync(orderId);
            if (trackedOrder == null)
            {
                _logger.LogWarning($"Tracked order not found for orderId: {orderId}");
                return NotFound();
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
                var trackedOrder = await _trackedOrderService.GetTrackedOrderByIDAsync(trackedOrderId);
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
    }
} 
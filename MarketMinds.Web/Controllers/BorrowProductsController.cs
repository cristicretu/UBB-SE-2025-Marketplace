using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebMarketplace.Models;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
    public class BorrowProductsController : Controller
    {
        private readonly ILogger<BorrowProductsController> _logger;
        private readonly IBorrowProductsService _borrowProductsService;
        private readonly IWaitlistService _waitlistService;
        private readonly IProductService _productService;
        private readonly INotificationService _notificationService;

        public BorrowProductsController(
            ILogger<BorrowProductsController> logger,
            IBorrowProductsService borrowProductsService,
            IWaitlistService waitlistService,
            IProductService productService,
            INotificationService notificationService)
        {
            _logger = logger;
            _borrowProductsService = borrowProductsService;
            _waitlistService = waitlistService ?? throw new ArgumentNullException(nameof(waitlistService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// Gets the current user ID (placeholder - would be replaced with actual authentication)
        /// </summary>
        /// <returns>The current user ID</returns>
        private int GetCurrentUserId()
        {
            // Use UserSession to get the current user ID if available
            // otherwise fallback to a default value for development purposes
            return UserSession.CurrentUserId ?? 1;
        }

        // GET: BorrowProducts
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching all borrow products");
                var borrowProducts = await _borrowProductsService.GetAllBorrowProductsAsync();
                return View(borrowProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching borrow products");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching borrow products");
                return View(new List<BorrowProduct>());
            }
        }

        // GET: BorrowProducts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching borrow product with ID {id}");
                var borrowProduct = await _borrowProductsService.GetBorrowProductByIdAsync(id);

                if (borrowProduct == null || borrowProduct.Id == 0)
                {
                    _logger.LogWarning($"Borrow product with ID {id} not found");
                    return NotFound();
                }

                return View(borrowProduct);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"Borrow product with ID {id} not found");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching borrow product {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: BorrowProducts/CalculatePrice
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CalculatePrice(int productId, DateTime endDate)
        {
            try
            {
                var borrowProduct = await _borrowProductsService.GetBorrowProductByIdAsync(productId);

                if (borrowProduct == null || borrowProduct.Id == 0)
                {
                    return Json(new { success = false, error = "Product not found" });
                }

                DateTime startDate = borrowProduct.StartDate ?? DateTime.Now;

                if (endDate < startDate)
                {
                    return Json(new { success = false, error = "End date cannot be before start date" });
                }

                if (endDate > borrowProduct.TimeLimit)
                {
                    return Json(new { success = false, error = "End date cannot exceed the time limit" });
                }

                int days = (int)Math.Ceiling((endDate - startDate).TotalDays);
                double totalPrice = days * borrowProduct.DailyRate;

                return Json(new
                {
                    success = true,
                    price = totalPrice,
                    formattedPrice = totalPrice.ToString("C"),
                    days = days
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating price for product {productId}");
                return Json(new { success = false, error = "An error occurred while calculating the price" });
            }
        }

        /// <summary>
        /// Joins the waitlist for a product
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>Redirects to the product details</returns>
        [HttpPost]
        public async Task<IActionResult> JoinWaitlist(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                await _waitlistService.AddUserToWaitlist(currentUserId, id);

                TempData["SuccessMessage"] = "You've joined the waitlist!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to join waitlist for product {ProductId}", id);
                TempData["ErrorMessage"] = $"Failed to join waitlist: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        /// <summary>
        /// Leaves the waitlist for a product
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>Redirects to the product details</returns>
        [HttpPost]
        public async Task<IActionResult> LeaveWaitlist(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                await _waitlistService.RemoveUserFromWaitlist(currentUserId, id);

                TempData["SuccessMessage"] = "You've left the waitlist";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to leave waitlist for product {ProductId}", id);
                TempData["ErrorMessage"] = $"Failed to leave waitlist: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        /// <summary>
        /// Shows the user's position in the waitlist
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>JSON with the position information</returns>
        [HttpGet]
        public async Task<IActionResult> GetWaitlistPosition(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                int position = await _waitlistService.GetUserWaitlistPosition(currentUserId, id);

                return Json(new { position });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get waitlist position for product {ProductId}", id);
                return Json(new { error = $"Failed to get waitlist position: {ex.Message}" });
            }
        }

        /// <summary>
        /// Shows the notifications page
        /// </summary>
        /// <returns>The notifications partial view</returns>
        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                var notifications = await _notificationService.GetUserNotificationsAsync(currentUserId);

                return PartialView("_Notifications", notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load notifications");
                return Json(new { error = $"Couldn't load notifications: {ex.Message}" });
            }
        }

        /// <summary>
        /// Gets notifications in JSON format for direct testing
        /// </summary>
        /// <returns>JSON notifications data</returns>
        [HttpGet]
        public async Task<IActionResult> NotificationsJson()
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                var notifications = await _notificationService.GetUserNotificationsAsync(currentUserId);

                return Json(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load notifications JSON");
                return Json(new { error = $"Couldn't load notifications: {ex.Message}" });
            }
        }

        // GET: BorrowProducts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BorrowProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBorrowProductDTO productDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var validationErrors = await _borrowProductsService.ValidateProductDTOAsync(productDTO);
                    if (validationErrors.Count > 0)
                    {
                        foreach (var error in validationErrors)
                        {
                            foreach (var message in error.Value)
                            {
                                ModelState.AddModelError(error.Key, message);
                            }
                        }
                        return View(productDTO);
                    }

                    var product = await _borrowProductsService.CreateProductAsync(productDTO);
                    if (product != null)
                    {
                        return RedirectToAction(nameof(Details), new { id = product.Id });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create the borrow product");
                        return View(productDTO);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating borrow product");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the borrow product");
                }
            }
            return View(productDTO);
        }

        // POST: BorrowProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BorrowProduct borrowProduct)
        {
            if (id != borrowProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _borrowProductsService.UpdateBorrowProductAsync(borrowProduct);
                    if (success)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update the borrow product");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating borrow product {id}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the borrow product");
                }
            }
            return View(borrowProduct);
        }

        // GET: BorrowProducts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var borrowProduct = await _borrowProductsService.GetBorrowProductByIdAsync(id);
                if (borrowProduct == null || borrowProduct.Id == 0)
                {
                    return NotFound();
                }
                return View(borrowProduct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching borrow product {id} for delete");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: BorrowProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _borrowProductsService.DeleteBorrowProductAsync(id);
                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to delete the borrow product");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting borrow product {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to calculate time left for borrowing
        [NonAction]
        public string GetTimeRemaining(DateTime? endDate)
        {
            if (!endDate.HasValue)
            {
                return "No end date specified";
            }

            var timeLeft = endDate.Value - DateTime.Now;

            if (timeLeft <= TimeSpan.Zero)
            {
                return "Borrowing period ended";
            }

            return $"{timeLeft.Days}d {timeLeft.Hours}h {timeLeft.Minutes}m";
        }
    }
}
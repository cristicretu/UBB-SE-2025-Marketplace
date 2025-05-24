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
        /// Joins the waitlist for a product with a specified end date preference
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <param name="endDate">The user's desired end date for borrowing</param>
        /// <returns>JSON result indicating success or failure</returns>
        [HttpPost]
        public async Task<IActionResult> JoinWaitlist(int id, DateTime endDate)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                // Validate the end date
                var borrowProduct = await _borrowProductsService.GetBorrowProductByIdAsync(id);
                if (borrowProduct == null || borrowProduct.Id == 0)
                {
                    return Json(new { success = false, error = "Product not found" });
                }

                // Check if end date is reasonable
                DateTime earliestStart = borrowProduct.EndDate ?? DateTime.Now;
                if (endDate <= earliestStart)
                {
                    return Json(new { success = false, error = "End date must be after the current borrowing period ends" });
                }

                if (endDate > borrowProduct.TimeLimit)
                {
                    return Json(new { success = false, error = "End date cannot exceed the product's time limit" });
                }

                // Store the end date preference in the waitlist
                await _waitlistService.AddUserToWaitlist(currentUserId, id, endDate);

                _logger.LogInformation($"User {currentUserId} joined waitlist for product {id} with preferred end date {endDate:yyyy-MM-dd}");

                return Json(new
                {
                    success = true,
                    message = $"You've joined the waitlist! When available, the product will be assigned to you until {endDate:yyyy-MM-dd}."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to join waitlist for product {ProductId}", id);
                return Json(new { success = false, error = $"Failed to join waitlist: {ex.Message}" });
            }
        }

        /// <summary>
        /// Leaves the waitlist for a product
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>JSON result indicating success or failure</returns>
        [HttpPost]
        public async Task<IActionResult> LeaveWaitlist(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                await _waitlistService.RemoveUserFromWaitlist(currentUserId, id);

                _logger.LogInformation($"User {currentUserId} left waitlist for product {id}");

                return Json(new
                {
                    success = true,
                    message = "You've successfully left the waitlist"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to leave waitlist for product {ProductId}", id);
                return Json(new { success = false, error = $"Failed to leave waitlist: {ex.Message}" });
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

        /// <summary>
        /// Borrows a product for the current user
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <param name="endDate">The desired end date for borrowing</param>
        /// <returns>JSON result indicating success or failure</returns>
        [HttpPost]
        public async Task<IActionResult> BorrowProduct(int id, DateTime endDate)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                _logger.LogInformation($"User {currentUserId} attempting to borrow product {id} until {endDate}");

                var borrowProduct = await _borrowProductsService.GetBorrowProductByIdAsync(id);

                if (borrowProduct == null || borrowProduct.Id == 0)
                {
                    return Json(new { success = false, error = "Product not found" });
                }

                // Check if product is already borrowed
                if (borrowProduct.IsBorrowed)
                {
                    return Json(new { success = false, error = "This product is currently borrowed by another user. Please join the waitlist." });
                }

                // Validate dates
                DateTime startDate = DateTime.Now;

                if (endDate < startDate.AddDays(1))
                {
                    return Json(new { success = false, error = "End date must be at least 1 day from now" });
                }

                if (endDate > borrowProduct.TimeLimit)
                {
                    return Json(new { success = false, error = "End date cannot exceed the product's time limit" });
                }

                // Calculate total price
                int days = (int)Math.Ceiling((endDate - startDate).TotalDays);
                double totalPrice = days * borrowProduct.DailyRate;

                // Update the product to mark it as borrowed
                borrowProduct.IsBorrowed = true;
                borrowProduct.BorrowerId = currentUserId;
                borrowProduct.StartDate = startDate;
                borrowProduct.EndDate = endDate;

                var success = await _borrowProductsService.UpdateBorrowProductAsync(borrowProduct);

                if (success)
                {
                    _logger.LogInformation($"Product {id} successfully borrowed by user {currentUserId}");
                    return Json(new
                    {
                        success = true,
                        message = "Product borrowed successfully!",
                        totalPrice = totalPrice.ToString("C"),
                        days = days,
                        endDate = endDate.ToString("yyyy-MM-dd")
                    });
                }
                else
                {
                    return Json(new { success = false, error = "Failed to update product status" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error borrowing product {id}");
                return Json(new { success = false, error = "An error occurred while borrowing the product" });
            }
        }

        /// <summary>
        /// Returns a borrowed product and moves it to the next person in waitlist if applicable
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>JSON result indicating success or failure</returns>
        [HttpPost]
        public async Task<IActionResult> ReturnProduct(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                _logger.LogInformation($"User {currentUserId} attempting to return product {id}");

                var borrowProduct = await _borrowProductsService.GetBorrowProductByIdAsync(id);

                if (borrowProduct == null || borrowProduct.Id == 0)
                {
                    return Json(new { success = false, error = "Product not found" });
                }

                // Check if the current user is the one who borrowed it
                if (!borrowProduct.IsBorrowed || borrowProduct.BorrowerId != currentUserId)
                {
                    return Json(new { success = false, error = "You are not currently borrowing this product" });
                }

                // Store the previous end date for history tracking (don't clear start/end dates)
                DateTime? previousEndDate = borrowProduct.EndDate;
                DateTime? previousStartDate = borrowProduct.StartDate;

                // Mark as returned but keep the borrowing history
                borrowProduct.IsBorrowed = false;
                borrowProduct.BorrowerId = null;
                // Don't clear start/end dates - keep them for history

                var success = await _borrowProductsService.UpdateBorrowProductAsync(borrowProduct);

                if (success)
                {
                    _logger.LogInformation($"Product {id} successfully returned by user {currentUserId}");

                    // Check if there's someone in waitlist before assignment
                    var waitlistUsers = await _waitlistService.GetUsersInWaitlist(id);
                    bool hasWaitlist = waitlistUsers != null && waitlistUsers.Count > 0;

                    // Automatically assign product to next person in waitlist
                    await NotifyNextInWaitlist(id);

                    string message = hasWaitlist
                        ? "Product returned successfully! It has been automatically assigned to the next person in the waitlist."
                        : "Product returned successfully! It is now available for borrowing.";

                    return Json(new
                    {
                        success = true,
                        message = message
                    });
                }
                else
                {
                    return Json(new { success = false, error = "Failed to update product status" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error returning product {id}");
                return Json(new { success = false, error = "An error occurred while returning the product" });
            }
        }

        /// <summary>
        /// Checks for expired borrowing periods and moves products to next in waitlist
        /// </summary>
        /// <returns>JSON result with update count</returns>
        [HttpPost]
        public async Task<IActionResult> ProcessExpiredBorrowings()
        {
            try
            {
                _logger.LogInformation("Processing expired borrowings");

                var allBorrowProducts = await _borrowProductsService.GetAllBorrowProductsAsync();
                int processedCount = 0;

                foreach (var product in allBorrowProducts.Where(p => p.IsBorrowed && p.EndDate.HasValue))
                {
                    if (DateTime.Now > product.EndDate.Value)
                    {
                        _logger.LogInformation($"Product {product.Id} borrowing period expired, returning automatically");

                        // Store the previous borrowing info for history (don't clear start/end dates)
                        DateTime? previousEndDate = product.EndDate;
                        DateTime? previousStartDate = product.StartDate;
                        int? previousBorrowerId = product.BorrowerId;

                        // Automatically return the product but keep borrowing history
                        product.IsBorrowed = false;
                        product.BorrowerId = null;
                        // Don't clear start/end dates - keep them for history

                        var success = await _borrowProductsService.UpdateBorrowProductAsync(product);
                        if (success)
                        {
                            processedCount++;
                            await NotifyNextInWaitlist(product.Id);
                        }
                    }
                }

                return Json(new { success = true, processedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired borrowings");
                return Json(new { success = false, error = "An error occurred while processing expired borrowings" });
            }
        }

        /// <summary>
        /// Automatically assigns the product to the next person in waitlist when it becomes available
        /// </summary>
        /// <param name="productId">The product ID</param>
        private async Task NotifyNextInWaitlist(int productId)
        {
            try
            {
                // Get the next person in waitlist
                var waitlistUsers = await _waitlistService.GetUsersInWaitlist(productId);

                if (waitlistUsers != null && waitlistUsers.Count > 0)
                {
                    var firstInLine = waitlistUsers.OrderBy(w => w.PositionInQueue).FirstOrDefault();
                    if (firstInLine != null)
                    {
                        _logger.LogInformation($"Automatically assigning product {productId} to user {firstInLine.UserID} (first in waitlist)");

                        // Get the product to assign
                        var borrowProduct = await _borrowProductsService.GetBorrowProductByIdAsync(productId);
                        if (borrowProduct != null)
                        {
                            // Calculate smart start date: 
                            // - If product was returned early, start from previous end date or now (whichever is later)
                            // - If product expired naturally, start from now
                            DateTime startDate = borrowProduct.EndDate ?? DateTime.Now;
                            if (startDate < DateTime.Now)
                            {
                                startDate = DateTime.Now; // Don't start in the past
                            }

                            // Use the user's preferred end date if available, otherwise default to 7 days
                            DateTime endDate;
                            if (firstInLine.PreferredEndDate.HasValue && firstInLine.PreferredEndDate.Value > startDate)
                            {
                                endDate = firstInLine.PreferredEndDate.Value;
                                _logger.LogInformation($"Using user's preferred end date: {endDate:yyyy-MM-dd}");
                            }
                            else
                            {
                                // Default to 7 days from start date if no preference or preference is invalid
                                endDate = startDate.AddDays(7);
                                _logger.LogInformation($"Using default 7-day period, end date: {endDate:yyyy-MM-dd}");
                            }

                            // Make sure end date doesn't exceed product time limit
                            if (endDate > borrowProduct.TimeLimit)
                            {
                                endDate = borrowProduct.TimeLimit;
                                _logger.LogInformation($"Adjusted end date to product time limit: {endDate:yyyy-MM-dd}");
                            }

                            // Make sure the borrowing period is at least 1 day
                            if (endDate <= startDate)
                            {
                                endDate = startDate.AddDays(1);
                                if (endDate > borrowProduct.TimeLimit)
                                {
                                    _logger.LogWarning($"Cannot assign product {productId} to user {firstInLine.UserID}: insufficient time remaining");
                                    return;
                                }
                            }

                            // Assign the product to the first person in waitlist
                            borrowProduct.IsBorrowed = true;
                            borrowProduct.BorrowerId = firstInLine.UserID;
                            borrowProduct.StartDate = startDate;
                            borrowProduct.EndDate = endDate;

                            var success = await _borrowProductsService.UpdateBorrowProductAsync(borrowProduct);
                            if (success)
                            {
                                // Remove the user from the waitlist since they now have the product
                                await _waitlistService.RemoveUserFromWaitlist(firstInLine.UserID, productId);

                                _logger.LogInformation($"Product {productId} successfully assigned to user {firstInLine.UserID}. Borrowing period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                                // Send notification to the user (optional)
                                try
                                {
                                    string notificationMessage = firstInLine.PreferredEndDate.HasValue
                                        ? $"Good news! The product you were waiting for is now assigned to you until your requested date {endDate:yyyy-MM-dd}."
                                        : $"Good news! The product you were waiting for is now assigned to you until {endDate:yyyy-MM-dd}.";

                                    await _notificationService.SendNotificationAsync(
                                        firstInLine.UserID,
                                        notificationMessage
                                    );
                                }
                                catch (Exception notifEx)
                                {
                                    // Don't fail the entire process if notification fails
                                    _logger.LogWarning(notifEx, $"Failed to send notification to user {firstInLine.UserID}");
                                }
                            }
                            else
                            {
                                _logger.LogError($"Failed to assign product {productId} to user {firstInLine.UserID}");
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"No users in waitlist for product {productId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning product {productId} to next in waitlist");
            }
        }
    }
}
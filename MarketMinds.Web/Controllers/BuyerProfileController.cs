using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WebMarketplace.Models;
using System.Net.Http;

namespace WebMarketplace.Controllers
{
    /// <summary>
    /// Controller for buyer profile operations with improved stability
    /// </summary>
    public class BuyerProfileController : Controller
    {
        private readonly IBuyerService _buyerService;
        private readonly IUserService _userService;
        private readonly ILogger<BuyerProfileController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerProfileController"/> class.
        /// </summary>
        /// <param name="buyerService">The buyer service.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="logger">The logger.</param>
        public BuyerProfileController(
            IBuyerService buyerService,
            IUserService userService,
            ILogger<BuyerProfileController> logger)
        {
            _buyerService = buyerService ?? throw new ArgumentNullException(nameof(buyerService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current user ID (placeholder - would be replaced with actual authentication)
        /// </summary>
        /// <returns>The current user ID</returns>
        private int GetCurrentUserId()
        {
            // This would be replaced with actual user authentication 
            return UserSession.CurrentUserId ?? 1;
        }

        /// <summary>
        /// Simple test endpoint to verify that the controller is working
        /// </summary>
        /// <returns>A simple text response</returns>
        public IActionResult Test()
        {
            return Content("BuyerProfileController is working!");
        }

        /// <summary>
        /// Displays the buyer profile page
        /// </summary>
        /// <returns>The buyer profile view</returns>
        public async Task<IActionResult> Index()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                int userId = GetCurrentUserId();
                _logger.LogInformation("Loading buyer profile for user ID: {UserId}", userId);

                // Create a basic User object instead of fetching all users
                // This avoids the expensive GetAllUsers() call that might be causing issues
                var user = new User(userId);
                _logger.LogInformation("Created user object with ID: {UserId}", user.UserId);

                // Add timeout to prevent hanging
                Buyer buyer =  await _buyerService.GetBuyerByUser(user);
          

                _logger.LogInformation("Found buyer: {BuyerId}, loading view model", buyer.Id);

                // Create the view model safely with null checks
                var viewModel = new BuyerProfileViewModel
                {
                    BuyerId = buyer.User?.UserId ?? 0,
                    FirstName = buyer.FirstName ?? string.Empty,
                    LastName = buyer.LastName ?? string.Empty,
                    Email = buyer.User?.Email ?? string.Empty,
                    PhoneNumber = buyer.User?.PhoneNumber ?? string.Empty,

                    // Billing address with null safety
                    BillingStreet = buyer.BillingAddress?.StreetLine ?? string.Empty,
                    BillingCity = buyer.BillingAddress?.City ?? string.Empty,
                    BillingCountry = buyer.BillingAddress?.Country ?? string.Empty,
                    BillingPostalCode = buyer.BillingAddress?.PostalCode ?? string.Empty,

                    // Shipping address with null safety
                    ShippingStreet = buyer.ShippingAddress?.StreetLine ?? string.Empty,
                    ShippingCity = buyer.ShippingAddress?.City ?? string.Empty,
                    ShippingCountry = buyer.ShippingAddress?.Country ?? string.Empty,
                    ShippingPostalCode = buyer.ShippingAddress?.PostalCode ?? string.Empty,

                    UseSameAddress = buyer.UseSameAddress,
                    Badge = buyer.Badge.ToString() ?? "None",
                    Discount = buyer.Discount
                };

                _logger.LogInformation("Profile loaded successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                return View(viewModel);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error loading buyer profile: {Message}", ex.Message);
                return Content($"Network error while loading your profile: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Operation timed out after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                return Content("The operation timed out. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading buyer profile: {Message}", ex.Message);
                return Content($"An error occurred: {ex.Message}. Please try again later.");
            }
        }

        /// <summary>
        /// Updates the buyer profile
        /// </summary>
        /// <param name="model">The buyer profile view model</param>
        /// <returns>Redirects to the updated buyer profile</returns>
        [HttpPost]
        public async Task<IActionResult> Update(BuyerProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index", model);
                }

                int userId = GetCurrentUserId();
                _logger.LogInformation("Updating profile for user ID: {UserId}", userId);

                // Create a basic User object instead of fetching all users
                var user = new User(userId);

                // Add timeout to prevent hanging
                var buyerTask = _buyerService.GetBuyerByUser(user);
                if (await Task.WhenAny(buyerTask, Task.Delay(5000)) != buyerTask)
                {
                    _logger.LogWarning("GetBuyerByUser operation timed out during update");
                    TempData["ErrorMessage"] = "Operation timed out while updating your profile. Please try again.";
                    return View("Index", model);
                }

                var buyer = await buyerTask;
                if (buyer == null)
                {
                    _logger.LogWarning("Update failed: No buyer found for user ID: {UserId}", userId);
                    return NotFound();
                }

                // Update buyer information
                buyer.FirstName = model.FirstName;
                buyer.LastName = model.LastName;
                buyer.User.PhoneNumber = model.PhoneNumber;
                buyer.UseSameAddress = model.UseSameAddress;

                // Update billing address
                if (buyer.BillingAddress == null)
                {
                    buyer.BillingAddress = new Address();
                }

                buyer.BillingAddress.StreetLine = model.BillingStreet;
                buyer.BillingAddress.City = model.BillingCity;
                buyer.BillingAddress.Country = model.BillingCountry;
                buyer.BillingAddress.PostalCode = model.BillingPostalCode;

                // Update shipping address (if not using same as billing)
                if (!model.UseSameAddress)
                {
                    if (buyer.ShippingAddress == null)
                    {
                        buyer.ShippingAddress = new Address();
                    }

                    buyer.ShippingAddress.StreetLine = model.ShippingStreet;
                    buyer.ShippingAddress.City = model.ShippingCity;
                    buyer.ShippingAddress.Country = model.ShippingCountry;
                    buyer.ShippingAddress.PostalCode = model.ShippingPostalCode;
                }
                else
                {
                    buyer.ShippingAddress = buyer.BillingAddress;
                }

                // Add timeout for save operation
                var saveTask = _buyerService.SaveInfo(buyer);
                if (await Task.WhenAny(saveTask, Task.Delay(5000)) != saveTask)
                {
                    _logger.LogWarning("SaveInfo operation timed out");
                    TempData["ErrorMessage"] = "Operation timed out while saving your profile. Please try again.";
                    return View("Index", model);
                }

                await saveTask;

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating buyer profile for user ID: {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = $"Failed to update profile: {ex.Message}";
                return View("Index", model);
            }
        }
    }
}

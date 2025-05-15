using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System;
using System.Threading.Tasks;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    /// <summary>
    /// Controller for Seller Profile functionality.
    /// </summary>
    public class SellerProfileController : Controller
    {
        private readonly ISellerService _sellerService;
        private readonly IUserService _userService;
        private readonly ILogger<SellerProfileController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerProfileController"/> class.
        /// </summary>
        /// <param name="sellerService">Service for seller-related operations.</param>
        /// <param name="userService">Service for user-related operations.</param>
        /// <param name="logger">Logger for the controller.</param>
        public SellerProfileController(
            ISellerService sellerService,
            IUserService userService,
            ILogger<SellerProfileController> logger)
        {
            _sellerService = sellerService ?? throw new ArgumentNullException(nameof(sellerService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current user ID.
        /// </summary>
        /// <returns>The current user ID.</returns>
        private int GetCurrentUserId()
        {
            // For testing purposes until authentication is implemented
            return UserSession.CurrentUserId ?? 1;
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns>A task containing the current user.</returns>
        private async Task<User> GetCurrentUser()
        {
            int userId = GetCurrentUserId();

            // For testing purposes, we'll use a placeholder method
            var users = await _userService.GetAllUsers();
            return users.FirstOrDefault(u => u.UserId == userId) ?? new User(userId, role: UserRole.Seller); // Default to Seller role
        }

        /// <summary>
        /// Displays the Seller Profile page.
        /// </summary>
        /// <returns>The view.</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading Seller Profile page");

                var user = await GetCurrentUser();

                // Only allow seller role users to access this page
                // Please modify here later on!!
                //if (user.Role != UserRole.Seller)
                //{
                //    return RedirectToAction("Index", "Home");
                //}

                var viewModel = new SellerProfileViewModel(user, _userService, _sellerService);
                await viewModel.InitializeAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Seller Profile page");
                return View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// Filters products based on search text.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <returns>A partial view of filtered products.</returns>
        [HttpPost]
        public async Task<IActionResult> FilterProducts(string searchText)
        {
            try
            {
                var user = await GetCurrentUser();
                var viewModel = new SellerProfileViewModel(user, _userService, _sellerService);
                await viewModel.InitializeAsync();

                viewModel.FilterProducts(searchText);

                return PartialView("_ProductsListPartial", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering products");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Sorts products by price.
        /// </summary>
        /// <returns>A partial view of sorted products.</returns>
        [HttpPost]
        public async Task<IActionResult> SortProducts()
        {
            try
            {
                var user = await GetCurrentUser();
                var viewModel = new SellerProfileViewModel(user, _userService, _sellerService);
                await viewModel.InitializeAsync();

                viewModel.SortProducts();

                return PartialView("_ProductsListPartial", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sorting products");
                return Json(new { success = false, message = ex.Message });
            }
        }


        /// <summary>
        /// Displays the form to update the seller profile.
        /// </summary>
        /// <returns>The update profile view.</returns>
        public async Task<IActionResult> UpdateProfile()
        {
            try
            {
                _logger.LogInformation("Loading Update Profile page");

                var user = await GetCurrentUser();
                var viewModel = new SellerProfileViewModel(user, _userService, _sellerService);
                await viewModel.InitializeAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Update Profile page");
                return View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// Handles the submission of the update profile form.
        /// </summary>
        /// <param name="viewModel">The updated seller profile data.</param>
        /// <returns>A redirect to the seller profile page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(SellerProfileViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await viewModel.UpdateSellerProfileAsync();
                    return RedirectToAction("Index");
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating seller profile");
                ModelState.AddModelError(string.Empty, "An error occurred while updating your profile.");
                return View(viewModel);
            }
        }
    }
}

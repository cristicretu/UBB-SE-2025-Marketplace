using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System;
using System.Threading.Tasks;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    /// <summary>
    /// Controller for My Market related functionality.
    /// </summary>
    public class MyMarketController : Controller
    {
        private readonly IBuyerService _buyerService;
        private readonly IUserService _userService;
        private readonly ILogger<MyMarketController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyMarketController"/> class.
        /// </summary>
        /// <param name="buyerService">Service for buyer-related operations.</param>
        /// <param name="userService">Service for user-related operations.</param>
        /// <param name="logger">Logger for the controller.</param>
        public MyMarketController(
            IBuyerService buyerService,
            IUserService userService,
            ILogger<MyMarketController> logger)
        {
            _buyerService = buyerService ?? throw new ArgumentNullException(nameof(buyerService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current user ID.
        /// </summary>
        /// <returns>The current user ID.</returns>
        private int GetCurrentUserId()
        {
            // For testing purposes until authentification is implemented
            return UserSession.CurrentUserId ?? 1;
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns>A task containing the current user.</returns>
        private async Task<User> GetCurrentUser()
        {
            int userId = GetCurrentUserId();
            // In a real application, this would be derived from the authentication system
            // For testing purposes, we'll use a placeholder method

            // This would need to be implemented in your UserService
            // Placeholder for now
            var users = await _userService.GetAllUsers();
            return users.FirstOrDefault(u => u.UserId == userId) ?? new User(userId);
        }

        /// <summary>
        /// Displays the My Market page.
        /// </summary>
        /// <returns>The view.</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading My Market page");

                var user = await GetCurrentUser();
                var viewModel = new MyMarketViewModel(_buyerService, user);
                await viewModel.InitializeAsync();

                return View("~/Views/MyMarket/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading My Market page");
                return View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// Updates the visibility of the followers list.
        /// </summary>
        /// <returns>A partial view of the followers list.</returns>
        [HttpPost]
        public async Task<IActionResult> ToggleFollowingList()
        {
            try
            {
                var user = await GetCurrentUser();
                var viewModel = new MyMarketViewModel(_buyerService, user);
                await viewModel.InitializeAsync();

                viewModel.ToggleFollowingListVisibility();

                return PartialView("_FollowingListPartial", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling following list");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Filters the products list based on search term.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <returns>A partial view of the products list.</returns>
        [HttpPost]
        public async Task<IActionResult> FilterProducts(string searchTerm)
        {
            try
            {
                var user = await GetCurrentUser();
                var viewModel = new MyMarketViewModel(_buyerService, user);
                await viewModel.InitializeAsync();

                viewModel.FilterProducts(searchTerm);

                return PartialView("_ProductsListPartial", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering products");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Filters the followed sellers list based on search term.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <returns>A partial view of the followed sellers list.</returns>
        [HttpPost]
        public async Task<IActionResult> FilterFollowing(string searchTerm)
        {
            try
            {
                var user = await GetCurrentUser();
                var viewModel = new MyMarketViewModel(_buyerService, user);
                await viewModel.InitializeAsync();

                viewModel.FilterFollowing(searchTerm);

                return PartialView("_FollowingListPartial", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering followed sellers");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Filters the all sellers list based on search term.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <returns>A partial view of the all sellers list.</returns>
        [HttpPost]
        public async Task<IActionResult> FilterAllSellers(string searchTerm)
        {
            try
            {
                var user = await GetCurrentUser();
                var viewModel = new MyMarketViewModel(_buyerService, user);
                await viewModel.InitializeAsync();

                viewModel.FilterAllSellers(searchTerm);

                return PartialView("_AllSellersListPartial", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering all sellers");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Displays the profile of a seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller to display.</param>
        /// <returns>The view.</returns>
        public async Task<IActionResult> Profile(int sellerId)
        {
            try
            {
                _logger.LogInformation("Loading seller profile: {SellerId}", sellerId);

                var user = await GetCurrentUser();
                var buyer = await _buyerService.GetBuyerByUser(user);

                var sellers = await _buyerService.GetAllSellers();
                var seller = sellers.FirstOrDefault(s => s.Id == sellerId);

                if (seller == null)
                {
                    _logger.LogWarning("Seller with ID {SellerId} not found", sellerId);
                    return NotFound();
                }

                var viewModel = new MyMarketProfileViewModel(_buyerService, buyer, seller);
                await viewModel.InitializeAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading seller profile");
                return View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// Filters the seller's products based on search term.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <returns>A partial view of the seller's products.</returns>
        [HttpPost]
        public async Task<IActionResult> FilterSellerProducts(int sellerId, string searchTerm)
        {
            try
            {
                var user = await GetCurrentUser();
                var buyer = await _buyerService.GetBuyerByUser(user);

                var sellers = await _buyerService.GetAllSellers();
                var seller = sellers.FirstOrDefault(s => s.Id == sellerId);

                if (seller == null)
                {
                    return NotFound();
                }

                var viewModel = new MyMarketProfileViewModel(_buyerService, buyer, seller);
                await viewModel.InitializeAsync();

                viewModel.FilterProducts(searchTerm);

                return PartialView("_SellerProductsPartial", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering seller products");
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        /// <summary>
        /// Toggles following status for a seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>The updated following status.</returns>
        [HttpPost]
        public async Task<IActionResult> ToggleFollow(int sellerId)
        {
            try
            {
                var user = await GetCurrentUser();
                var buyer = await _buyerService.GetBuyerByUser(user);

                var sellers = await _buyerService.GetAllSellers();
                var seller = sellers.FirstOrDefault(s => s.Id == sellerId);

                if (seller == null)
                {
                    return NotFound();
                }

                var viewModel = new MyMarketProfileViewModel(_buyerService, buyer, seller);
                await viewModel.InitializeAsync();
                await viewModel.ToggleFollowAsync();

                return Json(new { success = true, isFollowing = viewModel.IsFollowing });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling follow status");
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using WebMarketplace.Models;
using MarketMinds.Shared.Services.UserService;
using MarketMinds.Web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.BuyProductsService;

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
        private readonly IAuctionProductService _auctionProductService;
        private readonly IBorrowProductsService _borrowProductsService;
        private readonly IBuyProductsService _buyProductsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerProfileController"/> class.
        /// </summary>
        /// <param name="sellerService">Service for seller-related operations.</param>
        /// <param name="userService">Service for user-related operations.</param>
        /// <param name="logger">Logger for the controller.</param>
        /// <param name="auctionProductService">Service for auction products.</param>
        /// <param name="borrowProductsService">Service for borrow products.</param>
        /// <param name="buyProductsService">Service for buy products.</param>
        public SellerProfileController(
            ISellerService sellerService,
            IUserService userService,
            ILogger<SellerProfileController> logger,
            IAuctionProductService auctionProductService,
            IBorrowProductsService borrowProductsService,
            IBuyProductsService buyProductsService)
        {
            _sellerService = sellerService ?? throw new ArgumentNullException(nameof(sellerService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auctionProductService = auctionProductService ?? throw new ArgumentNullException(nameof(auctionProductService));
            _borrowProductsService = borrowProductsService ?? throw new ArgumentNullException(nameof(borrowProductsService));
            _buyProductsService = buyProductsService ?? throw new ArgumentNullException(nameof(buyProductsService));
        }

        /// <summary>
        /// Gets the current user ID from authentication claims.
        /// </summary>
        /// <returns>The current user ID.</returns>
        private int GetCurrentUserId()
        {
            // Get the user ID from claims (proper authentication approach)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogInformation("Got user ID {UserId} from claims", userId);
                return userId;
            }

            // Try custom claim as fallback
            var customIdClaim = User.FindFirst("UserId");
            if (customIdClaim != null && int.TryParse(customIdClaim.Value, out int customUserId))
            {
                _logger.LogInformation("Got user ID {UserId} from custom claim", customUserId);
                return customUserId;
            }

            // Fallback to UserSession (for backward compatibility)
            if (UserSession.CurrentUserId.HasValue)
            {
                _logger.LogWarning("Falling back to UserSession.CurrentUserId: {UserId}", UserSession.CurrentUserId.Value);
                return UserSession.CurrentUserId.Value;
            }

            // If no authentication found, return 0 to indicate unauthorized
            _logger.LogWarning("No valid user ID found in claims or session");
            return 0;
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
            return users.FirstOrDefault(u => u.Id == userId) ?? new User(userId, userType: (int)UserRole.Seller); // Default to Seller role
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <param name="userId">The user ID to look up.</param>
        /// <returns>A task containing the user with the specified ID.</returns>
        private async Task<User> GetUserById(int userId)
        {
            var users = await _userService.GetAllUsers();
            return users.FirstOrDefault(u => u.Id == userId);
        }

        /// <summary>
        /// Redirects to the Manage action for the seller's editable profile.
        /// The default /SellerProfile route now redirects to /SellerProfile/Manage.
        /// </summary>
        /// <returns>Redirect to Manage action.</returns>
        public IActionResult Index()
        {
            _logger.LogInformation("Redirecting from Index to Manage action");
            return RedirectToAction("Manage");
        }

        /// <summary>
        /// Displays a public seller profile page accessible to all users.
        /// This includes a "view as" feature where sellers can see their own profile as others see it.
        /// </summary>
        /// <param name="id">The seller's user ID.</param>
        /// <returns>The view.</returns>
        [Route("SellerProfile/{id:int}")]
        public async Task<IActionResult> PublicProfile(int id)
        {
            try
            {
                _logger.LogInformation("Loading public Seller Profile page for seller ID: {SellerId}", id);

                // Get the target seller's user information
                var targetUser = await GetUserById(id);
                if (targetUser == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound("Seller not found.");
                }

                // Check if the target user is actually a seller
                if (targetUser.UserType != (int)UserRole.Seller)
                {
                    _logger.LogWarning("User with ID {UserId} is not a seller (UserType: {UserType})", id, targetUser.UserType);
                    return NotFound("This user is not a seller.");
                }

                var viewModel = new SellerProfileViewModel(targetUser, _userService, _sellerService, 
                    _auctionProductService, _borrowProductsService, _buyProductsService);
                await viewModel.InitializeAsync();

                // Set flag to indicate this is a public view (cannot edit)
                // Even if the seller is viewing their own profile, it's read-only in public view
                ViewBag.IsOwnProfile = false;

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public Seller Profile page for seller ID: {SellerId}", id);
                return View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// Filters products based on search text.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="sellerId">Optional seller ID for public profiles.</param>
        /// <param name="isManageMode">Whether we're in manage/edit mode.</param>
        /// <returns>A partial view of filtered products.</returns>
        [HttpPost]
        public async Task<IActionResult> FilterProducts(string searchText, int? sellerId = null, bool isManageMode = false)
        {
            try
            {
                User user;
                bool isOwnProfile;
                
                if (sellerId.HasValue)
                {
                    // Public profile - get the target seller's user
                    user = await GetUserById(sellerId.Value);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "Seller not found" });
                    }
                    isOwnProfile = false; // Public profile - no edit buttons
                }
                else
                {
                    // Could be either Index (view-only) or Manage (editable)
                    user = await GetCurrentUser();
                    isOwnProfile = isManageMode; // Only show edit buttons in manage mode
                }

                var viewModel = new SellerProfileViewModel(user, _userService, _sellerService, 
                    _auctionProductService, _borrowProductsService, _buyProductsService);
                await viewModel.InitializeAsync();

                viewModel.FilterProducts(searchText);

                // Set the ViewBag for the partial view
                ViewBag.IsOwnProfile = isOwnProfile;

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
        /// <param name="sellerId">Optional seller ID for public profiles.</param>
        /// <param name="isManageMode">Whether we're in manage/edit mode.</param>
        /// <returns>A partial view of sorted products.</returns>
        [HttpPost]
        public async Task<IActionResult> SortProducts(int? sellerId = null, bool isManageMode = false)
        {
            try
            {
                User user;
                bool isOwnProfile;
                
                if (sellerId.HasValue)
                {
                    // Public profile - get the target seller's user
                    user = await GetUserById(sellerId.Value);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "Seller not found" });
                    }
                    isOwnProfile = false; // Public profile - no edit buttons
                }
                else
                {
                    // Could be either Index (view-only) or Manage (editable)
                    user = await GetCurrentUser();
                    isOwnProfile = isManageMode; // Only show edit buttons in manage mode
                }

                var viewModel = new SellerProfileViewModel(user, _userService, _sellerService, 
                    _auctionProductService, _borrowProductsService, _buyProductsService);
                await viewModel.InitializeAsync();

                viewModel.SortProducts();

                // Set the ViewBag for the partial view
                ViewBag.IsOwnProfile = isOwnProfile;

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
                var viewModel = new SellerProfileViewModel(user, _userService, _sellerService, 
                    _auctionProductService, _borrowProductsService, _buyProductsService);
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

        /// <summary>
        /// Displays the private editable Seller Profile page (seller's own profile management).
        /// </summary>
        /// <returns>The view.</returns>
        [Authorize]
        public async Task<IActionResult> Manage()
        {
            try
            {
                _logger.LogInformation("Loading private editable Seller Profile page");

                int userId = GetCurrentUserId();
                
                // Check if user is authenticated
                if (userId == 0)
                {
                    _logger.LogWarning("User not authenticated, redirecting to login");
                    return RedirectToAction("Login", "Account");
                }

                var user = await GetCurrentUser();

                // Only allow seller role users to access this page
                // Please modify here later on!!
                //if (user.Role != UserRole.Seller)
                //{
                //    return RedirectToAction("Index", "Home");
                //}

                var viewModel = new SellerProfileViewModel(user, _userService, _sellerService, 
                    _auctionProductService, _borrowProductsService, _buyProductsService);
                await viewModel.InitializeAsync();

                // Set flag to indicate this is the seller's own profile (can edit)
                ViewBag.IsOwnProfile = true;

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading private editable Seller Profile page");
                return View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}

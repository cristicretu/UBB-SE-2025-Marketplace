using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ReviewService;
using MarketMinds.Shared.Services.UserService;
using MarketMinds.Shared.Services.ImagineUploadService;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MarketMinds.Web.Controllers
{
    [Authorize] // Re-enabled to enforce authentication
    [Route("Reviews")]
    public class ReviewsController : Controller
    {
        private readonly ILogger<ReviewsController> _logger;
        private readonly IReviewsService _reviewsService;
        private readonly IUserService _userService;
        private readonly IImageUploadService _imageUploadService;

        public ReviewsController(
            ILogger<ReviewsController> logger,
            IReviewsService reviewsService,
            IUserService userService,
            IImageUploadService imageUploadService)
        {
            _logger = logger;
            _reviewsService = reviewsService;
            _userService = userService;
            _imageUploadService = imageUploadService;
        }

        // GET: Reviews/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                User currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var reviews = _reviewsService.GetReviewsBySeller(currentUser);
                return View(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews");
                return View("Error");
            }
        }

        // GET: Reviews/ReviewsReceived
        [HttpGet("ReviewsReceived")]
        public async Task<IActionResult> ReviewsReceived()
        {
            try
            {
                User currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var reviews = _reviewsService.GetReviewsBySeller(currentUser);
                return View(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews received");
                return View("Error");
            }
        }

        // GET: Reviews/ReviewsGiven
        [HttpGet("ReviewsGiven")]
        public async Task<IActionResult> ReviewsGiven()
        {
            try
            {
                User currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var reviews = _reviewsService.GetReviewsByBuyer(currentUser);
                return View(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews given");
                return View("Error");
            }
        }

        // GET: Reviews/Create/{sellerId:int}
        // [Authorize] // Temporarily commented out for testing
        [HttpGet("Create/{sellerId:int}")]
        public async Task<IActionResult> Create(int sellerId)
        {
            _logger.LogInformation("GET /Reviews/Create/{SellerId} action entered", sellerId);
            _logger.LogInformation("Create method called with seller ID: {SellerId}", sellerId);
            try
            {
                if (sellerId <= 0)
                {
                    _logger.LogWarning("Invalid seller ID provided: {SellerId}", sellerId);
                    return BadRequest("Invalid seller ID");
                }

                var seller = await _userService.GetUserByIdAsync(sellerId);
                if (seller == null)
                {
                    _logger.LogWarning("Seller not found with ID: {SellerId}", sellerId);
                    return NotFound($"Seller with ID {sellerId} not found");
                }

                // Check if user is authenticated
                User currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    _logger.LogWarning("User not authenticated when trying to create review");
                    return RedirectToAction("Login", "Account");
                }

                // Check if user has already reviewed this seller
                var existingReviews = _reviewsService.GetReviewsByBuyer(currentUser);
                if (existingReviews.Any(r => r.SellerId == sellerId))
                {
                    _logger.LogWarning("User {UserId} has already reviewed seller {SellerId}", currentUser.Id, sellerId);
                    return BadRequest("You have already reviewed this seller");
                }

                _logger.LogInformation("Preparing review creation view for seller {SellerId}", sellerId);
                ViewBag.Seller = seller;
                return View("Create", new Review { SellerId = sellerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing review creation for seller {SellerId}", sellerId);
                return View("Error");
            }
        }

        // POST: Reviews/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review, string imageUrls)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User currentUser = await GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    var seller = await _userService.GetUserByIdAsync(review.SellerId);
                    if (seller == null)
                    {
                        return NotFound();
                    }

                    // Process image URLs if provided
                    var reviewImages = new List<Image>();
                    if (!string.IsNullOrEmpty(imageUrls))
                    {
                        reviewImages = _imageUploadService.ParseImagesString(imageUrls);
                    }

                    // Validate rating range
                    if (review.Rating < 0 || review.Rating > 5)
                    {
                        ModelState.AddModelError("Rating", "Rating must be between 0 and 5");
                        ViewBag.Seller = seller;
                        return View(review);
                    }

                    try {
                        _reviewsService.AddReview(
                            review.Description,
                            reviewImages,
                            review.Rating,
                            seller,
                            currentUser);
                        
                        _logger.LogInformation($"Review created successfully for seller: {seller.Id} by buyer: {currentUser.Id}");
                        return RedirectToAction(nameof(ReviewsGiven));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error creating review: {ex.Message}");
                        ModelState.AddModelError("", $"Error creating review: {ex.Message}");
                        ViewBag.Seller = seller;
                        return View(review);
                    }
                }

                var sellerForView = await _userService.GetUserByIdAsync(review.SellerId);
                ViewBag.Seller = sellerForView;
                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return View("Error");
            }
        }

        // GET: Reviews/Edit/5
        [HttpGet("Edit/{id:int}/{sellerId:int}/{buyerId:int}")]
        public async Task<IActionResult> Edit(int id, int sellerId, int buyerId)
        {
            try
            {
                if (id <= 0 || sellerId <= 0 || buyerId <= 0)
                {
                    _logger.LogWarning("Invalid parameters provided for Edit: id={Id}, sellerId={SellerId}, buyerId={BuyerId}", id, sellerId, buyerId);
                    return BadRequest("Invalid parameters");
                }

                User currentUser = await GetCurrentUserAsync();
                if (currentUser == null || currentUser.Id != buyerId)
                {
                    _logger.LogWarning("Unauthorized edit attempt: currentUser={CurrentUser}, buyerId={BuyerId}", currentUser?.Id, buyerId);
                    return RedirectToAction("Login", "Account");
                }

                var reviews = _reviewsService.GetReviewsByBuyer(currentUser);
                var review = reviews.FirstOrDefault(r => r.SellerId == sellerId && r.BuyerId == buyerId);

                if (review == null)
                {
                    _logger.LogWarning("Review not found: sellerId={SellerId}, buyerId={BuyerId}", sellerId, buyerId);
                    return NotFound("Review not found");
                }

                var seller = await _userService.GetUserByIdAsync(review.SellerId);
                if (seller == null)
                {
                    _logger.LogWarning("Seller not found for review: sellerId={SellerId}", review.SellerId);
                    return NotFound("Seller not found");
                }

                ViewBag.Seller = seller;
                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review for editing: id={Id}, sellerId={SellerId}, buyerId={BuyerId}", id, sellerId, buyerId);
                return View("Error");
            }
        }

        // POST: Reviews/Edit/{id}/{sellerId}/{buyerId}
        [HttpPost("Edit/{id:int}/{sellerId:int}/{buyerId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int sellerId, int buyerId, Review review, string imageUrls)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User currentUser = await GetCurrentUserAsync();
                    if (currentUser == null || currentUser.Id != buyerId)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    // Get the current review first
                    var currentReview = _reviewsService.GetReviewsByBuyer(currentUser)
                        .FirstOrDefault(r => r.SellerId == review.SellerId && r.BuyerId == review.BuyerId);

                    if (currentReview == null)
                    {
                        ModelState.AddModelError("", "Review not found");
                        var sellerData = await _userService.GetUserByIdAsync(review.SellerId);
                        ViewBag.Seller = sellerData;
                        return View(review);
                    }

                    _reviewsService.EditReview(
                        currentReview.Description,
                        currentReview.Images.ToList(),
                        currentReview.Rating,
                        currentReview.SellerId,
                        currentReview.BuyerId,
                        review.Description,
                        review.Rating);

                    return RedirectToAction(nameof(ReviewsGiven));
                }

                var seller = await _userService.GetUserByIdAsync(review.SellerId);
                ViewBag.Seller = seller;
                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing review");
                return View("Error");
            }
        }

        // GET: Reviews/Delete/5
        [HttpGet("Delete/{id:int}/{sellerId:int}/{buyerId:int}")]
        public async Task<IActionResult> Delete(int id, int sellerId, int buyerId)
        {
            try
            {
                if (id <= 0 || sellerId <= 0 || buyerId <= 0)
                {
                    _logger.LogWarning("Invalid parameters provided for Delete: id={Id}, sellerId={SellerId}, buyerId={BuyerId}", id, sellerId, buyerId);
                    return BadRequest("Invalid parameters");
                }

                User currentUser = await GetCurrentUserAsync();
                if (currentUser == null || currentUser.Id != buyerId)
                {
                    _logger.LogWarning("Unauthorized delete attempt: currentUser={CurrentUser}, buyerId={BuyerId}", currentUser?.Id, buyerId);
                    return RedirectToAction("Login", "Account");
                }

                var reviews = _reviewsService.GetReviewsByBuyer(currentUser);
                var review = reviews.FirstOrDefault(r => r.SellerId == sellerId && r.BuyerId == buyerId);

                if (review == null)
                {
                    _logger.LogWarning("Review not found for deletion: sellerId={SellerId}, buyerId={BuyerId}", sellerId, buyerId);
                    return NotFound("Review not found");
                }

                // Get seller and buyer information
                var seller = await _userService.GetUserByIdAsync(sellerId);
                var buyer = await _userService.GetUserByIdAsync(buyerId);

                if (seller != null)
                {
                    review.SellerUsername = seller.Username;
                }

                if (buyer != null)
                {
                    review.BuyerUsername = buyer.Username;
                }

                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review for deletion: id={Id}, sellerId={SellerId}, buyerId={BuyerId}", id, sellerId, buyerId);
                return View("Error");
            }
        }

        // POST: Reviews/DeleteConfirmed
        [HttpPost("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int sellerId, int buyerId)
        {
            try
            {
                User currentUser = await GetCurrentUserAsync();
                if (currentUser == null || currentUser.Id != buyerId)
                {
                    return RedirectToAction("Login", "Account");
                }

                var reviews = _reviewsService.GetReviewsByBuyer(currentUser);
                var review = reviews.FirstOrDefault(r => r.SellerId == sellerId && r.BuyerId == buyerId);

                if (review == null)
                {
                    return NotFound();
                }

                _reviewsService.DeleteReview(
                    review.Description,
                    review.Images.ToList(),
                    review.Rating,
                    review.SellerId,
                    review.BuyerId);

                return RedirectToAction(nameof(ReviewsGiven));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review");
                return View("Error");
            }
        }

        // Helper method to get the current user
        private async Task<User> GetCurrentUserAsync()
        {
            // Get the user ID from claims
            int userId;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out userId))
            {
                // User ID found in the standard claim
                _logger.LogInformation($"Got user ID {userId} from standard claim");
            }
            else
            {
                // Try to get the user ID from a custom claim if needed
                var customIdClaim = User.FindFirst("UserId");
                if (customIdClaim != null && int.TryParse(customIdClaim.Value, out userId))
                {
                    _logger.LogInformation($"Got user ID {userId} from custom claim");
                }
                else
                {
                    // No valid user ID found in claims
                    _logger.LogWarning("No valid user ID found in claims");
                    return null;
                }
            }
            
            // Get the user from the service
            return await _userService.GetUserByIdAsync(userId);
        }
    }
} 
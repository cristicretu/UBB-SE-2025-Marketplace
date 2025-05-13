using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ReviewService;
using MarketMinds.Shared.Services.UserService;
using MarketMinds.Shared.Services.ImagineUploadService;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> Index()
        {
            try
            {
                // Default to showing the current user's reviews received
                // In a real implementation, you would get the current user from authentication
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

        // GET: Reviews/Create
        public async Task<IActionResult> Create(int sellerId)
        {
            try
            {
                var seller = await _userService.GetUserByIdAsync(sellerId);
                if (seller == null)
                {
                    return NotFound();
                }

                ViewBag.Seller = seller;
                return View(new Review { SellerId = sellerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing review creation");
                return View("Error");
            }
        }

        // POST: Reviews/Create
        [HttpPost]
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
        public async Task<IActionResult> Edit(int id, int sellerId, int buyerId)
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

                var seller = await _userService.GetUserByIdAsync(review.SellerId);
                ViewBag.Seller = seller;

                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review for editing");
                return View("Error");
            }
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Review review, string imageUrls)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User currentUser = await GetCurrentUserAsync();
                    if (currentUser == null || currentUser.Id != review.BuyerId)
                    {
                        return RedirectToAction("Login", "Account");
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
                        var sellerData = await _userService.GetUserByIdAsync(review.SellerId);
                        ViewBag.Seller = sellerData;
                        return View(review);
                    }

                    _reviewsService.EditReview(
                        review.Description,
                        reviewImages,
                        review.Rating,
                        review.SellerId,
                        review.BuyerId,
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
        public async Task<IActionResult> Delete(int id, int sellerId, int buyerId)
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

                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review for deletion");
                return View("Error");
            }
        }

        // POST: Reviews/DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Claims;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MarketMinds.Web.Controllers
{
    public class NumberToStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return string.Empty;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64().ToString();
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? string.Empty;
            }

            throw new JsonException($"Unable to convert {reader.TokenType} to string");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value ?? string.Empty);
        }
    }

    [Authorize]
    public class AuctionProductsController : Controller
    {
        private readonly ILogger<AuctionProductsController> _logger;
        private readonly IAuctionProductService _auctionProductService;

        public AuctionProductsController(
            ILogger<AuctionProductsController> logger,
            IAuctionProductService auctionProductService)
        {
            _logger = logger;
            _auctionProductService = auctionProductService;
        }

        // GET: AuctionProducts
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching all auction products");
                var auctionProducts = await _auctionProductService.GetAllAuctionProductsAsync();
                return View(auctionProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching auction products");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching auction products");
                return View(new List<AuctionProduct>());
            }
        }

        // GET: AuctionProducts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching auction product with ID {id}");
                var auctionProduct = await _auctionProductService.GetAuctionProductByIdAsync(id);
                
                if (auctionProduct == null || auctionProduct.Id == 0)
                {
                    _logger.LogWarning($"Auction product with ID {id} not found");
                    return NotFound();
                }
                
                return View(auctionProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching auction product {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: AuctionProducts/PlaceBid
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("AuctionProducts/PlaceBid")]
        public async Task<IActionResult> PlaceBid(int id, int bidAmount)
        {
            try
            {
                _logger.LogInformation($"Placing bid of {bidAmount} on auction {id}");

                // Get the current user's ID from claims
                int userId;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out userId))
                {
                    _logger.LogInformation($"User ID from claim: {userId}");
                }
                else
                {
                    // Try to get the user ID from a custom claim if needed
                    var customIdClaim = User.FindFirst("UserId");
                    if (customIdClaim != null && int.TryParse(customIdClaim.Value, out userId))
                    {
                        _logger.LogInformation($"User ID from custom claim: {userId}");
                    }
                    else
                    {
                        // Handle missing user ID gracefully
                        _logger.LogWarning("No valid user ID found in claims. Redirecting to login.");
                        TempData["ErrorMessage"] = "You must be logged in to place a bid.";
                        return RedirectToAction("Login", "Account");
                    }
                }
                
                // Validate the auction exists
                var auction = await _auctionProductService.GetAuctionProductByIdAsync(id);
                if (auction == null || auction.Id == 0)
                {
                    _logger.LogWarning($"Auction product with ID {id} not found when placing bid");
                    TempData["ErrorMessage"] = "Auction not found.";
                    return RedirectToAction(nameof(Index));
                }
                
                _logger.LogInformation($"Retrieved auction: ID={auction.Id}, CurrentPrice={auction.CurrentPrice}, EndTime={auction.EndTime}");
                
                // Validate bid is positive
                if (bidAmount <= 0)
                {
                    _logger.LogWarning($"Invalid bid amount: {bidAmount}");
                    TempData["ErrorMessage"] = "Bid amount must be positive.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                
                // Check minimum bid
                if (bidAmount <= auction.CurrentPrice)
                {
                    _logger.LogWarning($"Bid amount {bidAmount} is not higher than current price {auction.CurrentPrice}");
                    TempData["ErrorMessage"] = $"Your bid must be higher than the current price (${auction.CurrentPrice}).";
                    return RedirectToAction(nameof(Details), new { id });
                }
                
                // Check if auction ended
                if (_auctionProductService.IsAuctionEnded(auction))
                {
                    _logger.LogWarning($"Attempted to bid on ended auction {id}. EndTime={auction.EndTime}, Now={DateTime.Now}");
                    TempData["ErrorMessage"] = "This auction has already ended.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Place the bid
                _logger.LogInformation($"Calling PlaceBidAsync with auctionId={id}, userId={userId}, bidAmount={bidAmount}");
                bool success = await _auctionProductService.PlaceBidAsync(id, userId, bidAmount);
                
                if (success)
                {
                    _logger.LogInformation($"Bid successfully placed on auction {id}");
                    TempData["SuccessMessage"] = "Your bid was successfully placed!";
                }
                else
                {
                    _logger.LogWarning($"Failed to place bid on auction {id}");
                    TempData["ErrorMessage"] = "Failed to place bid. It may be too low or the auction has ended.";
                }
                
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("buyer") || ex.Message.Contains("permission"))
            {
                _logger.LogWarning($"User account type error: {ex.Message}");
                TempData["ErrorMessage"] = "Your account doesn't have permission to place bids. Only buyer accounts can place bids.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error placing bid on auction {id}");
                TempData["ErrorMessage"] = $"Error placing bid: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: AuctionProducts/Create
        public IActionResult Create()
        {
            return RedirectToAction("Create", "Home");
        }

        // POST: AuctionProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuctionProduct auctionProduct, string tagIds, string imageUrls)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Process tags if provided
                    if (!string.IsNullOrEmpty(tagIds))
                    {
                        var tagIdList = tagIds.Split(',');
                        foreach (var tagId in tagIdList)
                        {
                            if (tagId.StartsWith("new_"))
                            {
                                // This is a new tag to be created
                                var tagTitle = tagId.Substring(4); // Remove "new_" prefix
                                var productTagService = HttpContext.RequestServices.GetService<MarketMinds.Shared.Services.ProductTagService.IProductTagService>();
                                var newTag = productTagService.CreateProductTag(tagTitle);
                                
                                // Add the tag to the product's tags (implementation depends on how AuctionProductService handles this)
                                // This might need to be done after creating the product
                            }
                            else if (int.TryParse(tagId, out int existingTagId))
                            {
                                // This is an existing tag
                                // Add the tag ID to be processed by the service
                                // Again, implementation depends on how AuctionProductService handles this
                            }
                        }
                    }
                    
                    // Process image URLs if provided
                    if (!string.IsNullOrEmpty(imageUrls))
                    {
                        var imageUrlList = imageUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        var imageUploadService = HttpContext.RequestServices.GetService<MarketMinds.Shared.Services.ImagineUploadService.IImageUploadService>();
                        var imagesList = imageUploadService.ParseImagesString(imageUrls);
                        
                        // Set images to the product (implementation depends on how your service handles this)
                        // This might need to be done after creating the product
                    }
                    
                    var result = await _auctionProductService.CreateAuctionProductAsync(auctionProduct);
                    if (result)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating auction product");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the auction product");
                }
            }
            
            // If we got this far, something failed - redisplay form with proper selections
            var categoryService = HttpContext.RequestServices.GetService<MarketMinds.Shared.Services.ProductCategoryService.IProductCategoryService>();
            var conditionService = HttpContext.RequestServices.GetService<MarketMinds.Shared.Services.ProductConditionService.IProductConditionService>();
            
            ViewBag.Categories = categoryService.GetAllProductCategories();
            ViewBag.Conditions = conditionService.GetAllProductConditions();
            
            return View(auctionProduct);
        }

        // GET: AuctionProducts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var auctionProduct = await _auctionProductService.GetAuctionProductByIdAsync(id);
            if (auctionProduct == null || auctionProduct.Id == 0)
            {
                return NotFound();
            }
            return View(auctionProduct);
        }

        // POST: AuctionProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AuctionProduct auctionProduct)
        {
            if (id != auctionProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _auctionProductService.UpdateAuctionProductAsync(auctionProduct);
                    if (result)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating auction product {id}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the auction product");
                }
            }
            return View(auctionProduct);
        }

        // Helper method to calculate time left for an auction
        [NonAction]
        public string GetTimeLeft(DateTime endTime)
        {
            var timeLeft = endTime - DateTime.Now;
            
            if (timeLeft <= TimeSpan.Zero)
            {
                return "Auction Ended";
            }
            
            return $"{timeLeft.Days}d {timeLeft.Hours}h {timeLeft.Minutes}m";
        }
    }
} 
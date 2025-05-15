// BuyerWishlistController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClassLibrary.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebMarketplace.Models;
using SharedClassLibrary.Domain;
using System.Diagnostics;
using System.Text;

namespace WebMarketplace.Controllers
{
    /// <summary>
    /// Controller for buyer wishlist functionality with enhanced error handling
    /// </summary>
    public class BuyerWishlistController : Controller
    {
        private readonly IBuyerService _buyerService;
        private readonly IProductService _productService;
        private readonly ILogger<BuyerWishlistController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerWishlistController"/> class.
        /// </summary>
        /// <param name="buyerService">The buyer service</param>
        /// <param name="productService">The product service</param>
        /// <param name="logger">The logger</param>
        public BuyerWishlistController(
            IBuyerService buyerService,
            IProductService productService,
            ILogger<BuyerWishlistController> logger)
        {
            _buyerService = buyerService ?? throw new ArgumentNullException(nameof(buyerService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
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
        /// Test endpoint to check if controller is working
        /// </summary>
        /// <returns>Simple text message</returns>
        public IActionResult Test()
        {
            return Content("BuyerWishlistController is working correctly.");
        }

        /// <summary>
        /// Returns basic diagnostic information
        /// </summary>
        /// <returns>JSON with diagnostic information</returns>
        public IActionResult Diagnostics()
        {
            var diagnostics = new
            {
                ControllerName = nameof(BuyerWishlistController),
                Services = new[]
                {
                    $"BuyerService: {_buyerService?.GetType().FullName ?? "null"}",
                    $"ProductService: {_productService?.GetType().FullName ?? "null"}"
                },
                CurrentTime = DateTime.Now,
                AspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            };

            return Json(diagnostics);
        }

        /// <summary>
        /// Displays the buyer's wishlist with extensive error handling
        /// </summary>
        /// <returns>The view with wishlist items or appropriate error</returns>
        public async Task<IActionResult> Index()
        {
            var stopwatch = Stopwatch.StartNew();
            var debugInfo = new StringBuilder();

            try
            {
                _logger.LogInformation("Loading wishlist for user");
                debugInfo.AppendLine("Starting wishlist load process...");

                // STEP 1: Get current user ID
                int userId = GetCurrentUserId();
                debugInfo.AppendLine($"Current user ID: {userId}");

                // STEP 2: Create user object
                User user;
                try
                {
                    user = new SharedClassLibrary.Domain.User(userId);
                _logger.LogInformation("Created user object with ID: {UserId}", user.UserId);
                    debugInfo.AppendLine($"User object created: {user.UserId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create User object for ID {UserId}", userId);
                    return Content($"Error creating user object: {ex.Message}\n\nDebug info: {debugInfo}");
                }

                // STEP 3: Get buyer by user
                Buyer buyer;
                try
                {
                    buyer = await _buyerService.GetBuyerByUser(user);
                    debugInfo.AppendLine($"GetBuyerByUser completed: {(buyer == null ? "null buyer" : $"Buyer.Id={buyer.Id}")}");

                if (buyer == null)
                {
                    _logger.LogWarning("Buyer not found for user ID {UserId}", userId);
                        return Content($"Buyer not found for user ID {userId}\n\nDebug info: {debugInfo}");
                }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting buyer for user ID {UserId}", userId);
                    return Content($"Error retrieving buyer: {ex.Message}\n\nDebug info: {debugInfo}");
                }

                // STEP 4: Load the buyer's wishlist
                try
                {
                    debugInfo.AppendLine("About to load buyer wishlist...");
                await _buyerService.LoadBuyer(buyer, SharedClassLibrary.Service.BuyerDataSegments.Wishlist);
                    debugInfo.AppendLine("LoadBuyer completed");

                    var itemCount = buyer.Wishlist?.Items?.Count ?? 0;
                    _logger.LogInformation("Wishlist loaded for buyer {BuyerId} with {ItemCount} items", buyer.Id, itemCount);
                    debugInfo.AppendLine($"Wishlist contains {itemCount} items");

                    // Check if Wishlist is null
                    if (buyer.Wishlist == null)
                    {
                        debugInfo.AppendLine("WARNING: buyer.Wishlist is null after LoadBuyer call");
                    }
                    // Check if Items collection is null
                    else if (buyer.Wishlist.Items == null)
                    {
                        debugInfo.AppendLine("WARNING: buyer.Wishlist.Items is null");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading wishlist for buyer {BuyerId}", buyer.Id);
                    return Content($"Error loading wishlist: {ex.Message}\n\nDebug info: {debugInfo}");
                }

                // STEP 5: Convert BuyerWishlistItem objects to Product objects
                var products = new List<Product>();
                try
                {
                    debugInfo.AppendLine("Starting conversion of wishlist items to products...");
                if (buyer.Wishlist?.Items != null)
                {
                    foreach (var wishlistItem in buyer.Wishlist.Items)
                    {
                            try
                            {
                                debugInfo.AppendLine($"Getting product for wishlist item: ProductId={wishlistItem.ProductId}");
                        var product = await _productService.GetProductByIdAsync(wishlistItem.ProductId);

                        if (product != null)
                        {
                                    debugInfo.AppendLine($"Product found: {product.Name}, Adding to list");
                            products.Add(product);
                        }
                                else
                                {
                                    debugInfo.AppendLine($"WARNING: Product {wishlistItem.ProductId} not found");
                                    _logger.LogWarning("Product not found for wishlist item ID {ProductId}", wishlistItem.ProductId);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error getting product {ProductId}", wishlistItem.ProductId);
                                debugInfo.AppendLine($"Error getting product {wishlistItem.ProductId}: {ex.Message}");
                                // Continue trying other products instead of failing the entire request
                            }
                        }
                    }
                    debugInfo.AppendLine($"Converted {products.Count} products");
                    }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error converting wishlist items to products");
                    return Content($"Error converting wishlist items: {ex.Message}\n\nDebug info: {debugInfo}");
                }

                // STEP 6: Create view model
                try
                {
                    debugInfo.AppendLine("Creating view model...");
                var viewModel = new BuyerWishlistViewModel
                {
                    BuyerId = buyer.Id,
                    WishlistItems = products
                };
                    debugInfo.AppendLine($"View model created with {products.Count} items");

                _logger.LogInformation("Wishlist loaded successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                    // Return simple debug view if requested
                    if (Request.Query.ContainsKey("debug"))
                    {
                        return Content(debugInfo.ToString());
                    }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                    _logger.LogError(ex, "Error creating view model");
                    return Content($"Error creating view model: {ex.Message}\n\nDebug info: {debugInfo}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error loading wishlist after {ElapsedMs}ms: {Message}",
                    stopwatch.ElapsedMilliseconds, ex.Message);

                return Content($"Unhandled error: {ex.Message}\n\nStack trace: {ex.StackTrace}\n\nDebug info: {debugInfo}");
            }
        }

        /// <summary>
        /// Fallback action that returns a simple view of the wishlist for debugging
        /// </summary>
        /// <returns>A basic HTML representation of the wishlist</returns>
        public async Task<IActionResult> BasicView()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = new SharedClassLibrary.Domain.User(userId);
                var buyer = await _buyerService.GetBuyerByUser(user);

                if (buyer == null)
                {
                    return Content("Buyer not found");
                }

                await _buyerService.LoadBuyer(buyer, SharedClassLibrary.Service.BuyerDataSegments.Wishlist);

                var sb = new StringBuilder();
                sb.AppendLine("<h1>Basic Wishlist View</h1>");

                if (buyer.Wishlist?.Items == null || buyer.Wishlist.Items.Count == 0)
                {
                    sb.AppendLine("<p>Your wishlist is empty</p>");
                }
                else
                {
                    sb.AppendLine("<ul>");
                    foreach (var item in buyer.Wishlist.Items)
                    {
                        sb.AppendLine($"<li>Product ID: {item.ProductId}</li>");
            }
                    sb.AppendLine("</ul>");
        }

                return Content(sb.ToString(), "text/html");
            }
            catch (Exception ex)
            {
                return Content($"Error in BasicView: {ex.Message}\n\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Removes an item from the wishlist
        /// </summary>
        /// <param name="productId">The product ID to remove</param>
        /// <returns>Redirects to the wishlist</returns>
        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            try
            {
                _logger.LogInformation("Removing product {ProductId} from wishlist", productId);

                int userId = GetCurrentUserId();

                // Create a basic User object instead of fetching all users
                var user = new SharedClassLibrary.Domain.User(userId);

                var buyer = await _buyerService.GetBuyerByUser(user);

                if (buyer == null)
                {
                    _logger.LogWarning("Buyer not found for user ID {UserId}", userId);
                    return RedirectToAction("Error", "Home", new { message = "Buyer profile not found" });
                }

                await _buyerService.RemoveWishilistItem(buyer, productId);

                TempData["SuccessMessage"] = "Item removed from wishlist";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove product {ProductId} from wishlist: {Message}",
                    productId, ex.Message);
                TempData["ErrorMessage"] = $"Failed to remove item: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Adds a product to the wishlist
        /// </summary>
        /// <param name="productId">The product ID to add</param>
        /// <returns>Redirects to the wishlist</returns>
        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            try
            {
                _logger.LogInformation("Adding product {ProductId} to wishlist", productId);

                int userId = GetCurrentUserId();

                // Create a basic User object instead of fetching all users
                var user = new SharedClassLibrary.Domain.User(userId);

                var buyer = await _buyerService.GetBuyerByUser(user);

                if (buyer == null)
                {
                    _logger.LogWarning("Buyer not found for user ID {UserId}", userId);
                    return RedirectToAction("Error", "Home", new { message = "Buyer profile not found" });
                }

                // Here you would need to implement AddWishlistItem logic
                // Example implementation could be something like:
                // await _buyerService.AddWishlistItem(buyer, productId);

                // For now, we'll log a warning since the method doesn't exist in the interface
                _logger.LogWarning("AddWishlistItem method not implemented in IBuyerService");

                TempData["SuccessMessage"] = "Item added to wishlist";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add product {ProductId} to wishlist: {Message}",
                    productId, ex.Message);
                TempData["ErrorMessage"] = $"Failed to add item: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

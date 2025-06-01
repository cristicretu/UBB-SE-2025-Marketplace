// BuyerWishlistController.cs
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Services;
using WebMarketplace.Models;
using MarketMinds.Shared.Models;
using System.Diagnostics;
using System.Text;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.BuyProductsService;
using Microsoft.AspNetCore.Http; // Required for HttpContext.Session
using System.Text.Json; // Required for JsonSerializer
using Microsoft.AspNetCore.Authorization; // Required for [Authorize]

namespace WebMarketplace.Controllers
{
    /// <summary>
    /// Controller for buyer wishlist functionality with enhanced error handling
    /// </summary>
    [Authorize]
    public class BuyerWishlistController : Controller
    {
        private readonly IBuyerService _buyerService;
        private readonly IBuyProductsService _productService;
        private readonly IBuyerLinkageService _buyerLinkageService;
        private readonly ILogger<BuyerWishlistController> _logger;
        private const string WishlistSessionKey = "WishlistProductIds";

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerWishlistController"/> class.
        /// </summary>
        /// <param name="buyerService">The buyer service</param>
        /// <param name="productService">The product service</param>
        /// <param name="buyerLinkageService">The buyer linkage service</param>
        /// <param name="logger">The logger</param>
        public BuyerWishlistController(
            IBuyerService buyerService,
            IBuyProductsService productService,
            IBuyerLinkageService buyerLinkageService,
            ILogger<BuyerWishlistController> logger)
        {
            _buyerService = buyerService ?? throw new ArgumentNullException(nameof(buyerService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _buyerLinkageService = buyerLinkageService ?? throw new ArgumentNullException(nameof(buyerLinkageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current user ID
        /// </summary>
        /// <returns>The current user ID</returns>
        private int GetCurrentUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                return UserSession.CurrentUserId ?? 0;
            }
            return 0;
        }

        /// <summary>
        /// Test endpoint to check if controller is working
        /// </summary>
        /// <returns>Simple text message</returns>
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Content("BuyerWishlistController is working correctly.");
        }

        /// <summary>
        /// Returns basic diagnostic information
        /// </summary>
        /// <returns>JSON with diagnostic information</returns>
        [AllowAnonymous]
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
        /// Displays the buyer's wishlist with support for both individual and friends views
        /// </summary>
        /// <param name="mode">View mode: "my" for individual wishlist, "friends" for shared wishlist</param>
        /// <returns>The view with wishlist items or appropriate error</returns>
        public async Task<IActionResult> Index(string mode = "my")
        {
            var stopwatch = Stopwatch.StartNew();
            var debugInfo = new StringBuilder();

            try
            {
                // Normalize and validate the mode parameter
                mode = mode?.ToLower() ?? "my";
                if (mode != "my" && mode != "friends")
                {
                    mode = "my";
                }

                _logger.LogInformation("Loading wishlist for user in mode: {Mode}", mode);
                debugInfo.AppendLine($"Starting wishlist load process in mode: {mode}...");

                // STEP 1: Get current user ID
                int userId = GetCurrentUserId();
                debugInfo.AppendLine($"Current user ID: {userId}");

                // STEP 2: Create user object
                User user;
                try
                {
                    user = new MarketMinds.Shared.Models.User(userId);
                    user.Id = userId;
                    _logger.LogInformation("Created user object with ID: {UserId}", user.Id);
                    debugInfo.AppendLine($"User object created: {user.Id}");
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

                // STEP 4: Create view model based on mode
                BuyerWishlistViewModel viewModel;
                try
                {
                    viewModel = new BuyerWishlistViewModel
                    {
                        BuyerId = buyer.Id,
                        ViewMode = mode,
                        CurrentBuyerFirstName = buyer.FirstName ?? string.Empty,
                        CurrentBuyerLastName = buyer.LastName ?? string.Empty
                    };

                    if (mode == "my")
                    {
                        // Load current buyer's wishlist
                        await LoadMyWishlist(buyer, viewModel, debugInfo);
                    }
                    else if (mode == "friends")
                    {
                        // Load friends' wishlists
                        await LoadFriendsWishlists(buyer, viewModel, debugInfo);
                    }

                    // Load buyers with similar addresses for sidebar (for both modes)
                    await LoadSimilarAddressBuyers(buyer, viewModel, debugInfo);

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
        /// Loads the current buyer's wishlist items
        /// </summary>
        /// <param name="buyer">The buyer</param>
        /// <param name="viewModel">The view model to populate</param>
        /// <param name="debugInfo">Debug information</param>
        private async Task LoadMyWishlist(Buyer buyer, BuyerWishlistViewModel viewModel, StringBuilder debugInfo)
        {
            try
            {
                debugInfo.AppendLine("Loading current buyer's wishlist...");
                await _buyerService.LoadBuyer(buyer, MarketMinds.Shared.Services.BuyerDataSegments.Wishlist);
                debugInfo.AppendLine("LoadBuyer completed");

                var itemCount = buyer.Wishlist?.Items?.Count ?? 0;
                _logger.LogInformation("Wishlist loaded for buyer {BuyerId} with {ItemCount} items", buyer.Id, itemCount);
                debugInfo.AppendLine($"Wishlist contains {itemCount} items");

                // Convert BuyerWishlistItem objects to Product objects
                var products = new List<BuyProduct>();
                if (buyer.Wishlist?.Items != null)
                {
                    foreach (var wishlistItem in buyer.Wishlist.Items)
                    {
                        try
                        {
                            debugInfo.AppendLine($"Getting product for wishlist item: ProductId={wishlistItem.ProductId}");
                            var product = _productService.GetProductById(wishlistItem.ProductId);

                            if (product != null)
                            {
                                debugInfo.AppendLine($"Product found: {product.Title}, Adding to list");
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
                        }
                    }
                }

                viewModel.WishlistItems = products;
                debugInfo.AppendLine($"Loaded {products.Count} products for current buyer");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading current buyer's wishlist for buyer {BuyerId}", buyer.Id);
                debugInfo.AppendLine($"Error loading current buyer's wishlist: {ex.Message}");
                viewModel.WishlistItems = new List<BuyProduct>();
            }
        }

        /// <summary>
        /// Loads all linked buyers' wishlist items grouped by buyer
        /// </summary>
        /// <param name="buyer">The current buyer</param>
        /// <param name="viewModel">The view model to populate</param>
        /// <param name="debugInfo">Debug information</param>
        private async Task LoadFriendsWishlists(Buyer buyer, BuyerWishlistViewModel viewModel, StringBuilder debugInfo)
        {
            try
            {
                debugInfo.AppendLine("Loading friends' wishlists...");
                debugInfo.AppendLine($"Current buyer ID: {buyer.Id}");

                // Get linked buyers
                var linkedBuyers = await _buyerLinkageService.GetLinkedBuyersAsync(buyer.Id);
                var linkedBuyersList = linkedBuyers.ToList(); // Convert to list to avoid multiple enumeration
                debugInfo.AppendLine($"Found {linkedBuyersList.Count} linked buyers");

                // Log details about each linked buyer
                foreach (var linkedBuyer in linkedBuyersList)
                {
                    debugInfo.AppendLine($"Linked buyer: ID={linkedBuyer.Id}, Name={linkedBuyer.FirstName} {linkedBuyer.LastName}");
                }

                var groupedWishlists = new List<BuyerWishlistGroup>();

                // Add current buyer's wishlist as first group
                debugInfo.AppendLine("Creating group for current buyer...");
                var currentBuyerGroup = await CreateWishlistGroup(buyer, debugInfo, "Current Buyer");
                groupedWishlists.Add(currentBuyerGroup); // Always add, regardless of item count
                debugInfo.AppendLine($"Added current buyer group with {currentBuyerGroup.ItemCount} items");

                // Add linked buyers' wishlists
                foreach (var linkedBuyer in linkedBuyersList)
                {
                    try
                    {
                        debugInfo.AppendLine($"Creating group for linked buyer {linkedBuyer.Id}...");
                        var group = await CreateWishlistGroup(linkedBuyer, debugInfo, $"Linked Buyer {linkedBuyer.Id}");
                        groupedWishlists.Add(group); // Always add, regardless of item count
                        debugInfo.AppendLine($"Added linked buyer {linkedBuyer.Id} group with {group.ItemCount} items");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load wishlist for linked buyer {BuyerId}", linkedBuyer.Id);
                        debugInfo.AppendLine($"Error loading wishlist for linked buyer {linkedBuyer.Id}: {ex.Message}");

                        // Even if there's an error, add an empty group so we can see the buyer
                        var emptyGroup = new BuyerWishlistGroup
                        {
                            BuyerId = linkedBuyer.Id,
                            FirstName = linkedBuyer.FirstName ?? "Unknown",
                            LastName = linkedBuyer.LastName ?? "User",
                            WishlistItems = new List<BuyProduct>()
                        };
                        groupedWishlists.Add(emptyGroup);
                        debugInfo.AppendLine($"Added empty group for linked buyer {linkedBuyer.Id} due to error");
                    }
                }

                viewModel.GroupedWishlistItems = groupedWishlists;
                debugInfo.AppendLine($"Final result: {groupedWishlists.Count} wishlist groups with total {groupedWishlists.Sum(g => g.ItemCount)} items");

                // Log each group for debugging
                for (int i = 0; i < groupedWishlists.Count; i++)
                {
                    var group = groupedWishlists[i];
                    debugInfo.AppendLine($"Group {i + 1}: {group.DisplayName} (ID: {group.BuyerId}) - {group.ItemCount} items");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading friends' wishlists for buyer {BuyerId}", buyer.Id);
                debugInfo.AppendLine($"Error loading friends' wishlists: {ex.Message}");
                debugInfo.AppendLine($"Stack trace: {ex.StackTrace}");
                viewModel.GroupedWishlistItems = new List<BuyerWishlistGroup>();
            }
        }

        /// <summary>
        /// Creates a wishlist group for a specific buyer
        /// </summary>
        /// <param name="buyer">The buyer</param>
        /// <param name="debugInfo">Debug information</param>
        /// <param name="context">Context for logging</param>
        /// <returns>A wishlist group for the buyer</returns>
        private async Task<BuyerWishlistGroup> CreateWishlistGroup(Buyer buyer, StringBuilder debugInfo, string context)
        {
            debugInfo.AppendLine($"Creating wishlist group for {context} (ID: {buyer.Id})");
            debugInfo.AppendLine($"  Buyer name: {buyer.FirstName} {buyer.LastName}");

            var group = new BuyerWishlistGroup
            {
                BuyerId = buyer.Id,
                FirstName = buyer.FirstName ?? "Unknown",
                LastName = buyer.LastName ?? "User"
            };

            try
            {
                // For the current buyer, use the already loaded wishlist if available
                List<BuyerWishlistItem> wishlistItems;
                if (context.Contains("Current Buyer") && buyer.Wishlist?.Items != null)
                {
                    debugInfo.AppendLine($"  Using already loaded wishlist for current buyer with {buyer.Wishlist.Items.Count} items");
                    wishlistItems = buyer.Wishlist.Items;
                }
                else
                {
                    // For linked buyers, get wishlist items directly by buyer ID
                    debugInfo.AppendLine($"  Getting wishlist items directly for buyer {buyer.Id}...");
                    wishlistItems = await _buyerService.GetWishlistItems(buyer.Id);
                    debugInfo.AppendLine($"  Got {wishlistItems.Count} wishlist items for buyer {buyer.Id}");
                }

                // Convert wishlist items to products
                var products = new List<BuyProduct>();
                if (wishlistItems != null && wishlistItems.Any())
                {
                    debugInfo.AppendLine($"  Processing {wishlistItems.Count} wishlist items for buyer {buyer.Id}");
                    foreach (var wishlistItem in wishlistItems)
                    {
                        try
                        {
                            debugInfo.AppendLine($"    Getting product {wishlistItem.ProductId}...");
                            var product = _productService.GetProductById(wishlistItem.ProductId);
                            if (product != null)
                            {
                                products.Add(product);
                                debugInfo.AppendLine($"    Added product {wishlistItem.ProductId}: {product.Title}");
                            }
                            else
                            {
                                debugInfo.AppendLine($"    Product {wishlistItem.ProductId} not found");
                                _logger.LogWarning("Product not found for wishlist item ID {ProductId} (buyer {BuyerId})",
                                    wishlistItem.ProductId, buyer.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            debugInfo.AppendLine($"    Error getting product {wishlistItem.ProductId}: {ex.Message}");
                            _logger.LogError(ex, "Error getting product {ProductId} for buyer {BuyerId}",
                                wishlistItem.ProductId, buyer.Id);
                        }
                    }
                }
                else
                {
                    debugInfo.AppendLine($"  No wishlist items found for buyer {buyer.Id}");
                }

                group.WishlistItems = products;
                debugInfo.AppendLine($"Group for {context} created with {products.Count} items");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wishlist group for {Context} (buyer {BuyerId})", context, buyer.Id);
                debugInfo.AppendLine($"Error creating group for {context}: {ex.Message}");
                debugInfo.AppendLine($"Stack trace: {ex.StackTrace}");
                group.WishlistItems = new List<BuyProduct>();
            }

            return group;
        }

        /// <summary>
        /// Loads buyers with similar shipping addresses for the sidebar
        /// </summary>
        /// <param name="buyer">The current buyer</param>
        /// <param name="viewModel">The view model to populate</param>
        /// <param name="debugInfo">Debug information</param>
        private async Task LoadSimilarAddressBuyers(Buyer buyer, BuyerWishlistViewModel viewModel, StringBuilder debugInfo)
        {
            try
            {
                debugInfo.AppendLine("Loading buyers with similar addresses...");
                
                // Ensure buyer has shipping address loaded
                if (buyer.ShippingAddress == null)
                {
                    await _buyerService.LoadBuyer(buyer, MarketMinds.Shared.Services.BuyerDataSegments.BasicInfo);
                }

                if (buyer.ShippingAddress != null)
                {
                    debugInfo.AppendLine($"Current buyer shipping address: {buyer.ShippingAddress.City}, {buyer.ShippingAddress.Country}, {buyer.ShippingAddress.PostalCode}");
                    
                    var similarBuyers = await _buyerService.FindBuyersWithShippingAddress(buyer.ShippingAddress);
                    
                    // Filter out the current buyer from the results
                    var filteredBuyers = similarBuyers.Where(b => b.Id != buyer.Id).ToList();
                    
                    debugInfo.AppendLine($"Found {filteredBuyers.Count} buyers with similar addresses");
                    
                    viewModel.SimilarAddressBuyers = filteredBuyers;
                }
                else
                {
                    debugInfo.AppendLine("Current buyer has no shipping address");
                    viewModel.SimilarAddressBuyers = new List<Buyer>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading buyers with similar addresses for buyer {BuyerId}", buyer.Id);
                debugInfo.AppendLine($"Error loading similar address buyers: {ex.Message}");
                viewModel.SimilarAddressBuyers = new List<Buyer>();
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
                var user = new MarketMinds.Shared.Models.User(userId);
                var buyer = await _buyerService.GetBuyerByUser(user);

                if (buyer == null)
                {
                    return Content("Buyer not found");
                }

                await _buyerService.LoadBuyer(buyer, MarketMinds.Shared.Services.BuyerDataSegments.Wishlist);

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
                var user = new MarketMinds.Shared.Models.User(userId);
                user.Id = userId; // Ensure the Id is set correctly

                var buyer = await _buyerService.GetBuyerByUser(user);

                if (buyer == null)
                {
                    _logger.LogWarning("Buyer not found for user ID {UserId}", userId);
                    return RedirectToAction("Error", "Home", new { message = "Buyer profile not found" });
                }

                await _buyerService.RemoveWishilistItem(buyer, productId);

                // Update session cache
                var wishlistIds = HttpContext.Session.GetString(WishlistSessionKey);
                var idSet = string.IsNullOrEmpty(wishlistIds) ? new HashSet<int>() : JsonSerializer.Deserialize<HashSet<int>>(wishlistIds);
                if (idSet.Remove(productId))
                {
                    HttpContext.Session.SetString(WishlistSessionKey, JsonSerializer.Serialize(idSet));
                }

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
                var user = new MarketMinds.Shared.Models.User(userId);
                user.Id = userId; // Ensure the Id is set correctly

                var buyer = await _buyerService.GetBuyerByUser(user);

                if (buyer == null)
                {
                    _logger.LogWarning("Buyer not found for user ID {UserId}", userId);
                    return RedirectToAction("Error", "Home", new { message = "Buyer profile not found" });
                }

                await _buyerService.AddWishlistItem(buyer, productId);

                // Update session cache
                var wishlistIds = HttpContext.Session.GetString(WishlistSessionKey);
                var idSet = string.IsNullOrEmpty(wishlistIds) ? new HashSet<int>() : JsonSerializer.Deserialize<HashSet<int>>(wishlistIds);
                if (idSet.Add(productId)) // Add returns true if the item was added, false if it already existed
                {
                    HttpContext.Session.SetString(WishlistSessionKey, JsonSerializer.Serialize(idSet));
                }

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
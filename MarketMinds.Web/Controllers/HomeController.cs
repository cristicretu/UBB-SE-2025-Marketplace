using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MarketMinds.Web.Models;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.ProductTagService;
using MarketMinds.Shared.Services.ProductCategoryService;
using MarketMinds.Shared.Services.ProductConditionService;
using MarketMinds.Shared.Services.ImagineUploadService;
using MarketMinds.Shared.Services.BorrowProductsService;
using Microsoft.AspNetCore.Authorization;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Shared.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuctionProductService _auctionProductService;
        private readonly IProductTagService _productTagService;
        private readonly IProductCategoryService _categoryService;
        private readonly IProductConditionService _conditionService;
        private readonly IImageUploadService _imageUploadService;
        private readonly IBorrowProductsService _borrowProductsService;
        private readonly IBuyProductsService _buyProductsService;
        private readonly IBuyerService _buyerService;
        private const string WishlistSessionKey = "WishlistProductIds";

        public HomeController(
            ILogger<HomeController> logger,
            IAuctionProductService auctionProductService,
            IProductTagService productTagService,
            IProductCategoryService categoryService,
            IProductConditionService conditionService,
            IImageUploadService imageUploadService,
            IBorrowProductsService borrowProductsService,
            IBuyProductsService buyProductsService,
            IBuyerService buyerService)
        {
            _logger = logger;
            _auctionProductService = auctionProductService;
            _productTagService = productTagService;
            _categoryService = categoryService;
            _conditionService = conditionService;
            _imageUploadService = imageUploadService;
            _borrowProductsService = borrowProductsService;
            _buyProductsService = buyProductsService;
            _buyerService = buyerService;
        }

        private int GetCurrentUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                return UserSession.CurrentUserId ?? 0;
            }
            return 0;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int offset = 0, int count = 12, string tab = "buy",
            List<int>? conditionIds = null, List<int>? categoryIds = null, string search = null, double? maxPrice = null)
        {
            try
            {
                ViewBag.WishlistProductIds = new List<int>();
                int currentUserId = GetCurrentUserId();

                if (currentUserId > 0)
                {
                    var wishlistIdsJson = HttpContext.Session.GetString(WishlistSessionKey);
                    if (!string.IsNullOrEmpty(wishlistIdsJson))
                    {
                        try
                        {
                            var idSet = JsonSerializer.Deserialize<HashSet<int>>(wishlistIdsJson);
                            ViewBag.WishlistProductIds = idSet?.ToList() ?? new List<int>();
                            _logger.LogInformation("Loaded wishlist IDs from session for user {UserId}", currentUserId);
                        }
                        catch (JsonException jsonEx)
                        {
                            _logger.LogWarning(jsonEx, "Failed to deserialize wishlist IDs from session for user {UserId}. Will attempt to reload.", currentUserId);
                        }
                    }

                    if (ViewBag.WishlistProductIds == null || !((List<int>)ViewBag.WishlistProductIds).Any())
                    {
                        var user = new MarketMinds.Shared.Models.User(currentUserId);
                        user.Id = currentUserId;

                        try
                        {
                            var buyer = await _buyerService.GetBuyerByUser(user);
                            if (buyer != null)
                            {
                                await _buyerService.LoadBuyer(buyer, MarketMinds.Shared.Services.BuyerDataSegments.Wishlist);
                                if (buyer.Wishlist?.Items != null && buyer.Wishlist.Items.Any())
                                {
                                    var idSet = buyer.Wishlist.Items.Select(item => item.ProductId).ToHashSet();
                                    HttpContext.Session.SetString(WishlistSessionKey, JsonSerializer.Serialize(idSet));
                                    ViewBag.WishlistProductIds = idSet.ToList();
                                    _logger.LogInformation("Fetched and cached wishlist IDs for user {UserId}. Count: {Count}", currentUserId, idSet.Count);
                                }
                                else
                                {
                                    HttpContext.Session.SetString(WishlistSessionKey, JsonSerializer.Serialize(new HashSet<int>()));
                                    _logger.LogInformation("User {UserId} has an empty wishlist. Cached empty set.", currentUserId);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Buyer not found for user ID {UserId} when trying to load wishlist for caching.", currentUserId);
                                HttpContext.Session.SetString(WishlistSessionKey, JsonSerializer.Serialize(new HashSet<int>()));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to load buyer info for user ID {UserId}. This may be because the buyer doesn't exist yet. Attempting to create buyer profile.", currentUserId);

                            try
                            {
                                // Attempt to create buyer profile
                                // This is a placeholder - the actual buyer creation logic would go here
                                _logger.LogInformation("Creating buyer profile for user {UserId}", currentUserId);
                                HttpContext.Session.SetString(WishlistSessionKey, JsonSerializer.Serialize(new HashSet<int>()));
                                ViewBag.WishlistProductIds = new List<int>();
                            }
                            catch (Exception createEx)
                            {
                                _logger.LogError(createEx, "Failed to create buyer profile for user {UserId}. Continuing without wishlist data.", currentUserId);
                                HttpContext.Session.SetString(WishlistSessionKey, JsonSerializer.Serialize(new HashSet<int>()));
                                ViewBag.WishlistProductIds = new List<int>();
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No authenticated user or user ID not found. Wishlist IDs will not be loaded from session or DB.");
                    ViewBag.WishlistProductIds = new List<int>();
                }

                // Load products with pagination based on the active tab
                List<BuyProduct> buyProducts = new List<BuyProduct>();
                List<AuctionProduct> auctionProducts = new List<AuctionProduct>();
                List<BorrowProduct> borrowProducts = new List<BorrowProduct>();

                // Get total counts for all product types
                int totalBuyProducts = 0;
                int totalAuctionProducts = 0;
                int totalBorrowProducts = 0;

                try
                {
                    // Get total counts (filtered if filters are applied)
                    if (_buyProductsService is MarketMinds.Shared.Services.BuyProductsService.BuyProductsService concreteService)
                    {
                        if (conditionIds?.Any() == true || categoryIds?.Any() == true || !string.IsNullOrEmpty(search) || maxPrice.HasValue)
                        {
                            totalBuyProducts = concreteService.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, search);
                        }
                        else
                        {
                            totalBuyProducts = concreteService.GetProductCount();
                        }
                    }

                    if (conditionIds?.Any() == true || categoryIds?.Any() == true || !string.IsNullOrEmpty(search) || maxPrice.HasValue)
                    {
                        totalAuctionProducts = await _auctionProductService.GetFilteredAuctionProductCountAsync(conditionIds, categoryIds, maxPrice, search);
                        totalBorrowProducts = _borrowProductsService.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, search);
                    }
                    else
                    {
                        totalAuctionProducts = await _auctionProductService.GetAuctionProductCountAsync();
                        totalBorrowProducts = await _borrowProductsService.GetBorrowProductCountAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting product counts");
                }

                // Load products based on pagination parameters
                if (count > 0)
                {
                    // Apply pagination and filtering to the active tab only
                    switch (tab.ToLower())
                    {
                        case "auction":
                            auctionProducts = await _auctionProductService.GetFilteredAuctionProductsAsync(offset, count, conditionIds, categoryIds, maxPrice, search);
                            // Load first page of other tabs for tab switching
                            if (_buyProductsService is MarketMinds.Shared.Services.BuyProductsService.BuyProductsService auctionBuyService)
                            {
                                buyProducts = auctionBuyService.GetProducts(0, 12);
                            }
                            borrowProducts = await _borrowProductsService.GetAllBorrowProductsAsync(0, 12);
                            break;

                        case "borrow":
                            borrowProducts = _borrowProductsService.GetFilteredProducts(
                                offset, count, conditionIds, categoryIds, maxPrice, search, null);
                            // Load first page of other tabs for tab switching
                            if (_buyProductsService is MarketMinds.Shared.Services.BuyProductsService.BuyProductsService borrowBuyService)
                            {
                                buyProducts = borrowBuyService.GetProducts(0, 12);
                            }
                            auctionProducts = await _auctionProductService.GetAllAuctionProductsAsync(0, 12);
                            break;

                        default: // "buy"
                            if (_buyProductsService is MarketMinds.Shared.Services.BuyProductsService.BuyProductsService buyService)
                            {
                                buyProducts = buyService.GetFilteredProducts(
                                    offset, count, conditionIds, categoryIds, maxPrice, search);
                            }
                            // Load first page of other tabs for tab switching
                            auctionProducts = await _auctionProductService.GetAllAuctionProductsAsync(0, 12);
                            borrowProducts = await _borrowProductsService.GetAllBorrowProductsAsync(0, 12);
                            break;
                    }
                }
                else
                {
                    // Load all products when count = 0
                    if (_buyProductsService is MarketMinds.Shared.Services.BuyProductsService.BuyProductsService allBuyService)
                    {
                        buyProducts = allBuyService.GetProducts();
                    }
                    auctionProducts = await _auctionProductService.GetAllAuctionProductsAsync();
                    borrowProducts = await _borrowProductsService.GetAllBorrowProductsAsync();
                }

                // Calculate total products for the active tab
                int totalProducts = tab.ToLower() switch
                {
                    "auction" => totalAuctionProducts,
                    "borrow" => totalBorrowProducts,
                    _ => totalBuyProducts
                };

                bool hasNextPage = count > 0 && (offset + count) < totalProducts;
                bool hasPreviousPage = offset > 0;

                var categories = _categoryService.GetAllProductCategories();
                var conditions = _conditionService.GetAllProductConditions();

                ViewBag.Categories = categories;
                ViewBag.Conditions = conditions;

                // Calculate min and max prices using optimized GetMaxPriceAsync methods
                try
                {
                    // Get maximum prices from each product type using optimized database queries
                    double buyMaxPrice = 0;
                    double auctionMaxPrice = 0;
                    double borrowMaxPrice = 0;

                    // Call GetMaxPriceAsync methods in parallel for better performance
                    var maxPriceTasks = new List<Task<double>>();
                    
                    // Buy products max price
                    if (_buyProductsService is MarketMinds.Shared.Services.BuyProductsService.BuyProductsService maxPriceBuyService)
                    {
                        maxPriceTasks.Add(maxPriceBuyService.GetMaxPriceAsync());
                    }
                    else
                    {
                        maxPriceTasks.Add(Task.FromResult(0.0));
                    }

                    // Auction products max price
                    if (_auctionProductService is MarketMinds.Shared.Services.AuctionProductsService.AuctionProductsService auctionService)
                    {
                        maxPriceTasks.Add(auctionService.GetMaxPriceAsync());
                    }
                    else
                    {
                        maxPriceTasks.Add(Task.FromResult(0.0));
                    }

                    // Borrow products max price
                    maxPriceTasks.Add(_borrowProductsService.GetMaxPriceAsync());

                    // Wait for all max price queries to complete
                    var maxPrices = await Task.WhenAll(maxPriceTasks);
                    buyMaxPrice = maxPrices[0];
                    auctionMaxPrice = maxPrices[1];
                    borrowMaxPrice = maxPrices[2];

                    // Calculate overall maximum price across all product types
                    double overallMaxPrice = Math.Max(Math.Max(buyMaxPrice, auctionMaxPrice), borrowMaxPrice);

                    // Set price range - minimum is always 0, maximum is from database
                    ViewBag.MinPrice = 0;
                    ViewBag.MaxPrice = overallMaxPrice > 0 ? (int)Math.Ceiling(overallMaxPrice) : 1000; // Fallback to 1000 if no products

                    _logger.LogInformation($"HOME: Dynamic price calculation - Buy: {buyMaxPrice}, Auction: {auctionMaxPrice}, Borrow: {borrowMaxPrice}, Overall Max: {overallMaxPrice}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating dynamic max prices, using fallback values");
                    // Fallback to default values if there's an error
                    ViewBag.MinPrice = 0;
                    ViewBag.MaxPrice = 1000;
                }

                // Add pagination metadata
                ViewBag.CurrentOffset = offset;
                ViewBag.CurrentCount = count;
                ViewBag.CurrentTab = tab;
                ViewBag.TotalProducts = totalProducts;
                ViewBag.TotalBuyProducts = totalBuyProducts;
                ViewBag.TotalAuctionProducts = totalAuctionProducts;
                ViewBag.TotalBorrowProducts = totalBorrowProducts;
                ViewBag.HasNextPage = hasNextPage;
                ViewBag.HasPreviousPage = hasPreviousPage;

                // Add filter metadata
                ViewBag.SelectedConditionIds = conditionIds ?? new List<int>();
                ViewBag.SelectedCategoryIds = categoryIds ?? new List<int>();
                ViewBag.SearchQuery = search ?? string.Empty;
                ViewBag.SelectedMaxPrice = maxPrice;

                // Build pagination URLs
                int currentPage = count > 0 ? (offset / count) + 1 : 1;
                int totalPages = count > 0 ? (int)Math.Ceiling((double)totalProducts / count) : 1;

                ViewBag.PrevPageUrl = currentPage > 1 ? BuildPaginationUrl(Math.Max(0, offset - count), count, tab, conditionIds, categoryIds, search, maxPrice) : null;
                ViewBag.NextPageUrl = currentPage < totalPages ? BuildPaginationUrl(offset + count, count, tab, conditionIds, categoryIds, search, maxPrice) : null;

                // Dynamic pagination: show current page ± 2 pages (5 pages total)
                var pageUrls = new Dictionary<int, string>();
                int maxPagesToShow = 5;
                int halfRange = maxPagesToShow / 2; // 2 pages on each side

                // Calculate the start and end page numbers
                int startPage = Math.Max(1, currentPage - halfRange);
                int endPage = Math.Min(totalPages, currentPage + halfRange);

                // Adjust if we're near the beginning or end
                if (endPage - startPage + 1 < maxPagesToShow)
                {
                    if (startPage == 1)
                    {
                        endPage = Math.Min(totalPages, startPage + maxPagesToShow - 1);
                    }
                    else if (endPage == totalPages)
                    {
                        startPage = Math.Max(1, endPage - maxPagesToShow + 1);
                    }
                }

                // Build URLs for the visible page range
                for (int pageNum = startPage; pageNum <= endPage; pageNum++)
                {
                    int pageOffset = (pageNum - 1) * count;
                    pageUrls[pageNum] = BuildPaginationUrl(pageOffset, count, tab, conditionIds, categoryIds, search, maxPrice);
                }
                ViewBag.PageUrls = pageUrls;
                ViewBag.CurrentPage = currentPage;
                ViewBag.StartPage = startPage;
                ViewBag.EndPage = endPage;

                // Show "..." and last page if there are more pages beyond our range
                if (endPage < totalPages)
                {
                    int lastPageOffset = (totalPages - 1) * count;
                    ViewBag.LastPageUrl = BuildPaginationUrl(lastPageOffset, count, tab, conditionIds, categoryIds, search, maxPrice);
                    ViewBag.LastPageNumber = totalPages;
                    ViewBag.ShowLastPageEllipsis = endPage < totalPages - 1; // Show "..." if there's a gap
                }

                // Show first page if our range doesn't start at 1
                if (startPage > 1)
                {
                    ViewBag.FirstPageUrl = BuildPaginationUrl(0, count, tab, conditionIds, categoryIds, search, maxPrice);
                    ViewBag.ShowFirstPageEllipsis = startPage > 2; // Show "..." if there's a gap
                }

                // Debug logging to verify price range calculation
                _logger.LogInformation($"HOME: Calculated price range - Min: {ViewBag.MinPrice}, Max: {ViewBag.MaxPrice}");
                _logger.LogInformation($"HOME: Pagination - Offset: {offset}, Count: {count}, Total: {totalProducts}");
                _logger.LogInformation($"HOME: Returned products - Buy: {buyProducts.Count}, Auction: {auctionProducts.Count}, Borrow: {borrowProducts.Count}");

                var viewModel = new HomeViewModel
                {
                    BuyProducts = buyProducts,
                    AuctionProducts = auctionProducts,
                    BorrowProducts = borrowProducts
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products for home page");
                TempData["ErrorMessage"] = "Error loading products. Please try again later.";
                return View(new HomeViewModel());
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> FilterProducts([FromBody] FilterRequest request)
        {
            try
            {
                List<BuyProduct> buyProducts = new List<BuyProduct>();
                List<AuctionProduct> auctionProducts = new List<AuctionProduct>();
                List<BorrowProduct> borrowProducts = new List<BorrowProduct>();

                int totalProducts = 0;

                // Apply filtering based on the active tab
                switch (request.Tab.ToLower())
                {
                    case "auction":
                        auctionProducts = await _auctionProductService.GetFilteredAuctionProductsAsync(
                            request.Offset, request.Count, request.ConditionIds, request.CategoryIds, null, null, null);
                        totalProducts = await _auctionProductService.GetFilteredAuctionProductCountAsync(
                            request.ConditionIds, request.CategoryIds, null, null, null);
                        break;

                    case "borrow":
                        borrowProducts = _borrowProductsService.GetFilteredProducts(
                            request.Offset, request.Count, request.ConditionIds, request.CategoryIds, null, null, null);
                        totalProducts = _borrowProductsService.GetFilteredProductCount(
                            request.ConditionIds, request.CategoryIds, null, null, null);
                        break;

                    default: // "buy"
                        if (_buyProductsService is MarketMinds.Shared.Services.BuyProductsService.BuyProductsService buyService)
                        {
                            buyProducts = buyService.GetFilteredProducts(
                                request.Offset, request.Count, request.ConditionIds, request.CategoryIds, null, null);
                            totalProducts = buyService.GetFilteredProductCount(request.ConditionIds, request.CategoryIds, null, null);
                        }
                        break;
                }

                bool hasNextPage = request.Count > 0 && (request.Offset + request.Count) < totalProducts;
                bool hasPreviousPage = request.Offset > 0;

                return Json(new
                {
                    success = true,
                    buyProducts = buyProducts,
                    auctionProducts = auctionProducts,
                    borrowProducts = borrowProducts,
                    totalProducts = totalProducts,
                    hasNextPage = hasNextPage,
                    hasPreviousPage = hasPreviousPage,
                    currentOffset = request.Offset,
                    currentCount = request.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering products");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Account()
        {
            return View();
        }

        [Authorize]
        public IActionResult Create()
        {
            _logger.LogInformation("GET: Home/Create - Initializing create view");
            var model = new AuctionProduct();
            _logger.LogInformation("Created new AuctionProduct model with default values");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(AuctionProduct auctionProduct, string productType, string tagIds, string imageUrls)
        {
            _logger.LogInformation("POST: Home/Create - Starting product creation");
            _logger.LogInformation("Received parameters - productType: {ProductType}, tagIds: {TagIds}, imageUrls: {ImageUrls}",
                productType, tagIds, imageUrls);
            _logger.LogInformation("TRACE: Original EndTime from form: {EndTime}", auctionProduct?.EndTime);
            _logger.LogInformation("AuctionProduct details - Title: {Title}, Description: {Description}, StartPrice: {StartPrice}, CategoryId: {CategoryId}, ConditionId: {ConditionId}, SellerId: {SellerId}",
                auctionProduct?.Title,
                auctionProduct?.Description?.Substring(0, Math.Min(50, auctionProduct?.Description?.Length ?? 0)),
                auctionProduct?.StartPrice,
                auctionProduct?.CategoryId,
                auctionProduct?.ConditionId,
                auctionProduct?.SellerId);

            // Set default SellerId if not set
            if (auctionProduct.SellerId <= 0)
            {
                auctionProduct.SellerId = 1; // Default seller ID
                _logger.LogInformation("Setting default SellerId to 1");
            }

            // Set the Price property from StartPrice to fix validation
            // AuctionProduct inherits from Product which has a required Price property
            auctionProduct.Price = auctionProduct.StartPrice;
            _logger.LogInformation("Setting Price property to StartPrice value: {Price}", auctionProduct.Price);

            // Clear the ModelState error for Price since we've now set it properly
            ModelState.Remove("Price");
            _logger.LogInformation("Cleared ModelState error for Price field");

            // Log ModelState errors if any
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) });

                foreach (var error in errors)
                {
                    _logger.LogWarning("ModelState validation error for field '{Field}': {Errors}",
                        error.Field, string.Join(", ", error.Errors));
                }

                var allErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                _logger.LogWarning("All ModelState validation errors: {Errors}", string.Join(", ", allErrors));

            }

            if (auctionProduct.StartTime == default)
            {
                _logger.LogInformation("Setting default StartTime to current time");
                auctionProduct.StartTime = DateTime.Now;
            }

            // Only set a default EndTime if one wasn't provided or it's invalid
            // Check for both default value and a date that's earlier than startup
            _logger.LogInformation("TRACE: Before EndTime check - Current value: {EndTime}, Default: {IsDefault}, Earlier than now: {IsEarlier}",
                auctionProduct.EndTime,
                auctionProduct.EndTime == default,
                auctionProduct.EndTime < DateTime.Now);

            if (auctionProduct.EndTime == default || auctionProduct.EndTime < DateTime.Now)
            {
                _logger.LogInformation("Setting default EndTime to 7 days from now (Original was: {OriginalEndTime})", auctionProduct.EndTime);
                auctionProduct.EndTime = DateTime.Now.AddDays(7);
            }
            else
            {
                _logger.LogInformation("Using user-provided EndTime: {EndTime}", auctionProduct.EndTime);
            }

            _logger.LogInformation("TRACE: After EndTime check - Final value: {EndTime}", auctionProduct.EndTime);

            // Process image URLs
            if (!string.IsNullOrEmpty(imageUrls))
            {
                try
                {
                    _logger.LogInformation("Raw imageUrls string: {ImageUrls}", imageUrls);
                    var splitUrls = imageUrls.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    _logger.LogInformation("Split imageUrls: {SplitUrls}", string.Join(", ", splitUrls));
                    var images = _imageUploadService.ParseImagesString(imageUrls);
                    _logger.LogInformation("Successfully parsed {Count} images", images.Count);
                    auctionProduct.NonMappedImages = images;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing image URLs");
                    ModelState.AddModelError("imageUrls", "Error processing image URLs. Please try again.");
                }
            }
            else
            {
                _logger.LogWarning("No image URLs provided");
                ModelState.AddModelError("imageUrls", "At least one image is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to create auction product: {Title}", auctionProduct.Title);

                    var productTags = new List<ProductTag>();
                    if (!string.IsNullOrEmpty(tagIds))
                    {
                        _logger.LogInformation("Processing tags: {TagIds}", tagIds);
                        var tagIdList = tagIds.Split(',');
                        foreach (var tagId in tagIdList)
                        {
                            try
                            {
                                if (tagId.StartsWith("new_"))
                                {
                                    var tagTitle = tagId.Substring(4);
                                    _logger.LogInformation("Creating new tag: {TagTitle}", tagTitle);
                                    try
                                    {
                                        var newTag = _productTagService.CreateProductTag(tagTitle);
                                        productTags.Add(newTag);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, "Failed to create new tag '{TagTitle}', skipping it", tagTitle);
                                    }
                                }
                                else if (int.TryParse(tagId, out int existingTagId))
                                {
                                    try
                                    {
                                        var tag = _productTagService.GetAllProductTags().FirstOrDefault(t => t.Id == existingTagId);
                                        if (tag != null)
                                        {
                                            productTags.Add(tag);
                                        }
                                        else
                                        {
                                            _logger.LogWarning("Tag with ID {TagId} not found, skipping it", existingTagId);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, "Failed to process existing tag with ID {TagId}, skipping it", existingTagId);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing tag ID: {TagId}", tagId);
                            }
                        }
                    }

                    var productImages = new List<Image>();
                    if (!string.IsNullOrEmpty(imageUrls))
                    {
                        _logger.LogInformation("Processing image URLs: {ImageUrls}", imageUrls);
                        try
                        {
                            productImages = _imageUploadService.ParseImagesString(imageUrls);
                            _logger.LogInformation("Parsed {ImageCount} images", productImages.Count);
                            auctionProduct.NonMappedImages = productImages.ToList();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error parsing image URLs");
                        }
                    }

                    if (User.Identity.IsAuthenticated)
                    {
                        auctionProduct.SellerId = User.GetCurrentUserId();
                        _logger.LogInformation("Setting seller ID to authenticated user: {SellerId}", auctionProduct.SellerId);
                    }

                    // Set current price to start price if not set
                    if (auctionProduct.CurrentPrice <= 0)
                    {
                        auctionProduct.CurrentPrice = auctionProduct.StartPrice;
                        _logger.LogInformation("Setting current price to start price: {Price}", auctionProduct.CurrentPrice);
                    }

                    _logger.LogInformation("Auction product details before creation: " +
                        "Id={Id}, Title={Title}, Description={DescriptionLength}, " +
                        "CategoryId={CategoryId}, ConditionId={ConditionId}, " +
                        "StartPrice={StartPrice}, CurrentPrice={CurrentPrice}, " +
                        "StartTime={StartTime}, EndTime={EndTime}, " +
                        "SellerId={SellerId}, TagCount={TagCount}, ImageCount={ImageCount}",
                        auctionProduct.Id,
                        auctionProduct.Title,
                        auctionProduct.Description?.Length ?? 0,
                        auctionProduct.CategoryId,
                        auctionProduct.ConditionId,
                        auctionProduct.StartPrice,
                        auctionProduct.CurrentPrice,
                        auctionProduct.StartTime,
                        auctionProduct.EndTime,
                        auctionProduct.SellerId,
                        productTags.Count,
                        productImages.Count);

                    if (string.IsNullOrWhiteSpace(auctionProduct.Title))
                    {
                        ModelState.AddModelError("Title", "Title is required");
                    }

                    if (auctionProduct.CategoryId <= 0)
                    {
                        ModelState.AddModelError("CategoryId", "Please select a category");
                    }

                    if (auctionProduct.ConditionId <= 0)
                    {
                        ModelState.AddModelError("ConditionId", "Please select a condition");
                    }

                    if (auctionProduct.StartPrice <= 0)
                    {
                        ModelState.AddModelError("StartPrice", "Starting price must be greater than zero");
                    }

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            var testProduct = await _auctionProductService.GetAuctionProductByIdAsync(1);
                            if (testProduct != null)
                            {
                                _logger.LogInformation("Service connectivity check: Successfully retrieved a test product");
                            }
                            else
                            {
                                _logger.LogWarning("Service connectivity check: Retrieved null test product");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Service connectivity check failed");
                        }

                        // Assign the processed tags to the auction product
                        if (productTags.Any())
                        {
                            auctionProduct.Tags = productTags;
                            _logger.LogInformation("Assigned {TagCount} tags to auction product", productTags.Count);
                        }

                        // Create the auction product
                        _logger.LogInformation("TRACE: Before service call - EndTime: {EndTime}", auctionProduct.EndTime);
                        bool result = false;
                        try
                        {
                            result = await _auctionProductService.CreateAuctionProductAsync(auctionProduct);
                            _logger.LogInformation("TRACE: After service call - Result: {Result}", result);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Exception thrown by CreateAuctionProductAsync");

                            string errorMessage = ex.Message;
                            if (ex.InnerException != null)
                            {
                                if (ex.InnerException.Message.Contains("400 (Bad Request)"))
                                {
                                    errorMessage = "Please check that all required fields are filled in correctly.";
                                }
                                else
                                {
                                    errorMessage = ex.InnerException.Message;
                                }
                            }

                            ModelState.AddModelError(string.Empty, $"Failed to create product: {errorMessage}");
                            throw;
                        }

                        if (result)
                        {
                            _logger.LogInformation("Successfully created auction product");
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            _logger.LogWarning("CreateAuctionProductAsync returned false without throwing an exception");
                            ModelState.AddModelError(string.Empty, "Failed to create product. Please check the logs for details.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");

                    if (!ModelState.Any(m => m.Value.Errors.Count > 0))
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred while creating the product. Please check all fields and try again.");
                    }
                }
            }
            else
            {
                _logger.LogWarning("Invalid model state when creating auction product: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            ViewBag.Categories = _categoryService.GetAllProductCategories();
            ViewBag.Conditions = _conditionService.GetAllProductConditions();
            ViewBag.Tags = _productTagService.GetAllProductTags();

            return View(auctionProduct);
        }

        public IActionResult Basket()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateBorrowProduct(BorrowProduct borrowProduct, string tagIds, string imageUrls)
        {
            _logger.LogInformation("Creating a new borrow product");

            _logger.LogInformation("Received borrowProduct: Title={Title}, Description={Description}, " +
                "CategoryId={CategoryId}, ConditionId={ConditionId}, " +
                "TimeLimit={TimeLimit}, StartDate={StartDate}, EndDate={EndDate}, " +
                "DailyRate={DailyRate}, IsBorrowed={IsBorrowed}, " +
                "SellerId={SellerId}, tagIds={tagIds}, imageUrls={imageUrlsLength}",
                borrowProduct?.Title,
                borrowProduct?.Description?.Substring(0, Math.Min(20, borrowProduct?.Description?.Length ?? 0)) + "...",
                borrowProduct?.CategoryId,
                borrowProduct?.ConditionId,
                borrowProduct?.TimeLimit,
                borrowProduct?.StartDate,
                borrowProduct?.EndDate,
                borrowProduct?.DailyRate,
                borrowProduct?.IsBorrowed,
                borrowProduct?.SellerId,
                tagIds,
                imageUrls?.Length ?? 0);

            if (User.Identity.IsAuthenticated)
            {
                borrowProduct.SellerId = User.GetCurrentUserId();
                _logger.LogInformation("Overriding seller ID with authenticated user: {SellerId}", borrowProduct.SellerId);
            }
            else if (borrowProduct.SellerId <= 0)
            {
                borrowProduct.SellerId = 1;
                _logger.LogWarning("User not authenticated, using default seller ID: 1");
            }

            borrowProduct.Seller = new User { Id = borrowProduct.SellerId };

            // Remove Price field from validation since BorrowProduct doesn't use it
            ModelState.Remove("Price");
            ModelState.Remove("price");
            _logger.LogInformation("Removed Price field from ModelState validation for BorrowProduct");

            if (borrowProduct.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category");
                _logger.LogWarning("CategoryId was invalid or missing");
            }

            if (borrowProduct.ConditionId <= 0)
            {
                ModelState.AddModelError("ConditionId", "Please select a valid condition");
                _logger.LogWarning("ConditionId was invalid or missing");
            }

            if (borrowProduct.DailyRate <= 0)
            {
                _logger.LogWarning("DailyRate was invalid or missing, setting to default (1.0)");
                borrowProduct.DailyRate = 1.0;
            }

            if (borrowProduct.TimeLimit == default)
            {
                _logger.LogWarning("TimeLimit was not provided, setting to one month from now");
                borrowProduct.TimeLimit = DateTime.Now.AddMonths(1);
            }

            if (borrowProduct.StartDate == null)
            {
                _logger.LogWarning("StartDate was not provided, setting to now");
                borrowProduct.StartDate = DateTime.Now;
            }

            if (borrowProduct.EndDate == null)
            {
                _logger.LogWarning("EndDate was not provided, setting to one month from now");
                borrowProduct.EndDate = DateTime.Now.AddMonths(1);
            }

            var productTags = new List<ProductTag>();
            if (!string.IsNullOrEmpty(tagIds))
            {
                _logger.LogInformation("Processing tags: {TagIds}", tagIds);
                var tagIdList = tagIds.Split(',');
                foreach (var tagId in tagIdList)
                {
                    try
                    {
                        if (tagId.StartsWith("new_"))
                        {
                            var tagTitle = tagId.Substring(4);
                            _logger.LogInformation("Creating new tag: {TagTitle}", tagTitle);
                            try
                            {
                                var newTag = _productTagService.CreateProductTag(tagTitle);
                                productTags.Add(newTag);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to create new tag '{TagTitle}', skipping it", tagTitle);
                            }
                        }
                        else if (int.TryParse(tagId, out int existingTagId))
                        {
                            try
                            {
                                var tag = _productTagService.GetAllProductTags().FirstOrDefault(t => t.Id == existingTagId);
                                if (tag != null)
                                {
                                    productTags.Add(tag);
                                }
                                else
                                {
                                    _logger.LogWarning("Tag with ID {TagId} not found, skipping it", existingTagId);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to process existing tag with ID {TagId}, skipping it", existingTagId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing tag ID: {TagId}", tagId);
                    }
                }
            }

            var productImages = new List<Image>();
            if (!string.IsNullOrEmpty(imageUrls))
            {
                _logger.LogInformation("Processing image URLs: {ImageUrls}", imageUrls);
                try
                {
                    productImages = _imageUploadService.ParseImagesString(imageUrls);
                    _logger.LogInformation("Parsed {ImageCount} images", productImages.Count);
                    borrowProduct.NonMappedImages = productImages.ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing image URLs");
                }
            }

            // Assign tags to the borrow product
            borrowProduct.Tags = productTags;

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to create borrow product: {Title}", borrowProduct.Title);

                    _logger.LogInformation("Sending to service: Title={Title}, CategoryId={CategoryId}, ConditionId={ConditionId}, " +
                        "TimeLimit={TimeLimit}, StartDate={StartDate}, EndDate={EndDate}, " +
                        "DailyRate={DailyRate}, SellerId={SellerId}, " +
                        "ImageCount={ImageCount}, TagCount={TagCount}",
                        borrowProduct.Title,
                        borrowProduct.CategoryId,
                        borrowProduct.ConditionId,
                        borrowProduct.TimeLimit,
                        borrowProduct.StartDate,
                        borrowProduct.EndDate,
                        borrowProduct.DailyRate,
                        borrowProduct.SellerId,
                        borrowProduct.NonMappedImages?.Count ?? 0,
                        productTags.Count);

                    var result = false;
                    try
                    {
                        result = await _borrowProductsService.CreateBorrowProductAsync(borrowProduct);
                        _logger.LogInformation("CreateBorrowProductAsync completed with result: {Result}", result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception in CreateBorrowProductAsync: {Message}", ex.Message);
                        if (ex.InnerException != null)
                        {
                            _logger.LogError("Inner exception: {Message}", ex.InnerException.Message);
                            if (ex.InnerException.InnerException != null)
                            {
                                _logger.LogError("Inner inner exception: {Message}", ex.InnerException.InnerException.Message);
                            }
                        }
                        throw;
                    }

                    if (result)
                    {
                        _logger.LogInformation("Borrow product created successfully");
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        _logger.LogWarning("CreateBorrowProductAsync returned false without throwing an exception");
                        ModelState.AddModelError(string.Empty, "Failed to create borrow product. Please check the logs for details.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating borrow product: {Message}", ex.Message);
                    ModelState.AddModelError(string.Empty, $"Failed to create borrow product: {ex.Message}");
                }
            }
            else
            {
                // Detailed model state validation logging
                _logger.LogWarning("Invalid model state when creating borrow product");
                foreach (var modelError in ModelState)
                {
                    var field = modelError.Key;
                    var errors = modelError.Value.Errors;
                    foreach (var error in errors)
                    {
                        _logger.LogWarning("Model validation error - Field: {Field}, Error: {ErrorMessage}, AttemptedValue: {AttemptedValue}",
                            field, error.ErrorMessage, modelError.Value.AttemptedValue);
                    }
                }

                _logger.LogWarning("Invalid model state when creating borrow product: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            ViewBag.Categories = _categoryService.GetAllProductCategories();
            ViewBag.Conditions = _conditionService.GetAllProductConditions();
            ViewBag.Tags = _productTagService.GetAllProductTags();

            return View("Create", borrowProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateBuyProduct(BuyProduct buyProduct, string tagIds, string imageUrls)
        {
            _logger.LogInformation("Creating a new buy product");

            _logger.LogInformation("User authentication status: {IsAuthenticated}, Claims count: {ClaimsCount}",
                User.Identity?.IsAuthenticated, User.Claims?.Count() ?? 0);

            _logger.LogInformation("Initial buyProduct.SellerId: {SellerId}", buyProduct?.SellerId);

            _logger.LogInformation("Received buyProduct: Title={Title}, Description={Description}, " +
                "CategoryId={CategoryId}, ConditionId={ConditionId}, " +
                "Price={Price}, SellerId={SellerId}, " +
                "tagIds={tagIds}, imageUrls={imageUrlsLength}",
                buyProduct?.Title,
                buyProduct?.Description?.Substring(0, Math.Min(20, buyProduct?.Description?.Length ?? 0)) + "...",
                buyProduct?.CategoryId,
                buyProduct?.ConditionId,
                buyProduct?.Price,
                buyProduct?.SellerId,
                tagIds,
                imageUrls?.Length ?? 0);

            if (User.Identity.IsAuthenticated)
            {
                buyProduct.SellerId = User.GetCurrentUserId();
                _logger.LogInformation("Overriding seller ID with authenticated user: {SellerId}", buyProduct.SellerId);
            }
            else if (buyProduct.SellerId <= 0)
            {
                buyProduct.SellerId = 1;
                _logger.LogWarning("User not authenticated, using default seller ID: 1");
            }

            buyProduct.Seller = new User { Id = buyProduct.SellerId };
            _logger.LogInformation("Buy product seller ID after setup: {SellerId}", buyProduct.SellerId);


            if (buyProduct.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category");
                _logger.LogWarning("CategoryId was invalid or missing");
            }

            if (buyProduct.ConditionId <= 0)
            {
                ModelState.AddModelError("ConditionId", "Please select a valid condition");
                _logger.LogWarning("ConditionId was invalid or missing");
            }

            if (buyProduct.Price <= 0)
            {
                _logger.LogWarning("Price was invalid or missing, setting to default (1.0)");
                buyProduct.Price = 1.0;
            }

            var productTags = new List<ProductTag>();
            if (!string.IsNullOrEmpty(tagIds))
            {
                _logger.LogInformation("Processing tags: {TagIds}", tagIds);
                var tagIdList = tagIds.Split(',');
                foreach (var tagId in tagIdList)
                {
                    try
                    {
                        if (tagId.StartsWith("new_"))
                        {
                            var tagTitle = tagId.Substring(4);
                            _logger.LogInformation("Creating new tag: {TagTitle}", tagTitle);
                            try
                            {
                                var newTag = _productTagService.CreateProductTag(tagTitle);
                                productTags.Add(newTag);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to create new tag '{TagTitle}', skipping it", tagTitle);
                            }
                        }
                        else if (int.TryParse(tagId, out int existingTagId))
                        {
                            try
                            {
                                var tag = _productTagService.GetAllProductTags().FirstOrDefault(t => t.Id == existingTagId);
                                if (tag != null)
                                {
                                    productTags.Add(tag);
                                }
                                else
                                {
                                    _logger.LogWarning("Tag with ID {TagId} not found, skipping it", existingTagId);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to process existing tag with ID {TagId}, skipping it", existingTagId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing tag ID: {TagId}", tagId);
                    }
                }
            }

            var productImages = new List<Image>();
            if (!string.IsNullOrEmpty(imageUrls))
            {
                _logger.LogInformation("Processing image URLs: {ImageUrls}", imageUrls);
                try
                {
                    productImages = _imageUploadService.ParseImagesString(imageUrls);
                    _logger.LogInformation("Parsed {ImageCount} images", productImages.Count);
                    buyProduct.NonMappedImages = productImages.ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing image URLs");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to create buy product: {Title}", buyProduct.Title);

                    if (buyProduct.CategoryId > 0 && buyProduct.Category == null)
                    {
                        buyProduct.Category = new Category { Id = buyProduct.CategoryId };
                    }
                    if (buyProduct.ConditionId > 0 && buyProduct.Condition == null)
                    {
                        buyProduct.Condition = new Condition { Id = buyProduct.ConditionId };
                    }

                    _logger.LogInformation("Buy product details: " +
                        "Title={Title}, Description={Description}, " +
                        "CategoryId={CategoryId}, Category={Category}, " +
                        "ConditionId={ConditionId}, Condition={Condition}, " +
                        "Price={Price}, SellerId={SellerId}, Seller={Seller}, " +
                        "TagCount={TagCount}, ImageCount={ImageCount}",
                        buyProduct.Title,
                        buyProduct.Description,
                        buyProduct.CategoryId,
                        buyProduct.Category?.Id,
                        buyProduct.ConditionId,
                        buyProduct.Condition?.Id,
                        buyProduct.Price,
                        buyProduct.SellerId,
                        buyProduct.Seller?.Id,
                        productTags.Count,
                        productImages.Count);

                    if (productTags.Any())
                    {
                        buyProduct.Tags = productTags;
                    }

                    if (productImages.Any())
                    {
                        buyProduct.NonMappedImages = productImages;
                    }

                    var apiProduct = new
                    {
                        Title = buyProduct.Title,
                        Description = buyProduct.Description ?? string.Empty,
                        SellerId = buyProduct.SellerId,
                        ConditionId = buyProduct.ConditionId,
                        CategoryId = buyProduct.CategoryId,
                        Price = buyProduct.Price,
                        Stock = buyProduct.Stock,
                        Tags = productTags.Select(t => new { Id = t.Id, Title = t.Title }).ToList(),
                        Images = productImages.Select(img => new { Url = img.Url }).ToList()
                    };

                    _logger.LogInformation("Sending simplified object to API with SellerId={SellerId}, CategoryId={CategoryId}, ConditionId={ConditionId}",
                        apiProduct.SellerId, apiProduct.CategoryId, apiProduct.ConditionId);

                    var buyProductsService = HttpContext.RequestServices.GetService<MarketMinds.Shared.Services.BuyProductsService.IBuyProductsService>();
                    if (buyProductsService == null)
                    {
                        throw new InvalidOperationException("Buy Products Service is not available");
                    }

                    var repoType = buyProductsService.GetType();
                    var repositoryField = repoType.GetField("buyProductsRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (repositoryField != null)
                    {
                        var repository = repositoryField.GetValue(buyProductsService);
                        var repoMethodInfo = repository.GetType().GetMethod("CreateListing");
                        if (repoMethodInfo != null)
                        {
                            repoMethodInfo.Invoke(repository, new[] { apiProduct });
                            _logger.LogInformation("Buy product created successfully via direct repository call");
                            return RedirectToAction("Index", "BuyProducts");
                        }
                    }

                    _logger.LogInformation("About to call buyProductsService.CreateListing with SellerId={SellerId}", buyProduct.SellerId);
                    buyProductsService.CreateListing(buyProduct);
                    _logger.LogInformation("Buy product created successfully");
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating buy product: {Message}", ex.Message);
                    ModelState.AddModelError(string.Empty, $"Failed to create buy product: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("Invalid model state when creating buy product: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            ViewBag.Categories = _categoryService.GetAllProductCategories();
            ViewBag.Conditions = _conditionService.GetAllProductConditions();
            ViewBag.Tags = _productTagService.GetAllProductTags();
            return View("Create", buyProduct);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = TempData["ErrorMessage"] as string ?? "An unexpected error occurred"
            };

            _logger.LogInformation("Displaying error page. RequestId: {RequestId}, Message: {Message}",
                errorModel.RequestId, errorModel.ErrorMessage);

            return View(errorModel);
        }

        private string BuildPaginationUrl(int offset, int count, string tab, List<int>? conditionIds, List<int>? categoryIds, string? search, double? maxPrice = null)
        {
            var queryParams = new List<string>
            {
                $"offset={offset}",
                $"count={count}",
                $"tab={tab}"
            };

            if (conditionIds?.Any() == true)
            {
                queryParams.AddRange(conditionIds.Select(id => $"conditionIds={id}"));
            }

            if (categoryIds?.Any() == true)
            {
                queryParams.AddRange(categoryIds.Select(id => $"categoryIds={id}"));
            }

            if (!string.IsNullOrEmpty(search))
            {
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            }

            if (maxPrice.HasValue)
            {
                queryParams.Add($"maxPrice={maxPrice.Value}");
            }

            return $"{Url.Action("Index")}?{string.Join("&", queryParams)}";
        }
    }

    public class FilterRequest
    {
        public string Tab { get; set; } = "buy";
        public int Offset { get; set; } = 0;
        public int Count { get; set; } = 12;
        public List<int>? ConditionIds { get; set; }
        public List<int>? CategoryIds { get; set; }
        public string? Search { get; set; }
        public double? MaxPrice { get; set; }
    }
}

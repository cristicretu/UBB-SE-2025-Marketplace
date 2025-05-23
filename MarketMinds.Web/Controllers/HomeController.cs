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
        public async Task<IActionResult> Index()
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
                }
                else
                {
                    _logger.LogInformation("No authenticated user or user ID not found. Wishlist IDs will not be loaded from session or DB.");
                    ViewBag.WishlistProductIds = new List<int>();
                }

                List<BuyProduct> buyProducts = new List<BuyProduct>();

                if (_buyProductsService is MarketMinds.Shared.Services.BuyProductsService.BuyProductsService concreteService)
                {
                    var methodInfo = concreteService.GetType().GetMethod("GetProducts", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (methodInfo != null)
                    {
                        buyProducts = (List<BuyProduct>)methodInfo.Invoke(concreteService, null);

                        buyProducts = buyProducts.Where(p => p.Stock > 0).ToList();
                    }
                }

                var auctionProducts = await _auctionProductService.GetAllAuctionProductsAsync();

                var categories = _categoryService.GetAllProductCategories();
                var conditions = _conditionService.GetAllProductConditions();

                ViewBag.Categories = categories;
                ViewBag.Conditions = conditions;

                ViewBag.MinPrice = buyProducts.Any() ? (int)Math.Floor(buyProducts.Min(p => p.Price)) : 0;
                ViewBag.MaxPrice = buyProducts.Any() ? (int)Math.Ceiling(buyProducts.Max(p => p.Price)) : 1000;

                var viewModel = new HomeViewModel
                {
                    BuyProducts = buyProducts,
                    AuctionProducts = auctionProducts
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
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Account()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View(new AuctionProduct());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuctionProduct auctionProduct, string productType, string tagIds, string imageUrls)
        {
            if (auctionProduct.StartTime == default)
            {
                auctionProduct.StartTime = DateTime.Now;
            }

            if (auctionProduct.EndTime == default)
            {
                auctionProduct.EndTime = DateTime.Now.AddDays(7);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Creating a new auction product");

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
                                        var allTags = _productTagService.GetAllProductTags();
                                        var tag = allTags.FirstOrDefault(t => t.Id == existingTagId);
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
                        _logger.LogInformation("Overriding seller ID with authenticated user: {SellerId}", auctionProduct.SellerId);
                    }
                    else if (auctionProduct.SellerId <= 0)
                    {
                        auctionProduct.SellerId = 1;
                        _logger.LogWarning("User not authenticated, using default seller ID: 1");
                    }

                    if (auctionProduct.CurrentPrice <= 0)
                    {
                        auctionProduct.CurrentPrice = auctionProduct.StartPrice;
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

                        bool result = false;
                        try
                        {
                            result = await _auctionProductService.CreateAuctionProductAsync(auctionProduct);
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
                            _logger.LogInformation("Auction product created successfully");
                            return RedirectToAction("Index", "AuctionProducts");
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
                                var allTags = _productTagService.GetAllProductTags();
                                var tag = allTags.FirstOrDefault(t => t.Id == existingTagId);
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
                        return RedirectToAction("Index", "BorrowProducts");
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
                _logger.LogWarning("Invalid model state when creating borrow product: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            ViewBag.Categories = _categoryService.GetAllProductCategories();
            ViewBag.Conditions = _conditionService.GetAllProductConditions();
            ViewBag.Tags = _productTagService.GetAllProductTags();

            return View("Create", new BorrowProduct());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                                var allTags = _productTagService.GetAllProductTags();
                                var tag = allTags.FirstOrDefault(t => t.Id == existingTagId);
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
                    return RedirectToAction("Index", "BuyProducts");
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

            return View("Create", new BuyProduct());
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = HttpContext.Items["ErrorMessage"]?.ToString()
            });
        }
    }
}
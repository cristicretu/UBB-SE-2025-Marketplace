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

        public HomeController(
            ILogger<HomeController> logger,
            IAuctionProductService auctionProductService,
            IProductTagService productTagService,
            IProductCategoryService categoryService,
            IProductConditionService conditionService,
            IImageUploadService imageUploadService,
            IBorrowProductsService borrowProductsService,
            IBuyProductsService buyProductsService)
        {
            _logger = logger;
            _auctionProductService = auctionProductService;
            _productTagService = productTagService;
            _categoryService = categoryService;
            _conditionService = conditionService;
            _imageUploadService = imageUploadService;
            _borrowProductsService = borrowProductsService;
            _buyProductsService = buyProductsService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Work around the missing GetProducts method by creating an empty list initially
                List<BuyProduct> buyProducts = new List<BuyProduct>();
                
                // Try to access the concrete implementation if possible
                if (_buyProductsService is MarketMinds.Shared.Services.BuyProductsService.BuyProductsService concreteService)
                {
                    // Use reflection to call the GetProducts method
                    var methodInfo = concreteService.GetType().GetMethod("GetProducts", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (methodInfo != null)
                    {
                        buyProducts = (List<BuyProduct>)methodInfo.Invoke(concreteService, null);
                    }
                }
                
                var auctionProducts = await _auctionProductService.GetAllAuctionProductsAsync();
                
                // Get categories and conditions for filters
                var categories = _categoryService.GetAllProductCategories();
                var conditions = _conditionService.GetAllProductConditions();
                
                // Pass data to ViewBag for filters
                ViewBag.Categories = categories;
                ViewBag.Conditions = conditions;
                
                // Get min and max prices for the price filter
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

        // GET: Home/Account
        public IActionResult Account()
        {
            // This is a placeholder for future account management functionality
            return View();
        }

        // GET: Home/Create
        public IActionResult Create()
        {
            _logger.LogInformation("GET: Home/Create - Initializing create view");
            var model = new AuctionProduct();
            _logger.LogInformation("Created new AuctionProduct model with default values");
            return View(model);
        }

        // POST: Home/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            // Log ModelState errors if any
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState validation errors: {Errors}", string.Join(", ", errors));
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

                    // Process tags
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
                                    var newTag = _productTagService.CreateProductTag(tagTitle);
                                    productTags.Add(newTag);
                                }
                                else if (int.TryParse(tagId, out int existingTagId))
                                {
                                    var tag = _productTagService.GetAllProductTags().FirstOrDefault(t => t.Id == existingTagId);
                                    if (tag != null)
                                    {
                                        productTags.Add(tag);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error processing tag ID: {TagId}", tagId);
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
                            // Set the images to the product (implementation may vary based on your model)
                            auctionProduct.NonMappedImages = productImages.ToList();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error parsing image URLs");
                            // Continue without images rather than failing the entire request
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

                    // Log detailed product info before creating
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

                    // Check required fields before submitting
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

                    // Provide a user-friendly error message
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

            // If we get here, something went wrong
            // Reload categories, conditions, and tags for the view
            ViewBag.Categories = _categoryService.GetAllProductCategories();
            ViewBag.Conditions = _conditionService.GetAllProductConditions();
            ViewBag.Tags = _productTagService.GetAllProductTags();

            return View(auctionProduct);
        }

        // GET: Home/Basket
        public IActionResult Basket()
        {
            // This is a placeholder for future basket functionality
            return View();
        }

        // POST: Home/CreateBorrowProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBorrowProduct(BorrowProduct borrowProduct, string tagIds, string imageUrls)
        {
            _logger.LogInformation("Creating a new borrow product");

            // Debug log all received values
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

            // CRITICAL DATA VALIDATION AND FIXING
            // ===================================

            // Ensure we have a valid SellerId
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

            // Ensure we have a valid Seller object
            borrowProduct.Seller = new User { Id = borrowProduct.SellerId };

            // Ensure we have valid Category and Condition
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

            // Ensure DailyRate is set
            if (borrowProduct.DailyRate <= 0)
            {
                _logger.LogWarning("DailyRate was invalid or missing, setting to default (1.0)");
                borrowProduct.DailyRate = 1.0;
            }

            // Ensure TimeLimit is set
            if (borrowProduct.TimeLimit == default)
            {
                _logger.LogWarning("TimeLimit was not provided, setting to one month from now");
                borrowProduct.TimeLimit = DateTime.Now.AddMonths(1);
            }

            // Ensure StartDate is set
            if (borrowProduct.StartDate == null)
            {
                _logger.LogWarning("StartDate was not provided, setting to now");
                borrowProduct.StartDate = DateTime.Now;
            }

            // Ensure EndDate is set
            if (borrowProduct.EndDate == null)
            {
                _logger.LogWarning("EndDate was not provided, setting to one month from now");
                borrowProduct.EndDate = DateTime.Now.AddMonths(1);
            }

            // Process tags - Error resilient version that won't fail the entire request if tag processing fails
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
                            var tagTitle = tagId.Substring(4); // Remove "new_" prefix
                            _logger.LogInformation("Creating new tag: {TagTitle}", tagTitle);
                            try
                            {
                                var newTag = _productTagService.CreateProductTag(tagTitle);
                                productTags.Add(newTag);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to create new tag '{TagTitle}', skipping it", tagTitle);
                                // Don't stop the whole process for a tag creation failure
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
                                // Don't stop the whole process for a tag processing failure
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing tag ID: {TagId}", tagId);
                        // Don't stop the whole process if one tag fails
                    }
                }
            }

            // Process image URLs
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
                    // Continue without images rather than failing the entire request
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to create borrow product: {Title}", borrowProduct.Title);

                    // Create the borrow product using the service
                    var result = false;
                    try
                    {
                        // Log the complete object being sent
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

                        // Attempt creation
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

            // If we get here, something went wrong
            // Reload categories, conditions, and tags for the view
            ViewBag.Categories = _categoryService.GetAllProductCategories();
            ViewBag.Conditions = _conditionService.GetAllProductConditions();
            ViewBag.Tags = _productTagService.GetAllProductTags();

            return View("Create", new BorrowProduct());
        }

        // POST: Home/CreateBuyProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBuyProduct(BuyProduct buyProduct, string tagIds, string imageUrls)
        {
            _logger.LogInformation("Creating a new buy product");
            
            // Log user authentication status and count of claims
            _logger.LogInformation("User authentication status: {IsAuthenticated}, Claims count: {ClaimsCount}", 
                User.Identity?.IsAuthenticated, User.Claims?.Count() ?? 0);
            
            // Debug log all received values
            _logger.LogInformation("Initial buyProduct.SellerId: {SellerId}", buyProduct?.SellerId);
            
            // Debug log all received values
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

            // Ensure we have a valid SellerId
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
            
            // Ensure we have a valid Seller object
            buyProduct.Seller = new User { Id = buyProduct.SellerId };
            _logger.LogInformation("Buy product seller ID after setup: {SellerId}", buyProduct.SellerId);
            
            // Ensure we have valid Category and Condition
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

            // Ensure Price is set
            if (buyProduct.Price <= 0)
            {
                _logger.LogWarning("Price was invalid or missing, setting to default (1.0)");
                buyProduct.Price = 1.0;
            }

            // Process tags - Error resilient version that won't fail the entire request if tag processing fails
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
                            var tagTitle = tagId.Substring(4); // Remove "new_" prefix
                            _logger.LogInformation("Creating new tag: {TagTitle}", tagTitle);
                            try
                            {
                                var newTag = _productTagService.CreateProductTag(tagTitle);
                                productTags.Add(newTag);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to create new tag '{TagTitle}', skipping it", tagTitle);
                                // Don't stop the whole process for a tag creation failure
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
                                // Don't stop the whole process for a tag processing failure
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing tag ID: {TagId}", tagId);
                        // Don't stop the whole process if one tag fails
                    }
                }
            }

            // Process image URLs
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
                    // Continue without images rather than failing the entire request
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to create buy product: {Title}", buyProduct.Title);

                    // Set Category and Condition objects
                    if (buyProduct.CategoryId > 0 && buyProduct.Category == null)
                    {
                        buyProduct.Category = new Category { Id = buyProduct.CategoryId };
                    }
                    if (buyProduct.ConditionId > 0 && buyProduct.Condition == null)
                    {
                        buyProduct.Condition = new Condition { Id = buyProduct.ConditionId };
                    }

                    // Add detailed logging
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

                    // Ensure tags and images are properly attached to the product
                    if (productTags.Any())
                    {
                        buyProduct.Tags = productTags;
                    }

                    if (productImages.Any())
                    {
                        buyProduct.NonMappedImages = productImages;
                    }

                    // Create a simplified object that matches exactly what the API expects
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

                    // Create the buy product using the service with custom object
                    var buyProductsService = HttpContext.RequestServices.GetService<MarketMinds.Shared.Services.BuyProductsService.IBuyProductsService>();
                    if (buyProductsService == null)
                    {
                        throw new InvalidOperationException("Buy Products Service is not available");
                    }

                    // Use reflection to call a non-public method that accepts our custom object
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
                    // Fallback to standard method if reflection fails
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
            
            // If we get here, something went wrong
            // Reload categories, conditions, and tags for the view
            ViewBag.Categories = _categoryService.GetAllProductCategories();
            ViewBag.Conditions = _conditionService.GetAllProductConditions();
            ViewBag.Tags = _productTagService.GetAllProductTags();
            return View("Create", new BuyProduct());
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
    }
} 

using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
    public class BuyProductsController : Controller
    {
        private readonly ILogger<BuyProductsController> _logger;
        private readonly IBuyProductsService _buyProductsService;
        private readonly IProductService _productService;
        private readonly IBuyerService _buyerService;
        private const string WishlistSessionKey = "WishlistProductIds";

        public BuyProductsController(
            ILogger<BuyProductsController> logger,
            IBuyProductsService buyProductsService,
            IProductService productService,
            IBuyerService buyerService)
        {
            _logger = logger;
            _buyProductsService = buyProductsService;
            _productService = productService;
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

        // GET: BuyProducts
        [AllowAnonymous]
        public IActionResult Index()
        {
            try
            {
                _logger.LogInformation("Fetching all buy products");
                var products = _productService.GetSortedFilteredProducts(null, null, null, null, null);
                var buyProducts = new List<BuyProduct>();

                foreach (var product in products)
                {
                    if (product is BuyProduct buyProduct)
                    {
                        buyProducts.Add(buyProduct);
                    }
                }

                return View(buyProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching buy products");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching buy products");
                return View(new List<BuyProduct>());
            }
        }

        // GET: BuyProducts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // _logger.LogInformation($"Fetching buy product with ID {id}");
                // BEST PIECE OF CODE EVER WRITTEN IN THE HISTORY OF CODE WRITING
                // I'm so proud of myself for writing this code
                // I'm so proud of myself for writing this code
                // I'm so proud of myself for writing this code
                // I'm so proud of myself for writing this code
                // I'm so proud of myself for writing this code
                // I'm so proud of myself for writing this code
                // // We need to get the product from the products list and cast it to BuyProduct
                // var products = _productService.GetSortedFilteredProducts(null, null, null, null, null);
                // BuyProduct buyProduct = null;

                // foreach (var product in products)
                // {
                //     if (product is BuyProduct bp && bp.Id == id)
                //     {
                //         buyProduct = bp;
                //         break;
                //     }
                // }

                var buyProduct = await _productService.GetProductByIdAsync(id);

                if (buyProduct == null)
                {
                    _logger.LogWarning($"Buy product with ID {id} not found");
                    return NotFound();
                }

                // Get wishlist IDs from session or buyer service
                ViewBag.WishlistProductIds = new List<int>();
                if (User.Identity.IsAuthenticated && UserSession.CurrentUserId.HasValue)
                {
                    var wishlistIdsJson = HttpContext.Session.GetString(WishlistSessionKey);
                    if (!string.IsNullOrEmpty(wishlistIdsJson))
                    {
                        ViewBag.WishlistProductIds = JsonSerializer.Deserialize<HashSet<int>>(wishlistIdsJson)?.ToList() ?? new List<int>();
                    }
                    else
                    {
                        var buyer = await _buyerService.GetBuyerByUser(new User(UserSession.CurrentUserId.Value));
                        if (buyer != null)
                        {
                            await _buyerService.LoadBuyer(buyer, BuyerDataSegments.Wishlist);
                            var ids = buyer.Wishlist?.Items?.Select(x => x.ProductId).ToList() ?? new List<int>();
                            HttpContext.Session.SetString(WishlistSessionKey, JsonSerializer.Serialize(ids.ToHashSet()));
                            ViewBag.WishlistProductIds = ids;
                        }
                    }
                }

                return View(buyProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching buy product {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: BuyProducts/Create
        public IActionResult Create()
        {
            return RedirectToAction("Create", "Home");
        }
    }
}
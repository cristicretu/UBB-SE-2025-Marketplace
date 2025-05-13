using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BuyProductsService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
    public class BuyProductsController : Controller
    {
        private readonly ILogger<BuyProductsController> _logger;
        private readonly IBuyProductsService _buyProductsService;
        private readonly IProductService _productService;

        public BuyProductsController(
            ILogger<BuyProductsController> logger,
            IBuyProductsService buyProductsService,
            IProductService productService)
        {
            _logger = logger;
            _buyProductsService = buyProductsService;
            _productService = productService;
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
        public IActionResult Details(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching buy product with ID {id}");
                // We need to get the product from the products list and cast it to BuyProduct
                var products = _productService.GetSortedFilteredProducts(null, null, null, null, null);
                BuyProduct buyProduct = null;
                
                foreach (var product in products)
                {
                    if (product is BuyProduct bp && bp.Id == id)
                    {
                        buyProduct = bp;
                        break;
                    }
                }
                
                if (buyProduct == null)
                {
                    _logger.LogWarning($"Buy product with ID {id} not found");
                    return NotFound();
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
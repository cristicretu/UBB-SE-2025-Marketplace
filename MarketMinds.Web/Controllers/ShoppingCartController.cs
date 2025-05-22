using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebMarketplace.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IBuyProductsService _buyProductsService;

        // Only have one constructor that accepts both services
        public ShoppingCartController(IShoppingCartService shoppingCartService, IBuyProductsService buyProductsService)
        {
            _shoppingCartService = shoppingCartService ?? throw new ArgumentNullException(nameof(shoppingCartService));
            _buyProductsService = buyProductsService ?? throw new ArgumentNullException(nameof(buyProductsService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int buyerId = GetCurrentBuyerId();
            var cartItems = await _shoppingCartService.GetCartItemsAsync(buyerId);

            // Create a dictionary to store quantities for each product
            var quantities = new Dictionary<int, int>();
            double calculatedTotal = 0;

            // Transform the product list to ensure BuyProducts with correct Stock values
            var enhancedCartItems = new List<Product>();

            foreach (var item in cartItems)
            {
                // Try to get the actual BuyProduct with stock information
                BuyProduct buyProduct = null;
                try
                {
                    // Use sync version for immediate results
                    buyProduct = _buyProductsService.GetProductById(item.Id);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting buy product: {ex.Message}");
                }

                if (buyProduct != null)
                {
                    enhancedCartItems.Add(buyProduct);

                    var quantity = await _shoppingCartService.GetProductQuantityAsync(buyerId, item.Id);

                    // Ensure the quantity doesn't exceed stock
                    if (quantity > buyProduct.Stock)
                    {
                        quantity = buyProduct.Stock;
                        // Update the quantity in the cart
                        await _shoppingCartService.UpdateProductQuantityAsync(buyerId, item.Id, quantity);
                    }

                    quantities[item.Id] = quantity;
                    calculatedTotal += buyProduct.Price * quantity;
                }
                else
                {
                    // If we can't get a BuyProduct, still add the original product
                    enhancedCartItems.Add(item);
                    var quantity = await _shoppingCartService.GetProductQuantityAsync(buyerId, item.Id);
                    quantities[item.Id] = quantity;
                    calculatedTotal += item.Price * quantity;
                }
            }

            // Use the calculated total instead of relying solely on the service
            ViewBag.CartTotal = calculatedTotal;
            ViewBag.Quantities = quantities;
            return View(enhancedCartItems);
        }

        // Helper method to get a product with stock information
        private async Task<BuyProduct> GetProductWithStockAsync(int productId)
        {
            try
            {
                // Use the dedicated buy products service
                var product = await _buyProductsService.GetProductByIdAsync(productId);
                if (product is BuyProduct buyProduct)
                {
                    return buyProduct;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting product: {ex.Message}");
                return null;
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                if (UserSession.CurrentUserRole == "Seller")
                {
                    return NoContent();
                }

                int buyerId = GetCurrentBuyerId();
                await _shoppingCartService.AddProductToCartAsync(buyerId, productId, quantity);
                return RedirectToAction("Index");
            }
            catch
            {
                return NoContent();
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                int buyerId = GetCurrentBuyerId();
                await _shoppingCartService.RemoveProductFromCartAsync(buyerId, productId);

                // For AJAX requests return success
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }

                // For regular form posts, redirect
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            try
            {
                // Validate quantity
                if (quantity <= 0)
                {
                    return BadRequest(new { success = false, message = "Quantity must be greater than zero." });
                }

                // Get product to check stock
                BuyProduct buyProduct = null;
                try
                {
                    // Try direct sync access first
                    buyProduct = _buyProductsService.GetProductById(productId);
                }
                catch
                {
                    // Fall back to async if needed
                    buyProduct = await GetProductWithStockAsync(productId);
                }

                if (buyProduct != null && quantity > buyProduct.Stock)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Cannot add more than the available stock ({buyProduct.Stock})."
                    });
                }

                int buyerId = GetCurrentBuyerId();
                await _shoppingCartService.UpdateProductQuantityAsync(buyerId, productId, quantity);

                // Calculate total manually
                var cartItems = await _shoppingCartService.GetCartItemsAsync(buyerId);
                double calculatedTotal = 0;

                foreach (var item in cartItems)
                {
                    var itemQuantity = await _shoppingCartService.GetProductQuantityAsync(buyerId, item.Id);
                    calculatedTotal += item.Price * itemQuantity;
                }

                // Get the specific product price for this item total
                var productPrice = await _shoppingCartService.GetProductPriceAsync(buyerId, productId);
                var itemTotal = productPrice * quantity;

                // Return JSON with Ok status code
                return Ok(new
                {
                    success = true,
                    message = "Quantity updated successfully",
                    total = calculatedTotal.ToString("C"),
                    itemTotal = itemTotal.ToString("C")
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            int buyerId = GetCurrentBuyerId();
            await _shoppingCartService.ClearCartAsync(buyerId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            int buyerId = GetCurrentBuyerId();
            var cartItems = await _shoppingCartService.GetCartItemsAsync(buyerId);

            if (cartItems == null || cartItems.Count == 0)
            {
                TempData["Error"] = "Your cart is empty. Please add items before checkout.";
                return RedirectToAction("Index");
            }

            // Redirect to checkout page
            return RedirectToAction("BillingInfo", "Checkout");
        }

        [HttpGet]
        public async Task<IActionResult> GetCartTotal()
        {
            try
            {
                int buyerId = GetCurrentBuyerId();

                // Calculate total manually
                var cartItems = await _shoppingCartService.GetCartItemsAsync(buyerId);
                double calculatedTotal = 0;

                foreach (var item in cartItems)
                {
                    var itemQuantity = await _shoppingCartService.GetProductQuantityAsync(buyerId, item.Id);
                    calculatedTotal += item.Price * itemQuantity;
                }

                return Content(calculatedTotal.ToString("C"), "text/plain");
            }
            catch (Exception)
            {
                return Content("$0.00", "text/plain");
            }
        }


        //HARDCODED, TO BE REPLACED WITH REAL AUTHENTICATION WHEN MERGING
        private int GetCurrentBuyerId()
        {
            // In a real application, you would get the current user's ID from claims
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                // If not logged in or ID not available, redirect to login
                throw new InvalidOperationException("User is not authenticated or buyer ID not available");
            }

            return userId;
        }
    }
}

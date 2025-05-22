using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Services;

namespace WebMarketplace.Controllers
{
    // [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;

        public ShoppingCartController(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService ?? throw new ArgumentNullException(nameof(shoppingCartService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int buyerId = GetCurrentBuyerId();
            var cartItems = await _shoppingCartService.GetCartItemsAsync(buyerId);

            // Create a dictionary to store quantities for each product
            var quantities = new Dictionary<int, int>();
            double calculatedTotal = 0;

            foreach (var item in cartItems)
            {
                var quantity = await _shoppingCartService.GetProductQuantityAsync(buyerId, item.Id);
                quantities[item.Id] = quantity;

                // Calculate the item total and add to cart total
                calculatedTotal += item.Price * quantity;
            }

            // Use the calculated total instead of relying solely on the service
            ViewBag.CartTotal = calculatedTotal;
            ViewBag.Quantities = quantities;
            return View(cartItems);
        }

        [HttpPost]
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
            //var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            //{
            //    // If not logged in or ID not available, redirect to login
            //    throw new InvalidOperationException("User is not authenticated or buyer ID not available");
            //}

            //return userId;

            return UserSession.CurrentUserId ?? 1; // For testing purposes, return a hardcoded buyer ID
        }
    }
}

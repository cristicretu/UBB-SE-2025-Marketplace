using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Service;
using SharedClassLibrary.Domain;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Authorization;

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
            var total = await _shoppingCartService.GetCartTotalAsync(buyerId);

            ViewBag.CartTotal = total;
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            int buyerId = GetCurrentBuyerId();
            await _shoppingCartService.AddProductToCartAsync(buyerId, productId, quantity);
            return RedirectToAction("Index");
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
                int buyerId = GetCurrentBuyerId();
                await _shoppingCartService.UpdateProductQuantityAsync(buyerId, productId, quantity);
                var total = await _shoppingCartService.GetCartTotalAsync(buyerId);

                // Return JSON with Ok status code
                return Ok(new
                {
                    success = true,
                    message = "Quantity updated successfully",
                    total = total.ToString("C"),
                    itemTotal = (await _shoppingCartService.GetProductPriceAsync(buyerId, productId) * quantity).ToString("C")
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
                var total = await _shoppingCartService.GetCartTotalAsync(buyerId);
                return Content(total.ToString("C"), "text/plain");
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

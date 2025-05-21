using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IOrderHistoryService _orderHistoryService;
        private readonly IOrderSummaryService _orderSummaryService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IDummyWalletService _dummyWalletService;
        private readonly IShoppingCartService _shoppingCartService;

        public CheckoutController(
            IOrderHistoryService orderHistoryService,
            IOrderSummaryService orderSummaryService,
            IOrderService orderService,
            IProductService productService,
            IDummyWalletService dummyWalletService,
            IShoppingCartService shoppingCartService)
        {
            _orderHistoryService = orderHistoryService;
            _orderSummaryService = orderSummaryService;
            _orderService = orderService;
            _productService = productService;
            _dummyWalletService = dummyWalletService;
            _shoppingCartService = shoppingCartService;
        }

        public async Task<IActionResult> BillingInfo(int orderHistoryId)
        {
            var model = new BillingInfoViewModel(orderHistoryId);

            // Get cart items and populate the model
            try
            {
                var cartItems = await _shoppingCartService.GetCartItemsAsync(UserSession.CurrentUserId ?? 1);
                model.ProductList = cartItems;
                model.CalculateOrderTotal(); // Calculate totals after getting cart items
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - we want to show the page even if cart loading fails
                System.Diagnostics.Debug.WriteLine($"Error loading cart items: {ex.Message}");
                model.ProductList = new List<Product>();
                model.Subtotal = 0;
                model.DeliveryFee = 0;
                model.Total = 0;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BillingInfo(BillingInfoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Re-load cart items to maintain the view state if validation fails
                try
                {
                    var cartItems = await _shoppingCartService.GetCartItemsAsync(UserSession.CurrentUserId ?? 1);
                    model.ProductList = cartItems;
                    model.CalculateOrderTotal();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reloading cart items on validation failure: {ex.Message}");
                    model.ProductList = new List<Product>();
                }

                return View(model);
            }

            // Process payment method specific logic (e.g., wallet refill)
            if (model.SelectedPaymentMethod == "wallet")
            {
                await ProcessWalletRefill(model);
            }

            // Redirect to the FinalizePurchase confirmation page
            // NOTE: Order creation logic needs to be implemented elsewhere or the Order model needs to be updated.
            return RedirectToAction("FinalizePurchase", new { orderHistoryId = model.OrderHistoryID });
        }

        public IActionResult CardInfo(int orderHistoryId)
        {
            var model = new CardInfoViewModel(orderHistoryId);
            return View(model);
        }

        [HttpPost]
        public IActionResult CardInfo(CardInfoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Process card payment
            return RedirectToAction("FinalizePurchase", new { orderHistoryId = model.OrderHistoryID });
        }

        public async Task<IActionResult> FinalizePurchase(int orderHistoryId)
        {
            var model = new FinalizePurchaseViewModel(orderHistoryId);

            // Fetch cart items and populate the model
            try
            {
                var cartItems = await _shoppingCartService.GetCartItemsAsync(UserSession.CurrentUserId ?? 1);
                model.ProductList = cartItems;

                // Calculate totals (assuming a similar logic to BillingInfoViewModel's CalculateOrderTotal)
                if (cartItems != null && cartItems.Any())
                {
                    double subtotalProducts = 0;
                    foreach (var product in cartItems)
                    {
                        if (product is BuyProduct buyProduct)
                        {
                            subtotalProducts += buyProduct.Price;
                        }
                        else if (product is BorrowProduct borrowProduct)
                        {
                            // Handle borrow product pricing if different
                            subtotalProducts += product.Price; // Use base price as fallback
                        }
                        else
                        {
                            subtotalProducts += product.Price;
                        }
                    }

                    model.Subtotal = subtotalProducts;

                    // Determine product type for delivery fee calculation (assuming same logic)
                    bool hasSpecialType = cartItems.Any(p =>
                         (p is BorrowProduct) ||
                         (p.GetType().Name.Contains("Refill")) ||
                         (p.GetType().Name.Contains("Auction")));

                    if (subtotalProducts >= 200 || hasSpecialType)
                    {
                        model.Total = subtotalProducts;
                        model.DeliveryFee = 0;
                    }
                    else
                    {
                        model.DeliveryFee = 13.99;
                        model.Total = subtotalProducts + model.DeliveryFee;
                    }
                }
                else
                {
                    model.Subtotal = 0;
                    model.DeliveryFee = 0;
                    model.Total = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cart items for FinalizePurchase: {ex.Message}");
                model.ProductList = new List<Product>();
                model.Subtotal = 0;
                model.DeliveryFee = 0;
                model.Total = 0;
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateBorrowedTax(int productId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // Create a BuyProduct instance instead of the abstract Product class
                var product = new BuyProduct
                {
                    Id = productId, // Use Id instead of ProductId
                    Title = "Borrowed Product", // Add a title for the placeholder
                    Price = 100 // Default price that will be modified by the borrowed tax calculation
                };

                var model = new BillingInfoViewModel();
                model.StartDate = startDate;
                model.EndDate = endDate;
                await model.ApplyBorrowedTax(product);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task ProcessWalletRefill(BillingInfoViewModel model)
        {
            double walletBalance = await _dummyWalletService.GetWalletBalanceAsync(1);
            double newBalance = walletBalance - model.Total;
            await _dummyWalletService.UpdateWalletBalance(1, newBalance);
        }
    }
}
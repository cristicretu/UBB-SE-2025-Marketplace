using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;
using WebMarketplace.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

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
            // Fetch cart items early, as they are needed for both validation failure and order creation
            List<Product> cartItems = new List<Product>();
            try
            {
                cartItems = await _shoppingCartService.GetCartItemsAsync(UserSession.CurrentUserId ?? 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cart items initially: {ex.Message}");
                // If cart loading fails, we might want to show an error or proceed with an empty cart
                // For now, let's allow proceeding, and order creation will likely fail or create an empty order if cartItems is empty.
            }
            model.ProductList = cartItems; // Ensure model has the latest cart items
            // Recalculate totals if cart items were just fetched, in case they changed or failed to load previously
            // model.CalculateOrderTotal(); // This should be called after ProductList is set.

            if (!ModelState.IsValid)
            {
                // Model state is invalid, ProductList is already set from above.
                // We might need to re-calculate totals if they depend on other model properties that might have been invalid.
                model.CalculateOrderTotal(); // Recalculate with potentially invalid model data but correct product list.
                return View(model);
            }

            // Process payment method specific logic (e.g., wallet refill)
            if (model.SelectedPaymentMethod == "wallet")
            {
                // Ensure total is calculated before wallet processing
                model.CalculateOrderTotal(); 
                await ProcessWalletRefill(model);
            }
            else
            {
                // Ensure totals are calculated if not wallet payment either
                model.CalculateOrderTotal();
            }

            // Create the order
            try
            {
                var userId = UserSession.CurrentUserId ?? 1; // Fallback to a default user ID if null
                
                // Ensure cartItems are available for order creation
                if (cartItems == null || !cartItems.Any())
                {
                    // Handle empty cart scenario - perhaps redirect to cart page with a message
                    ModelState.AddModelError(string.Empty, "Your cart is empty. Please add items before proceeding.");
                    return View(model); // Return to view with error
                }

                // Create DTO from ViewModel
                var orderRequestDto = new OrderCreationRequestDto
                {
                    Subtotal = model.Subtotal,
                    WarrantyTax = model.WarrantyTax,
                    DeliveryFee = model.DeliveryFee,
                    Total = model.Total,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    ZipCode = model.ZipCode,
                    AdditionalInfo = model.AdditionalInfo,
                    SelectedPaymentMethod = model.SelectedPaymentMethod
                };

                var newOrderHistoryId = await _orderService.CreateOrderFromCartAsync(orderRequestDto, userId, cartItems);
                model.OrderHistoryID = newOrderHistoryId; // Update the model with the new ID for the redirect
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating order: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while creating your order. Please try again.");
                // ProductList is already set from above.
                model.CalculateOrderTotal(); // Recalculate for view
                return View(model);
            }

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
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found." });
                }

                if (startDate > endDate)
                {
                    return Json(new { success = false, message = "Start date cannot be after end date." });
                }

                int monthsBorrowed = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;
                if (monthsBorrowed <= 0)
                {
                    monthsBorrowed = 1; // Minimum 1 month for calculation
                }

                double warrantyTaxRate = 0.2; // Example: 20% warranty tax
                double originalPrice = product.Price; // Assuming product.Price is the base price per month for borrowable items
                
                double totalBorrowPrice = originalPrice * monthsBorrowed;
                double calculatedWarrantyTax = totalBorrowPrice * warrantyTaxRate;
                double finalPriceWithTax = totalBorrowPrice + calculatedWarrantyTax;

                // The client-side will need to know the new subtotal/total after this change.
                // This response should provide enough info for the client to update relevant parts of the order summary display.
                // For now, returning the calculated tax and the final price for this specific item.
                // The client-side JS will then need to recalculate the overall order total or refresh the page.

                // Note: This action currently only calculates. It does not persist these values anywhere permanently yet.
                // The actual order creation (CreateOrderFromCartAsync) will use the values present in the cart items and BillingInfoViewModel/DTO at that time.
                // If this tax needs to be stored before finalizing purchase, more logic would be needed here or in a service.

                return Json(new 
                {
                    success = true, 
                    productId = productId,
                    originalItemPrice = originalPrice, // Price per period (e.g. month)
                    monthsBorrowed = monthsBorrowed,
                    borrowPriceBeforeTax = totalBorrowPrice, // Total price for the duration, before tax
                    calculatedWarrantyTax = calculatedWarrantyTax, // The tax amount for this item
                    finalItemPriceWithTax = finalPriceWithTax, // Total price for this item for the duration, including tax
                    message = "Tax calculated successfully. Please ensure your cart totals are updated if necessary."
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateBorrowedTax: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while calculating tax: " + ex.Message });
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
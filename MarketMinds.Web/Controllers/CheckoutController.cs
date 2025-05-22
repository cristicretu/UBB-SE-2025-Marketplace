using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;
using WebMarketplace.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Text.Json;

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
            List<Product> cartItems = new List<Product>();
            try
            {
                cartItems = await _shoppingCartService.GetCartItemsAsync(UserSession.CurrentUserId ?? 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cart items initially: {ex.Message}");
                // Potentially add a model error here if cart loading is critical before validation
            }
            model.ProductList = cartItems;

            if (!ModelState.IsValid)
            {
                model.CalculateOrderTotal();
                return View(model);
            }

            if (model.SelectedPaymentMethod == "wallet")
            {
                model.CalculateOrderTotal();
                await ProcessWalletRefill(model);
            }
            else
            {
                model.CalculateOrderTotal();
            }

            try
            {
                var userId = UserSession.CurrentUserId ?? 1;

                if (cartItems == null || !cartItems.Any())
                {
                    ModelState.AddModelError(string.Empty, "Your cart is empty. Please add items before proceeding.");
                    return View(model);
                }

                var orderRequestDto = new OrderCreationRequestDto { /* ... (mapping) ... */ }; // Assume mapping is correct as before
                // Mapping DTO from ViewModel
                orderRequestDto.Subtotal = model.Subtotal;
                orderRequestDto.WarrantyTax = model.WarrantyTax;
                orderRequestDto.DeliveryFee = model.DeliveryFee;
                orderRequestDto.Total = model.Total;
                orderRequestDto.FullName = model.FullName;
                orderRequestDto.Email = model.Email;
                orderRequestDto.PhoneNumber = model.PhoneNumber;
                orderRequestDto.Address = model.Address;
                orderRequestDto.ZipCode = model.ZipCode;
                orderRequestDto.AdditionalInfo = model.AdditionalInfo;
                orderRequestDto.SelectedPaymentMethod = model.SelectedPaymentMethod;

                var newOrderHistoryId = await _orderService.CreateOrderFromCartAsync(orderRequestDto, userId, cartItems);
                model.OrderHistoryID = newOrderHistoryId;

                // Save this data in TempData so the next request can access it
                TempData["OrderFullName"] = model.FullName;
                TempData["OrderEmail"] = model.Email;
                TempData["OrderPhone"] = model.PhoneNumber;
                TempData["OrderAddress"] = model.Address;
                TempData["OrderPaymentMethod"] = model.SelectedPaymentMethod;
                TempData["OrderSubtotal"] = model.Subtotal.ToString();
                TempData["OrderDeliveryFee"] = model.DeliveryFee.ToString();
                TempData["OrderTotal"] = model.Total.ToString();

                // Store the cart items - create a simplified list of product info that can be serialized
                var productList = new List<Dictionary<string, string>>();
                foreach (var item in cartItems)
                {
                    productList.Add(new Dictionary<string, string> {
                        { "Id", item.Id.ToString() },
                        { "Title", item.Title },
                        { "Price", item.Price.ToString() },
                        { "SellerId", item.SellerId.ToString() }
                    });
                }

                // Store serialized product info
                TempData["OrderProducts"] = JsonSerializer.Serialize(productList);
                TempData["HasOrderData"] = "true";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating order (FULL EXCEPTION): {ex.ToString()}"); // Log full exception
                ModelState.AddModelError(string.Empty, "An error occurred while creating your order. Please try again.");
                ModelState.AddModelError(string.Empty, $"DEBUG: {ex.ToString()}"); // ADD FULL EXCEPTION TO MODELSTATE
                model.CalculateOrderTotal();
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

            // Debug info
            System.Diagnostics.Debug.WriteLine($"FinalizePurchase action called with orderHistoryId: {orderHistoryId}");

            try
            {
                // First check if we have order data from TempData (from the same request flow)
                if (TempData["HasOrderData"]?.ToString() == "true")
                {
                    // Use the data we just stored in TempData
                    model.FullName = TempData["OrderFullName"]?.ToString();
                    model.Email = TempData["OrderEmail"]?.ToString();
                    model.PhoneNumber = TempData["OrderPhone"]?.ToString();
                    model.Address = TempData["OrderAddress"]?.ToString();
                    model.PaymentMethod = TempData["OrderPaymentMethod"]?.ToString();
                    model.OrderStatus = "Completed";

                    if (double.TryParse(TempData["OrderSubtotal"]?.ToString(), out double subtotal))
                        model.Subtotal = subtotal;

                    if (double.TryParse(TempData["OrderDeliveryFee"]?.ToString(), out double deliveryFee))
                        model.DeliveryFee = deliveryFee;

                    if (double.TryParse(TempData["OrderTotal"]?.ToString(), out double total))
                        model.Total = total;

                    // Recover product list from TempData
                    if (TempData["OrderProducts"] != null)
                    {
                        try
                        {
                            string productsJson = TempData["OrderProducts"]?.ToString();
                            var productDictList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(productsJson);

                            List<Product> orderProducts = new List<Product>();
                            foreach (var productDict in productDictList)
                            {
                                var product = new BuyProduct(); // Using concrete BuyProduct instead of abstract Product

                                if (productDict.TryGetValue("Id", out string idStr) && int.TryParse(idStr, out int id))
                                    product.Id = id;

                                if (productDict.TryGetValue("Title", out string title))
                                    product.Title = title;

                                if (productDict.TryGetValue("Price", out string priceStr) && double.TryParse(priceStr, out double price))
                                    product.Price = price;

                                if (productDict.TryGetValue("SellerId", out string sellerIdStr) && int.TryParse(sellerIdStr, out int sellerId))
                                    product.SellerId = sellerId;

                                orderProducts.Add(product);
                            }

                            model.ProductList = orderProducts;
                            System.Diagnostics.Debug.WriteLine($"Recovered {orderProducts.Count} products from TempData");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error deserializing product list: {ex.Message}");
                        }
                    }

                    // Don't try to get products from cart as it might have been cleared already
                    if (model.ProductList == null || !model.ProductList.Any())
                    {
                        model.ProductList = new List<Product>();
                        // Try to get from cart as a last resort
                        try
                        {
                            var cartItems = await _shoppingCartService.GetCartItemsAsync(UserSession.CurrentUserId ?? 1);
                            if (cartItems != null && cartItems.Any())
                            {
                                model.ProductList = cartItems;
                            }
                        }
                        catch
                        {
                            // Ignore cart errors here
                        }
                    }
                }
                else
                {
                    // Try to get from database as before
                    var orders = await _orderService.GetOrdersFromOrderHistoryAsync(orderHistoryId);
                    System.Diagnostics.Debug.WriteLine($"Orders found: {orders?.Count ?? 0}");

                    // Get products from order history
                    var orderProducts = await _orderHistoryService.GetProductsFromOrderHistoryAsync(orderHistoryId);
                    if (orderProducts != null && orderProducts.Any())
                    {
                        model.ProductList = orderProducts;
                        System.Diagnostics.Debug.WriteLine($"Products found: {model.ProductList?.Count ?? 0}");
                    }
                    else
                    {
                        // Fallback to cart
                        var cartItems = await _shoppingCartService.GetCartItemsAsync(UserSession.CurrentUserId ?? 1);
                        model.ProductList = cartItems ?? new List<Product>();
                    }

                    // Only try to get order data if we have orders
                    if (orders != null && orders.Any())
                    {
                        // Get first order to extract basic info
                        var firstOrder = orders.FirstOrDefault();
                        if (firstOrder != null)
                        {
                            // Get order summary
                            var orderSummary = await _orderService.GetOrderSummaryAsync(firstOrder.OrderSummaryID);

                            if (orderSummary != null)
                            {
                                // Populate model with order summary data
                                model.FullName = orderSummary.FullName;
                                model.Email = orderSummary.Email;
                                model.PhoneNumber = orderSummary.PhoneNumber;
                                model.Address = orderSummary.Address;
                                model.PaymentMethod = firstOrder.PaymentMethod;
                                model.OrderStatus = "Completed"; // Default status

                                // Set financial values
                                model.Subtotal = orderSummary.Subtotal;
                                model.DeliveryFee = orderSummary.DeliveryFee;
                                model.Total = orderSummary.FinalTotal;
                            }
                        }
                    }
                }

                // If we couldn't get values from the order summary, calculate them
                if (model.Subtotal == 0 && model.ProductList.Any())
                {
                    model.CalculateOrderTotal();
                }

                // Clear the shopping cart after successful order display
                if (UserSession.CurrentUserId.HasValue && !string.IsNullOrEmpty(model.FullName))
                {
                    await _shoppingCartService.ClearCartAsync(UserSession.CurrentUserId.Value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing order data for FinalizePurchase: {ex.Message}");
                // Instead of showing empty data, try to recover with cart items as a fallback
                try
                {
                    var cartItems = await _shoppingCartService.GetCartItemsAsync(UserSession.CurrentUserId ?? 1);
                    model.ProductList = cartItems;
                    model.CalculateOrderTotal();
                }
                catch (Exception innerEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner error: {innerEx.Message}");
                    model.ProductList = new List<Product>();
                    model.Subtotal = 0;
                    model.DeliveryFee = 0;
                    model.Total = 0;
                }
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
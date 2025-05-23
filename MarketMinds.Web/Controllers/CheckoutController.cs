using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.BuyProductsService;
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
        private readonly IBuyProductsService _buyProductsService;

        public CheckoutController(
            IOrderHistoryService orderHistoryService,
            IOrderSummaryService orderSummaryService,
            IOrderService orderService,
            IProductService productService,
            IDummyWalletService dummyWalletService,
            IShoppingCartService shoppingCartService,
            IBuyProductsService buyProductsService)
        {
            _orderHistoryService = orderHistoryService;
            _orderSummaryService = orderSummaryService;
            _orderService = orderService;
            _productService = productService;
            _dummyWalletService = dummyWalletService;
            _shoppingCartService = shoppingCartService;
            _buyProductsService = buyProductsService;
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
            Dictionary<int, int> quantities = new Dictionary<int, int>();
            System.Diagnostics.Debug.WriteLine("BillingInfo POST action initiated");

            try
            {
                // Get cart items and quantities
                int userId = UserSession.CurrentUserId ?? 1;
                cartItems = await _shoppingCartService.GetCartItemsAsync(userId);
                System.Diagnostics.Debug.WriteLine($"Retrieved {cartItems?.Count ?? 0} cart items for user {userId}");

                // Store quantities separately
                foreach (var item in cartItems)
                {
                    var quantity = await _shoppingCartService.GetProductQuantityAsync(userId, item.Id);
                    quantities[item.Id] = quantity;
                    System.Diagnostics.Debug.WriteLine($"Product {item.Id} ({item.Title}) quantity: {quantity}");

                    // Log the product type
                    if (item is BuyProduct buyProduct)
                    {
                        System.Diagnostics.Debug.WriteLine($"Product {item.Id} is BuyProduct with stock: {buyProduct.Stock}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Product {item.Id} is of type: {item.GetType().Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cart items initially: {ex.Message}");
            }

            model.ProductList = cartItems;

            if (!ModelState.IsValid)
            {
                model.CalculateOrderTotal();
                System.Diagnostics.Debug.WriteLine("Model state is invalid, returning to form");
                return View(model);
            }

            if (model.SelectedPaymentMethod == "wallet")
            {
                model.CalculateOrderTotal();
                await ProcessWalletRefill(model);
                System.Diagnostics.Debug.WriteLine("Wallet payment processed");
            }
            else
            {
                model.CalculateOrderTotal();
                System.Diagnostics.Debug.WriteLine($"Payment method: {model.SelectedPaymentMethod}, Total: {model.Total}");
            }

            try
            {
                var userId = UserSession.CurrentUserId ?? 1;

                if (cartItems == null || !cartItems.Any())
                {
                    ModelState.AddModelError(string.Empty, "Your cart is empty. Please add items before proceeding.");
                    System.Diagnostics.Debug.WriteLine("Cart is empty, returning to form");
                    return View(model);
                }

                var orderRequestDto = new OrderCreationRequestDto(); // Assume mapping is correct as before
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
                System.Diagnostics.Debug.WriteLine("Created order request DTO");

                var newOrderHistoryId = await _orderService.CreateOrderFromCartAsync(orderRequestDto, userId, cartItems);
                model.OrderHistoryID = newOrderHistoryId;
                System.Diagnostics.Debug.WriteLine($"Created order with history ID: {newOrderHistoryId}");

                // Save this data in TempData so the next request can access it
                TempData["OrderFullName"] = model.FullName;
                TempData["OrderEmail"] = model.Email;
                TempData["OrderPhone"] = model.PhoneNumber;
                TempData["OrderAddress"] = model.Address;
                TempData["OrderPaymentMethod"] = model.SelectedPaymentMethod;
                TempData["OrderSubtotal"] = model.Subtotal.ToString();
                TempData["OrderDeliveryFee"] = model.DeliveryFee.ToString();
                TempData["OrderTotal"] = model.Total.ToString();
                TempData["OrderHistoryID"] = newOrderHistoryId.ToString();
                System.Diagnostics.Debug.WriteLine("Stored order details in TempData");

                // Store the cart items - create a simplified list of product info that can be serialized
                var productList = new List<Dictionary<string, string>>();

                // Update stock for each product in the cart
                foreach (var item in cartItems)
                {
                    try
                    {
                        // Get quantity for this product
                        int quantity = quantities.ContainsKey(item.Id) ? quantities[item.Id] : 1;

                        // Log the product info for debugging
                        System.Diagnostics.Debug.WriteLine($"Processing product: ID={item.Id}, Title={item.Title}, Type={item.GetType().Name}, Quantity={quantity}");

                        // Only decrease stock for BuyProducts
                        if (item is BuyProduct buyProduct)
                        {
                            // Log stock before update
                            System.Diagnostics.Debug.WriteLine($"Current stock for product {item.Id}: {buyProduct.Stock}");

                            try
                            {
                                // IMPORTANT: Pass the actual quantity to decrease by
                                await _buyProductsService.DecreaseProductStockAsync(item.Id, quantity);

                                // Log confirmation
                                System.Diagnostics.Debug.WriteLine($"Stock update called for product {item.Id}: decreased by {quantity}");
                            }
                            catch (Exception stockEx)
                            {
                                // Log any errors when updating stock
                                System.Diagnostics.Debug.WriteLine($"ERROR updating stock: {stockEx.Message}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Skipping stock update for non-BuyProduct: {item.Id} ({item.GetType().Name})");
                        }

                        // Add to product list for serialization with quantity information
                        var productDict = new Dictionary<string, string> {
                            { "Id", item.Id.ToString() },
                            { "Title", item.Title },
                            { "Price", item.Price.ToString() },
                            { "Quantity", quantity.ToString() },
                            { "SellerId", item.SellerId.ToString() }
                        };

                        productList.Add(productDict);
                        System.Diagnostics.Debug.WriteLine($"Added product to serialized list: {item.Id} ({item.Title})");
                    }
                    catch (Exception ex)
                    {
                        // Log but continue with other products
                        System.Diagnostics.Debug.WriteLine($"Error processing product {item.Id}: {ex.Message}");
                    }
                }

                // Store serialized product info
                var serializedProducts = System.Text.Json.JsonSerializer.Serialize(productList);
                TempData["OrderProducts"] = serializedProducts;
                TempData["HasOrderData"] = "true";

                // Debug logging
                System.Diagnostics.Debug.WriteLine($"Saved {productList.Count} products to TempData");
                System.Diagnostics.Debug.WriteLine($"Serialized data: {serializedProducts.Substring(0, Math.Min(100, serializedProducts.Length))}...");

                // Directly pass products to FinalizePurchase to ensure they're available
                TempData["ProductCount"] = cartItems.Count.ToString();
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

            // Force TempData persistence
            TempData.Keep("OrderProducts");
            TempData.Keep("HasOrderData");

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

                    if (int.TryParse(TempData["OrderHistoryID"]?.ToString(), out int historyId))
                        model.OrderHistoryID = historyId;

                    // Recover product list from TempData
                    string productsJson = TempData["OrderProducts"]?.ToString();
                    System.Diagnostics.Debug.WriteLine($"Products JSON: {(productsJson?.Length > 0 ? $"{productsJson.Substring(0, Math.Min(100, productsJson.Length))}..." : "empty")}");

                    if (!string.IsNullOrEmpty(productsJson))
                    {
                        try
                        {
                            var productDictList = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(productsJson);
                            TempData.Keep("OrderProducts"); // Keep it for the view

                            if (productDictList != null && productDictList.Any())
                            {
                                List<Product> orderProducts = new List<Product>();
                                foreach (var productDict in productDictList)
                                {
                                    var product = new BuyProduct();

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
                                System.Diagnostics.Debug.WriteLine($"Successfully recovered {orderProducts.Count} products from TempData");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error deserializing product list: {ex.Message}");
                        }
                    }

                    // Make sure we have products
                    if (model.ProductList == null || !model.ProductList.Any())
                    {
                        // Try to get products directly from order history
                        try
                        {
                            var orderProducts = await _orderHistoryService.GetProductsFromOrderHistoryAsync(orderHistoryId);
                            if (orderProducts != null && orderProducts.Any())
                            {
                                model.ProductList = orderProducts;
                                System.Diagnostics.Debug.WriteLine($"Retrieved {orderProducts.Count} products from order history");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error getting products from order history: {ex.Message}");
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

                // If we still don't have products, try the cart as a final fallback
                if ((model.ProductList == null || !model.ProductList.Any()) && UserSession.CurrentUserId.HasValue)
                {
                    try
                    {
                        var cartItems = await _shoppingCartService.GetCartItemsAsync(UserSession.CurrentUserId.Value);
                        if (cartItems != null && cartItems.Any())
                        {
                            model.ProductList = cartItems;
                            System.Diagnostics.Debug.WriteLine($"Using cart items as fallback: {cartItems.Count} products");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error getting cart items as fallback: {ex.Message}");
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

            // Ensure we have a non-null ProductList
            if (model.ProductList == null)
            {
                model.ProductList = new List<Product>();
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

        [HttpPost]
        [Route("api/checkout/updatestock")]
        public async Task<IActionResult> UpdateStock([FromBody] StockUpdateModel model)
        {
            try
            {
                if (model == null || model.ProductId <= 0 || model.Quantity <= 0)
                {
                    return BadRequest(new { Message = "Invalid product data" });
                }

                // Get product to check current stock
                var product = await _buyProductsService.GetProductByIdAsync(model.ProductId);

                if (product == null)
                {
                    return NotFound(new { Message = $"Product with ID {model.ProductId} not found" });
                }

                if (product is BuyProduct buyProduct)
                {
                    // Log initial stock
                    System.Diagnostics.Debug.WriteLine($"Current stock for product {model.ProductId}: {buyProduct.Stock}");

                    // Calculate new stock
                    int newStock = Math.Max(0, buyProduct.Stock - model.Quantity);
                    int oldStock = buyProduct.Stock;

                    // Update using service as intended
                    await _buyProductsService.UpdateProductStockAsync(model.ProductId, newStock);

                    // Log the results
                    System.Diagnostics.Debug.WriteLine($"Stock updated: Product {model.ProductId} decreased by {model.Quantity}, new stock: {newStock}");

                    return Ok(new
                    {
                        Message = $"Stock updated successfully for product {model.ProductId}",
                        ProductId = model.ProductId,
                        OldStock = oldStock,
                        NewStock = newStock
                    });
                }
                else
                {
                    return BadRequest(new { Message = "Product is not a buyable product" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating stock: {ex.Message}");
                return StatusCode(500, new { Message = $"Error updating stock: {ex.Message}" });
            }
        }

        public class StockUpdateModel
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
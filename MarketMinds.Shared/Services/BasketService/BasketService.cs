using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.IRepository;
using System.Text.Json.Nodes;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.Models.DTOs.Mappers;

namespace MarketMinds.Shared.Services.BasketService
{
    public class BasketService : IBasketService
    {
        public const int MAXIMUM_QUANTITY_PER_ITEM = 10;
        private const int MINIMUM_USER_ID = 0;
        private const int MINIMUM_ITEM_ID = 0;
        private const int MINIMUM_BASKET_ID = 0;
        private const double MINIMUM_DISCOUNT = 0;
        private const int MINIMUM_QUANTITY = 0;
        private const int DEFAULT_QUANTITY = 1;
        private const int INVALID_USER_ID = -1;
        private const int INVALID_BASKET_ID = -1;
        private const int MINIMUM_PRODUCT_ID = 0;
        private const double MINIMUM_PRICE = 0;

        // Dictionary of valid promo codes (moved from controller)
        private static readonly Dictionary<string, double> VALID_CODES = new Dictionary<string, double>
        {
            { "DISCOUNT10", 0.10 },  // 10% discount
            { "WELCOME20", 0.20 },   // 20% discount
            { "FLASH30", 0.30 },     // 30% discount
        };

        private static readonly Func<string, string> Normalize = code =>
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }
            return code.ToUpper().Trim();
        };

        private readonly IBasketRepository basketRepository;
        private readonly JsonSerializerOptions jsonOptions;

        // Constructor with configuration
        public BasketService(IBasketRepository basketRepository)
        {
            this.basketRepository = basketRepository;

            // Configure JSON options
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }

        public void AddProductToBasket(int userId, int productId, int quantity)
        {
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }
            if (quantity < MINIMUM_QUANTITY)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }

            try
            {
                // Apply the maximum quantity limit
                int limitedQuantity = Math.Min(quantity, MAXIMUM_QUANTITY_PER_ITEM);

                // Get the user's basket
                Basket basket = GetBasketByUserId(userId);

                // Make the API call to add product to basket
                if (basketRepository.GetType().Name.Contains("Proxy"))
                {
                    ((BasketProxyRepository)basketRepository).AddProductToBasketRaw(userId, productId, limitedQuantity);
                }
                else
                {
                    // If not a proxy repository, use the standard interface method
                    basketRepository.AddItemToBasket(basket.Id, productId, limitedQuantity);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to add product to basket: {ex.Message}", ex);
            }
        }

        public Basket GetBasketByUser(User user)
        {
            if (user == null || user.Id <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Valid user must be provided");
            }

            try
            {
                if (basketRepository.GetType().Name.Contains("Proxy"))
                {
                    // Use the proxy repository to get the raw JSON response
                    var responseJson = ((BasketProxyRepository)basketRepository).GetBasketByUserRaw(user.Id);

                    try
                    {
                        // Create improved JsonSerializerOptions optimized for the client
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            AllowTrailingCommas = true,
                            ReadCommentHandling = JsonCommentHandling.Skip,
                            NumberHandling = JsonNumberHandling.AllowReadingFromString,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            ReferenceHandler = ReferenceHandler.IgnoreCycles
                        };

                        // Add UserJsonConverter to handle password field type mismatch
                        options.Converters.Add(new MarketMinds.Shared.Services.UserJsonConverter());

                        // Deserialize directly
                        var basket = JsonSerializer.Deserialize<Basket>(responseJson, options);

                        // Make sure we have an Items collection
                        if (basket.Items == null)
                        {
                            basket.Items = new List<BasketItem>();
                        }

                        // Ensure all basket items have ProductId set
                        if (basket.Items != null)
                        {
                            foreach (var item in basket.Items)
                            {
                                if (item.Product != null && item.ProductId == MINIMUM_PRODUCT_ID)
                                {
                                    // If ProductId is not set, set it from the Product object
                                    item.ProductId = item.Product.Id;
                                }
                            }
                        }

                        return basket;
                    }
                    catch (JsonException ex)
                    {
                        // Try to fallback to a simpler deserialization
                        var fallbackBasket = new Basket { Id = user.Id, Items = new List<BasketItem>() };
                        return fallbackBasket;
                    }
                }
                else
                {
                    // Use the standard interface method
                    return basketRepository.GetBasketByUserId(user.Id);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to retrieve user's basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to retrieve user's basket", ex);
            }
        }

        public BasketDTO GetBasketDTOByUserId(int userId)
        {
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }

            var basket = GetBasketByUserId(userId);

            // Ensure all basket items have ProductId set
            if (basket.Items != null)
            {
                foreach (var item in basket.Items)
                {
                    if (item.Product != null && item.ProductId == MINIMUM_PRODUCT_ID)
                    {
                        // If ProductId is not set, set it from the Product object
                        item.ProductId = item.Product.Id;
                    }
                }
            }

            return BasketMapper.ToDTO(basket);
        }

        public List<BasketItemDTO> GetBasketItemDTOs(int basketId)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            var items = GetBasketItems(basketId);

            // Ensure each item has ProductId set
            foreach (var item in items)
            {
                if (item.Product != null && item.ProductId == MINIMUM_ITEM_ID)
                {
                    // If ProductId is not set, set it from the Product object
                    item.ProductId = item.Product.Id;
                }
            }

            return items.Select(item => BasketMapper.ToDTO(item)).ToList();
        }

        public void RemoveProductFromBasket(int userId, int productId)
        {
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Get the user's basket
                Basket basket = GetBasketByUserId(userId);

                if (basketRepository.GetType().Name.Contains("Proxy"))
                {
                    // Make the API call to remove product from basket
                    ((BasketProxyRepository)basketRepository).RemoveProductFromBasketRaw(userId, productId);
                }
                else
                {
                    // Use the standard interface method
                    basketRepository.RemoveItemByProductId(basket.Id, productId);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to remove product from basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to remove product from basket: {ex.Message}", ex);
            }
        }

        public void UpdateProductQuantity(int userId, int productId, int quantity)
        {
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }
            if (quantity < MINIMUM_QUANTITY)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }

            try
            {
                // Apply the maximum quantity limit
                int limitedQuantity = Math.Min(quantity, MAXIMUM_QUANTITY_PER_ITEM);

                // Get the user's basket
                Basket basket = GetBasketByUserId(userId);

                if (limitedQuantity == MINIMUM_QUANTITY)
                {
                    // If quantity is zero, remove the item
                    if (basketRepository.GetType().Name.Contains("Proxy"))
                    {
                        ((BasketProxyRepository)basketRepository).RemoveProductFromBasketRaw(userId, productId);
                    }
                    else
                    {
                        basketRepository.RemoveItemByProductId(basket.Id, productId);
                    }
                }
                else
                {
                    if (basketRepository.GetType().Name.Contains("Proxy"))
                    {
                        // Make the API call to update product quantity
                        ((BasketProxyRepository)basketRepository).UpdateProductQuantityRaw(userId, productId, limitedQuantity);
                    }
                    else
                    {
                        // Use the standard interface method
                        basketRepository.UpdateItemQuantityByProductId(basket.Id, productId, limitedQuantity);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Failed to update product quantity: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to update product quantity: {ex.Message}", ex);
            }
        }

        public bool ValidateQuantityInput(string quantityText, out int quantity)
        {
            // Initialize output parameter
            quantity = MINIMUM_QUANTITY;

            // Check if the input is empty
            if (string.IsNullOrWhiteSpace(quantityText))
            {
                return false;
            }

            // Try to parse the input string to an integer
            if (!int.TryParse(quantityText, out quantity))
            {
                return false;
            }

            // Check if quantity is non-negative
            if (quantity < MINIMUM_QUANTITY)
            {
                return false;
            }

            return true;
        }

        public int GetLimitedQuantity(int quantity)
        {
            return Math.Min(quantity, MAXIMUM_QUANTITY_PER_ITEM);
        }

        public void ClearBasket(int userId)
        {
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }

            try
            {
                // Get the user's basket
                Basket basket = GetBasketByUserId(userId);

                if (basketRepository.GetType().Name.Contains("Proxy"))
                {
                    // Clear the basket through API
                    ((BasketProxyRepository)basketRepository).ClearBasketRaw(userId);
                }
                else
                {
                    // Use the standard interface method
                    basketRepository.ClearBasket(basket.Id);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not clear basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not clear basket: {ex.Message}", ex);
            }
        }

        public bool ValidateBasketBeforeCheckOut(int basketId)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                if (basketRepository.GetType().Name.Contains("Proxy"))
                {
                    // Validate basket through API
                    var responseContent = ((BasketProxyRepository)basketRepository).ValidateBasketBeforeCheckOutRaw(basketId);

                    try
                    {
                        return JsonSerializer.Deserialize<bool>(responseContent, jsonOptions);
                    }
                    catch (JsonException)
                    {
                        // Try a simple parsing approach as fallback
                        responseContent = responseContent.Trim().ToLower();
                        if (responseContent == "true")
                        {
                            return true;
                        }

                        return false;
                    }
                }
                else
                {
                    // Get the basket items
                    List<BasketItem> items = basketRepository.GetBasketItems(basketId);

                    // Check if the basket is empty
                    if (items.Count == 0)
                    {
                        return false;
                    }

                    // Check if all items have valid quantities
                    foreach (BasketItem item in items)
                    {
                        if (item.Quantity <= MINIMUM_QUANTITY)
                        {
                            return false;
                        }

                        if (item.Price <= MINIMUM_PRICE)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not validate basket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not validate basket: {ex.Message}", ex);
            }
        }

        public void ApplyPromoCode(int basketId, string code)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Promo code cannot be empty");
            }

            try
            {
                // Convert to uppercase for case-insensitive comparison
                string normalizedCode = Normalize(code);

                // Check if the code exists in the valid codes
                if (!VALID_CODES.TryGetValue(normalizedCode, out double discountRate))
                {
                    throw new InvalidOperationException("Invalid promo code");
                }
                // Successfully applied the promo code
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not apply promo code: {ex.Message}", ex);
            }
        }

        // Class for deserializing discount rate response
        private class DiscountResponse
        {
            public double DiscountRate { get; set; }
        }

        // Method to get the discount amount based on a promo code
        public double GetPromoCodeDiscount(string code, double subtotal)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return MINIMUM_DISCOUNT;
            }

            try
            {
                // Convert to uppercase for case-insensitive comparison
                string normalizedCode = Normalize(code);

                // Check if the code exists in the valid codes
                if (VALID_CODES.TryGetValue(normalizedCode, out double discountRate))
                {
                    return subtotal * discountRate;
                }

                return MINIMUM_DISCOUNT;
            }
            catch (Exception)
            {
                return MINIMUM_DISCOUNT;
            }
        }

        // Add a method to get the discount rate for a promo code
        public double GetPromoCodeDiscountRate(int basketId, string code)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Promo code cannot be empty");
            }

            // Convert to uppercase for case-insensitive comparison
            string normalizedCode = Normalize(code);

            // Check if the code exists in the valid codes
            if (VALID_CODES.TryGetValue(normalizedCode, out double discountRate))
            {
                return discountRate;
            }

            throw new InvalidOperationException("Invalid promo code");
        }

        // Add a new method to calculate basket totals
        public BasketTotals CalculateBasketTotals(int basketId, string promoCode)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                if (basketRepository.GetType().Name.Contains("Proxy"))
                {
                    // Get totals from API
                    var responseJson = ((BasketProxyRepository)basketRepository).CalculateBasketTotalsRaw(basketId, promoCode);

                    try
                    {
                        var totals = JsonSerializer.Deserialize<BasketTotals>(responseJson, jsonOptions);
                        return totals;
                    }
                    catch (JsonException)
                    {
                        // Create a default totals object
                        return new BasketTotals();
                    }
                }
                else
                {
                    // Calculate totals manually
                    List<BasketItem> items = basketRepository.GetBasketItems(basketId);
                    double subtotal = 0;

                    foreach (var item in items)
                    {
                        subtotal += item.GetPrice();
                    }

                    double discount = MINIMUM_DISCOUNT;

                    if (!string.IsNullOrEmpty(promoCode))
                    {
                        discount = GetPromoCodeDiscount(promoCode, subtotal);
                    }

                    double totalAmount = subtotal - discount;

                    var basketTotals = new BasketTotals
                    {
                        Subtotal = subtotal,
                        Discount = discount,
                        TotalAmount = totalAmount
                    };

                    return basketTotals;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not calculate basket totals: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not calculate basket totals: {ex.Message}", ex);
            }
        }

        public void DecreaseProductQuantity(int userId, int productId)
        {
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Get the user's basket
                Basket basket = GetBasketByUserId(userId);

                if (basketRepository.GetType().Name.Contains("Proxy"))
                {
                    // Decrease quantity through API
                    ((BasketProxyRepository)basketRepository).DecreaseProductQuantityRaw(userId, productId);
                }
                else
                {
                    // Get item directly without checking Product.Id which might cause JSON issues
                    var items = basketRepository.GetBasketItems(basket.Id);
                    var targetItem = items.FirstOrDefault(item => item.ProductId == productId);

                    if (targetItem == null)
                    {
                        throw new InvalidOperationException("Item not found in basket");
                    }

                    if (targetItem.Quantity > 1)
                    {
                        basketRepository.UpdateItemQuantityByProductId(basket.Id, productId, targetItem.Quantity - 1);
                    }
                    else
                    {
                        basketRepository.RemoveItemByProductId(basket.Id, productId);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not decrease quantity: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not decrease quantity: {ex.Message}", ex);
            }
        }

        public void IncreaseProductQuantity(int userId, int productId)
        {
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }
            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                // Get the user's basket
                Basket basket = GetBasketByUserId(userId);

                if (basketRepository.GetType().Name.Contains("Proxy"))
                {
                    // Increase quantity through API
                    ((BasketProxyRepository)basketRepository).IncreaseProductQuantityRaw(userId, productId);
                }
                else
                {
                    // Get item directly without checking Product.Id which might cause JSON issues
                    var items = basketRepository.GetBasketItems(basket.Id);
                    var targetItem = items.FirstOrDefault(item => item.ProductId == productId);

                    if (targetItem == null)
                    {
                        throw new InvalidOperationException("Item not found in basket");
                    }

                    // Calculate new quantity, ensuring it doesn't exceed the maximum
                    int newQuantity = Math.Min(targetItem.Quantity + 1, MAXIMUM_QUANTITY_PER_ITEM);

                    basketRepository.UpdateItemQuantityByProductId(basket.Id, productId, newQuantity);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not increase quantity: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not increase quantity: {ex.Message}", ex);
            }
        }

        public async Task<bool> CheckoutBasketAsync(int userId, int basketId, double discountAmount = 0, double totalAmount = 0)
        {
            if (userId <= INVALID_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }

            if (basketId <= INVALID_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                // First, validate the basket before checkout
                if (!ValidateBasketBeforeCheckOut(basketId))
                {
                    throw new InvalidOperationException("Basket validation failed");
                }

                // Get the current basket totals
                var basketTotals = CalculateBasketTotals(basketId, null);
                System.Diagnostics.Debug.WriteLine($"DIAGNOSTIC: CheckoutBasketAsync - Raw basketTotal: {basketTotals.TotalAmount}, Provided discount: {discountAmount}, Provided total: {totalAmount}");

                // Use provided values if they are valid
                if (discountAmount > 0 && totalAmount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"DIAGNOSTIC: Using provided discount ({discountAmount}) and total ({totalAmount})");
                    basketTotals.Discount = discountAmount;
                    basketTotals.TotalAmount = totalAmount;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"DIAGNOSTIC: No valid discount/total provided, using calculated totals");
                }

                // Create request for the account API to create orders from the basket
                var request = new
                {
                    BasketId = basketId,
                    DiscountAmount = basketTotals.Discount,
                    TotalAmount = basketTotals.TotalAmount
                };

                System.Diagnostics.Debug.WriteLine($"Checkout request: BasketId={basketId}, Discount={basketTotals.Discount}, Total={basketTotals.TotalAmount}");

                if (basketRepository.GetType().Name.Contains("Proxy"))
                {
                    // Call the account API to create the order
                    var response = await ((BasketProxyRepository)basketRepository).CheckoutBasketRaw(userId, basketId, request);

                    // Handle the response
                    if (response.IsSuccessStatusCode)
                    {
                        // If successful order creation, the API should have cleared the basket already
                        System.Diagnostics.Debug.WriteLine($"Successfully created order from basket {basketId} for user {userId}");
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Error creating order: {response.StatusCode}, Content: {errorContent}");

                        // If it's a balance issue (400 status code), throw a more user-friendly message
                        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest &&
                            errorContent.Contains("Insufficient funds"))
                        {
                            throw new InvalidOperationException(errorContent);
                        }

                        return false;
                    }
                }
                else
                {
                    // For non-proxy implementations, we'd need a different checkout mechanism
                    // This is a placeholder - in a real implementation, you'd process the order locally
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Could not checkout basket: {ex.Message}", ex);
            }
            catch (InvalidOperationException)
            {
                // Re-throw InvalidOperationException as-is to preserve the message
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not checkout basket: {ex.Message}", ex);
            }
        }

        // Methods for the standard IBasketRepository operations with validation
        public Basket GetBasketByUserId(int userId)
        {
            if (userId <= MINIMUM_USER_ID)
            {
                throw new ArgumentException("Invalid user ID");
            }

            try
            {
                var basketEntity = basketRepository.GetBasketByUserId(userId);

                // Ensure all basket items have ProductId set
                if (basketEntity.Items != null)
                {
                    foreach (var item in basketEntity.Items)
                    {
                        if (item.Product != null && item.ProductId == MINIMUM_PRODUCT_ID)
                        {
                            // If ProductId is not set, set it from the Product object
                            item.ProductId = item.Product.Id;
                        }
                    }
                }

                return basketEntity;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve user's basket: {ex.Message}", ex);
            }
        }

        public void RemoveItemByProductId(int basketId, int productId)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }

            try
            {
                basketRepository.RemoveItemByProductId(basketId, productId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to remove item from basket: {ex.Message}", ex);
            }
        }

        public List<BasketItem> GetBasketItems(int basketId)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            try
            {
                var basketItems = basketRepository.GetBasketItems(basketId);

                // Ensure each item has ProductId set correctly
                foreach (var item in basketItems)
                {
                    if (item.Product != null && item.ProductId != item.Product.Id)
                    {
                        item.ProductId = item.Product.Id;
                    }
                }

                return basketItems;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve basket items: {ex.Message}", ex);
            }
        }

        public void AddItemToBasket(int basketId, int productId, int quantity)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }

            if (quantity < MINIMUM_QUANTITY)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }

            try
            {
                // Apply the maximum quantity limit
                int limitedQuantity = Math.Min(quantity, MAXIMUM_QUANTITY_PER_ITEM);

                basketRepository.AddItemToBasket(basketId, productId, limitedQuantity);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to add item to basket: {ex.Message}", ex);
            }
        }

        public void UpdateItemQuantityByProductId(int basketId, int productId, int quantity)
        {
            if (basketId <= MINIMUM_BASKET_ID)
            {
                throw new ArgumentException("Invalid basket ID");
            }

            if (productId <= MINIMUM_ITEM_ID)
            {
                throw new ArgumentException("Invalid product ID");
            }

            if (quantity < MINIMUM_QUANTITY)
            {
                throw new ArgumentException("Quantity cannot be negative");
            }

            try
            {
                // Apply the maximum quantity limit
                int limitedQuantity = Math.Min(quantity, MAXIMUM_QUANTITY_PER_ITEM);

                basketRepository.UpdateItemQuantityByProductId(basketId, productId, limitedQuantity);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to update item quantity: {ex.Message}", ex);
            }
        }
    }

    // Helper class to return the basket total values
    public class BasketTotals
    {
        public double Subtotal { get; set; }
        public double Discount { get; set; }
        public double TotalAmount { get; set; }
    }
}
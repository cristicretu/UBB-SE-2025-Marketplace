using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.BasketService
{
    /// <summary>
    /// Interface for BasketService to manage basket operations.
    /// </summary>
    public interface IBasketService
    {
        /// <summary>
        /// Adds a product to the user's basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product to add.</param>
        /// <param name="quantity">The quantity of the product to add.</param>
        void AddProductToBasket(int userId, int productId, int quantity);

        /// <summary>
        /// Retrieves the user's basket.
        /// </summary>
        /// <param name="user">The user object.</param>
        /// <returns>The user's basket.</returns>
        Basket GetBasketByUser(User user);

        /// <summary>
        /// Removes a product from the user's basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product to remove.</param>
        void RemoveProductFromBasket(int userId, int productId);

        /// <summary>
        /// Updates the quantity of a product in the user's basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product to update.</param>
        /// <param name="quantity">The new quantity of the product.</param>
        void UpdateProductQuantity(int userId, int productId, int quantity);

        /// <summary>
        /// Validates if a quantity input string is valid and can be parsed to a non-negative integer.
        /// </summary>
        /// <param name="quantityText">The quantity text to validate.</param>
        /// <param name="quantity">The parsed quantity if valid.</param>
        /// <returns>True if the input is valid, otherwise false.</returns>
        bool ValidateQuantityInput(string quantityText, out int quantity);

        /// <summary>
        /// Gets the limited quantity value that doesn't exceed maximum allowed quantity.
        /// </summary>
        /// <param name="quantity">The original quantity.</param>
        /// <returns>The limited quantity value.</returns>
        int GetLimitedQuantity(int quantity);

        /// <summary>
        /// Clears all items from the user's basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        void ClearBasket(int userId);

        /// <summary>
        /// Validates the basket before checkout.
        /// </summary>
        /// <param name="basketId">The ID of the basket.</param>
        /// <returns>True if the basket is valid for checkout, otherwise false.</returns>
        bool ValidateBasketBeforeCheckOut(int basketId);

        /// <summary>
        /// Applies a promo code to the basket.
        /// </summary>
        /// <param name="basketId">The ID of the basket.</param>
        /// <param name="code">The promo code to apply.</param>
        void ApplyPromoCode(int basketId, string code);

        /// <summary>
        /// Gets the discount for a promo code.
        /// </summary>
        /// <param name="code">The promo code.</param>
        /// <param name="subtotal">The subtotal amount.</param>
        /// <returns>The discount amount.</returns>
        double GetPromoCodeDiscount(string code, double subtotal);

        /// <summary>
        /// Calculates the total values for a basket including subtotal, discount, and final amount.
        /// </summary>
        /// <param name="basketId">The ID of the basket.</param>
        /// <param name="promoCode">The promo code to apply (if any).</param>
        /// <returns>A BasketTotals object containing subtotal, discount, and total amount.</returns>
        BasketTotals CalculateBasketTotals(int basketId, string promoCode);

        /// <summary>
        /// Decreases the quantity of a product in the user's basket by 1.
        /// If the current quantity is 1, the product is removed from the basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product to decrease.</param>
        void DecreaseProductQuantity(int userId, int productId);

        /// <summary>
        /// Increases the quantity of a product in the user's basket by 1.
        /// The quantity will not exceed the maximum allowed limit.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product to increase.</param>
        void IncreaseProductQuantity(int userId, int productId);

        /// <summary>
        /// Creates an order from the user's basket items and clears the basket.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="basketId">The ID of the basket.</param>
        /// <param name="discountAmount">The discount amount to apply to the order.</param>
        /// <param name="totalAmount">The total amount after discount.</param>
        /// <returns>True if checkout was successful, otherwise false.</returns>
        Task<bool> CheckoutBasketAsync(int userId, int basketId, double discountAmount = 0, double totalAmount = 0);
    }
}

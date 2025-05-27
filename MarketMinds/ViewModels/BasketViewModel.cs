using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BasketService;

namespace MarketMinds.ViewModels
{
    public class BasketViewModel
    {
        private const int NullDiscount = 0;
        private const int DefaultQuantity = 1;
        private User currentUser;
        private readonly IBasketService basketService;
        private Basket basket;

        public List<BasketItem> BasketItems { get; private set; }
        public double Subtotal { get; private set; }
        public double Discount { get; private set; }
        public double TotalAmount { get; private set; }
        public string PromoCode { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsCheckoutInProgress { get; private set; }
        public bool CheckoutSuccess { get; private set; }

        public BasketViewModel(User currentUser, BasketService basketService)
        {
            this.currentUser = currentUser;
            this.basketService = basketService;
            BasketItems = new List<BasketItem>();
            PromoCode = string.Empty;
            ErrorMessage = string.Empty;
            IsCheckoutInProgress = false;
            CheckoutSuccess = false;
        }

        public void LoadBasket()
        {
            try
            {
                basket = basketService.GetBasketByUser(currentUser);
                BasketItems = basket.Items;

                // Use service to calculate totals instead of local method
                UpdateTotals();
            }
            catch (Exception basketLoadException)
            {
                ErrorMessage = $"Failed to load basket: {basketLoadException.Message}";
            }
        }

        public void AddToBasket(int productId)
        {
            try
            {
                basketService.AddProductToBasket(currentUser.Id, productId, DefaultQuantity);
                LoadBasket();
                ErrorMessage = string.Empty;
            }
            catch (Exception basketAddException)
            {
                ErrorMessage = $"Failed to add product to basket: {basketAddException.Message}";
            }
        }

        public void RemoveProductFromBasket(int productId)
        {
            try
            {
                basketService.RemoveProductFromBasket(currentUser.Id, productId);
                LoadBasket();
            }
            catch (Exception basketRemoveException)
            {
                ErrorMessage = $"Failed to remove product: {basketRemoveException.Message}";
            }
        }

        public void UpdateProductQuantity(int productId, int quantity)
        {
            try
            {
                if (quantity > BasketService.MAXIMUM_QUANTITY_PER_ITEM)
                {
                    ErrorMessage = $"Quantity cannot exceed {BasketService.MAXIMUM_QUANTITY_PER_ITEM}";

                    basketService.UpdateProductQuantity(currentUser.Id, productId, BasketService.MAXIMUM_QUANTITY_PER_ITEM);
                }
                else
                {
                    ErrorMessage = string.Empty;
                    basketService.UpdateProductQuantity(currentUser.Id, productId, quantity);
                }
                LoadBasket();
            }
            catch (Exception productQuantityUpdateException)
            {
                ErrorMessage = $"Failed to update quantity: {productQuantityUpdateException.Message}";
            }
        }

        public BasketItem GetBasketItemById(int itemId)
        {
            return BasketItems.FirstOrDefault(item => item.Id == itemId);
        }

        public bool UpdateQuantityFromText(int itemId, string quantityText, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                // Validate input using service
                if (!basketService.ValidateQuantityInput(quantityText, out int newQuantity))
                {
                    errorMessage = "Please enter a valid quantity";
                    return false;
                }

                // Find the corresponding basket item
                var basketItem = GetBasketItemById(itemId);
                if (basketItem == null)
                {
                    errorMessage = "Item not found";
                    return false;
                }

                // Update the quantity through proper service call
                UpdateProductQuantity(basketItem.Product.Id, newQuantity);
                return true;
            }
            catch (Exception quantityUpdateException)
            {
                errorMessage = $"Failed to update quantity: {quantityUpdateException.Message}";
                return false;
            }
        }

        public void ApplyPromoCode(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    ErrorMessage = "Please enter a promo code.";
                    return;
                }

                basketService.ApplyPromoCode(basket.Id, code);
                PromoCode = code;

                // Update totals with the new promo code
                UpdateTotals();
                ErrorMessage = $"Promo code '{code}' applied successfully.";
            }
            catch (Exception promoCodeApplicationException)
            {
                ErrorMessage = $"Failed to apply promo code: {promoCodeApplicationException.Message}";
                Discount = NullDiscount;
                TotalAmount = Subtotal;
            }
        }

        public void ClearBasket()
        {
            try
            {
                basketService.ClearBasket(currentUser.Id);
                LoadBasket();
                PromoCode = string.Empty;
            }
            catch (Exception basketClearException)
            {
                ErrorMessage = $"Failed to clear basket: {basketClearException.Message}";
            }
        }

        public bool CanCheckout()
        {
            return basket != null && BasketItems.Any() && !IsCheckoutInProgress &&
                   basketService.ValidateBasketBeforeCheckOut(basket.Id);
        }

        public async Task<bool> CheckoutAsync()
        {
            if (!CanCheckout())
            {
                ErrorMessage = "Cannot checkout. Please check your basket items.";
                return false;
            }

            try
            {
                IsCheckoutInProgress = true;
                ErrorMessage = string.Empty;

                // Call the service to process the checkout
                CheckoutSuccess = await basketService.CheckoutBasketAsync(currentUser.Id, basket.Id, Discount, TotalAmount);

                if (CheckoutSuccess)
                {
                    // Clear the local basket after successful checkout
                    BasketItems.Clear();
                    UpdateTotals();
                    ErrorMessage = string.Empty;
                    return true;
                }
                else
                {
                    Debug.WriteLine("Checkout failed");
                    ErrorMessage = "Checkout failed. Please try again later.";
                    return false;
                }
            }
            catch (InvalidOperationException insufficientFundsCheckoutException) when (insufficientFundsCheckoutException.Message.Contains("Insufficient funds"))
            {
                Debug.WriteLine($"Insufficient funds error: {insufficientFundsCheckoutException.Message}");
                ErrorMessage = insufficientFundsCheckoutException.Message;
                return false;
            }
            catch (Exception checkoutException)
            {
                Debug.WriteLine($"Error during checkout: {checkoutException.Message}");
                ErrorMessage = $"Checkout error: {checkoutException.Message}";
                return false;
            }
            finally
            {
                IsCheckoutInProgress = false;
            }
        }

        public void DecreaseProductQuantity(int productId)
        {
            try
            {
                ErrorMessage = string.Empty;
                basketService.DecreaseProductQuantity(currentUser.Id, productId);
                LoadBasket();
            }
            catch (Exception productQuantityDecreaseException)
            {
                ErrorMessage = $"Failed to decrease quantity: {productQuantityDecreaseException.Message}";
            }
        }

        public void IncreaseProductQuantity(int productId)
        {
            try
            {
                ErrorMessage = string.Empty;
                basketService.IncreaseProductQuantity(currentUser.Id, productId);
                LoadBasket();
            }
            catch (Exception productQuantityIncreaseException)
            {
                ErrorMessage = $"Failed to increase quantity: {productQuantityIncreaseException.Message}";
            }
        }

        // New method to update totals using the service
        private void UpdateTotals()
        {
            try
            {
                Subtotal = BasketItems?.Sum(item => item.Price * item.Quantity) ?? 0;

                Discount = string.IsNullOrEmpty(PromoCode) ? NullDiscount :
                    basketService.GetPromoCodeDiscount(PromoCode, Subtotal);

                TotalAmount = Math.Max(0, Subtotal - Discount);
            }
            catch (Exception totalUpdateException)
            {
                Subtotal = BasketItems?.Sum(item => item.Price * item.Quantity) ?? 0;
                Discount = NullDiscount;
                TotalAmount = Subtotal;
            }
        }
    }
}
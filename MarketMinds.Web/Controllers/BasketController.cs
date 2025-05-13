using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BasketService;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using MarketMinds.Web.Models;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
    public class BasketController : Controller
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        // GET: Basket
        public IActionResult Index(string promoCode = null)
        {
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
            try
            {
                // Get the user's basket
                var user = new User { Id = userId };
                var basket = _basketService.GetBasketByUser(user);
                
                var basketTotals = _basketService.CalculateBasketTotals(basket.Id, promoCode);
                
                // Add debug info to diagnose the issue with basketTotals
                if (basketTotals == null)
                {
                    Debug.WriteLine("ERROR: basketTotals is null");
                    basketTotals = new MarketMinds.Shared.Services.BasketService.BasketTotals(); // Create a default object to avoid null reference
                }
                else
                {
                    Debug.WriteLine($"DEBUG: basketTotals - Subtotal: {basketTotals.Subtotal}, Discount: {basketTotals.Discount}, Total: {basketTotals.TotalAmount}");
                }
                
                // Recalculate subtotal if needed by summing basket items
                if (basket.Items != null && basket.Items.Count > 0)
                {
                    double calculatedSubtotal = 0;
                    foreach (var item in basket.Items)
                    {
                        calculatedSubtotal += (item.Price * item.Quantity);
                    }
                    
                    if (basketTotals.Subtotal <= 0 || Math.Abs(basketTotals.Subtotal - calculatedSubtotal) > 0.01)
                    {
                        Debug.WriteLine($"DEBUG: Had to recalculate subtotal: {calculatedSubtotal}");
                        basketTotals.Subtotal = calculatedSubtotal;
                    }
                    
                    if (!string.IsNullOrEmpty(promoCode))
                    {
                        try
                        {
                            double discountAmount = _basketService.GetPromoCodeDiscount(promoCode, calculatedSubtotal);
                            if (Math.Abs(basketTotals.Discount - discountAmount) > 0.01)
                            {
                                Debug.WriteLine($"DEBUG: Recalculated discount: {discountAmount}");
                                basketTotals.Discount = discountAmount;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"DEBUG: Error calculating discount: {ex.Message}");
                        }
                    }
                    
                    basketTotals.TotalAmount = basketTotals.Subtotal - basketTotals.Discount;
                }
                
                ViewBag.BasketTotals = basketTotals;
                ViewBag.AppliedPromoCode = promoCode;
                
                return View(basket);
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error getting basket: {ex.Message}");
                
                // Return an empty basket if there was an error
                var emptyBasket = new Basket { Id = 0, Items = new System.Collections.Generic.List<BasketItem>() };
                
                // Create default basket totals to avoid null reference in the view
                ViewBag.BasketTotals = new MarketMinds.Shared.Services.BasketService.BasketTotals();
                
                return View(emptyBasket);
            }
        }

        // POST: Basket/AddItem
        [HttpPost]
        public IActionResult AddItem(int productId, int quantity = 1)
        {
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
            try
            {
                // Validate the quantity input
                if (quantity <= 0)
                {
                    quantity = 1;
                }

                // Add the product to the basket
                _basketService.AddProductToBasket(userId, productId, quantity);
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error adding item to basket: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to add item to basket. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/RemoveItem
        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
            try
            {
                // Remove the product from the basket
                _basketService.RemoveProductFromBasket(userId, productId);
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error removing item from basket: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to remove item from basket. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
            try
            {
                // Validate the quantity input
                if (quantity <= 0)
                {
                    // If quantity is 0 or negative, remove the item
                    _basketService.RemoveProductFromBasket(userId, productId);
                }
                else
                {
                    // Update the quantity
                    _basketService.UpdateProductQuantity(userId, productId, quantity);
                }
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error updating quantity: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to update quantity. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/IncreaseQuantity
        [HttpPost]
        public IActionResult IncreaseQuantity(int productId)
        {
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
            try
            {
                // Increase the quantity by 1
                _basketService.IncreaseProductQuantity(userId, productId);
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error increasing quantity: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to increase quantity. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/DecreaseQuantity
        [HttpPost]
        public IActionResult DecreaseQuantity(int productId)
        {
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
            try
            {
                // Decrease the quantity by 1
                _basketService.DecreaseProductQuantity(userId, productId);
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error decreasing quantity: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to decrease quantity. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/ClearBasket
        [HttpPost]
        public IActionResult ClearBasket()
        {
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
            try
            {
                // Clear the basket
                _basketService.ClearBasket(userId);
                
                // Redirect to the basket page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error clearing basket: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to clear basket. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/ApplyPromoCode
        [HttpPost]
        public IActionResult ApplyPromoCode(string promoCode)
        {
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            
            try
            {
                if (string.IsNullOrWhiteSpace(promoCode))
                {
                    TempData["ErrorMessage"] = "Please enter a promo code.";
                    return RedirectToAction("Index");
                }
                
                promoCode = promoCode.Trim().ToUpper();
                
                // Get the user's basket
                var user = new User { Id = userId };
                var basket = _basketService.GetBasketByUser(user);
                
                try
                {
                    _basketService.ApplyPromoCode(basket.Id, promoCode);
                    
                    double subtotal = 0;
                    if (basket.Items != null)
                    {
                        foreach (var item in basket.Items)
                        {
                            subtotal += (item.Price * item.Quantity);
                        }
                    }
                    
                    double discount = _basketService.GetPromoCodeDiscount(promoCode, subtotal);
                    
                    if (discount > 0)
                    {
                        int percentage = (int)Math.Round(discount / subtotal * 100);
                        TempData["SuccessMessage"] = $"Promo code '{promoCode}' applied for {percentage}% discount!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Promo code applied, but no discount was awarded.";
                    }
                    
                    return RedirectToAction("Index", new { promoCode });
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine($"Invalid promo code: {ex.Message}");
                    TempData["ErrorMessage"] = $"Invalid promo code: {ex.Message}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error applying promo code: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "Failed to apply promo code. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Basket/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(string promoCode)
        {
            // Get the current user's ID from claims
            int userId = User.GetCurrentUserId();
            Debug.WriteLine($"DEBUG: Checkout called with promoCode: {promoCode}");
            
            try
            {
                // Get the user's basket
                var user = new User { Id = userId };
                var basket = _basketService.GetBasketByUser(user);
                
                // Validate the basket before checkout
                if (basket.Items == null || basket.Items.Count == 0)
                {
                    TempData["ErrorMessage"] = "Your basket is empty. Please add items before checkout.";
                    return RedirectToAction("Index");
                }
                
                if (!_basketService.ValidateBasketBeforeCheckOut(basket.Id))
                {
                    TempData["ErrorMessage"] = "Your basket is invalid for checkout. Please ensure all items have valid quantities.";
                    return RedirectToAction("Index");
                }
                
                // Calculate subtotal
                double subtotal = 0;
                foreach (var item in basket.Items)
                {
                    subtotal += (item.Price * item.Quantity);
                }
                
                double discount = 0;
                if (!string.IsNullOrEmpty(promoCode))
                {
                    try
                    {
                        discount = _basketService.GetPromoCodeDiscount(promoCode, subtotal);
                        Debug.WriteLine($"DEBUG: Applied promo code {promoCode} with discount: ${discount:F2}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"DEBUG: Error applying promo code during checkout: {ex.Message}");
                    }
                }
                
                double total = subtotal - discount;
                
                Debug.WriteLine($"DEBUG: Proceeding to checkout - Subtotal: ${subtotal:F2}, Discount: ${discount:F2}, Total: ${total:F2}");
                
                // Perform the checkout
                bool success = await _basketService.CheckoutBasketAsync(userId, basket.Id, discount, total);
                
                if (success)
                {
                    // Redirect to a confirmation page
                    TempData["SuccessMessage"] = "Your order has been successfully placed!";
                    return RedirectToAction("OrderConfirmation");
                }
                else
                {
                    // Return to the basket page with an error message
                    TempData["ErrorMessage"] = "Checkout failed. Please try again.";
                    return RedirectToAction("Index", new { promoCode });
                }
            }
            catch (InvalidOperationException ex)
            {
                // Log the error
                Debug.WriteLine($"Checkout validation error: {ex.Message}");
                
                // Return to the basket page with the specific error message
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", new { promoCode });
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error during checkout: {ex.Message}");
                
                // Return to the basket page with an error message
                TempData["ErrorMessage"] = "An error occurred during checkout. Please try again.";
                return RedirectToAction("Index", new { promoCode });
            }
        }

        // GET: Basket/OrderConfirmation
        public IActionResult OrderConfirmation()
        {
            return View();
        }
    }
} 
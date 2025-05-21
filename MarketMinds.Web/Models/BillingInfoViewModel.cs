using System.ComponentModel.DataAnnotations;
using MarketMinds.Shared.Models;
// using MarketMinds.Shared.Services; // No longer directly instantiating services
// using MarketMinds.Shared.Services.Interfaces; // No longer directly instantiating services
// using Microsoft.Extensions.Configuration; // No longer used here
// using System.IO; // No longer used here
using System.Collections.Generic; // For List<T>
using System.Linq; // For .Any()
using System;

namespace WebMarketplace.Models
{
    public class BillingInfoViewModel
    {
        // Removed service fields: _orderHistoryService, _orderService, _orderSummaryService, _productService, _dummyWalletService, _shoppingCartService
        
        public int OrderHistoryID { get; set; }

        public bool IsWalletEnabled { get; set; } = true; // Default to true, controller can override
        public bool IsCashEnabled { get; set; } = true;   // Default to true, controller can override
        public bool IsCardEnabled { get; set; } = true;   // Default to true, controller can override

        [Required(ErrorMessage = "Please select a payment method")]
        public string SelectedPaymentMethod { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Zip code is required")]
        [Display(Name = "Zip Code")]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Please enter a valid zip code")]
        public string ZipCode { get; set; }

        [Display(Name = "Additional Information")]
        public string AdditionalInfo { get; set; }

        // StartDate and EndDate for borrow products - these might be better associated per product 
        // or handled differently if there are multiple borrowable items in the cart.
        // For now, keeping them as they are, but their usage in ApplyBorrowedTax needs review.
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public double Subtotal { get; set; } // Should be populated by the controller or CalculateOrderTotal
        public double DeliveryFee { get; set; } // Should be populated by the controller or CalculateOrderTotal
        public double Total { get; set; } // Should be populated by the controller or CalculateOrderTotal
        public double WarrantyTax { get; set; } // Should be populated by the controller or influenced by tax logic

        public List<Product> ProductList { get; set; }

        public BillingInfoViewModel()
        {
            // Service instantiations removed.
            ProductList = new List<Product>();
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(1);
            // SetVisibilityRadioButtons(); // Controller should handle this if needed based on app settings
        }

        public BillingInfoViewModel(int orderHistoryID) : this()
        {
            OrderHistoryID = orderHistoryID;
        }

        // Removed: public async Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryID)
        // This logic should be in the controller.

        // Removed: public void SetVisibilityRadioButtons()
        // Payment method availability should be determined by config/settings and passed by the controller if needed.

        public void CalculateOrderTotal() // This method is okay as it operates on the ViewModel's own data.
        {
            if (ProductList == null || !ProductList.Any())
            {
                Total = 0;
                Subtotal = 0;
                DeliveryFee = 0;
                WarrantyTax = 0; // Ensure warranty tax is reset too
                return;
            }

            double subtotalProducts = 0;
            foreach (var product in ProductList)
            {
                if (product is BuyProduct buyProduct)
                {
                    // Multiply price by stock (quantity) for BuyProducts
                    subtotalProducts += buyProduct.Price * buyProduct.Stock;
                }
                else if (product is BorrowProduct borrowProduct)
                {
                    subtotalProducts += borrowProduct.Price; 
                }
                else
                {
                    subtotalProducts += product.Price;
                }
            }
            Subtotal = subtotalProducts;

            // Recalculate WarrantyTax based on current ProductList and dates if logic is kept here.
            // For simplicity in this step, assuming WarrantyTax is either set externally or calculated 
            // by a more focused method if ApplyBorrowedTax is called.
            // The current ApplyBorrowedTax modifies WarrantyTax directly.

            bool hasSpecialType = ProductList.Any(p =>
                (p is BorrowProduct) || 
                (p.GetType().Name.Contains("Refill")) || 
                (p.GetType().Name.Contains("Auction"))); 

            if (Subtotal >= 200 || hasSpecialType)
            {
                DeliveryFee = 0;
            }
            else
            {
                DeliveryFee = 13.99;
            }
            Total = Subtotal + DeliveryFee + WarrantyTax; // Make sure WarrantyTax is included
        }

        // Removed: public async Task ApplyBorrowedTax(Product product)
        // This logic needs to move to a service or controller action.
        // The controller's UpdateBorrowedTax action will handle this.
    }
}
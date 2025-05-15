using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;

namespace WebMarketplace.Models
{
    public class BillingInfoViewModel
    {
        private readonly IOrderHistoryService _orderHistoryService;
        private readonly IOrderSummaryService _orderSummaryService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IDummyWalletService _dummyWalletService;
        private readonly IShoppingCartService _shoppingCartService;
        public int OrderHistoryID { get; set; }

        public bool IsWalletEnabled { get; set; }
        public bool IsCashEnabled { get; set; }
        public bool IsCardEnabled { get; set; }

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

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public double Subtotal { get; set; }
        public double DeliveryFee { get; set; }
        public double Total { get; set; }
        public double WarrantyTax { get; set; }

        public List<Product> ProductList { get; set; }
        
        // Default parameterless constructor for model binding
        public BillingInfoViewModel()
        {
            _orderHistoryService = new OrderHistoryService();
            _orderService = new OrderService();
            _orderSummaryService = new OrderSummaryService();
            _dummyWalletService = new DummyWalletService();
            _productService = new ProductService();
            _shoppingCartService = new ShoppingCartService();
            ProductList = _shoppingCartService.GetCartItemsAsync(UserSession.CurrentUserId ?? 1).Result;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(1);
            CalculateOrderTotal();
        }

        public BillingInfoViewModel(int orderHistoryID) : this()
        {
            OrderHistoryID = orderHistoryID;
            InitializeViewModel(orderHistoryID);
        }

        private async void InitializeViewModel(int orderHistoryID)
        {
            try
            {
                ProductList = await GetProductsFromOrderHistoryAsync(orderHistoryID);
                SetVisibilityRadioButtons();
                CalculateOrderTotal();
            }
            catch (Exception ex)
            {
                SetVisibilityRadioButtons();
                // Log the exception
                Console.WriteLine($"Error loading from order history: {ex.Message}");
            }
        }

        public async Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            return await _orderHistoryService.GetProductsFromOrderHistoryAsync(orderHistoryID);
        }

        public void SetVisibilityRadioButtons()
        {
            IsCardEnabled = true;
            IsCashEnabled = true;
            IsWalletEnabled = true;
        }

        public void CalculateOrderTotal()
        {
            if (ProductList == null || ProductList.Count == 0)
            {
                Total = 0;
                Subtotal = 0;
                DeliveryFee = 0;
                return;
            }

            double subtotalProducts = 0;
            foreach (var product in ProductList)
            {
                subtotalProducts += product.Price;
            }

            // For orders over 200, a fixed delivery fee of 13.99 will be added
            // (this is only for orders of new, used or borrowed products)
            Subtotal = subtotalProducts;

            string productType = ProductList[0].ProductType;
            if (subtotalProducts >= 200 || productType == "refill" || productType == "bid")
            {
                Total = subtotalProducts;
                DeliveryFee = 0;
            }
            else
            {
                DeliveryFee = 13.99;
                Total = subtotalProducts + DeliveryFee;
            }
        }

        public async Task ApplyBorrowedTax(Product product)
        {
            if (product == null || product.ProductType != "borrowed")
            {
                return;
            }
            
            if (StartDate > EndDate)
            {
                return;
            }
            
            int monthsBorrowed = ((EndDate.Year - StartDate.Year) * 12) + EndDate.Month - StartDate.Month;
            if (monthsBorrowed <= 0)
            {
                monthsBorrowed = 1;
            }

            double warrantyTaxAmount = 0.2;
            double finalPrice = product.Price * monthsBorrowed;
            WarrantyTax += finalPrice * warrantyTaxAmount;
            product.Price = finalPrice + WarrantyTax;

            CalculateOrderTotal();

            product.StartDate = StartDate;
            product.EndDate = EndDate;

            // Ensure sellerId is a valid integer, default to 1 if null
            int sellerId = ((int?)product.SellerId).HasValue ? (int)product.SellerId : 1;

            await _productService.UpdateProductAsync(
                product.ProductId, 
                product.Name, 
                product.Price, 
                sellerId, 
                product.ProductType, 
                StartDate, 
                EndDate);
        }
    }
} 
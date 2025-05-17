using System.ComponentModel.DataAnnotations;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;

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
            // get the IConfiguration
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var productRepository = new MarketMinds.Shared.ProxyRepository.BuyProductsProxyRepository(configuration);
            _productService = new ProductService(productRepository);
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
                // Check product type for proper price access
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

            // For orders over 200, a fixed delivery fee of 13.99 will be added
            // (this is only for orders of new, used or borrowed products)
            Subtotal = subtotalProducts;

            // Determine product type for delivery fee calculation
            // Try to determine if any product is a special type
            bool hasSpecialType = ProductList.Any(p => 
                (p is BorrowProduct) || // For borrowed products
                (p.GetType().Name.Contains("Refill")) || // For refill products
                (p.GetType().Name.Contains("Auction"))); // For auction/bid products
            
            if (subtotalProducts >= 200 || hasSpecialType)
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
            if (product == null)
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
            
            // Cast to correct product type if needed
            double productPrice = 0;
            if (product is BuyProduct buyProduct)
            {
                productPrice = buyProduct.Price;
                double finalPrice = productPrice * monthsBorrowed;
                WarrantyTax += finalPrice * warrantyTaxAmount;
                buyProduct.Price = finalPrice + WarrantyTax;
            }
            else if (product is BorrowProduct borrowProduct)
            {
                // Handle BorrowProduct if needed
                // Access properties specific to BorrowProduct
            }
            else
            {
                // Fall back to the base Product price
                productPrice = product.Price;
                double finalPrice = productPrice * monthsBorrowed;
                WarrantyTax += finalPrice * warrantyTaxAmount;
                // Note: Can't modify price directly on base Product as it might be read-only
            }

            CalculateOrderTotal();

            // These dates should probably be stored in a separate model or service
            // rather than directly on the Product
        }
    }
} 
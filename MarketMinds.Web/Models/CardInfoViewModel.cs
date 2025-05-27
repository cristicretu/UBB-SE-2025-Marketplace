using System.ComponentModel.DataAnnotations;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.BuyProductsService;
using Microsoft.Extensions.Configuration;

namespace WebMarketplace.Models
{
    public class CardInfoViewModel
    {
        private readonly IOrderHistoryService _orderHistoryService;
        private readonly IBuyProductsService _productService;

        public int OrderHistoryID { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Cardholder name is required")]
        [Display(Name = "Cardholder's Name")]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "Card number is required")]
        [Display(Name = "Card Number")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Please enter a valid 16-digit card number")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expiry month is required")]
        [Display(Name = "Expiry Month")]
        [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "Please enter a valid month (01-12)")]
        public string CardMonth { get; set; }

        [Required(ErrorMessage = "Expiry year is required")]
        [Display(Name = "Expiry Year")]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Please enter a valid 2-digit year")]
        public string CardYear { get; set; }

        [Required(ErrorMessage = "CVC is required")]
        [Display(Name = "CVC")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "Please enter a valid 3-digit CVC")]
        public string CardCVC { get; set; }

        public double Subtotal { get; set; }
        public double DeliveryFee { get; set; }
        public double Total { get; set; }

        public List<Product> ProductList { get; set; }

        // Default parameterless constructor for model binding
        public CardInfoViewModel()
        {
            _orderHistoryService = new OrderHistoryService();

            // Create a configuration object that works with BuyProductsProxyRepository
            var configValues = new Dictionary<string, string>
            {
                {"ApiSettings:BaseUrl", MarketMinds.Shared.Helper.AppConfig.GetBaseApiUrl()}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            var buyProductsRepo = new MarketMinds.Shared.ProxyRepository.BuyProductsProxyRepository(configuration);
            _productService = new BuyProductsService(buyProductsRepo);

            ProductList = new List<Product>();
        }

        public CardInfoViewModel(int orderHistoryID) : this()
        {
            OrderHistoryID = orderHistoryID;
            InitializeViewModel(orderHistoryID);
        }

        private async void InitializeViewModel(int orderHistoryID)
        {
            try
            {
                ProductList = await _orderHistoryService.GetProductsFromOrderHistoryAsync(orderHistoryID);
                CalculateOrderTotal();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading from order history: {ex.Message}");
            }
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
    }
}
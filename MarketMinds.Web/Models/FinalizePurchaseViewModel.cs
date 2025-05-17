using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;

namespace WebMarketplace.Models
{
    public class FinalizePurchaseViewModel
    {
        private readonly IOrderHistoryService _orderHistoryService;
        private readonly IOrderSummaryService _orderSummaryService;
        
        public int OrderHistoryID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderStatus { get; set; } = "Pending";
        
        public double Subtotal { get; set; }
        public double DeliveryFee { get; set; }
        public double Total { get; set; }
        
        public List<Product> ProductList { get; set; }
        
        // Default parameterless constructor for model binding
        public FinalizePurchaseViewModel()
        {
            _orderHistoryService = new OrderHistoryService();
            _orderSummaryService = new OrderSummaryService();
            
            ProductList = new List<Product>();
        }
        
        public FinalizePurchaseViewModel(int orderHistoryID) : this()
        {
            OrderHistoryID = orderHistoryID;
            InitializeViewModel(orderHistoryID);
        }
        
        private async void InitializeViewModel(int orderHistoryID)
        {
            try
            {
                // Get products from order history
                ProductList = await _orderHistoryService.GetProductsFromOrderHistoryAsync(orderHistoryID);
                
                // Get order summary details
                var orderSummary = await _orderSummaryService.GetOrderSummaryByIdAsync(orderHistoryID);
                if (orderSummary != null)
                {
                    FullName = orderSummary.FullName;
                    Email = orderSummary.Email;
                    PhoneNumber = orderSummary.PhoneNumber;
                    Address = orderSummary.Address;
                    Subtotal = orderSummary.Subtotal;
                    DeliveryFee = orderSummary.DeliveryFee;
                    Total = orderSummary.FinalTotal;
                }
                else
                {
                    CalculateOrderTotal();
                }
                
                // Get payment method from first order
                var orders = await new OrderService().GetOrdersFromOrderHistoryAsync(orderHistoryID);
                if (orders != null && orders.Count > 0)
                {
                    PaymentMethod = orders[0].PaymentMethod;
                }
                else
                {
                    PaymentMethod = "Not specified";
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading order details: {ex.Message}");
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
            string productType = "standard";
            
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
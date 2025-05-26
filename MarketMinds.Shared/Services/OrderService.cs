using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Helper;
using MarketMinds.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MarketMinds.Shared.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;
        private readonly IOrderHistoryService _orderHistoryService;
        private readonly IOrderSummaryService _orderSummaryService;
        private readonly IShoppingCartService _shoppingCartService;

        public OrderService()
        {
            this.orderRepository = new OrderProxyRepository(AppConfig.GetBaseApiUrl());
            this._orderHistoryService = new OrderHistoryService();
            this._orderSummaryService = new OrderSummaryService();
            this._shoppingCartService = new ShoppingCartService();
        }


        public OrderService(
            IOrderRepository orderRepository,
            IOrderHistoryService orderHistoryService,
            IOrderSummaryService orderSummaryService,
            IShoppingCartService shoppingCartService)
        {
            this.orderRepository = orderRepository;
            _orderHistoryService = orderHistoryService;
            _orderSummaryService = orderSummaryService;
            _shoppingCartService = shoppingCartService;
        }

        public async Task AddOrderAsync(int productId, int buyerId, string productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            ValidateOrderParameters(productId, buyerId, productType, paymentMethod, orderSummaryId);
            await orderRepository.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);
        }

        public async Task UpdateOrderAsync(int orderId, string productType, string paymentMethod, DateTime orderDate)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Order ID must be positive", nameof(orderId));
            }
            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Payment method cannot be empty", nameof(paymentMethod));
            }
            await orderRepository.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate);
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Order ID must be positive", nameof(orderId));
            }
            await orderRepository.DeleteOrderAsync(orderId);
        }

        public async Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            if (buyerId < 0)
            {
                throw new ArgumentException("Buyer ID cannot be negative", nameof(buyerId));
            }
            return await orderRepository.GetBorrowedOrderHistoryAsync(buyerId);
        }

        public async Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            if (buyerId < 0)
            {
                throw new ArgumentException("Buyer ID cannot be negative", nameof(buyerId));
            }
            return await orderRepository.GetNewOrUsedOrderHistoryAsync(buyerId);
        }

        public async Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text)
        {
            if (buyerId < 0)
            {
                throw new ArgumentException("Buyer ID cannot be negative", nameof(buyerId));
            }
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Search text cannot be empty", nameof(text));
            }
            return await orderRepository.GetOrdersByNameAsync(buyerId, text);
        }

        public async Task<List<Order>> GetOrdersFrom2024Async(int buyerId)
        {
            if (buyerId < 0)
            {
                throw new ArgumentException("Buyer ID cannot be negative", nameof(buyerId));
            }
            return await orderRepository.GetOrdersFrom2024Async(buyerId);
        }

        public async Task<List<Order>> GetOrdersFrom2025Async(int buyerId)
        {
            if (buyerId < 0)
            {
                throw new ArgumentException("Buyer ID cannot be negative", nameof(buyerId));
            }
            return await orderRepository.GetOrdersFrom2025Async(buyerId);
        }

        public async Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId)
        {
            if (buyerId < 0)
            {
                throw new ArgumentException("Buyer ID cannot be negative", nameof(buyerId));
            }
            return await orderRepository.GetOrdersFromLastSixMonthsAsync(buyerId);
        }

        public async Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId)
        {
            if (buyerId < 0)
            {
                throw new ArgumentException("Buyer ID cannot be negative", nameof(buyerId));
            }
            return await orderRepository.GetOrdersFromLastThreeMonthsAsync(buyerId);
        }

        public async Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            if (orderHistoryId <= 0)
            {
                throw new ArgumentException("Order history ID must be positive", nameof(orderHistoryId));
            }
            return await orderRepository.GetOrdersFromOrderHistoryAsync(orderHistoryId);
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Order ID must be positive", nameof(orderId));
            }

            return await orderRepository.GetOrderByIdAsync(orderId);
        }

        public async Task<List<Order>> GetCombinedOrderHistoryAsync(int buyerId, string timePeriodFilter = "all")
        {
            if (buyerId < 0)
            {
                throw new ArgumentException("Buyer ID cannot be negative", nameof(buyerId));
            }
            List<Order> orders = new List<Order>();

            switch (timePeriodFilter.ToLower())
            {
                case "3months":
                    orders = await orderRepository.GetOrdersFromLastThreeMonthsAsync(buyerId);
                    break;
                case "6months":
                    orders = await orderRepository.GetOrdersFromLastSixMonthsAsync(buyerId);
                    break;
                case "2024":
                    orders = await orderRepository.GetOrdersFrom2024Async(buyerId);
                    break;
                case "2025":
                    orders = await orderRepository.GetOrdersFrom2025Async(buyerId);
                    break;
                case "all":
                default:
                    var borrowedOrders = await orderRepository.GetBorrowedOrderHistoryAsync(buyerId);
                    var newUsedOrders = await orderRepository.GetNewOrUsedOrderHistoryAsync(buyerId);
                    orders.AddRange(borrowedOrders);
                    orders.AddRange(newUsedOrders);
                    break;
            }

            return orders;
        }

        public async Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, string searchText = null, string timePeriod = null)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be positive", nameof(userId));
            }

            return await orderRepository.GetOrdersWithProductInfoAsync(userId, searchText, timePeriod);
        }

        public async Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, int offset, int count, string searchText = null, string timePeriod = null)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be positive", nameof(userId));
            }
            if (offset < 0)
            {
                throw new ArgumentException("Offset cannot be negative", nameof(offset));
            }
            if (count <= 0)
            {
                throw new ArgumentException("Count must be positive", nameof(count));
            }

            return await orderRepository.GetOrdersWithProductInfoAsync(userId, offset, count, searchText, timePeriod);
        }

        public async Task<int> GetOrdersCountAsync(int userId, string searchText = null, string timePeriod = null)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be positive", nameof(userId));
            }

            return await orderRepository.GetOrdersCountAsync(userId, searchText, timePeriod);
        }

        public async Task<Dictionary<int, string>> GetProductCategoryTypesAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be positive", nameof(userId));
            }

            return await orderRepository.GetProductCategoryTypesAsync(userId);
        }

        public async Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId)
        {
            if (orderSummaryId <= 0)
            {
                throw new ArgumentException("Order summary ID must be positive", nameof(orderSummaryId));
            }

            return await orderRepository.GetOrderSummaryAsync(orderSummaryId);
        }

        public async Task<int> CreateOrderFromCartAsync(OrderCreationRequestDto orderRequestDto, int userId, List<Product> cartItems)
        {
            Debug.WriteLine("[OrderService] Attempting to create order from cart.");
            Debug.WriteLine($"[OrderService] Using Base API URL: {AppConfig.GetBaseApiUrl()} (ensure this is correct)");

            if (orderRequestDto == null)
            {
                Debug.WriteLine("[OrderService] Error: orderRequestDto is null.");
                throw new ArgumentNullException(nameof(orderRequestDto));
            }
            if (cartItems == null || !cartItems.Any())
            {
                Debug.WriteLine("[OrderService] Error: cartItems is null or empty.");
                throw new ArgumentException("Cart items cannot be null or empty", nameof(cartItems));
            }
            if (userId <= 0)
            {
                Debug.WriteLine("[OrderService] Error: userId is invalid.");
                throw new ArgumentException("User ID must be positive", nameof(userId));
            }

            int orderHistoryId = 0;
            int orderSummaryId = 0;

            try
            {
                Debug.WriteLine($"[OrderService] Step 1: Creating OrderHistory for userId: {userId}");
                orderHistoryId = await _orderHistoryService.CreateOrderHistoryAsync(userId);
                Debug.WriteLine($"[OrderService] Step 1 Success: Created OrderHistoryId: {orderHistoryId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderService] CRITICAL ERROR in Step 1 (CreateOrderHistoryAsync): {ex.ToString()}");
                throw;
            }

            try
            {
                Debug.WriteLine($"[OrderService] Step 2: Creating OrderSummary. OrderHistoryId: {orderHistoryId}");
                var newOrderSummary = new OrderSummary
                {
                    Subtotal = orderRequestDto.Subtotal,
                    WarrantyTax = orderRequestDto.WarrantyTax,
                    DeliveryFee = orderRequestDto.DeliveryFee,
                    FinalTotal = orderRequestDto.Total,
                    FullName = orderRequestDto.FullName,
                    Email = orderRequestDto.Email,
                    PhoneNumber = orderRequestDto.PhoneNumber,
                    Address = orderRequestDto.Address,
                    PostalCode = orderRequestDto.ZipCode,
                    AdditionalInfo = orderRequestDto.AdditionalInfo,
                    ContractDetails = null
                };
                orderSummaryId = await _orderSummaryService.CreateOrderSummaryAsync(newOrderSummary);
                Debug.WriteLine($"[OrderService] Step 2 Success: Created OrderSummaryId: {orderSummaryId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderService] CRITICAL ERROR in Step 2 (CreateOrderSummaryAsync): {ex.ToString()}");
                throw;
            }

            try
            {
                Debug.WriteLine($"[OrderService] Step 3: Adding Order items. OrderSummaryId: {orderSummaryId}");
                foreach (var product in cartItems)
                {
                    string productType;
                    if (product is BorrowProduct)
                    {
                        productType = "borrowed";
                    }
                    else if (product is BuyProduct)
                    {
                        productType = "new";
                    }
                    else
                    {
                        throw new ArgumentException($"Unsupported product type: {product.GetType().Name}");
                    }

                    Debug.WriteLine($"[OrderService] Adding product to order: ProductId={product.Id}, Type={productType}, BuyerId={userId}, PaymentMethod={orderRequestDto.SelectedPaymentMethod}");
                    await orderRepository.AddOrderAsync(
                        productId: product.Id,
                        buyerId: userId,
                        productType: productType,
                        paymentMethod: orderRequestDto.SelectedPaymentMethod,
                        orderSummaryId: orderSummaryId,
                        orderDate: DateTime.UtcNow
                    );
                    Debug.WriteLine($"[OrderService] Successfully added product: {product.Id}");
                }
                Debug.WriteLine("[OrderService] Step 3 Success: All order items added.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderService] CRITICAL ERROR in Step 3 (AddOrderAsync loop): {ex.ToString()}");
                throw;
            }

            try
            {
                Debug.WriteLine($"[OrderService] Step 4: Clearing shopping cart for userId: {userId}");
                await _shoppingCartService.ClearCartAsync(userId);
                Debug.WriteLine("[OrderService] Step 4 Success: Shopping cart cleared.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderService] ERROR in Step 4 (ClearCartAsync): {ex.ToString()}");
                throw;
            }

            return orderHistoryId;
        }

        private void ValidateOrderParameters(int productId, int buyerId, string productType, string paymentMethod, int orderSummaryId)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be positive", nameof(productId));
            }
            if (buyerId < 0)
            {
                throw new ArgumentException("Buyer ID cannot be negative", nameof(buyerId));
            }
            if (string.IsNullOrWhiteSpace(productType))
            {
                throw new ArgumentException("Product type cannot be empty", nameof(productType));
            }
            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Payment method cannot be empty", nameof(paymentMethod));
            }
            if (orderSummaryId <= 0)
            {
                throw new ArgumentException("Order summary ID must be positive", nameof(orderSummaryId));
            }
        }
    }
}
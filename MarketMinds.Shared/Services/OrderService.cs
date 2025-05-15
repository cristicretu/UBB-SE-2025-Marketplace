using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Helper;

namespace SharedClassLibrary.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;


        public OrderService()
        {
            this.orderRepository = new OrderProxyRepository(AppConfig.GetBaseApiUrl());
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

            var borrowedOrders = await orderRepository.GetBorrowedOrderHistoryAsync(0);
            foreach (var order in borrowedOrders)
            {
                if (order.OrderID == orderId)
                {
                    return order;
                }
            }

            var newUsedOrders = await orderRepository.GetNewOrUsedOrderHistoryAsync(0);
            foreach (var order in newUsedOrders)
            {
                if (order.OrderID == orderId)
                {
                    return order;
                }
            }

            return null;
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

        private void ValidateOrderParameters(int productId, int buyerId, string productType, string paymentMethod, int orderSummaryId)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be positive", nameof(productId));
            }
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be positive", nameof(buyerId));
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
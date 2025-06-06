using MarketMinds.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketMinds.Shared.Services
{
    public interface IOrderService
    {
        Task AddOrderAsync(int productId, int buyerId, string productType, string paymentMethod, int orderSummaryId, System.DateTime orderDate);
        Task UpdateOrderAsync(int orderId, string productType, string paymentMethod, System.DateTime orderDate);
        Task DeleteOrderAsync(int orderId);
        Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId);
        Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId);
        Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text);
        Task<List<Order>> GetOrdersFrom2024Async(int buyerId);
        Task<List<Order>> GetOrdersFrom2025Async(int buyerId);
        Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId);
        Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId);
        Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetCombinedOrderHistoryAsync(int buyerId, string timePeriodFilter = "all");
        Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, string searchText = null, string timePeriod = null);
        Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, int offset, int count, string searchText = null, string timePeriod = null);
        Task<int> GetOrdersCountAsync(int userId, string searchText = null, string timePeriod = null);
        Task<Dictionary<int, string>> GetProductCategoryTypesAsync(int userId);
        Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId);

        Task<int> CreateOrderFromCartAsync(OrderCreationRequestDto orderRequestDto, int userId, List<Product> cartItems);
    }
}
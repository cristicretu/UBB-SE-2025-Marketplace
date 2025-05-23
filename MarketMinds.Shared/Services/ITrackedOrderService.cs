using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services
{
    public interface ITrackedOrderService
    {
        Task<TrackedOrder?> GetTrackedOrderByIDAsync(int trackedOrderID);
        Task<OrderCheckpoint?> GetOrderCheckpointByIDAsync(int checkpointID);
        Task<List<TrackedOrder>> GetAllTrackedOrdersAsync();
        Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID);
        Task<bool> DeleteTrackedOrderAsync(int trackedOrderID);
        Task<bool> DeleteOrderCheckpointAsync(int checkpointID);
        Task<int> AddTrackedOrderAsync(TrackedOrder trackedOrder);
        Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint);
        Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status);
        Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, OrderStatus currentStatus);
        Task<OrderCheckpoint?> GetLastCheckpointAsync(TrackedOrder order);
        Task<int> GetNumberOfCheckpointsAsync(TrackedOrder order);
        Task RevertToPreviousCheckpointAsync(TrackedOrder? order);
        Task<TrackedOrder?> GetTrackedOrderByOrderIdAsync(int orderId);
        Task<int> UpdateOrderStatusWithCheckpointAsync(int trackedOrderID, OrderStatus newStatus, string description = "Status updated", string? location = null);
        Task<int> CreateTrackedOrderForOrderAsync(
            int orderId,
            DateOnly estimatedDeliveryDate,
            string deliveryAddress,
            OrderStatus initialStatus = OrderStatus.PROCESSING,
            string initialDescription = "Order received");
        Task<List<TrackedOrder>> GetTrackedOrdersByBuyerIdAsync(int buyerId);
        Task<int> CalculateDeliveryProgressPercentageAsync(int trackedOrderId);

        // Potentially add other business logic methods here in the future
    }
}
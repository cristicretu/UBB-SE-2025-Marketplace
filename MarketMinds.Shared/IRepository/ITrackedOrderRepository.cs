using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.IRepository
{
    public interface ITrackedOrderRepository
    {
        Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint);
        Task<int> AddTrackedOrderAsync(TrackedOrder order);
        Task<bool> DeleteOrderCheckpointAsync(int checkpointID);
        Task<bool> DeleteTrackedOrderAsync(int trackOrderID);
        Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID);
        Task<List<TrackedOrder>> GetAllTrackedOrdersAsync();
        Task<OrderCheckpoint> GetOrderCheckpointByIdAsync(int checkpointID);
        Task<TrackedOrder> GetTrackedOrderByIdAsync(int trackOrderID);
        Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status);
        Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, OrderStatus currentStatus);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.Service
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

        // Potentially add other business logic methods here in the future
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace MarketPlace924.ViewModel
{
    /// <summary>
    /// Defines operations for tracking orders including checkpoints management, reversion, and updates.
    /// </summary>
    internal interface ITrackedOrderViewModel
    {
        /// <summary>
        /// Asynchronously adds a new order checkpoint.
        /// </summary>
        /// <param name="checkpoint">The <see cref="OrderCheckpoint"/> object to add.</param>
        /// <returns>A task that returns the unique identifier of the added checkpoint.</returns>
        Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint);

        /// <summary>
        /// Asynchronously adds a new tracked order.
        /// </summary>
        /// <param name="order">The <see cref="TrackedOrder"/> object to add.</param>
        /// <returns>A task that returns the unique identifier of the added tracked order.</returns>
        Task<int> AddTrackedOrderAsync(TrackedOrder order);

        /// <summary>
        /// Asynchronously deletes the specified order checkpoint.
        /// </summary>
        /// <param name="checkpointID">The unique identifier of the checkpoint to delete.</param>
        /// <returns>A task that returns <c>true</c> if deletion was successful; otherwise, <c>false</c>.</returns>
        Task<bool> DeleteOrderCheckpointAsync(int checkpointID);

        /// <summary>
        /// Asynchronously deletes the specified tracked order.
        /// </summary>
        /// <param name="trackOrderID">The unique identifier of the tracked order to delete.</param>
        /// <returns>A task that returns <c>true</c> if deletion was successful; otherwise, <c>false</c>.</returns>
        Task<bool> DeleteTrackedOrderAsync(int trackOrderID);

        /// <summary>
        /// Asynchronously retrieves all order checkpoints for the specified tracked order.
        /// </summary>
        /// <param name="trackedOrderID">The unique identifier of the tracked order.</param>
        /// <returns>A task that returns a list of <see cref="OrderCheckpoint"/> objects.</returns>
        Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID);

        /// <summary>
        /// Asynchronously retrieves all tracked orders.
        /// </summary>
        /// <returns>A task that returns a list of <see cref="TrackedOrder"/> objects.</returns>
        Task<List<TrackedOrder>> GetAllTrackedOrdersAsync();

        /// <summary>
        /// Asynchronously retrieves the last checkpoint for the specified order.
        /// </summary>
        /// <param name="order">The tracked order for which to retrieve the last checkpoint.</param>
        /// <returns>
        /// A task that returns the last <see cref="OrderCheckpoint"/> object, or <c>null</c> if none exists.
        /// </returns>
        Task<OrderCheckpoint?> GetLastCheckpoint(TrackedOrder order);

        /// <summary>
        /// Asynchronously retrieves an order checkpoint by its unique identifier.
        /// </summary>
        /// <param name="checkpointID">The unique identifier of the checkpoint.</param>
        /// <returns>
        /// A task that returns the <see cref="OrderCheckpoint"/> object if found; otherwise, <c>null</c>.
        /// </returns>
        Task<OrderCheckpoint?> GetOrderCheckpointByIDAsync(int checkpointID);

        /// <summary>
        /// Asynchronously retrieves a tracked order by its unique identifier.
        /// </summary>
        /// <param name="trackedOrderID">The unique identifier of the tracked order.</param>
        /// <returns>
        /// A task that returns the <see cref="TrackedOrder"/> object if found; otherwise, <c>null</c>.
        /// </returns>
        Task<TrackedOrder?> GetTrackedOrderByIDAsync(int trackedOrderID);

        /// <summary>
        /// Asynchronously reverts the specified order to its last checkpoint.
        /// </summary>
        /// <param name="order">The tracked order to revert.</param>
        /// <returns>
        /// A task that returns <c>true</c> if the revert was successful; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> RevertToLastCheckpoint(TrackedOrder order);

        /// <summary>
        /// Asynchronously reverts the specified order to its previous checkpoint.
        /// </summary>
        /// <param name="order">The tracked order to revert. Can be <c>null</c>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RevertToPreviousCheckpoint(TrackedOrder? order);

        /// <summary>
        /// Asynchronously updates the specified order checkpoint with the provided parameters.
        /// </summary>
        /// <param name="checkpointID">The unique identifier of the checkpoint to update.</param>
        /// <param name="timestamp">The timestamp of the checkpoint.</param>
        /// <param name="location">An optional location associated with the checkpoint.</param>
        /// <param name="description">A description of the checkpoint.</param>
        /// <param name="status">The status of the order at the checkpoint.</param>
        /// <param name="trackedOrderID">The unique identifier of the tracked order associated with the checkpoint.</param>
        /// <returns>
        /// A task that returns <c>true</c> if the update was successful; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status, int trackedOrderID);

        /// <summary>
        /// Asynchronously updates the specified order checkpoint with the provided parameters.
        /// </summary>
        /// <param name="checkpointID">The unique identifier of the checkpoint to update.</param>
        /// <param name="timestamp">The timestamp of the checkpoint.</param>
        /// <param name="location">The location associated with the checkpoint.</param>
        /// <param name="description">A description of the checkpoint.</param>
        /// <param name="orderStatus">The status of the order at the checkpoint.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string location, string description, OrderStatus orderStatus);

        /// <summary>
        /// Asynchronously updates the tracked order with a new estimated delivery date, delivery address, current status, and associated order ID.
        /// </summary>
        /// <param name="trackedOrderID">The unique identifier of the tracked order to update.</param>
        /// <param name="estimatedDeliveryDate">The new estimated delivery date.</param>
        /// <param name="deliveryAddress">The delivery address.</param>
        /// <param name="currentStatus">The current status of the tracked order.</param>
        /// <param name="orderID">The unique identifier of the associated order.</param>
        /// <returns>
        /// A task that returns <c>true</c> if the update was successful; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, string deliveryAddress, OrderStatus currentStatus, int orderID);

        /// <summary>
        /// Asynchronously updates the tracked order with a new estimated delivery date and current status.
        /// </summary>
        /// <param name="trackedOrderID">The unique identifier of the tracked order to update.</param>
        /// <param name="newEstimatedDeliveryDate">The new estimated delivery date.</param>
        /// <param name="currentStatus">The current status of the tracked order.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly newEstimatedDeliveryDate, OrderStatus currentStatus);

        // New methods for handling business logic from views
        Task LoadOrderDataAsync(int trackedOrderID);
        Task UpdateEstimatedDeliveryDateAsync(int trackedOrderID, DateTime newDate);
        Task AddNewCheckpointAsync(int trackedOrderID, string description);
        Task UpdateLastCheckpointAsync(int trackedOrderID, string description);
        Task RevertLastCheckpointAsync(int trackedOrderID);
    }
}

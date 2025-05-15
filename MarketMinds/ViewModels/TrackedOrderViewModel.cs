using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Shared;
using SharedClassLibrary.Service;

namespace MarketPlace924.ViewModel
{
    /// <summary>
    /// Interface for sending notifications about order shipping progress
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a notification about order shipping progress
        /// </summary>
        /// <param name="buyerId">ID of the buyer receiving the notification</param>
        /// <param name="trackedOrderId">ID of the tracked order</param>
        /// <param name="status">Current order status</param>
        /// <param name="estimatedDeliveryDate">Estimated delivery date</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SendShippingProgressNotificationAsync(int buyerId, int trackedOrderId, string status, DateTime estimatedDeliveryDate);
    }

    /// <summary>
    /// Default implementation of the notification service using NotificationViewModel
    /// </summary>
    public class DefaultNotificationService : INotificationService
    {
        /// <summary>
        /// Sends a notification about order shipping progress
        /// </summary>
        /// <param name="buyerId">ID of the buyer receiving the notification</param>
        /// <param name="trackedOrderId">ID of the tracked order</param>
        /// <param name="status">Current order status</param>
        /// <param name="estimatedDeliveryDate">Estimated delivery date</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task SendShippingProgressNotificationAsync(int buyerId, int trackedOrderId, string status, DateTime estimatedDeliveryDate)
        {
            try
            {
                NotificationViewModel buyerNotificationViewModel = new NotificationViewModel(buyerId);
                Notification orderShippingNotification = new OrderShippingProgressNotification(
                    buyerId,
                    DateTime.Now,
                    trackedOrderId,
                    status,
                    estimatedDeliveryDate);
                await buyerNotificationViewModel.AddNotificationAsync(orderShippingNotification);
            }
            catch (Exception)
            {
                // Silently handle notification failures
                // Consider logging in a production environment
            }
        }
    }

    /// <summary>
    /// ViewModel for managing tracked orders
    /// </summary>
    public class TrackedOrderViewModel : ITrackedOrderViewModel
    {
        private readonly ITrackedOrderService trackedOrderService;
        private readonly IOrderViewModel orderViewModel;
        private readonly INotificationService notificationService;

        // Properties for data binding
        public int TrackedOrderID { get; private set; }
        public int OrderID { get; private set; }
        public OrderStatus CurrentStatus { get; private set; }
        public DateOnly EstimatedDeliveryDate { get; private set; }
        public string DeliveryAddress { get; private set; }
        public List<OrderCheckpoint> Checkpoints { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TrackedOrderViewModel with default implementations
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        public TrackedOrderViewModel(string connectionString)
        {
            IDatabaseProvider databaseProvider = new SqlDatabaseProvider();
            trackedOrderService = new TrackedOrderService();
            orderViewModel = new OrderViewModel(connectionString);
            notificationService = new DefaultNotificationService();
            Checkpoints = new List<OrderCheckpoint>();
        }

        /// <summary>
        /// Initializes a new instance of the TrackedOrderViewModel with specified dependencies (for testing)
        /// </summary>
        /// <param name="trackedOrderService">Model for tracked order operations</param>
        /// <param name="orderViewModel">View model for order operations</param>
        /// <param name="notificationService">Service for sending notifications</param>
        public TrackedOrderViewModel(
            ITrackedOrderService trackedOrderService,
            IOrderViewModel orderViewModel,
            INotificationService notificationService)
        {
            if (trackedOrderService == null)
            {
                throw new ArgumentNullException(nameof(trackedOrderService));
            }
            if (orderViewModel == null)
            {
                throw new ArgumentNullException(nameof(orderViewModel));
            }
            if (notificationService == null)
            {
                throw new ArgumentNullException(nameof(notificationService));
            }

            this.trackedOrderService = trackedOrderService;
            this.orderViewModel = orderViewModel;
            this.notificationService = notificationService;
            Checkpoints = new List<OrderCheckpoint>();
        }

        /// <summary>
        /// Retrieves a tracked order by its ID
        /// </summary>
        /// <param name="trackedOrderID">The ID of the tracked order to retrieve</param>
        /// <returns>The tracked order with the specified ID</returns>
        public async Task<TrackedOrder?> GetTrackedOrderByIDAsync(int trackedOrderID)
        {
            try
            {
                return await trackedOrderService.GetTrackedOrderByIDAsync(trackedOrderID);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves an order checkpoint by its ID
        /// </summary>
        /// <param name="checkpointID">The ID of the checkpoint to retrieve</param>
        /// <returns>The order checkpoint with the specified ID</returns>
        public async Task<OrderCheckpoint?> GetOrderCheckpointByIDAsync(int checkpointID)
        {
            try
            {
                return await trackedOrderService.GetOrderCheckpointByIDAsync(checkpointID);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves all tracked orders
        /// </summary>
        /// <returns>A list of all tracked orders</returns>
        public async Task<List<TrackedOrder>> GetAllTrackedOrdersAsync()
        {
            return await trackedOrderService.GetAllTrackedOrdersAsync();
        }

        /// <summary>
        /// Retrieves all checkpoints for a specific tracked order
        /// </summary>
        /// <param name="trackedOrderID">The ID of the tracked order</param>
        /// <returns>A list of all checkpoints for the specified tracked order</returns>
        public async Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID)
        {
            return await trackedOrderService.GetAllOrderCheckpointsAsync(trackedOrderID);
        }

        /// <summary>
        /// Deletes a tracked order by its ID
        /// </summary>
        /// <param name="trackedOrderID">The ID of the tracked order to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteTrackedOrderAsync(int trackedOrderID)
        {
            return await trackedOrderService.DeleteTrackedOrderAsync(trackedOrderID);
        }

        /// <summary>
        /// Deletes an order checkpoint by its ID
        /// </summary>
        /// <param name="checkpointID">The ID of the checkpoint to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteOrderCheckpointAsync(int checkpointID)
        {
            return await trackedOrderService.DeleteOrderCheckpointAsync(checkpointID);
        }

        /// <summary>
        /// Adds a new tracked order and sends a notification to the buyer
        /// </summary>
        /// <param name="trackedOrder">The tracked order to add</param>
        /// <returns>The ID of the newly added tracked order</returns>
        /// <exception cref="Exception">Thrown when there's an error adding the tracked order</exception>
        public async Task<int> AddTrackedOrderAsync(TrackedOrder trackedOrder)
        {
            try
            {
                int newTrackedOrderID = await trackedOrderService.AddTrackedOrderAsync(trackedOrder);
                trackedOrder.TrackedOrderID = newTrackedOrderID;

                try
                {
                    Order order = await orderViewModel.GetOrderByIdAsync(trackedOrder.OrderID);
                    if (order != null)
                    {
                        await notificationService.SendShippingProgressNotificationAsync(
                            order.BuyerID,
                            trackedOrder.TrackedOrderID,
                            trackedOrder.CurrentStatus.ToString(),
                            trackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)));
                    }
                }
                catch
                {
                    // Notification failures shouldn't stop the order tracking process
                }

                return newTrackedOrderID;
            }
            catch (Exception exception)
            {
                throw new Exception($"Error adding TrackedOrder: {exception.Message}", exception);
            }
        }

        /// <summary>
        /// Adds a new order checkpoint and updates the tracked order status
        /// </summary>
        /// <param name="checkpoint">The checkpoint to add</param>
        /// <returns>The ID of the newly added checkpoint</returns>
        /// <exception cref="Exception">Thrown when there's an error adding the checkpoint</exception>
        public async Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint)
        {
            try
            {
                int newCheckpointID = await trackedOrderService.AddOrderCheckpointAsync(checkpoint);
                TrackedOrder trackedOrder = await trackedOrderService.GetTrackedOrderByIDAsync(checkpoint.TrackedOrderID);
                await UpdateTrackedOrderAsync(trackedOrder.TrackedOrderID, trackedOrder.EstimatedDeliveryDate, checkpoint.Status);

                if (checkpoint.Status == OrderStatus.SHIPPED || checkpoint.Status == OrderStatus.OUT_FOR_DELIVERY)
                {
                    try
                    {
                        Order order = await orderViewModel.GetOrderByIdAsync(trackedOrder.OrderID);
                        if (order != null)
                        {
                            await notificationService.SendShippingProgressNotificationAsync(
                                order.BuyerID,
                                trackedOrder.TrackedOrderID,
                                trackedOrder.CurrentStatus.ToString(),
                                trackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)));
                        }
                    }
                    catch
                    {
                        // Notification failures shouldn't stop the order tracking process
                    }
                }

                return newCheckpointID;
            }
            catch (Exception exception)
            {
                throw new Exception($"Error adding OrderCheckpoint: {exception.Message}", exception);
            }
        }

        /// <summary>
        /// Updates an existing order checkpoint and updates the tracked order status
        /// </summary>
        /// <param name="checkpointID">The ID of the checkpoint to update</param>
        /// <param name="timestamp">The new timestamp</param>
        /// <param name="location">The new location</param>
        /// <param name="description">The new description</param>
        /// <param name="status">The new order status</param>
        /// <exception cref="Exception">Thrown when there's an error updating the checkpoint</exception>
        public async Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string location, string description, OrderStatus status)
        {
            try
            {
                await trackedOrderService.UpdateOrderCheckpointAsync(checkpointID, timestamp, location, description, status);

                OrderCheckpoint updatedCheckpoint = await trackedOrderService.GetOrderCheckpointByIDAsync(checkpointID);
                TrackedOrder associatedTrackedOrder = await trackedOrderService.GetTrackedOrderByIDAsync(updatedCheckpoint.TrackedOrderID);

                await UpdateTrackedOrderAsync(
                    associatedTrackedOrder.TrackedOrderID,
                    associatedTrackedOrder.EstimatedDeliveryDate,
                    updatedCheckpoint.Status);
            }
            catch (Exception exception)
            {
                throw new Exception($"Error updating OrderCheckpoint: {exception.Message}", exception);
            }
        }

        /// <summary>
        /// Updates an existing order checkpoint with additional tracked order ID parameter
        /// </summary>
        /// <param name="checkpointID">The ID of the checkpoint to update</param>
        /// <param name="timestamp">The new timestamp</param>
        /// <param name="location">The new location</param>
        /// <param name="description">The new description</param>
        /// <param name="status">The new order status</param>
        /// <param name="trackedOrderID">The tracked order ID</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public async Task<bool> UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status, int trackedOrderID)
        {
            try
            {
                // First verify that the checkpoint belongs to the specified tracked order
                OrderCheckpoint checkpoint = await trackedOrderService.GetOrderCheckpointByIDAsync(checkpointID);
                if (checkpoint.TrackedOrderID != trackedOrderID)
                {
                    return false;
                }

                await trackedOrderService.UpdateOrderCheckpointAsync(checkpointID, timestamp, location, description, status);

                TrackedOrder associatedTrackedOrder = await trackedOrderService.GetTrackedOrderByIDAsync(trackedOrderID);
                await UpdateTrackedOrderAsync(
                    associatedTrackedOrder.TrackedOrderID,
                    associatedTrackedOrder.EstimatedDeliveryDate,
                    status);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Updates an existing tracked order with new status and delivery date
        /// </summary>
        /// <param name="trackedOrderID">The ID of the tracked order to update</param>
        /// <param name="estimatedDeliveryDate">The new estimated delivery date</param>
        /// <param name="currentStatus">The new order status</param>
        /// <exception cref="Exception">Thrown when there's an error updating the tracked order</exception>
        public async Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, OrderStatus currentStatus)
        {
            try
            {
                await trackedOrderService.UpdateTrackedOrderAsync(trackedOrderID, estimatedDeliveryDate, currentStatus);
                TrackedOrder updatedTrackedOrder = await trackedOrderService.GetTrackedOrderByIDAsync(trackedOrderID);

                if (updatedTrackedOrder.CurrentStatus == OrderStatus.SHIPPED || updatedTrackedOrder.CurrentStatus == OrderStatus.OUT_FOR_DELIVERY)
                {
                    try
                    {
                        Order order = await orderViewModel.GetOrderByIdAsync(updatedTrackedOrder.OrderID);
                        if (order != null)
                        {
                            await notificationService.SendShippingProgressNotificationAsync(
                                order.BuyerID,
                                updatedTrackedOrder.TrackedOrderID,
                                updatedTrackedOrder.CurrentStatus.ToString(),
                                updatedTrackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)));
                        }
                    }
                    catch
                    {
                        // Notification failures shouldn't stop the order tracking process
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error updating TrackedOrder: {exception.Message}", exception);
            }
        }

        /// <summary>
        /// Updates an existing tracked order with full parameter list
        /// </summary>
        /// <param name="trackedOrderID">The ID of the tracked order to update</param>
        /// <param name="estimatedDeliveryDate">The new estimated delivery date</param>
        /// <param name="deliveryAddress">The new delivery address</param>
        /// <param name="currentStatus">The new order status</param>
        /// <param name="orderID">The order ID</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public async Task<bool> UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, string deliveryAddress, OrderStatus currentStatus, int orderID)
        {
            try
            {
                // First update the tracked order status and delivery date
                await trackedOrderService.UpdateTrackedOrderAsync(trackedOrderID, estimatedDeliveryDate, currentStatus);

                // Then get the updated tracked order to verify and send notifications if needed
                TrackedOrder updatedTrackedOrder = await trackedOrderService.GetTrackedOrderByIDAsync(trackedOrderID);

                // Verify that the order ID matches
                if (updatedTrackedOrder.OrderID != orderID)
                {
                    return false;
                }

                // Send notifications for shipping status changes
                if (currentStatus == OrderStatus.SHIPPED || currentStatus == OrderStatus.OUT_FOR_DELIVERY)
                {
                    try
                    {
                        Order order = await orderViewModel.GetOrderByIdAsync(orderID);
                        if (order != null)
                        {
                            await notificationService.SendShippingProgressNotificationAsync(
                                order.BuyerID,
                                trackedOrderID,
                                currentStatus.ToString(),
                                estimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)));
                        }
                    }
                    catch
                    {
                        // Notification failures shouldn't stop the order tracking process
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the most recent checkpoint for a tracked order
        /// </summary>
        /// <param name="order">The tracked order</param>
        /// <returns>The most recent checkpoint, or null if none exists</returns>
        public async Task<OrderCheckpoint?> GetLastCheckpoint(TrackedOrder order)
        {
            List<OrderCheckpoint> allCheckpoints = await trackedOrderService.GetAllOrderCheckpointsAsync(order.TrackedOrderID);
            return allCheckpoints.OrderByDescending(c => c.Timestamp).FirstOrDefault();
        }

        /// <summary>
        /// Gets the total number of checkpoints for a tracked order
        /// </summary>
        /// <param name="order">The tracked order</param>
        /// <returns>The number of checkpoints</returns>
        public async Task<int> GetNumberOfCheckpoints(TrackedOrder order)
        {
            List<OrderCheckpoint> allCheckpoints = await trackedOrderService.GetAllOrderCheckpointsAsync(order.TrackedOrderID);
            return allCheckpoints.Count;
        }

        /// <summary>
        /// Reverts the tracked order to its previous checkpoint state
        /// </summary>
        /// <param name="order">The tracked order to revert</param>
        /// <exception cref="Exception">Thrown when the order cannot be reverted or an error occurs</exception>
        public async Task RevertToPreviousCheckpoint(TrackedOrder? order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order), "Order cannot be null");
            }

            int checkpointCount = await GetNumberOfCheckpoints(order);
            const int minimumCheckpointsForReversion = 1;

            if (checkpointCount <= minimumCheckpointsForReversion)
            {
                throw new Exception("Cannot revert further");
            }

            var currentCheckpoint = await GetLastCheckpoint(order);
            if (currentCheckpoint != null)
            {
                bool deletionSuccessful = await DeleteOrderCheckpointAsync(currentCheckpoint.CheckpointID);
                if (deletionSuccessful)
                {
                    OrderCheckpoint? previousCheckpoint = await GetLastCheckpoint(order);
                    if (previousCheckpoint != null)
                    {
                        await UpdateTrackedOrderAsync(order.TrackedOrderID, order.EstimatedDeliveryDate, previousCheckpoint.Status);
                    }
                }
                else
                {
                    throw new Exception("Failed to delete the current checkpoint");
                }
            }
            else
            {
                throw new Exception("No checkpoints found to revert");
            }
        }

        /// <summary>
        /// Reverts to the last checkpoint for a tracked order
        /// </summary>
        /// <param name="order">The tracked order</param>
        /// <returns>True if reversion was successful, false otherwise</returns>
        public async Task<bool> RevertToLastCheckpoint(TrackedOrder order)
        {
            try
            {
                if (order == null)
                {
                    return false;
                }

                var lastCheckpoint = await GetLastCheckpoint(order);
                if (lastCheckpoint == null)
                {
                    return false;
                }

                // Revert to the status of the last checkpoint
                await UpdateTrackedOrderAsync(
                    order.TrackedOrderID,
                    order.EstimatedDeliveryDate,
                    lastCheckpoint.Status);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Loads order data and updates ViewModel properties
        /// </summary>
        /// <param name="trackedOrderID">The ID of the tracked order to load</param>
        public async Task LoadOrderDataAsync(int trackedOrderID)
        {
            var trackedOrder = await GetTrackedOrderByIDAsync(trackedOrderID);
            if (trackedOrder != null)
            {
                TrackedOrderID = trackedOrder.TrackedOrderID;
                OrderID = trackedOrder.OrderID;
                CurrentStatus = trackedOrder.CurrentStatus;
                EstimatedDeliveryDate = trackedOrder.EstimatedDeliveryDate;
                DeliveryAddress = trackedOrder.DeliveryAddress;
                Checkpoints = await GetAllOrderCheckpointsAsync(trackedOrderID);

            }
        }

        public async Task UpdateEstimatedDeliveryDateAsync(int trackedOrderID, DateTime newDate)
        {
            try
            {
                var order = await GetTrackedOrderByIDAsync(trackedOrderID);
                if (order != null)
                {
                    await UpdateTrackedOrderAsync(
                        trackedOrderID,
                        DateOnly.FromDateTime(newDate),
                        order.CurrentStatus);
                }
            }
            catch (Exception)
            {
                // Handle error appropriately
            }
        }

        public async Task AddNewCheckpointAsync(int trackedOrderID, string description)
        {
            try
            {
                var order = await GetTrackedOrderByIDAsync(trackedOrderID);
                if (order != null)
                {
                    await AddOrderCheckpointAsync(new OrderCheckpoint
                    {
                        Timestamp = DateTime.Now,
                        Location = string.Empty,
                        Description = description,
                        Status = order.CurrentStatus,
                        TrackedOrderID = trackedOrderID
                    });
                }
            }
            catch (Exception)
            {
                // Handle error appropriately
            }
        }

        public async Task UpdateLastCheckpointAsync(int trackedOrderID, string description)
        {
            try
            {
                var order = await GetTrackedOrderByIDAsync(trackedOrderID);
                if (order != null)
                {
                    var lastCheckpoint = await GetLastCheckpoint(order);
                    if (lastCheckpoint != null)
                    {
                        await UpdateOrderCheckpointAsync(
                            lastCheckpoint.CheckpointID,
                            lastCheckpoint.Timestamp,
                            lastCheckpoint.Location,
                            description,
                            lastCheckpoint.Status);
                    }
                }
            }
            catch (Exception)
            {
                // Handle error appropriately
            }
        }

        public async Task RevertLastCheckpointAsync(int trackedOrderID)
        {
            try
            {
                var order = await GetTrackedOrderByIDAsync(trackedOrderID);
                if (order != null)
                {
                    await RevertToPreviousCheckpoint(order);
                }
            }
            catch (Exception)
            {
                // Handle error appropriately
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MarketMinds.Services;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using Microsoft.UI.Xaml.Input;

namespace MarketMinds.ViewModels
{
    /// <summary>
    /// ViewModel for managing tracked orders
    /// </summary>
    public class TrackedOrderViewModel : INotifyPropertyChanged, ITrackedOrderViewModel
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private readonly ITrackedOrderService trackedOrderService;
        private readonly IOrderViewModel orderViewModel;
        private readonly MarketMinds.Shared.Services.INotificationService notificationService;
        private string orderId;
        private TrackedOrder currentOrder;
        private bool isLoading;
        private string errorMessage;
        private ObservableCollection<OrderCheckpoint> checkpoints;
        private int trackedOrderID;
        private int orderID;
        private OrderStatus currentStatus;
        private DateOnly estimatedDeliveryDate;
        private string deliveryAddress;

        public ICommand TrackOrderCommand { get; }
        public ICommand RefreshCommand { get; }

        // Properties for data binding
        public TrackedOrder CurrentOrder
        {
            get => currentOrder;
            private set => SetProperty(ref currentOrder, value);
        }

        public string OrderId
        {
            get => orderId;
            set => SetProperty(ref orderId, value);
        }

        public int TrackedOrderID
        {
            get => trackedOrderID;
            private set => SetProperty(ref trackedOrderID, value);
        }

        public int OrderID
        {
            get => orderID;
            private set => SetProperty(ref orderID, value);
        }

        public OrderStatus CurrentStatus
        {
            get => currentStatus;
            private set => SetProperty(ref currentStatus, value);
        }

        public DateOnly EstimatedDeliveryDate
        {
            get => estimatedDeliveryDate;
            private set => SetProperty(ref estimatedDeliveryDate, value);
        }

        public string DeliveryAddress
        {
            get => deliveryAddress;
            private set => SetProperty(ref deliveryAddress, value);
        }

        public ObservableCollection<OrderCheckpoint> Checkpoints
        {
            get => checkpoints;
            private set => SetProperty(ref checkpoints, value);
        }

        public bool IsLoading
        {
            get => isLoading;
            private set => SetProperty(ref isLoading, value);
        }

        public string ErrorMessage
        {
            get => errorMessage;
            private set => SetProperty(ref errorMessage, value);
        }

        public TrackedOrderViewModel(
            ITrackedOrderService trackedOrderService,
            IOrderViewModel orderViewModel,
            MarketMinds.Shared.Services.INotificationService notificationService)
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
            Checkpoints = new ObservableCollection<OrderCheckpoint>();

            TrackOrderCommand = new MarketMinds.Shared.Helper.RelayCommand<string>(async (orderId) => await TrackOrderAsync(orderId));
            RefreshCommand = new MarketMinds.Shared.Helper.RelayCommand(async () => await RefreshOrderDataAsync());
        }

        private async Task RefreshOrderDataAsync()
        {
            if (CurrentOrder != null)
            {
                await LoadOrderDataAsync(CurrentOrder.TrackedOrderID);
            }
        }

        private async Task TrackOrderAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                ErrorMessage = "Please enter an order ID";
                return;
            }

            if (!int.TryParse(orderId, out int orderIdInt))
            {
                ErrorMessage = "Please enter a valid order ID";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var trackedOrder = await trackedOrderService.GetTrackedOrderByOrderIdAsync(orderIdInt);
                if (trackedOrder == null)
                {
                    ErrorMessage = "Order not found";
                    return;
                }

                await LoadOrderDataAsync(trackedOrder.TrackedOrderID);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error tracking order: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
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
                    // Order order = await orderViewModel.GetOrderByIdAsync(trackedOrder.OrderID);
                    // if (order != null)
                    // {
                    //     await notificationService.SendShippingProgressNotificationAsync(
                    //         order.Id,
                    //         trackedOrder.TrackedOrderID,
                    //         trackedOrder.CurrentStatus.ToString(),
                    //         trackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)));
                    // }
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
                // Get the latest checkpoint for the tracked order
                var lastCheckpoint = await GetLastCheckpoint(new TrackedOrder { TrackedOrderID = checkpoint.TrackedOrderID, DeliveryAddress = string.Empty });

                // If there are existing checkpoints, ensure the new checkpoint's timestamp is after the latest one
                if (lastCheckpoint != null && checkpoint.Timestamp <= lastCheckpoint.Timestamp)
                {
                    throw new Exception("New checkpoint timestamp must be after the latest existing checkpoint's timestamp.");
                }

                int newCheckpointID = await trackedOrderService.AddOrderCheckpointAsync(checkpoint);
                TrackedOrder trackedOrder = await trackedOrderService.GetTrackedOrderByIDAsync(checkpoint.TrackedOrderID);
                await UpdateTrackedOrderAsync(trackedOrder.TrackedOrderID, trackedOrder.EstimatedDeliveryDate, checkpoint.Status);

                if (checkpoint.Status == OrderStatus.SHIPPED || checkpoint.Status == OrderStatus.OUT_FOR_DELIVERY)
                {
                    try
                    {
                        // Order order = await orderViewModel.GetOrderByIdAsync(trackedOrder.OrderID);
                        // if (order != null)
                        // {
                        //     await notificationService.SendShippingProgressNotificationAsync(
                        //         order.Id,
                        //         trackedOrder.TrackedOrderID,
                        //         trackedOrder.CurrentStatus.ToString(),
                        //         trackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)));
                        // }
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
                        // Order order = await orderViewModel.GetOrderByIdAsync(updatedTrackedOrder.OrderID);
                        // if (order != null)
                        // {
                        //     await notificationService.SendShippingProgressNotificationAsync(
                        //        order.Id,
                        //        updatedTrackedOrder.TrackedOrderID,
                        //        updatedTrackedOrder.CurrentStatus.ToString(),
                        //        updatedTrackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)));
                        // }
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
                        // Order order = await orderViewModel.GetOrderByIdAsync(orderID);
                        // if (order != null)
                        // {
                        //     await notificationService.SendShippingProgressNotificationAsync(
                        //         order.Id,
                        //         trackedOrderID,
                        //         currentStatus.ToString(),
                        //         estimatedDeliveryDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)));
                        // }
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

            Debug.WriteLine($"Reverting to previous checkpoint for order {order.TrackedOrderID}");

            // Get all checkpoints ordered by timestamp
            var allCheckpoints = await GetAllOrderCheckpointsAsync(order.TrackedOrderID);
            if (allCheckpoints == null || allCheckpoints.Count <= 1)
            {
                throw new Exception("Cannot revert: Need at least 2 checkpoints to perform reversion");
            }

            // Order checkpoints by timestamp descending (newest first)
            var orderedCheckpoints = allCheckpoints.OrderByDescending(c => c.Timestamp).ToList();

            // Get the current (last) checkpoint
            var currentCheckpoint = orderedCheckpoints.First();
            Debug.WriteLine($"Current checkpoint ID: {currentCheckpoint.CheckpointID}, Status: {currentCheckpoint.Status}");

            // Get the previous checkpoint
            var previousCheckpoint = orderedCheckpoints.Skip(1).First();
            Debug.WriteLine($"Previous checkpoint ID: {previousCheckpoint.CheckpointID}, Status: {previousCheckpoint.Status}");

            try
            {
                // Delete the current checkpoint
                bool deletionSuccessful = await DeleteOrderCheckpointAsync(currentCheckpoint.CheckpointID);
                if (!deletionSuccessful)
                {
                    throw new Exception("Failed to delete the current checkpoint");
                }

                // Update the tracked order with the previous checkpoint's status
                await UpdateTrackedOrderAsync(
                    order.TrackedOrderID,
                    order.EstimatedDeliveryDate,
                    previousCheckpoint.Status);

                // Reload the order data to update the UI
                await LoadOrderDataAsync(order.TrackedOrderID);

                // Explicitly notify that the Checkpoints collection has changed
                OnPropertyChanged(nameof(Checkpoints));

                Debug.WriteLine("Successfully reverted to previous checkpoint");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reverting checkpoint: {ex.Message}");
                throw;
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
                CurrentOrder = trackedOrder;

                // Clear the observable collection before adding new checkpoints
                checkpoints.Clear();
                var loadedCheckpoints = await GetAllOrderCheckpointsAsync(trackedOrderID);
                if (loadedCheckpoints != null)
                {
                    // Ensure timestamp is in local timezone for display
                    foreach (var checkpoint in loadedCheckpoints)
                    {
                        checkpoint.Timestamp = checkpoint.Timestamp.ToLocalTime();
                    }

                    // Sort checkpoints by timestamp in descending order (newest first)
                    var sortedCheckpoints = loadedCheckpoints
                        .OrderByDescending(c => c.Timestamp)
                        .ToList();

                    // Add sorted checkpoints to the observable collection
                    foreach (var checkpoint in sortedCheckpoints)
                    {
                        Checkpoints.Add(checkpoint);
                    }
                }
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

                    // Reload order data to update ViewModel properties
                    await LoadOrderDataAsync(trackedOrderID);
                }
            }
            catch (Exception)
            {
                // Handle error appropriately
            }
        }

        private async Task<DateTime> GetOrderPlacementDateAsync(int orderId)
        {
            var order = await orderViewModel.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception("Order not found");
            }
            return order.OrderDate.DateTime;
        }

        public async Task UpdateLastCheckpointAsync(int trackedOrderID, string description)
        {
            await UpdateLastCheckpointAsync(trackedOrderID, description, DateTime.UtcNow);
        }

        public async Task UpdateLastCheckpointAsync(int trackedOrderID, string description, DateTime timestamp)
        {
            try
            {
                var order = await GetTrackedOrderByIDAsync(trackedOrderID);
                if (order == null)
                {
                    throw new Exception("Order not found");
                }

                // Validate timestamp is after order placement date
                var orderPlacementDate = await GetOrderPlacementDateAsync(order.OrderID);
                if (timestamp < orderPlacementDate)
                {
                    throw new Exception("Checkpoint timestamp cannot be before the order placement date");
                }

                var lastCheckpoint = await GetLastCheckpoint(order);
                if (lastCheckpoint != null)
                {
                    await UpdateOrderCheckpointAsync(
                        lastCheckpoint.CheckpointID,
                        timestamp.ToUniversalTime(), // Convert to UTC for storage
                        lastCheckpoint.Location,
                        description,
                        lastCheckpoint.Status);

                    // Reload data to update UI
                    await LoadOrderDataAsync(trackedOrderID);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating checkpoint: {ex.Message}");
                throw;
            }
        }

        public async Task AddNewCheckpointAsync(int trackedOrderID, string description, OrderStatus status)
        {
            await AddNewCheckpointAsync(trackedOrderID, description, DateTime.UtcNow, status);
        }

        public async Task AddNewCheckpointAsync(int trackedOrderID, string description, DateTime timestamp, OrderStatus status)
        {
            try
            {
                var order = await GetTrackedOrderByIDAsync(trackedOrderID);
                if (order == null)
                {
                    throw new Exception("Order not found");
                }

                // Validate timestamp is after order placement date
                var orderPlacementDate = await GetOrderPlacementDateAsync(order.OrderID);
                if (timestamp < orderPlacementDate)
                {
                    throw new Exception("Checkpoint timestamp cannot be before the order placement date");
                }

                await AddOrderCheckpointAsync(new OrderCheckpoint
                {
                    Timestamp = timestamp.ToUniversalTime(), // Convert to UTC for storage
                    Location = string.Empty,
                    Description = description,
                    Status = status,
                    TrackedOrderID = trackedOrderID
                });

                // Reload data to update UI
                await LoadOrderDataAsync(trackedOrderID);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding new checkpoint: {ex.Message}");
                throw;
            }
        }

        public async Task RevertLastCheckpointAsync(int trackedOrderID)
        {
            try
            {
                var order = await GetTrackedOrderByIDAsync(trackedOrderID);
                if (order == null)
                {
                    throw new Exception("Order not found");
                }

                await RevertToPreviousCheckpoint(order);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RevertLastCheckpointAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<DateTime> GetOrderDateAsync(int orderId)
        {
            var order = await orderViewModel.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception("Order not found");
            }
            return order.OrderDate.DateTime;
        }
    }
}


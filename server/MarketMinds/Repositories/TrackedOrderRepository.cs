// <copyright file="TrackedOrderRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using Microsoft.EntityFrameworkCore;
    using Server.DataAccessLayer;

    /// <summary>
    /// Provides data access functionality for tracking orders and their checkpoints.
    /// </summary>
    public class TrackedOrderRepository : ITrackedOrderRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly INotificationRepository notificationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedOrderRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="notificationRepository">The notification repository.</param>
        public TrackedOrderRepository(ApplicationDbContext dbContext, INotificationRepository notificationRepository)
        {
            this.dbContext = dbContext;
            this.notificationRepository = notificationRepository;
        }

        /// <summary>
        /// Adds a new order checkpoint to the database.
        /// </summary>
        /// <param name="checkpoint">The checkpoint data to add.</param>
        /// <returns>The ID of the newly created checkpoint.</returns>
        /// <summary>
        /// Adds a new order checkpoint to the database.
        /// </summary>
        /// <param name="checkpoint">The checkpoint data to add.</param>
        /// <returns>The ID of the newly created checkpoint.</returns>
        public async Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint)
        {
            await dbContext.OrderCheckpoints.AddAsync(checkpoint);
            await dbContext.SaveChangesAsync();
            
            // Get the associated tracked order
            var trackedOrder = await dbContext.TrackedOrders.FindAsync(checkpoint.TrackedOrderID);
            if (trackedOrder != null)
            {
                await CreateAndSendShippingNotificationAsync(trackedOrder, checkpoint.Status, checkpoint.Timestamp);
            }
            
            return checkpoint.CheckpointID;
        }

        /// <summary>
        /// Adds a new tracked order to the database.
        /// </summary>
        /// <param name="order">The tracked order data to add.</param>
        /// <returns>The ID of the newly created tracked order.</returns>
        /// <exception cref="Exception">Thrown when the tracked order cannot be added.</exception>
        public async Task<int> AddTrackedOrderAsync(TrackedOrder order)
        {
            await dbContext.TrackedOrders.AddAsync(order);
            await dbContext.SaveChangesAsync();
            return order.TrackedOrderID;
        }

        /// <summary>
        /// Deletes an order checkpoint from the database.
        /// </summary>
        /// <param name="checkpointID">ID of the checkpoint to delete.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        /// <exception cref="Exception">Thrown when the checkpoint cannot be deleted.</exception>
        public async Task<bool> DeleteOrderCheckpointAsync(int checkpointID)
        {
            OrderCheckpoint checkpoint = await dbContext.OrderCheckpoints.FindAsync(checkpointID)
                                            ?? throw new Exception($"DeleteOrderCheckpointAsync: No OrderCheckpoint with id: {checkpointID}");
            dbContext.OrderCheckpoints.Remove(checkpoint);

            // Get the current status of the tracked order
            var trackedOrder = await dbContext.TrackedOrders.FindAsync(checkpoint.TrackedOrderID);
            if (trackedOrder != null && trackedOrder.CurrentStatus != checkpoint.Status)
            {
                var currentStatus = trackedOrder.CurrentStatus;
                var currentTimestamp = DateTime.Now;
                await CreateAndSendShippingNotificationAsync(trackedOrder, currentStatus, currentTimestamp);
            }
            await dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a tracked order from the database.
        /// </summary>
        /// <param name="trackOrderID">ID of the tracked order to delete.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        /// <exception cref="Exception">Thrown when the tracked order cannot be deleted.</exception>
        public async Task<bool> DeleteTrackedOrderAsync(int trackOrderID)
        {
            TrackedOrder order = await dbContext.TrackedOrders.FindAsync(trackOrderID)
                                        ?? throw new Exception($"DeleteTrackedOrderAsync: No TrackedOrder with id: {trackOrderID}");
            dbContext.TrackedOrders.Remove(order);
            await dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves all order checkpoints for a specified tracked order.
        /// </summary>
        /// <param name="trackedOrderID">ID of the tracked order.</param>
        /// <returns>List of order checkpoints.</returns>
        public async Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID)
        {
            return await dbContext.OrderCheckpoints.Where(checkpoint => checkpoint.TrackedOrderID == trackedOrderID).ToListAsync();
        }

        /// <summary>
        /// Retrieves all tracked orders from the database.
        /// </summary>
        /// <returns>List of all tracked orders.</returns>
        public async Task<List<TrackedOrder>> GetAllTrackedOrdersAsync()
        {
            return await dbContext.TrackedOrders.ToListAsync();
        }

        /// <summary>
        /// Retrieves an order checkpoint by its ID.
        /// </summary>
        /// <param name="checkpointID">ID of the checkpoint to retrieve.</param>
        /// <returns>The specified order checkpoint.</returns>
        /// <exception cref="Exception">Thrown when the checkpoint is not found.</exception>
        public async Task<OrderCheckpoint> GetOrderCheckpointByIdAsync(int checkpointID)
        {
            return await dbContext.OrderCheckpoints.FindAsync(checkpointID)
                            ?? throw new Exception($"GetOrderCheckpointByIdAsync: No OrderCheckpoint with id: {checkpointID}");
        }

        /// <summary>
        /// Retrieves a tracked order by its ID.
        /// </summary>
        /// <param name="trackOrderID">ID of the tracked order to retrieve.</param>
        /// <returns>The specified tracked order.</returns>
        /// <exception cref="Exception">Thrown when the tracked order is not found.</exception>
        public async Task<TrackedOrder> GetTrackedOrderByIdAsync(int trackOrderID)
        {
            return await dbContext.TrackedOrders.FindAsync(trackOrderID)
                            ?? throw new Exception($"GetTrackedOrderByIdAsync: No TrackedOrder with id: {trackOrderID}");
        }

        /// <summary>
        /// Retrieves a tracked order associated with a specific order ID.
        /// </summary>
        /// <param name="orderId">The order ID to find the tracked order for.</param>
        /// <returns>The tracked order associated with the specified order ID.</returns>
        /// <exception cref="Exception">Thrown when no tracked order is found for the given order ID.</exception>
        public async Task<TrackedOrder> GetTrackedOrderByOrderIdAsync(int orderId)
        {
            TrackedOrder? trackedOrder = await dbContext.TrackedOrders
                .FirstOrDefaultAsync(order => order.OrderID == orderId);
                
            if (trackedOrder == null)
            {
                throw new Exception($"GetTrackedOrderByOrderIdAsync: No TrackedOrder found for OrderID: {orderId}");
            }
            
            return trackedOrder;
        }

        /// <summary>
        /// Updates an existing order checkpoint in the database.
        /// </summary>
        /// <param name="checkpointID">ID of the checkpoint to update.</param>
        /// <param name="timestamp">New timestamp for the checkpoint.</param>
        /// <param name="location">New location for the checkpoint.</param>
        /// <param name="description">New description for the checkpoint.</param>
        /// <param name="status">New status for the checkpoint.</param>
        /// <returns>The updated order checkpoint.</returns>
        /// <exception cref="Exception">Thrown when the checkpoint is not found.</exception>
        public async Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status)
        {
            OrderCheckpoint checkpoint = await dbContext.OrderCheckpoints.FindAsync(checkpointID)
                                                ?? throw new Exception($"UpdateOrderCheckpointAsync: No OrderCheckpoint with id: {checkpointID}");
            
            // Store original status to check if it changed
            OrderStatus originalStatus = checkpoint.Status;
            
            // Update checkpoint properties
            checkpoint.Timestamp = timestamp;
            checkpoint.Location = location;
            checkpoint.Description = description;
            checkpoint.Status = status;
            await dbContext.SaveChangesAsync();
            
            // If status changed, send a notification
            if (originalStatus != status)
            {
                // Get the associated tracked order
                var trackedOrder = await dbContext.TrackedOrders.FindAsync(checkpoint.TrackedOrderID);
                if (trackedOrder != null)
                {
                    await CreateAndSendShippingNotificationAsync(trackedOrder, status, timestamp);
                }
            }
        }

        /// <summary>
        /// Updates an existing tracked order in the database.
        /// </summary>
        /// <param name="trackedOrderID">ID of the tracked order to update.</param>
        /// <param name="estimatedDeliveryDate">New estimated delivery date.</param>
        /// <param name="currentStatus">New order status.</param>
        /// <returns>The updated tracked order.</returns>
        /// <exception cref="Exception">Thrown when the tracked order is not found.</exception>
        public async Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, OrderStatus currentStatus)
        {
            TrackedOrder trackedOrder = await dbContext.TrackedOrders.FindAsync(trackedOrderID)
                                            ?? throw new Exception($"UpdateTrackedOrderAsync: No TrackedOrder with id: {trackedOrderID}");
            
            // Store original status to check if it changed
            OrderStatus originalStatus = trackedOrder.CurrentStatus;
            
            // Update tracked order properties
            trackedOrder.CurrentStatus = currentStatus;
            trackedOrder.EstimatedDeliveryDate = estimatedDeliveryDate;
            await dbContext.SaveChangesAsync();
            
            // If status changed, send a notification
            if (originalStatus != currentStatus)
            {
                await CreateAndSendShippingNotificationAsync(trackedOrder, currentStatus, DateTime.Now);
            }
        }
        /// <summary>
        /// Creates a new checkpoint and updates the tracked order status, triggering a notification.
        /// </summary>
        /// <param name="trackedOrderID">The tracked order ID to update.</param>
        /// <param name="newStatus">The new status to set.</param>
        /// <param name="description">Description of the status change.</param>
        /// <param name="location">Optional location information.</param>
        /// <returns>The ID of the newly created checkpoint.</returns>
        public async Task<int> UpdateOrderStatusWithCheckpointAsync(int trackedOrderID, OrderStatus newStatus, string description = "Status updated", string? location = null)
        {
            // Find the tracked order
            var trackedOrder = await dbContext.TrackedOrders.FindAsync(trackedOrderID)
                ?? throw new Exception($"UpdateOrderStatusWithCheckpointAsync: No TrackedOrder with id: {trackedOrderID}");
            
            // Store original status to check if it changed
            OrderStatus originalStatus = trackedOrder.CurrentStatus;
            
            // Create a new checkpoint
            var checkpoint = new OrderCheckpoint
            {
                TrackedOrderID = trackedOrderID,
                Timestamp = DateTime.Now,
                Location = location,
                Description = description,
                Status = newStatus
            };
            
            // Add the checkpoint
            await dbContext.OrderCheckpoints.AddAsync(checkpoint);
            
            // Update the tracked order status
            trackedOrder.CurrentStatus = newStatus;
            
            // Save all changes
            await dbContext.SaveChangesAsync();
            
            // If status changed, send a notification
            if (originalStatus != newStatus)
            {
                await CreateAndSendShippingNotificationAsync(trackedOrder, newStatus, checkpoint.Timestamp);
            }
            
            return checkpoint.CheckpointID;
        }
        
        /// <summary>
        /// Creates and sends a shipping progress notification for the buyer.
        /// </summary>
        /// <param name="trackedOrder">The tracked order that was updated.</param>
        /// <param name="newStatus">The new status of the order.</param>
        /// <param name="timestamp">The timestamp of the status change.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task CreateAndSendShippingNotificationAsync(TrackedOrder trackedOrder, OrderStatus newStatus, DateTime timestamp)
        {
            try
            {
                // Get the order information
                var order = await dbContext.Orders.FindAsync(trackedOrder.OrderID);
                if (order == null)
                {
                    // Cannot find the associated order, so we can't determine the buyer
                    Console.WriteLine($"Error sending notification: Order with ID {trackedOrder.OrderID} not found");
                    return;
                }
                
                // Create a new notification for the buyer
                var deliveryDate = trackedOrder.EstimatedDeliveryDate.ToDateTime(new TimeOnly(12, 0)); // Use noon as default time
                
                var notification = new OrderShippingProgressNotification(
                    recipientID: order.BuyerId,
                    timestamp: timestamp,
                    orderID: order.Id, // Use the actual order ID from the Order entity
                    shippingState: newStatus.ToString(),
                    deliveryDate: deliveryDate,
                    isRead: false
                );
                
                // Send the notification
                await notificationRepository.AddNotification(notification);
                Console.WriteLine($"Notification created for order {order.Id}, recipient {order.BuyerId}, status {newStatus}");
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the main operation
                Console.WriteLine($"Error sending notification: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // In a production environment, this should be properly logged
            }
        }
    }
}
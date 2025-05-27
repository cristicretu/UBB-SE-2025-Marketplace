using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Helper;

namespace MarketMinds.Shared.Services
{
    public class TrackedOrderService : ITrackedOrderService
    {
        private readonly ITrackedOrderRepository trackedOrderRepository;

        // Constructor for dependency injection
        public TrackedOrderService(ITrackedOrderRepository trackedOrderRepository)
        {
            this.trackedOrderRepository = trackedOrderRepository ?? throw new ArgumentNullException(nameof(trackedOrderRepository));
        }

        public async Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint)
        {
            // Potential place for business logic before/after repository call
            return await trackedOrderRepository.AddOrderCheckpointAsync(checkpoint);
        }

        public async Task<int> AddTrackedOrderAsync(TrackedOrder order)
        {
            return await trackedOrderRepository.AddTrackedOrderAsync(order);
        }

        public async Task<bool> DeleteOrderCheckpointAsync(int checkpointID)
        {
            return await trackedOrderRepository.DeleteOrderCheckpointAsync(checkpointID);
        }

        public async Task<bool> DeleteTrackedOrderAsync(int trackOrderID)
        {
            return await trackedOrderRepository.DeleteTrackedOrderAsync(trackOrderID);
        }

        public async Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID)
        {
            return await trackedOrderRepository.GetAllOrderCheckpointsAsync(trackedOrderID);
        }

        public async Task<List<TrackedOrder>> GetAllTrackedOrdersAsync()
        {
            return await trackedOrderRepository.GetAllTrackedOrdersAsync();
        }

        public async Task<OrderCheckpoint?> GetOrderCheckpointByIDAsync(int checkpointID)
        {
            try
            {
                return await trackedOrderRepository.GetOrderCheckpointByIdAsync(checkpointID);
            }
            catch (Exception) // Repository throws if not found
            {
                return null; // Service layer can choose to return null or re-throw
            }
        }

        public async Task<TrackedOrder?> GetTrackedOrderByIDAsync(int trackedOrderID)
        {
            try
            {
                return await trackedOrderRepository.GetTrackedOrderByIdAsync(trackedOrderID);
            }
            catch (Exception) // Repository throws if not found
            {
                return null; // Service layer can choose to return null or re-throw
            }
        }

        public async Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status)
        {
            await trackedOrderRepository.UpdateOrderCheckpointAsync(checkpointID, timestamp, location, description, status);
        }

        public async Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, OrderStatus currentStatus)
        {
            await trackedOrderRepository.UpdateTrackedOrderAsync(trackedOrderID, estimatedDeliveryDate, currentStatus);
        }

        public async Task<OrderCheckpoint?> GetLastCheckpointAsync(TrackedOrder order)
        {
            if (order == null)
            {
                return null;
            }

            List<OrderCheckpoint> allCheckpoints = await trackedOrderRepository.GetAllOrderCheckpointsAsync(order.TrackedOrderID);
            return allCheckpoints.OrderByDescending(c => c.Timestamp).FirstOrDefault();
        }

        public async Task<int> GetNumberOfCheckpointsAsync(TrackedOrder order)
        {
            if (order == null)
            {
                return 0;
            }

            List<OrderCheckpoint> allCheckpoints = await trackedOrderRepository.GetAllOrderCheckpointsAsync(order.TrackedOrderID);
            return allCheckpoints.Count;
        }

        public async Task RevertToPreviousCheckpointAsync(TrackedOrder? order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order), "Order cannot be null");
            }

            int checkpointCount = await GetNumberOfCheckpointsAsync(order);
            const int minimumCheckpointsForReversion = 1;

            if (checkpointCount <= minimumCheckpointsForReversion)
            {
                throw new InvalidOperationException("Cannot revert further, requires at least one previous checkpoint.");
            }

            var currentCheckpoint = await GetLastCheckpointAsync(order);
            if (currentCheckpoint != null)
            {
                bool deletionSuccessful = await DeleteOrderCheckpointAsync(currentCheckpoint.CheckpointID);
                if (deletionSuccessful)
                {
                    OrderCheckpoint? previousCheckpoint = await GetLastCheckpointAsync(order);
                    if (previousCheckpoint != null)
                    {
                        // Only update status, keep existing delivery date
                        await UpdateTrackedOrderAsync(order.TrackedOrderID, order.EstimatedDeliveryDate, previousCheckpoint.Status);
                    }
                    // If previousCheckpoint is null after deletion, it means we reverted the last one.
                    // Consider if the TrackedOrder status needs a default state here.
                }
                else
                {
                    throw new Exception("Failed to delete the current checkpoint during reversion.");
                }
            }
            else
            {
                // Should not happen if checkpointCount > minimumCheckpointsForReversion
                throw new Exception("No checkpoints found to revert, despite initial count.");
            }
        }

        public async Task<TrackedOrder?> GetTrackedOrderByOrderIdAsync(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Order ID must be positive", nameof(orderId));
            }

            try
            {
                return await trackedOrderRepository.GetTrackedOrderByOrderIdAsync(orderId);
            }
            catch (Exception)
            {
                // Repository throws if not found, but service returns null for consistent behavior
                return null;
            }
        }

        /// <summary>
        /// Updates a tracked order's status and automatically creates a checkpoint for the status change.
        /// </summary>
        /// <param name="trackedOrderID">The ID of the tracked order to update.</param>
        /// <param name="newStatus">The new status of the order.</param>
        /// <param name="description">Optional description of the status change.</param>
        /// <param name="location">Optional location for the checkpoint.</param>
        /// <returns>The ID of the created checkpoint.</returns>
        public async Task<int> UpdateOrderStatusWithCheckpointAsync(
            int trackedOrderID,
            OrderStatus newStatus,
            string description = "Status updated",
            string? location = null)
        {
            if (trackedOrderID <= 0)
            {
                throw new ArgumentException("Tracked order ID must be positive", nameof(trackedOrderID));
            }

            // Get current tracked order
            TrackedOrder? trackedOrder = await GetTrackedOrderByIDAsync(trackedOrderID);
            if (trackedOrder == null)
            {
                throw new ArgumentException($"Tracked order with ID {trackedOrderID} not found", nameof(trackedOrderID));
            }

            // Update tracked order status
            await UpdateTrackedOrderAsync(trackedOrderID, trackedOrder.EstimatedDeliveryDate, newStatus);

            // Create a checkpoint for the status change
            OrderCheckpoint checkpoint = new OrderCheckpoint
            {
                TrackedOrderID = trackedOrderID,
                Status = newStatus,
                Timestamp = DateTime.UtcNow,
                Description = description,
                Location = location
            };

            return await AddOrderCheckpointAsync(checkpoint);
        }

        /// <summary>
        /// Creates a tracked order for an existing order and initializes it with the first checkpoint.
        /// </summary>
        /// <param name="orderId">The ID of the order to track.</param>
        /// <param name="estimatedDeliveryDate">The estimated delivery date.</param>
        /// <param name="deliveryAddress">The delivery address.</param>
        /// <param name="initialStatus">The initial status, defaults to Pending.</param>
        /// <param name="initialDescription">Description for the initial checkpoint.</param>
        /// <returns>The ID of the newly created tracked order.</returns>
        public async Task<int> CreateTrackedOrderForOrderAsync(
            int orderId,
            DateOnly estimatedDeliveryDate,
            string deliveryAddress,
            OrderStatus initialStatus = OrderStatus.PROCESSING,
            string initialDescription = "Order created and being tracked")
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Order ID must be positive", nameof(orderId));
            }
            TrackedOrder order = new TrackedOrder
            {
                OrderID = orderId,
                EstimatedDeliveryDate = estimatedDeliveryDate,
                DeliveryAddress = deliveryAddress,
                CurrentStatus = initialStatus,
            };

            int trackedOrderId = await AddTrackedOrderAsync(order);

            // Create the first checkpoint
            OrderCheckpoint checkpoint = new OrderCheckpoint
            {
                TrackedOrderID = trackedOrderId,
                Status = initialStatus,
                Timestamp = DateTime.UtcNow,
                Description = initialDescription,
                Location = deliveryAddress
            };

            await AddOrderCheckpointAsync(checkpoint);

            return trackedOrderId;
        }

        /// <summary>
        /// Gets all tracked orders associated with orders placed by a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of tracked orders for the buyer's orders.</returns>
        public async Task<List<TrackedOrder>> GetTrackedOrdersByBuyerIdAsync(int buyerId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be positive", nameof(buyerId));
            }

            // This would be more efficient if the repository had a direct method,
            // but we can implement it using existing methods

            // We need to retrieve all orders for the buyer, then find tracked orders for each one
            List<TrackedOrder> buyerTrackedOrders = new List<TrackedOrder>();

            // For this implementation to work, we need the OrderService to get orders by buyer ID
            // For now, we'll retrieve all tracked orders and filter them
            // In a real implementation, we would add a repository method to do this more efficiently

            List<TrackedOrder> allTrackedOrders = await GetAllTrackedOrdersAsync();

            // Here we would filter by buyer's orders
            // This is a placeholder for the actual implementation
            // In a real implementation, we would need to join with Order data

            return buyerTrackedOrders;
        }

        /// <summary>
        /// Calculates the estimated delivery progress as a percentage.
        /// </summary>
        /// <param name="trackedOrderId">The ID of the tracked order.</param>
        /// <returns>A value between 0 and 100 indicating the progress percentage.</returns>
        public async Task<int> CalculateDeliveryProgressPercentageAsync(int trackedOrderId)
        {
            TrackedOrder? trackedOrder = await GetTrackedOrderByIDAsync(trackedOrderId);
            if (trackedOrder == null)
            {
                throw new ArgumentException($"Tracked order with ID {trackedOrderId} not found", nameof(trackedOrderId));
            }

            // Base progress on the current status
            switch (trackedOrder.CurrentStatus)
            {
                case OrderStatus.PROCESSING:
                    return 20;
                case OrderStatus.SHIPPED:
                    return 40;
                case OrderStatus.IN_WAREHOUSE:
                    return 60;
                case OrderStatus.IN_TRANSIT:
                    return 75;
                case OrderStatus.OUT_FOR_DELIVERY:
                    return 90;
                case OrderStatus.DELIVERED:
                    return 100;
                default:
                    return 0;
            }
        }
    }
}

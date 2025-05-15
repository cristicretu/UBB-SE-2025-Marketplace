using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Shared;
using Microsoft.Data.SqlClient; // Assuming Configuration and SqlDatabaseProvider are here
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Helper;

namespace SharedClassLibrary.Service
{
    public class TrackedOrderService : ITrackedOrderService
    {
        private readonly ITrackedOrderRepository trackedOrderRepository;

        // Constructor for dependency injection
        public TrackedOrderService()
        {
            this.trackedOrderRepository = new TrackedOrderProxyRepository(AppConfig.GetBaseApiUrl());
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
    }
}

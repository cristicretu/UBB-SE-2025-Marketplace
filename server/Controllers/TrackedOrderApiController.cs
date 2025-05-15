// <copyright file="TrackedOrderApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers // Assuming the controller is in the Server project
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.DataTransferObjects;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing tracked order data.
    /// </summary>
    [Authorize]
    [Route("api/trackedorders")]
    [ApiController]
    public class TrackedOrderApiController : ControllerBase
    {
        private readonly ITrackedOrderRepository trackedOrderRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedOrderApiController"/> class.
        /// </summary>
        /// <param name="trackedOrderRepository">The tracked order repository dependency.</param>
        public TrackedOrderApiController(ITrackedOrderRepository trackedOrderRepository)
        {
            this.trackedOrderRepository = trackedOrderRepository ?? throw new ArgumentNullException(nameof(trackedOrderRepository));
        }

        /// <summary>
        /// Asynchronously adds a new order checkpoint.
        /// </summary>
        /// <param name="checkpoint">The order checkpoint to add.</param>
        /// <returns>The newly created order checkpoint with its ID.</returns>
        [HttpPost("checkpoints")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> AddOrderCheckpoint([FromBody] OrderCheckpoint checkpoint)
        {
            if (checkpoint == null)
            {
                return this.BadRequest("Order checkpoint data is required.");
            }

            try
            {
                int newId = await this.trackedOrderRepository.AddOrderCheckpointAsync(checkpoint);
                return this.Ok(newId);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding the order checkpoint: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously adds a new tracked order.
        /// </summary>
        /// <param name="order">The tracked order to add.</param>
        /// <returns>The newly created tracked order with its ID.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TrackedOrder>> AddTrackedOrder([FromBody] TrackedOrder order)
        {
            if (order == null)
            {
                return this.BadRequest("Tracked order data is required.");
            }

            try
            {
                int newId = await this.trackedOrderRepository.AddTrackedOrderAsync(order);
                return this.Ok(newId);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding the tracked order: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously deletes an order checkpoint.
        /// </summary>
        /// <param name="checkpointId">The ID of the order checkpoint to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("checkpoints/{checkpointId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOrderCheckpoint(int checkpointId)
        {
            try
            {
                bool deleted = await this.trackedOrderRepository.DeleteOrderCheckpointAsync(checkpointId);
                if (!deleted)
                {
                    return this.NotFound($"OrderCheckpoint with ID {checkpointId} not found or could not be deleted.");
                }

                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred deleting order checkpoint {checkpointId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously deletes a tracked order.
        /// </summary>
        /// <param name="trackedOrderId">The ID of the tracked order to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{trackedOrderId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTrackedOrder(int trackedOrderId)
        {
            try
            {
                bool deleted = await this.trackedOrderRepository.DeleteTrackedOrderAsync(trackedOrderId);
                if (!deleted)
                {
                    // If the repository method returns false, it implies the item wasn't found or deleted.
                    return this.NotFound($"TrackedOrder with ID {trackedOrderId} not found or could not be deleted.");
                }

                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred deleting tracked order {trackedOrderId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves all order checkpoints for a specific tracked order.
        /// </summary>
        /// <param name="trackedOrderId">The ID of the tracked order.</param>
        /// <returns>A list of order checkpoints.</returns>
        [HttpGet("{trackedOrderId}/checkpoints")]
        [ProducesResponseType(typeof(List<OrderCheckpoint>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OrderCheckpoint>>> GetAllOrderCheckpoints(int trackedOrderId)
        {
            try
            {
                var checkpoints = await this.trackedOrderRepository.GetAllOrderCheckpointsAsync(trackedOrderId);
                return this.Ok(checkpoints);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred retrieving checkpoints for tracked order {trackedOrderId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves all tracked orders.
        /// </summary>
        /// <returns>A list of all tracked orders.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<TrackedOrder>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<TrackedOrder>>> GetAllTrackedOrders()
        {
            try
            {
                var orders = await this.trackedOrderRepository.GetAllTrackedOrdersAsync();
                return this.Ok(orders);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred retrieving all tracked orders: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves an order checkpoint by its ID.
        /// </summary>
        /// <param name="checkpointId">The ID of the order checkpoint.</param>
        /// <returns>The order checkpoint.</returns>
        [HttpGet("checkpoints/{checkpointId}")]
        [ProducesResponseType(typeof(OrderCheckpoint), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderCheckpoint>> GetOrderCheckpointById(int checkpointId)
        {
            try
            {
                var checkpoint = await this.trackedOrderRepository.GetOrderCheckpointByIdAsync(checkpointId);
                if (checkpoint == null)
                {
                    return this.NotFound($"OrderCheckpoint with ID {checkpointId} not found.");
                }

                return this.Ok(checkpoint);
            }
            catch (Exception ex) when (ex.Message.Contains("No OrderCheckpoint with id"))
            {
                return this.NotFound($"OrderCheckpoint with ID {checkpointId} not found.");
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred retrieving order checkpoint {checkpointId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves a tracked order by its ID.
        /// </summary>
        /// <param name="trackedOrderId">The ID of the tracked order.</param>
        /// <returns>The tracked order.</returns>
        [HttpGet("{trackedOrderId}")]
        [ProducesResponseType(typeof(TrackedOrder), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TrackedOrder>> GetTrackedOrderById(int trackedOrderId)
        {
            try
            {
                var order = await this.trackedOrderRepository.GetTrackedOrderByIdAsync(trackedOrderId);
                if (order == null)
                {
                    return this.NotFound($"TrackedOrder with ID {trackedOrderId} not found.");
                }

                return this.Ok(order);
            }
            catch (Exception ex) when (ex.Message.Contains("No TrackedOrder with id"))
            {
                return this.NotFound($"TrackedOrder with ID {trackedOrderId} not found.");
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred retrieving tracked order {trackedOrderId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously updates an existing order checkpoint.
        /// </summary>
        /// <param name="checkpointId">The ID of the checkpoint to update.</param>
        /// <param name="checkpointUpdate">The updated checkpoint data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("checkpoints/{checkpointId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrderCheckpoint(int checkpointId, [FromBody] OrderCheckpointUpdateRequest checkpointUpdate)
        {
            if (checkpointUpdate == null)
            {
                return this.BadRequest("Order checkpoint update data is required.");
            }

            try
            {
                await this.trackedOrderRepository.UpdateOrderCheckpointAsync(
                    checkpointId,
                    checkpointUpdate.Timestamp,
                    checkpointUpdate.Location,
                    checkpointUpdate.Description,
                    checkpointUpdate.Status);

                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred updating order checkpoint {checkpointId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously updates an existing tracked order.
        /// </summary>
        /// <param name="trackedOrderId">The ID of the tracked order to update.</param>
        /// <param name="orderUpdate">The updated tracked order data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{trackedOrderId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTrackedOrder(int trackedOrderId, [FromBody] TrackedOrderUpdateRequest orderUpdate)
        {
            if (orderUpdate == null)
            {
                return this.BadRequest("Tracked order update data is required.");
            }

            try
            {
                await this.trackedOrderRepository.UpdateTrackedOrderAsync(
                    trackedOrderId,
                    orderUpdate.EstimatedDeliveryDate,
                    orderUpdate.CurrentStatus);

                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred updating tracked order {trackedOrderId}: {ex.Message}");
            }
        }
    }
}

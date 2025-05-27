// <copyright file="OrderTrackingController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using global::MarketMinds.Shared.Models.DTOs;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// API controller for tracking orders and their delivery status.
    /// </summary>
    [Authorize]
    [Route("api/ordertracking")]
    [ApiController]
    public class OrderTrackingController : ControllerBase
    {
        private readonly ITrackedOrderRepository trackedOrderRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IOrderSummaryRepository orderSummaryRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderTrackingController"/> class.
        /// </summary>
        /// <param name="trackedOrderRepository">The tracked order repository dependency.</param>
        /// <param name="orderRepository">The order repository dependency.</param>
        /// <param name="orderSummaryRepository">The order summary repository dependency.</param>
        public OrderTrackingController(
            ITrackedOrderRepository trackedOrderRepository,
            IOrderRepository orderRepository,
            IOrderSummaryRepository orderSummaryRepository)
        {
            this.trackedOrderRepository = trackedOrderRepository ?? throw new ArgumentNullException(nameof(trackedOrderRepository));
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            this.orderSummaryRepository = orderSummaryRepository ?? throw new ArgumentNullException(nameof(orderSummaryRepository));
        }

        /// <summary>
        /// Gets tracked order information by order ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to track.</param>
        /// <returns>The tracked order information.</returns>
        [HttpGet("order/{orderId}")]
        [ProducesResponseType(typeof(TrackedOrder), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TrackedOrder>> GetTrackedOrderByOrderId(int orderId)
        {
            try
            {
                var trackedOrder = await this.trackedOrderRepository.GetTrackedOrderByOrderIdAsync(orderId);

                if (trackedOrder == null)
                {
                    return this.NotFound($"No tracking information found for order ID: {orderId}");
                }

                return this.Ok(trackedOrder);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving tracking information: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all checkpoints for a tracked order.
        /// </summary>
        /// <param name="trackedOrderId">The ID of the tracked order.</param>
        /// <returns>A list of order checkpoints.</returns>
        [HttpGet("{trackedOrderId}/checkpoints")]
        [ProducesResponseType(typeof(List<OrderCheckpoint>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OrderCheckpoint>>> GetOrderCheckpoints(int trackedOrderId)
        {
            try
            {
                var checkpoints = await this.trackedOrderRepository.GetAllOrderCheckpointsAsync(trackedOrderId);

                // Sort checkpoints by timestamp (newest first)
                checkpoints.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));

                return this.Ok(checkpoints);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving order checkpoints: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all tracked orders for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of tracked orders with their status.</returns>
        [HttpGet("buyer/{buyerId}")]
        [ProducesResponseType(typeof(List<OrderTrackingInfoDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OrderTrackingInfoDTO>>> GetBuyerTrackedOrders(int buyerId)
        {
            try
            {
                // Get all orders for this buyer
                var displayInfo = await this.orderRepository.GetOrdersWithProductInfoAsync(buyerId);

                if (displayInfo == null || displayInfo.Count == 0)
                {
                    return this.Ok(new List<OrderTrackingInfoDTO>()); // Return empty list
                }

                var result = new List<OrderTrackingInfoDTO>();

                foreach (var order in displayInfo)
                {
                    // Get tracking information
                    var trackedOrder = await this.trackedOrderRepository.GetTrackedOrderByOrderIdAsync(order.OrderID);

                    if (trackedOrder == null)
                    {
                        continue; // Skip orders without tracking
                    }

                    // Get the latest checkpoint
                    var checkpoints = await this.trackedOrderRepository.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID);

                    // Sort checkpoints by timestamp (newest first)
                    checkpoints.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));

                    var latestCheckpoint = checkpoints.Count > 0 ? checkpoints[0] : null;

                    // Create tracking info DTO
                    var trackingInfo = new OrderTrackingInfoDTO
                    {
                        Id = order.OrderID,
                        ProductName = order.ProductName ?? "Unknown Product",
                        OrderDate = order.OrderDate ?? DateTime.Now.ToString("yyyy-MM-dd"),
                        CurrentStatus = trackedOrder.CurrentStatus,
                        EstimatedDeliveryDate = trackedOrder.EstimatedDeliveryDate.ToString("yyyy-MM-dd"),
                        DeliveryAddress = trackedOrder.DeliveryAddress,
                        CurrentLocation = latestCheckpoint?.Location ?? "Processing Center",
                        LastUpdated = latestCheckpoint?.Timestamp.ToString("yyyy-MM-dd HH:mm") ?? "N/A"
                    };

                    result.Add(trackingInfo);
                }

                // Sort by order date (newest first)
                result.Sort((a, b) => DateTime.Parse(b.OrderDate).CompareTo(DateTime.Parse(a.OrderDate)));

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving tracked orders: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// DTO for displaying order tracking information.
    /// </summary>
    public class OrderTrackingInfoDTO
    {
        /// <summary>
        /// Gets or sets the order ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the order date.
        /// </summary>
        public string OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the current status of the order.
        /// </summary>
        public OrderStatus CurrentStatus { get; set; }

        /// <summary>
        /// Gets or sets the estimated delivery date.
        /// </summary>
        public string EstimatedDeliveryDate { get; set; }

        /// <summary>
        /// Gets or sets the delivery address.
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Gets or sets the current location of the order.
        /// </summary>
        public string CurrentLocation { get; set; }

        /// <summary>
        /// Gets or sets when the order status was last updated.
        /// </summary>
        public string LastUpdated { get; set; }
    }
}
// <copyright file="OrderHistoryApiController.cs" company="PlaceholderCompany">
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
    /// API controller for managing order history data.
    /// </summary>
    [Route("api/orderhistory")]
    [ApiController]
    public class OrderHistoryApiController : ControllerBase
    {
        private readonly IOrderHistoryRepository orderHistoryRepository;
        private readonly IOrderRepository orderRepository;
        private readonly ITrackedOrderRepository trackedOrderRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryApiController"/> class.
        /// </summary>
        /// <param name="orderHistoryRepository">The order history repository dependency.</param>
        /// <param name="orderRepository">The order repository dependency.</param>
        /// <param name="trackedOrderRepository">The tracked order repository dependency.</param>
        public OrderHistoryApiController(
            IOrderHistoryRepository orderHistoryRepository,
            IOrderRepository orderRepository,
            ITrackedOrderRepository trackedOrderRepository)
        {
            this.orderHistoryRepository = orderHistoryRepository;
            this.orderRepository = orderRepository;
            this.trackedOrderRepository = trackedOrderRepository;
        }

        /// <summary>
        /// Asynchronously creates a new order history record.
        /// </summary>
        /// <param name="request">Request containing buyer ID.</param>
        /// <returns>The ID of the newly created order history.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> CreateOrderHistory([FromBody] OrderHistoryCreateRequest request)
        {
            if (request == null || request.BuyerId <= 0)
            {
                return this.BadRequest("A valid buyer ID is required.");
            }

            try
            {
                var orderHistoryId = await this.orderHistoryRepository.CreateOrderHistoryAsync(request.BuyerId);

                // Return the ID of the newly created order history with a 201 Created status.
                return this.StatusCode(StatusCodes.Status201Created, orderHistoryId);
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a 500 Internal Server Error status
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while creating a new order history: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves products from order history by ID.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history.</param>
        /// <returns>A list of products.</returns>
        [HttpGet("{orderHistoryId}/products")]
        [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Product>>> GetProductsFromOrderHistory(int orderHistoryId)
        {
            try
            {
                // Call the repository method to get the products
                var products = await this.orderHistoryRepository.GetProductsFromOrderHistoryAsync(orderHistoryId);

                // Return the list of products with a 200 OK status
                return this.Ok(products);
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a 500 Internal Server Error status
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving products for order history ID {orderHistoryId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves all order histories for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of order histories.</returns>
        [HttpGet("buyer/{buyerId}")]
        [ProducesResponseType(typeof(List<OrderHistory>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OrderHistory>>> GetOrderHistoriesByBuyer(int buyerId)
        {
            try
            {
                // Retrieve all order histories for this buyer
                var orderHistories = await this.orderHistoryRepository.GetOrderHistoriesByBuyerAsync(buyerId);

                // Return the list of order histories with a 200 OK status
                return this.Ok(orderHistories);
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a 500 Internal Server Error status
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving order histories for buyer ID {buyerId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves a specific order history by ID.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history.</param>
        /// <returns>The order history.</returns>
        [HttpGet("{orderHistoryId}")]
        [ProducesResponseType(typeof(OrderHistory), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderHistory>> GetOrderHistoryById(int orderHistoryId)
        {
            try
            {
                // Retrieve the order history by ID
                var orderHistory = await this.orderHistoryRepository.GetOrderHistoryByIdAsync(orderHistoryId);

                if (orderHistory == null)
                {
                    return this.NotFound($"Order history with ID {orderHistoryId} not found.");
                }

                // Return the order history with a 200 OK status
                return this.Ok(orderHistory);
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a 500 Internal Server Error status
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving order history with ID {orderHistoryId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves orders for a specific order history.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history.</param>
        /// <returns>A list of orders with tracking information.</returns>
        [HttpGet("{orderHistoryId}/orders")]
        [ProducesResponseType(typeof(List<OrderWithTrackingDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OrderWithTrackingDTO>>> GetOrdersWithTracking(int orderHistoryId)
        {
            try
            {
                // Get the orders for this order history
                var orders = await this.orderRepository.GetOrdersFromOrderHistoryAsync(orderHistoryId);

                var result = new List<OrderWithTrackingDTO>();

                // Get tracking info for each order
                foreach (var order in orders)
                {
                    var trackedOrder = await this.trackedOrderRepository.GetTrackedOrderByOrderIdAsync(order.Id);

                    var orderWithTracking = new OrderWithTrackingDTO
                    {
                        Order = order,
                        TrackedOrder = trackedOrder
                    };

                    result.Add(orderWithTracking);
                }

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving orders with tracking for order history ID {orderHistoryId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing order history.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history to update.</param>
        /// <param name="updateRequest">The updated data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{orderHistoryId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrderHistory(int orderHistoryId, [FromBody] OrderHistoryUpdateRequest updateRequest)
        {
            if (updateRequest == null)
            {
                return this.BadRequest("Update data is required.");
            }

            try
            {
                // Check if the order history exists
                var existing = await this.orderHistoryRepository.GetOrderHistoryByIdAsync(orderHistoryId);

                if (existing == null)
                {
                    return this.NotFound($"Order history with ID {orderHistoryId} not found.");
                }

                // Update the order history
                await this.orderHistoryRepository.UpdateOrderHistoryAsync(
                    orderHistoryId,
                    updateRequest.Note,
                    updateRequest.ShippingAddress,
                    updateRequest.PaymentMethod);

                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating order history with ID {orderHistoryId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves order history display data for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of order history display DTOs.</returns>
        [HttpGet("buyer/{buyerId}/display")]
        [ProducesResponseType(typeof(List<OrderHistoryDisplayDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OrderHistoryDisplayDTO>>> GetOrderHistoryDisplay(int buyerId)
        {
            try
            {
                // Get all order histories for this buyer
                var orderHistories = await this.orderHistoryRepository.GetOrderHistoriesByBuyerAsync(buyerId);

                var result = new List<OrderHistoryDisplayDTO>();

                foreach (var history in orderHistories)
                {
                    // Get all orders for this history
                    var orders = await this.orderRepository.GetOrdersFromOrderHistoryAsync(history.OrderID);

                    if (orders.Count == 0)
                    {
                        continue; // Skip empty order histories
                    }

                    // Get tracked orders to get statuses
                    var statuses = new List<OrderStatus>();
                    var productNames = new List<string>();
                    double totalCost = 0;

                    foreach (var order in orders)
                    {
                        var trackedOrder = await this.trackedOrderRepository.GetTrackedOrderByOrderIdAsync(order.Id);

                        if (trackedOrder != null)
                        {
                            statuses.Add(trackedOrder.CurrentStatus);
                        }

                        productNames.Add(order.Name);
                        totalCost += order.Cost;
                    }

                    // Create display DTO
                    var displayDto = new OrderHistoryDisplayDTO
                    {
                        OrderHistoryID = history.OrderID,
                        OrderDate = history.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                        TotalItems = orders.Count,
                        TotalCost = totalCost,
                        OrderStatuses = statuses,
                        ProductNames = productNames,
                        ShippingAddress = history.ShippingAddress ?? string.Empty,
                        PaymentMethod = history.PaymentMethod ?? string.Empty
                    };

                    result.Add(displayDto);
                }

                // Sort by date descending (most recent first)
                result.Sort((a, b) => DateTime.Parse(b.OrderDate).CompareTo(DateTime.Parse(a.OrderDate)));

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving order history display for buyer ID {buyerId}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Request model for creating a new order history.
    /// </summary>
    public class OrderHistoryCreateRequest
    {
        /// <summary>
        /// Gets or sets the buyer ID.
        /// </summary>
        public int BuyerId { get; set; }
    }

    /// <summary>
    /// Request model for updating an existing order history.
    /// </summary>
    public class OrderHistoryUpdateRequest
    {
        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Gets or sets the shipping address.
        /// </summary>
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        public string? PaymentMethod { get; set; }
    }

    /// <summary>
    /// DTO for combining order and tracking information.
    /// </summary>
    public class OrderWithTrackingDTO
    {
        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public Order Order { get; set; }

        /// <summary>
        /// Gets or sets the tracked order.
        /// </summary>
        public TrackedOrder TrackedOrder { get; set; }
    }
}

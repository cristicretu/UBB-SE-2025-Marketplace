// <copyright file="OrderApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
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
    /// API controller for managing order data.
    /// </summary>
    [Authorize]
    [Route("api/orders")]
    [ApiController]
    public class OrderApiController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderApiController"/> class.
        /// </summary>
        /// <param name="orderRepository">The order repository dependency.</param>
        public OrderApiController(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        /// <summary>
        /// Adds a new order.
        /// </summary>
        /// <param name="requestDto">Data for the new order.</param>
        /// <returns>Status code indicating success or failure.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // Or 201 Created if returning the created resource URL
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddOrder([FromBody] AddOrderRequestDto requestDto)
        {
            if (requestDto == null)
            {
                return this.BadRequest("Order data is required.");
            }

            try
            {
                await this.orderRepository.AddOrderAsync(
                    requestDto.ProductId,
                    requestDto.BuyerId,
                    requestDto.ProductType,
                    requestDto.PaymentMethod,
                    requestDto.OrderSummaryId,
                    requestDto.OrderDate);

                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding order: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes an order by its ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to delete.</param>
        /// <returns>Status code indicating success or failure.</returns>
        [HttpDelete("{orderId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            try
            {
                await this.orderRepository.DeleteOrderAsync(orderId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting order: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets borrowed order history for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of borrowed orders.</returns>
        [HttpGet("buyer/{buyerId}/history/borrowed")]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> GetBorrowedOrderHistory(int buyerId)
        {
            try
            {
                var orders = await this.orderRepository.GetBorrowedOrderHistoryAsync(buyerId);
                return this.Ok(orders);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets new or used order history for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of new/used orders.</returns>
        [HttpGet("buyer/{buyerId}/history/new-used")]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> GetNewOrUsedOrderHistory(int buyerId)
        {
            try
            {
                var orders = await this.orderRepository.GetNewOrUsedOrderHistoryAsync(buyerId);
                return this.Ok(orders);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Searches orders by product name for a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="text">The search text for the product name.</param>
        /// <returns>A list of matching orders.</returns>
        [HttpGet("buyer/{buyerId}/search")]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> GetOrdersByName(int buyerId, [FromQuery] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return this.BadRequest("Search text cannot be empty.");
            }

            try
            {
                var orders = await this.orderRepository.GetOrdersByNameAsync(buyerId, text);
                return this.Ok(orders);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets orders from the year 2024 for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of orders from 2024.</returns>
        [HttpGet("buyer/{buyerId}/history/year/2024")]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> GetOrdersFrom2024(int buyerId)
        {
            try
            {
                var orders = await this.orderRepository.GetOrdersFrom2024Async(buyerId);
                return this.Ok(orders);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets orders from the year 2025 for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of orders from 2025.</returns>
        [HttpGet("buyer/{buyerId}/history/year/2025")]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> GetOrdersFrom2025(int buyerId)
        {
            try
            {
                var orders = await this.orderRepository.GetOrdersFrom2025Async(buyerId);
                return this.Ok(orders);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets orders from the last 6 months for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of orders from the last 6 months.</returns>
        [HttpGet("buyer/{buyerId}/history/months/6")]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> GetOrdersFromLastSixMonths(int buyerId)
        {
            try
            {
                var orders = await this.orderRepository.GetOrdersFromLastSixMonthsAsync(buyerId);
                return this.Ok(orders);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets orders from the last 3 months for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of orders from the last 3 months.</returns>
        [HttpGet("buyer/{buyerId}/history/months/3")]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> GetOrdersFromLastThreeMonths(int buyerId)
        {
            try
            {
                var orders = await this.orderRepository.GetOrdersFromLastThreeMonthsAsync(buyerId);
                return this.Ok(orders);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets orders belonging to a specific order history group.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history group.</param>
        /// <returns>A list of orders in that group.</returns>
        [HttpGet("history-group/{orderHistoryId}")]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> GetOrdersFromOrderHistory(int orderHistoryId)
        {
            try
            {
                var orders = await this.orderRepository.GetOrdersFromOrderHistoryAsync(orderHistoryId);
                return this.Ok(orders);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the details of a specific order summary.
        /// </summary>
        /// <param name="orderSummaryId">The ID of the order summary.</param>
        /// <returns>The order summary details.</returns>
        [HttpGet("summary/{orderSummaryId}")]
        [ProducesResponseType(typeof(OrderSummary), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderSummary>> GetOrderSummary(int orderSummaryId)
        {
            try
            {
                var summary = await this.orderRepository.GetOrderSummaryAsync(orderSummaryId);
                return this.Ok(summary);
            }
            catch (KeyNotFoundException)
            {
                return this.NotFound($"OrderSummary with ID {orderSummaryId} not found.");
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets enhanced order display information for a user, with optional filtering.
        /// </summary>
        /// <param name="userId">The ID of the user (buyer).</param>
        /// <param name="searchText">Optional text to filter product names.</param>
        /// <param name="timePeriod">Optional time period filter (e.g., "Last 3 Months", "This Year").</param>
        /// <returns>A list of order display information objects.</returns>
        [HttpGet("buyer/{userId}/display")]
        [ProducesResponseType(typeof(List<OrderDisplayInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OrderDisplayInfo>>> GetOrdersWithProductInfo(int userId, [FromQuery] string searchText = null, [FromQuery] string timePeriod = null)
        {
            try
            {
                var orderInfos = await this.orderRepository.GetOrdersWithProductInfoAsync(userId, searchText, timePeriod);
                return this.Ok(orderInfos);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a dictionary mapping OrderSummaryID to product category type (e.g., "new", "borrowed") for a user.
        /// </summary>
        /// <param name="userId">The ID of the user (buyer).</param>
        /// <returns>A dictionary of category types.</returns>
        [HttpGet("buyer/{userId}/category-types")]
        [ProducesResponseType(typeof(Dictionary<int, string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Dictionary<int, string>>> GetProductCategoryTypes(int userId)
        {
            try
            {
                var categories = await this.orderRepository.GetProductCategoryTypesAsync(userId);
                return this.Ok(categories);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing order.
        /// </summary>
        /// <param name="orderId">The ID of the order to update.</param>
        /// <param name="requestDto">The updated order data.</param>
        /// <returns>Status code indicating success or failure.</returns>
        [HttpPut("{orderId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] UpdateOrderRequestDto requestDto)
        {
            if (requestDto == null)
            {
                return this.BadRequest("Order update data is required.");
            }

            try
            {
                await this.orderRepository.UpdateOrderAsync(
                    orderId,
                    requestDto.ProductType,
                    requestDto.PaymentMethod,
                    requestDto.OrderDate);

                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating order: {ex.Message}");
            }
        }
    }
}

// <copyright file="OrderHistoryApiController.cs" company="PlaceholderCompany">
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
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing order history data.
    /// </summary>
    [Authorize]
    [Route("api/orderhistory")]
    [ApiController]
    public class OrderHistoryApiController : ControllerBase
    {
        private readonly IOrderHistoryRepository orderHistoryRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryApiController"/> class.
        /// </summary>
        /// <param name="orderHistoryRepository">The order history repository dependency.</param>
        public OrderHistoryApiController(IOrderHistoryRepository orderHistoryRepository)
        {
            this.orderHistoryRepository = orderHistoryRepository;
        }

        /// <summary>
        /// Asynchronously retrieves dummy products from order history by ID.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history.</param>
        /// <returns>A list of dummy products.</returns>
        [HttpGet("{orderHistoryId}/dummy-products")]
        [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Product>>> GetProductsFromOrderHistory(int orderHistoryId)
        {
            try
            {
                // Call the repository method to get the dummy products
                var products = await this.orderHistoryRepository.GetProductsFromOrderHistoryAsync(orderHistoryId);

                // Return the list of dummy products with a 200 OK status
                return this.Ok(products);
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a 500 Internal Server Error status
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving dummy products for order history ID {orderHistoryId}: {ex.Message}");
            }
        }
    }
}

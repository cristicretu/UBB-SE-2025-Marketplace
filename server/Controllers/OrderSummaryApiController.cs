// <copyright file="OrderSummaryApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.DataTransferObjects;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing order summary data.
    /// </summary>
    [Authorize]
    [Route("api/ordersummaries")]
    [ApiController]
    public class OrderSummaryApiController : ControllerBase
    {
        private readonly IOrderSummaryRepository orderSummaryRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderSummaryApiController"/> class.
        /// </summary>
        /// <param name="orderSummaryRepository">The order summary repository dependency.</param>
        public OrderSummaryApiController(IOrderSummaryRepository orderSummaryRepository)
        {
            this.orderSummaryRepository = orderSummaryRepository;
        }

        /// <summary>
        /// Asynchronously retrieves an order summary by its ID.
        /// </summary>
        /// <param name="id">The ID of the order summary to retrieve.</param>
        /// <returns>The order summary.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderSummary), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderSummary>> GetOrderSummaryById(int id)
        {
            try
            {
                var orderSummary = await this.orderSummaryRepository.GetOrderSummaryByIdAsync(id);
                if (orderSummary == null)
                {
                    return this.NotFound($"Order summary with ID {id} not found.");
                }

                return this.Ok(orderSummary);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving order summary with ID {id}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously updates an existing order summary.
        /// </summary>
        /// <param name="request">The request containing the updated order summary data.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateOrderSummary([FromBody] UpdateOrderSummaryRequest request)
        {
            if (request == null)
            {
                return this.BadRequest("Valid order summary data is required for update.");
            }

            try
            {
                await this.orderSummaryRepository.UpdateOrderSummaryAsync(
                    request.Id,
                    request.Subtotal,
                    request.WarrantyTax,
                    request.DeliveryFee,
                    request.FinalTotal,
                    request.FullName,
                    request.Email,
                    request.PhoneNumber,
                    request.Address,
                    request.PostalCode,
                    request.AdditionalInfo,
                    request.ContractDetails);

                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating order summary with ID {request.Id}: {ex.Message}");
            }
        }
    }
}

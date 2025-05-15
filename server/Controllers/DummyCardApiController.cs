// <copyright file="DummyCardApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing dummy card data.
    /// </summary>
    [Authorize]
    [Route("api/dummycards")]
    [ApiController]
    public class DummyCardApiController : ControllerBase
    {
        private readonly IDummyCardRepository dummyCardRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyCardApiController"/> class.
        /// </summary>
        /// <param name="dummyCardRepository">The dummy card repository dependency.</param>
        public DummyCardApiController(IDummyCardRepository dummyCardRepository)
        {
            this.dummyCardRepository = dummyCardRepository;
        }

        /// <summary>
        /// Deletes a card by card number.
        /// </summary>
        /// <param name="cardNumber">The card number.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpDelete("{cardNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCard(string cardNumber)
        {
            try
            {
                await this.dummyCardRepository.DeleteCardAsync(cardNumber);
                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting card with number {cardNumber}: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the balance of a card by card number.
        /// </summary>
        /// <param name="cardNumber">The card number.</param>
        /// <returns>The card balance.</returns>
        [HttpGet("{cardNumber}/balance")]
        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<double>> GetCardBalance(string cardNumber)
        {
            try
            {
                var balance = await this.dummyCardRepository.GetCardBalanceAsync(cardNumber);
                return this.Ok(balance);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving balance for card with number {cardNumber}: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the balance of a card by card number.
        /// </summary>
        /// <param name="cardNumber">The card number.</param>
        /// <param name="balance">The new balance.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpPut("{cardNumber}/balance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCardBalance(string cardNumber, [FromBody] double balance)
        {
            try
            {
                await this.dummyCardRepository.UpdateCardBalanceAsync(cardNumber, balance);
                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating balance for card with number {cardNumber}: {ex.Message}");
            }
        }
    }
}
// <copyright file="DummyWalletApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing dummy wallet data.
    /// </summary>
    [Authorize]
    [Route("api/wallets")] // Define the base route for wallet operations
    [ApiController]
    public class DummyWalletApiController : ControllerBase
    {
        private readonly IDummyWalletRepository dummyWalletRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyWalletApiController"/> class.
        /// </summary>
        /// <param name="dummyWalletRepository">The dummy wallet repository dependency.</param>
        public DummyWalletApiController(IDummyWalletRepository dummyWalletRepository)
        {
            // Inject the repository implementation (which would be the original DummyWalletRepository on the server)
            this.dummyWalletRepository = dummyWalletRepository ?? throw new ArgumentNullException(nameof(dummyWalletRepository));
        }

        /// <summary>
        /// Asynchronously retrieves the wallet balance for a given user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The wallet balance.</returns>
        [HttpGet("{userId}/balance")]
        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<double>> GetWalletBalance(int userId)
        {
            try
            {
                var balance = await this.dummyWalletRepository.GetWalletBalanceAsync(userId);
                return this.Ok(balance);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving wallet balance for user ID {userId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously updates the wallet balance for a given user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="newBalance">The new balance to set.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        [HttpPut("{userId}/balance")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateWalletBalance(int userId, [FromBody] double newBalance)
        {
            if (newBalance < 0)
            {
                return this.BadRequest("Balance cannot be negative.");
            }

            try
            {
                await this.dummyWalletRepository.UpdateWalletBalance(userId, newBalance);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating wallet balance for user ID {userId}: {ex.Message}");
            }
        }
    }
}

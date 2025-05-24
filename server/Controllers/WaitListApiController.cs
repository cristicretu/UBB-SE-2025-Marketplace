// <copyright file="WaitListApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    /// <summary>
    /// API controller for managing waitlist data.
    /// </summary>
    [Route("api/waitlist")]
    [ApiController]
    public class WaitListApiController : ControllerBase
    {
        private readonly IWaitListRepository waitListRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitListApiController"/> class.
        /// </summary>
        /// <param name="waitListRepository">The waitlist repository dependency.</param>
        public WaitListApiController(IWaitListRepository waitListRepository)
        {
            this.waitListRepository = waitListRepository ?? throw new ArgumentNullException(nameof(waitListRepository));
        }

        /// <summary>
        /// Adds a user to a product waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productWaitListId">The ID of the product waitlist.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        [HttpPost("user/{userId}/product/{productWaitListId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddUserToWaitlist(int userId, int productWaitListId)
        {
            Console.WriteLine($"[API] AddUserToWaitlist endpoint called with userId: {userId}, productWaitListId: {productWaitListId}");

            if (userId <= 0 || productWaitListId <= 0)
            {
                Console.WriteLine($"[API] Invalid parameters: userId={userId}, productWaitListId={productWaitListId}");
                return this.BadRequest("User ID and Product Waitlist ID must be positive integers.");
            }

            try
            {
                Console.WriteLine("[API] Calling repository AddUserToWaitlist method...");
                await this.waitListRepository.AddUserToWaitlist(userId, productWaitListId);
                Console.WriteLine("[API] Repository call completed successfully");
                return this.NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Exception caught: {ex.Message}");
                Console.WriteLine($"[API] Exception type: {ex.GetType().Name}");
                Console.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding user to waitlist: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a user to a product waitlist with a preferred end date.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productWaitListId">The ID of the product waitlist.</param>
        /// <param name="requestData">The request data containing the preferred end date.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        [HttpPost("user/{userId}/product/{productWaitListId}/with-enddate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddUserToWaitlistWithEndDate(int userId, int productWaitListId, [FromBody] AddToWaitlistRequest requestData)
        {
            Console.WriteLine($"[API] AddUserToWaitlistWithEndDate endpoint called with userId: {userId}, productWaitListId: {productWaitListId}, preferredEndDate: {requestData?.PreferredEndDate}");

            if (userId <= 0 || productWaitListId <= 0)
            {
                Console.WriteLine($"[API] Invalid parameters: userId={userId}, productWaitListId={productWaitListId}");
                return this.BadRequest("User ID and Product Waitlist ID must be positive integers.");
            }

            try
            {
                DateTime? preferredEndDate = null;
                if (!string.IsNullOrEmpty(requestData?.PreferredEndDate))
                {
                    if (DateTime.TryParse(requestData.PreferredEndDate, out DateTime parsedDate))
                    {
                        preferredEndDate = parsedDate;
                    }
                }

                Console.WriteLine("[API] Calling repository AddUserToWaitlist method with preferred end date...");
                await this.waitListRepository.AddUserToWaitlist(userId, productWaitListId, preferredEndDate);
                Console.WriteLine("[API] Repository call completed successfully");
                return this.NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Exception caught: {ex.Message}");
                Console.WriteLine($"[API] Exception type: {ex.GetType().Name}");
                Console.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding user to waitlist: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all users in a waitlist for the specified product.
        /// </summary>
        /// <param name="productWaitListId">The ID of the product waitlist.</param>
        /// <returns>A list of users in the waitlist.</returns>
        [HttpGet("product/{productWaitListId}/users")]
        [ProducesResponseType(typeof(List<UserWaitList>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserWaitList>>> GetUsersInWaitlist(int productWaitListId)
        {
            if (productWaitListId <= 0)
            {
                return this.BadRequest("Product Waitlist ID must be a positive integer.");
            }

            try
            {
                var users = await this.waitListRepository.GetUsersInWaitlist(productWaitListId);
                return this.Ok(users);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving users in waitlist: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all users in a waitlist for a given product, ordered by their position.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>An ordered list of users in the waitlist.</returns>
        [HttpGet("product/{productId}/users/ordered")]
        [ProducesResponseType(typeof(List<UserWaitList>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserWaitList>>> GetUsersInWaitlistOrdered(int productId)
        {
            if (productId <= 0)
            {
                return this.BadRequest("Product ID must be a positive integer.");
            }

            try
            {
                var users = await this.waitListRepository.GetUsersInWaitlistOrdered(productId);
                return this.Ok(users);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving ordered users in waitlist: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the position of a user in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The position of the user, or -1 if not found.</returns>
        [HttpGet("user/{userId}/product/{productId}/position")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> GetUserWaitlistPosition(int userId, int productId)
        {
            if (userId <= 0 || productId <= 0)
            {
                return this.BadRequest("User ID and Product ID must be positive integers.");
            }

            try
            {
                var position = await this.waitListRepository.GetUserWaitlistPosition(userId, productId);
                return this.Ok(position);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting user waitlist position: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all waitlists a specific user is part of.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of waitlist entries for the user.</returns>
        [HttpGet("user/{userId}/waitlists")]
        [ProducesResponseType(typeof(List<UserWaitList>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserWaitList>>> GetUserWaitlists(int userId)
        {
            if (userId <= 0)
            {
                return this.BadRequest("User ID must be a positive integer.");
            }

            try
            {
                var waitlists = await this.waitListRepository.GetUserWaitlists(userId);
                return this.Ok(waitlists);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving user waitlists: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the number of users in a product's waitlist.
        /// </summary>
        /// <param name="productWaitListId">The ID of the product waitlist.</param>
        /// <returns>The size of the waitlist.</returns>
        [HttpGet("product/{productWaitListId}/size")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> GetWaitlistSize(int productWaitListId)
        {
            if (productWaitListId <= 0)
            {
                return this.BadRequest("Product Waitlist ID must be a positive integer.");
            }

            try
            {
                var size = await this.waitListRepository.GetWaitlistSize(productWaitListId);
                return this.Ok(size);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting waitlist size: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a user is in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>True if the user is in the waitlist, otherwise false.</returns>
        [HttpGet("user/{userId}/product/{productId}/exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> IsUserInWaitlist(int userId, int productId)
        {
            if (userId <= 0 || productId <= 0)
            {
                return BadRequest("User ID and Product ID must be positive integers.");
            }

            try
            {
                bool isInWaitlist = await waitListRepository.IsUserInWaitlist(userId, productId);
                return Ok(isInWaitlist);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"An error occurred while checking waitlist status: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Removes a user from a product waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productWaitListId">The ID of the product waitlist.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        [HttpDelete("user/{userId}/product/{productWaitListId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveUserFromWaitlist(int userId, int productWaitListId)
        {
            if (userId <= 0 || productWaitListId <= 0)
            {
                return this.BadRequest("User ID and Product Waitlist ID must be positive integers.");
            }

            try
            {
                await this.waitListRepository.RemoveUserFromWaitlist(userId, productWaitListId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while removing user from waitlist: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a specific user's waitlist entry for a product.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The user's waitlist entry or null if not found.</returns>
        [HttpGet("user/{userId}/product/{productId}/entry")]
        [ProducesResponseType(typeof(UserWaitList), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserWaitList?>> GetUserWaitlistEntry(int userId, int productId)
        {
            if (userId <= 0 || productId <= 0)
            {
                return this.BadRequest("User ID and Product ID must be positive integers.");
            }

            try
            {
                var entry = await this.waitListRepository.GetUserWaitlistEntry(userId, productId);
                return this.Ok(entry);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting user waitlist entry: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Request model for adding a user to waitlist with preferred end date.
    /// </summary>
    public class AddToWaitlistRequest
    {
        /// <summary>
        /// Gets or sets the preferred end date as a string.
        /// </summary>
        public string? PreferredEndDate { get; set; }
    }
}
// <copyright file="SellerApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing seller data.
    /// </summary>
    [Authorize]
    [Route("api/sellers")]
    [ApiController]
    public class SellerApiController : ControllerBase
    {
        private readonly ISellerRepository sellerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerApiController"/> class.
        /// </summary>
        /// <param name="sellerRepository">The seller repository dependency.</param>
        public SellerApiController(ISellerRepository sellerRepository)
        {
            this.sellerRepository = sellerRepository;
        }

        /// <summary>
        /// Adds a new follower notification for a seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="currentFollowerCount">The current follower count to record.</param>
        /// <param name="message">The notification message.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPost("{sellerId}/notifications/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddNewFollowerNotification(int sellerId, [FromQuery] int currentFollowerCount, [FromQuery] string message)
        {
            if (sellerId <= 0 || currentFollowerCount < 0 || string.IsNullOrEmpty(message))
            {
                return this.BadRequest("Valid sellerId, non-negative currentFollowerCount, and message are required.");
            }

            try
            {
                await this.sellerRepository.AddNewFollowerNotification(sellerId, currentFollowerCount, message);
                return this.StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding follower notification for Seller ID: {sellerId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new seller profile.
        /// </summary>
        /// <param name="seller">The seller entity to add.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status201Created)] // Use 201 Created for new resource
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddSeller([FromBody] Seller seller)
        {
            // Assuming basic info like UserId are provided in the Seller object
            if (seller == null || seller.Id <= 0)
            {
                return this.BadRequest("Valid Seller entity with at least UserId is required.");
            }

            try
            {
                await this.sellerRepository.AddSeller(seller);
                return this.Created();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding seller. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the last recorded follower count for a seller (from notifications).
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>An ActionResult containing the last follower count.</returns>
        [HttpGet("{sellerId}/last-follower-count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> GetLastFollowerCount(int sellerId)
        {
            if (sellerId <= 0)
            {
                return this.BadRequest("Valid sellerId is required.");
            }

            try
            {
                int count = await this.sellerRepository.GetLastFollowerCount(sellerId);
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting last follower count for Seller ID: {sellerId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets notifications for a specific seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="maxNotifications">The maximum number of notifications to retrieve.</param>
        /// <returns>An ActionResult containing a list of notifications.</returns>
        [HttpGet("{sellerId}/notifications")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<string>>> GetNotifications(int sellerId, [FromQuery] int maxNotifications = 6)
        {
            if (sellerId <= 0 || maxNotifications <= 0)
            {
                return this.BadRequest("Valid sellerId and positive maxNotifications are required.");
            }

            try
            {
                var notifications = await this.sellerRepository.GetNotifications(sellerId, maxNotifications);
                return this.Ok(notifications);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting notifications for Seller ID: {sellerId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets products listed by a specific seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>An ActionResult containing a list of products.</returns>
        [HttpGet("{sellerId}/products")]
        [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Product>>> GetProducts(int sellerId)
        {
            if (sellerId <= 0)
            {
                return this.BadRequest("Valid sellerId is required.");
            }

            try
            {
                var products = await this.sellerRepository.GetProducts(sellerId);
                return this.Ok(products);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting products for Seller ID: {sellerId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets reviews for a specific seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>An ActionResult containing a list of reviews.</returns>
        [HttpGet("{sellerId}/reviews")]
        [ProducesResponseType(typeof(List<Review>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Review>>> GetReviews(int sellerId)
        {
            if (sellerId <= 0)
            {
                return this.BadRequest("Valid sellerId is required.");
            }

            try
            {
                var reviews = await this.sellerRepository.GetReviews(sellerId);
                return this.Ok(reviews);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting reviews for Seller ID: {sellerId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the seller information for a given user ID.
        /// </summary>
        /// <param name="userId">The user ID of the seller.</param>
        /// <returns>An ActionResult containing the seller information.</returns>
        [HttpGet("{userId}/info")]
        [ProducesResponseType(typeof(Seller), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Seller>> GetSellerInfo(int userId)
        {
            try
            {
                var user = new User();
                user.UserId = userId;
                var seller = await this.sellerRepository.GetSellerInfo(user);
                return this.Ok(seller);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting seller info for User ID: {userId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates seller information.
        /// </summary>
        /// <param name="seller">The seller entity with updated information.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateSeller([FromBody] Seller seller)
        {
            if (seller == null || seller.Id <= 0)
            {
                return this.BadRequest("Valid Seller entity with ID is required.");
            }

            try
            {
                await this.sellerRepository.UpdateSeller(seller);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating seller ID: {seller.Id}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the trust score for a seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="score">The new trust score.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPut("{sellerId}/trust-score")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateTrustScore(int sellerId, [FromQuery] double score)
        {
            if (sellerId <= 0)
            {
                return this.BadRequest("Valid sellerId is required.");
            }

            try
            {
                await this.sellerRepository.UpdateTrustScore(sellerId, score);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating trust score for Seller ID: {sellerId}. Error: {ex.Message}");
            }
        }
    }
}

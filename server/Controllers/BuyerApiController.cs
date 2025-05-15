// <copyright file="BuyerApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing buyer data.
    /// </summary>
    [Authorize]
    [Route("api/buyers")]
    [ApiController]
    public class BuyerApiController : ControllerBase
    {
        private readonly IBuyerRepository buyerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerApiController"/> class.
        /// </summary>
        /// <param name="buyerRepository">The buyer repository dependency.</param>
        public BuyerApiController(IBuyerRepository buyerRepository)
        {
            this.buyerRepository = buyerRepository;
        }

        /// <summary>
        /// Checks if a buyer profile exists for a given user ID.
        /// </summary>
        /// <param name="buyerId">The user ID to check.</param>
        /// <returns>An ActionResult containing true if the buyer exists, false otherwise.</returns>
        [HttpGet("{buyerId}/exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CheckIfBuyerExists(int buyerId)
        {
            try
            {
                bool exists = await this.buyerRepository.CheckIfBuyerExists(buyerId);
                return this.Ok(exists);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while checking buyer existence for ID: {buyerId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new buyer profile.
        /// </summary>
        /// <param name="buyerEntity">The buyer entity to create.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateBuyer([FromBody] Buyer buyerEntity)
        {
            if (buyerEntity == null)
            {
                return this.BadRequest("Valid buyer entity is required.");
            }

            try
            {
                await this.buyerRepository.CreateBuyer(buyerEntity);
                return this.Created();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while creating buyer. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a linkage request between two buyers.
        /// </summary>
        /// <param name="requestingBuyerId">The ID of the buyer initiating the request.</param>
        /// <param name="receivingBuyerId">The ID of the buyer receiving the request.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPost("linkages/create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateLinkageRequest([FromQuery] int requestingBuyerId, [FromQuery] int receivingBuyerId)
        {
            if (requestingBuyerId <= 0 || receivingBuyerId <= 0 || requestingBuyerId == receivingBuyerId)
            {
                return this.BadRequest("Valid and distinct requestingBuyerId and receivingBuyerId are required.");
            }

            try
            {
                await this.buyerRepository.CreateLinkageRequest(requestingBuyerId, receivingBuyerId);
                return this.Created();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while creating linkage request. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a linkage request.
        /// </summary>
        /// <param name="requestingBuyerId">The ID of the buyer who initiated the request.</param>
        /// <param name="receivingBuyerId">The ID of the buyer who received the request.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpDelete("linkages/delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteLinkageRequest([FromQuery] int requestingBuyerId, [FromQuery] int receivingBuyerId)
        {
            if (requestingBuyerId <= 0 || receivingBuyerId <= 0)
            {
                return this.BadRequest("Valid requestingBuyerId and receivingBuyerId are required.");
            }

            try
            {
                bool deleted = await this.buyerRepository.DeleteLinkageRequest(requestingBuyerId, receivingBuyerId);
                if (!deleted)
                {
                    return this.NotFound("Linkage request not found.");
                }

                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting linkage request. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds buyers with a specific shipping address.
        /// </summary>
        /// <param name="shippingAddress">The shipping address to search for.</param>
        /// <returns>An ActionResult containing a list of buyers matching the address.</returns>
        [HttpPost("find-by-shipping-address")] // POST because Address object might be complex for GET query params
        [ProducesResponseType(typeof(List<Buyer>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Buyer>>> FindBuyersWithShippingAddress([FromBody] Address shippingAddress)
        {
            if (shippingAddress == null)
            {
                return this.BadRequest("Valid shipping address details are required.");
            }

            try
            {
                var buyers = await this.buyerRepository.FindBuyersWithShippingAddress(shippingAddress);
                return this.Ok(buyers);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while finding buyers by shipping address. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Makes a buyer follow a seller.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="sellerId">The ID of the seller to follow.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPost("{buyerId}/follow/{sellerId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> FollowSeller(int buyerId, int sellerId)
        {
            if (buyerId <= 0 || sellerId <= 0 || buyerId == sellerId)
            {
                return this.BadRequest("Valid and distinct buyerId and sellerId are required.");
            }

            try
            {
                await this.buyerRepository.FollowSeller(buyerId, sellerId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while following seller. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all sellers available in the system.
        /// </summary>
        /// <returns>An ActionResult containing a list of all sellers.</returns>
        [HttpGet("sellers/all")] // Route distinct from user-specific following
        [ProducesResponseType(typeof(List<Seller>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Seller>>> GetAllSellers()
        {
            try
            {
                var sellers = await this.buyerRepository.GetAllSellers();
                return this.Ok(sellers);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting all sellers. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets buyer linkages for a specific buyer.
        /// </summary>
        /// <param name="userId">The ID of the buyer (user).</param>
        /// <returns>An ActionResult containing a list of buyer linkages.</returns>
        [HttpGet("{userId}/linkages")]
        [ProducesResponseType(typeof(List<BuyerLinkage>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<BuyerLinkage>>> GetBuyerLinkages(int userId)
        {
            try
            {
                var linkages = await this.buyerRepository.GetBuyerLinkages(userId);
                return this.Ok(linkages);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting linkages for user ID: {userId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the full Seller details for followed sellers.
        /// </summary>
        /// <param name="sellerIds">The IDs of the sellers.</param>
        /// <returns>An ActionResult containing a list of followed sellers.</returns>
        [HttpPost("followed-sellers")] // POST because List<int> might be complex for GET query params
        [ProducesResponseType(typeof(List<Seller>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Seller>>> GetFollowedSellers([FromBody] List<int> sellerIds)
        {
            try
            {
                if (sellerIds == null)
                {
                    return this.Ok(new List<Seller>()); // Return empty list if not following anyone
                }

                var sellers = await this.buyerRepository.GetFollowedSellers(sellerIds);
                return this.Ok(sellers);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting followed sellers for seller IDs: {sellerIds}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the IDs of users (sellers) that a specific buyer is following.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>An ActionResult containing a list of followed user IDs.</returns>
        [HttpGet("{buyerId}/following/ids")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<int>>> GetFollowingUsersIds(int buyerId)
        {
            try
            {
                var ids = await this.buyerRepository.GetFollowingUsersIds(buyerId);
                return this.Ok(ids);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting following user IDs for buyer ID: {buyerId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets products listed by a specific seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>An ActionResult containing a list of products from the seller.</returns>
        [HttpGet("sellers/{sellerId}/products")]
        [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Product>>> GetProductsFromSeller(int sellerId)
        {
            try
            {
                var products = await this.buyerRepository.GetProductsFromSeller(sellerId);
                return this.Ok(products);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting products for seller ID: {sellerId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the total count of buyers.
        /// </summary>
        /// <returns>An ActionResult containing the total number of buyers.</returns>
        [HttpGet("count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> GetTotalCount()
        {
            try
            {
                int count = await this.buyerRepository.GetTotalCount();
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                // Log the exception ex
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting total buyer count. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the wishlist for a specific buyer.
        /// </summary>
        /// <param name="userId">The ID of the buyer (user).</param>
        /// <returns>An ActionResult containing the buyer's wishlist.</returns>
        [HttpGet("{userId}/wishlist")]
        [ProducesResponseType(typeof(BuyerWishlist), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BuyerWishlist>> GetWishlist(int userId)
        {
            try
            {
                var wishlist = await this.buyerRepository.GetWishlist(userId);
                return this.Ok(wishlist);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while getting wishlist for user ID: {userId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a buyer is following a specific seller.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>An ActionResult containing true if the buyer is following the seller, false otherwise.</returns>
        [HttpGet("{buyerId}/following/check/{sellerId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> IsFollowing(int buyerId, int sellerId)
        {
            try
            {
                bool isFollowing = await this.buyerRepository.IsFollowing(buyerId, sellerId);
                return this.Ok(isFollowing);
            }
            catch (Exception ex)
            {
                // Log the exception ex
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while checking following status. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads buyer information for a given buyer entity (identified by Id).
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>An ActionResult containing the buyer information.</returns>
        [HttpGet("{buyerId}/info")]
        [ProducesResponseType(typeof(Buyer), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Buyer>> LoadBuyerInfo(int buyerId)
        {
            try
            {
                var user = new User { UserId = buyerId };
                var buyer = new Buyer { User = user };
                await this.buyerRepository.LoadBuyerInfo(buyer);

                //// Check if buyer info was actually loaded
                //if (string.IsNullOrEmpty(buyer.FirstName))
                //{
                //    return this.NotFound($"Buyer info not found for ID: {buyerId}");
                //}

                return this.Ok(buyer);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while loading buyer info for ID: {buyerId}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes an item from a buyer's wishlist.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to remove.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpDelete("{buyerId}/wishlist/remove/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RemoveWishlistItem(int buyerId, int productId)
        {
            if (buyerId <= 0 || productId <= 0)
            {
                return this.BadRequest("Valid buyerId and productId are required.");
            }

            try
            {
                await this.buyerRepository.RemoveWishilistItem(buyerId, productId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while removing wishlist item. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves (creates or updates) buyer information.
        /// </summary>
        /// <param name="buyerEntity">The buyer entity to save.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPost("save")] // Using POST for create/update simplicity, could be PUT if ID is known for update
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SaveInfo([FromBody] Buyer buyerEntity)
        {
            if (buyerEntity == null || buyerEntity.Id <= 0)
            {
                return this.BadRequest("Valid Buyer entity with ID is required.");
            }

            try
            {
                await this.buyerRepository.SaveInfo(buyerEntity);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while saving buyer info. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Makes a buyer unfollow a seller.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="sellerId">The ID of the seller to unfollow.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpDelete("{buyerId}/unfollow/{sellerId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UnfollowSeller(int buyerId, int sellerId)
        {
            if (buyerId <= 0 || sellerId <= 0)
            {
                return this.BadRequest("Valid buyerId and sellerId are required.");
            }

            try
            {
                await this.buyerRepository.UnfollowSeller(buyerId, sellerId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while unfollowing seller. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates buyer statistics after a purchase.
        /// </summary>
        /// <param name="buyerEntity">The buyer entity with updated purchase info (TotalSpending, NumberOfPurchases, Badge, Discount).</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPut("update-after-purchase")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateAfterPurchase([FromBody] Buyer buyerEntity)
        {
            if (buyerEntity == null || buyerEntity.Id <= 0)
            {
                return this.BadRequest("Valid Buyer entity with ID is required.");
            }

            try
            {
                await this.buyerRepository.UpdateAfterPurchase(buyerEntity);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating buyer after purchase. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates (approves) a linkage request.
        /// </summary>
        /// <param name="requestingBuyerId">The ID of the buyer who initiated the request.</param>
        /// <param name="receivingBuyerId">The ID of the buyer who received (and is approving) the request.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPut("linkages/update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateLinkageRequest([FromQuery] int requestingBuyerId, [FromQuery] int receivingBuyerId)
        {
            if (requestingBuyerId <= 0 || receivingBuyerId <= 0)
            {
                return this.BadRequest("Valid requestingBuyerId and receivingBuyerId are required.");
            }

            try
            {
                await this.buyerRepository.UpdateLinkageRequest(requestingBuyerId, receivingBuyerId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating linkage request. Error: {ex.Message}");
            }
        }


    }
}

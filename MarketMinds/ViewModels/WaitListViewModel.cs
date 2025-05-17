using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;

namespace MarketMinds.ViewModels
{
    public class WaitListViewModel : IWaitListViewModel
    {
        private readonly IWaitlistService waitlistService;
        private readonly IProductService productService;

        /// <summary>
        /// Default constructor for WaitListViewModel.
        /// </summary>
        /// <param name="waitlistService">The waitlist service.</param>
        /// <param name="productService">The product service.</param>
        /// <remarks>
        /// Initializes a new instance of the WaitListViewModel class with the specified services.
        /// This constructor is typically used in production scenarios with dependency injection.
        /// </remarks>
        public WaitListViewModel(IWaitlistService waitlistService, IProductService productService)
        {
            this.waitlistService = waitlistService;
            this.productService = productService;
        }

        /// <summary>
        /// Adds a user to the waitlist for a specific product.
        /// </summary>
        /// <param name="userId">The ID of the user to be added to the waitlist. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        public async Task AddUserToWaitlist(int userId, int productId)
        {
            await waitlistService.AddUserToWaitlist(userId, productId);
        }

        /// <summary>
        /// Removes a user from the waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user to be removed from the waitlist. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        public async Task RemoveUserFromWaitlist(int userId, int productId)
        {
            await waitlistService.RemoveUserFromWaitlist(userId, productId);
        }

        /// <summary>
        /// Retrieves all users in a waitlist for a given product.
        /// </summary>
        /// <param name="productId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the users in the waitlist.</returns>
        public async Task<List<UserWaitList>> GetUsersInWaitlist(int productId)
        {
            return await waitlistService.GetUsersInWaitlist(productId);
        }

        /// <summary>
        /// Gets all waitlists that a user is part of.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the waitlists the user is part of.</returns>
        public async Task<List<UserWaitList>> GetUserWaitlists(int userId)
        {
            return await waitlistService.GetUserWaitlists(userId);
        }

        /// <summary>
        /// Gets the number of users in a product's waitlist.
        /// </summary>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>The number of users in the waitlist.</returns>
        public async Task<int> GetWaitlistSize(int productId)
        {
            return await waitlistService.GetWaitlistSize(productId);
        }

        /// <summary>
        /// Gets the position of a user in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>The position of the user in the waitlist, or -1 if the user is not in the waitlist.</returns>
        public async Task<int> GetUserWaitlistPosition(int userId, int productId)
        {
            return await waitlistService.GetUserWaitlistPosition(userId, productId);
        }

        /// <summary>
        /// Checks if a user is in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>True if the user is in the waitlist, otherwise false.</returns>
        public async Task<bool> IsUserInWaitlist(int userId, int productId)
        {
            return await waitlistService.IsUserInWaitlist(userId, productId);
        }

        /// <summary>
        /// Gets the name of the seller asynchronously.
        /// </summary>
        /// <param name="sellerId">The ID of the seller. Can be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the name of the seller.</returns>
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            return await productService.GetSellerNameAsync(sellerId);
        }

        /// <summary>
        /// Gets a product by its ID asynchronously.
        /// </summary>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the Product object.</returns>
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await productService.GetProductByIdAsync(productId);
        }
    }
}
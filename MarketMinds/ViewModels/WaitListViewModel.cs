using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;
using Microsoft.Data.SqlClient;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Helper;

namespace MarketPlace924.Services
{
    public class WaitListViewModel : IWaitListViewModel
    {
        private readonly IWaitListRepository waitListModel;
        private readonly IProductRepository productModel;

        /// <summary>
        /// Default constructor for WaitListViewModel.
        /// </summary>
        /// <param name="connectionString">The database connection string. Cannot be null or empty.</param>
        /// <remarks>
        /// Initializes a new instance of the WaitListViewModel class with the specified connection string,
        /// creating new instances of the required model dependencies (WaitListRepository and ProductModel).
        /// This constructor is typically used in production scenarios where real database connections are needed.
        /// </remarks>
        public WaitListViewModel(string connectionString)
        {
            waitListModel = new WaitListProxyRepository(AppConfig.GetBaseApiUrl());
            productModel = new ProductProxyRepository(AppConfig.GetBaseApiUrl());
        }

        /// <summary>
        /// Adds a user to the waitlist for a specific product.
        /// </summary>
        /// <param name="userId">The ID of the user to be added to the waitlist. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task AddUserToWaitlist(int userId, int productId)
        {
            await waitListModel.AddUserToWaitlist(userId, productId);
        }

        /// <summary>
        /// Removes a user from the waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user to be removed from the waitlist. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task RemoveUserFromWaitlist(int userId, int productId)
        {
            await waitListModel.RemoveUserFromWaitlist(userId, productId);
        }

        /// <summary>
        /// Retrieves all users in a waitlist for a given product.
        /// </summary>
        /// <param name="waitListProductId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the users in the waitlist.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<List<UserWaitList>> GetUsersInWaitlist(int productId)
        {
            return await waitListModel.GetUsersInWaitlist(productId);
        }

        /// <summary>
        /// Gets all waitlists that a user is part of.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the waitlists the user is part of.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<List<UserWaitList>> GetUserWaitlists(int userId)
        {
            return await waitListModel.GetUserWaitlists(userId);
        }

        /// <summary>
        /// Gets the number of users in a product's waitlist.
        /// </summary>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>The number of users in the waitlist.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<int> GetWaitlistSize(int productId)
        {
            return await waitListModel.GetWaitlistSize(productId);
        }

        /// <summary>
        /// Gets the position of a user in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>The position of the user in the waitlist, or -1 if the user is not in the waitlist.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<int> GetUserWaitlistPosition(int userId, int productId)
        {
            return await waitListModel.GetUserWaitlistPosition(userId, productId);
        }

        /// <summary>
        /// Checks if a user is in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>True if the user is in the waitlist, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<bool> IsUserInWaitlist(int userId, int productId)
        {
            return await waitListModel.IsUserInWaitlist(userId, productId);
        }

        /// <summary>
        /// Gets the name of the seller asynchronously.
        /// </summary>
        /// <param name="sellerId">The ID of the seller. Can be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the name of the seller.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            return await productModel.GetSellerNameAsync(sellerId);
        }

        /// <summary>
        /// Gets a dummy product by its ID asynchronously.
        /// </summary>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the Product object.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await productModel.GetProductByIdAsync(productId);
        }
    }
}
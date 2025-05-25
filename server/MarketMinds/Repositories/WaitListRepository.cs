// <copyright file="WaitListRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using Microsoft.EntityFrameworkCore;
    using Server.DataAccessLayer;

    /// <summary>
    /// Provides data access functionality for managing waitlists and user waitlist entries.
    /// </summary>
    public class WaitListRepository : IWaitListRepository
    {
        // private readonly string connectionString;
        // private readonly IDatabaseProvider databaseProvider;
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitListRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public WaitListRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Adds a user to the waitlist for a specific product.
        /// OBS: Changed to productId instead of productWaitListId because in code it was used as a productId -Alex.
        /// </summary>
        /// <param name="userId">The ID of the user to be added to the waitlist.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the user or product does not exist.</exception>
        public async Task AddUserToWaitlist(int userId, int productId)
        {
            Console.WriteLine($"[DEBUG] AddUserToWaitlist called with userId: {userId}, productId: {productId}");

            try
            {
                Console.WriteLine($"[DEBUG] Checking database connection state: {this.dbContext.Database.GetDbConnection().State}");

                // Ensure connection is open
                if (this.dbContext.Database.GetDbConnection().State != ConnectionState.Open)
                {
                    Console.WriteLine("[DEBUG] Opening database connection...");
                    await this.dbContext.Database.OpenConnectionAsync();
                    Console.WriteLine($"[DEBUG] Connection state after opening: {this.dbContext.Database.GetDbConnection().State}");
                }

                Console.WriteLine("[DEBUG] Starting buyer existence check...");

                // Check if the Buyer exists with better error handling
                bool buyerExists = false;
                try
                {
                    buyerExists = await this.dbContext.Buyers.AnyAsync(buyer => buyer.Id == userId);
                    Console.WriteLine($"[DEBUG] Buyer exists check completed. Result: {buyerExists}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to check buyer existence: {ex.Message}");
                    Console.WriteLine($"[ERROR] Exception type: {ex.GetType().Name}");
                    Console.WriteLine($"[ERROR] Connection state during error: {this.dbContext.Database.GetDbConnection().State}");
                    Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                    throw new Exception($"Database connection failed while checking buyer existence: {ex.Message}", ex);
                }

                if (!buyerExists)
                {
                    Console.WriteLine($"[DEBUG] No buyer found with id: {userId}");
                    throw new Exception($"AddUserToWaitlist: No Buyer with id: {userId}");
                }

                Console.WriteLine("[DEBUG] Starting product existence check...");

                // Check if the Product exists
                bool productExists = false;
                try
                {
                    productExists = await this.dbContext.BorrowProducts.AnyAsync(product => product.Id == productId);
                    Console.WriteLine($"[DEBUG] Product exists check completed. Result: {productExists}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to check product existence: {ex.Message}");
                    Console.WriteLine($"[ERROR] Connection state during error: {this.dbContext.Database.GetDbConnection().State}");
                    throw new Exception($"Database connection failed while checking product existence: {ex.Message}", ex);
                }

                if (!productExists)
                {
                    Console.WriteLine($"[DEBUG] No product found with id: {productId}");
                    throw new Exception($"AddUserToWaitlist: No Product with id: {productId}");
                }

                Console.WriteLine("[DEBUG] Fetching borrow product details...");

                // Fetch the WaitlistProductEntity with the given productId
                BorrowProduct borrowProduct;
                try
                {
                    borrowProduct = await this.dbContext.BorrowProducts.FirstOrDefaultAsync(waitlistProduct => waitlistProduct.Id == productId);
                    if (borrowProduct == null)
                    {
                        Console.WriteLine($"[DEBUG] No borrow product found with id: {productId}");
                        throw new Exception($"AddUserToWaitlist: No ProductWaitList with id: {productId}");
                    }
                    Console.WriteLine($"[DEBUG] Borrow product fetched successfully. ID: {borrowProduct.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to fetch borrow product: {ex.Message}");
                    Console.WriteLine($"[ERROR] Connection state during error: {this.dbContext.Database.GetDbConnection().State}");
                    throw new Exception($"Database connection failed while fetching borrow product: {ex.Message}", ex);
                }

                Console.WriteLine("[DEBUG] Getting waitlist size...");

                // Get the next position in the queue for the new user
                int positionInQueue;
                try
                {
                    positionInQueue = await this.GetWaitlistSize(borrowProduct.Id) + 1;
                    Console.WriteLine($"[DEBUG] Position in queue calculated: {positionInQueue}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to get waitlist size: {ex.Message}");
                    Console.WriteLine($"[ERROR] Connection state during error: {this.dbContext.Database.GetDbConnection().State}");
                    throw new Exception($"Database connection failed while getting waitlist size: {ex.Message}", ex);
                }

                Console.WriteLine("[DEBUG] Creating new waitlist entry...");

                // Create the new entry in the UserWaitList table
                UserWaitList userWaitListToBeAdded = new UserWaitList
                {
                    UserWaitListID = 0, // This will be auto-generated by the database
                    UserID = userId,
                    ProductWaitListID = borrowProduct.Id,
                    PositionInQueue = positionInQueue,
                    JoinedTime = DateTime.UtcNow,
                };

                try
                {
                    this.dbContext.UserWaitList.Add(userWaitListToBeAdded);
                    Console.WriteLine("[DEBUG] UserWaitList entry added to context");

                    await this.dbContext.SaveChangesAsync();
                    Console.WriteLine("[DEBUG] Changes saved successfully to database");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to save waitlist entry: {ex.Message}");
                    Console.WriteLine($"[ERROR] Connection state during error: {this.dbContext.Database.GetDbConnection().State}");
                    throw new Exception($"Database connection failed while saving waitlist entry: {ex.Message}", ex);
                }

                Console.WriteLine($"[DEBUG] AddUserToWaitlist completed successfully for userId: {userId}, productId: {productId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AddUserToWaitlist failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Final connection state: {this.dbContext.Database.GetDbConnection().State}");
                throw; // Re-throw the exception to maintain the original behavior
            }
        }

        /// <summary>
        /// Adds a user to the waitlist for a specific product with a preferred end date.
        /// </summary>
        /// <param name="userId">The ID of the user to be added to the waitlist.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="preferredEndDate">The user's preferred end date for borrowing.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the user or product does not exist.</exception>
        public async Task AddUserToWaitlist(int userId, int productId, DateTime? preferredEndDate)
        {
            Console.WriteLine($"[DEBUG] AddUserToWaitlist with preferred end date called with userId: {userId}, productId: {productId}, preferredEndDate: {preferredEndDate}");

            try
            {
                Console.WriteLine($"[DEBUG] Checking database connection state: {this.dbContext.Database.GetDbConnection().State}");

                // Ensure connection is open
                if (this.dbContext.Database.GetDbConnection().State != ConnectionState.Open)
                {
                    Console.WriteLine("[DEBUG] Opening database connection...");
                    await this.dbContext.Database.OpenConnectionAsync();
                    Console.WriteLine($"[DEBUG] Connection state after opening: {this.dbContext.Database.GetDbConnection().State}");
                }

                // Check if the Buyer exists
                bool buyerExists = await this.dbContext.Buyers.AnyAsync(buyer => buyer.Id == userId);
                if (!buyerExists)
                {
                    throw new Exception($"AddUserToWaitlist: No Buyer with id: {userId}");
                }

                // Check if the Product exists
                bool productExists = await this.dbContext.BorrowProducts.AnyAsync(product => product.Id == productId);
                if (!productExists)
                {
                    throw new Exception($"AddUserToWaitlist: No Product with id: {productId}");
                }

                // Fetch the BorrowProduct
                BorrowProduct borrowProduct = await this.dbContext.BorrowProducts.FirstOrDefaultAsync(waitlistProduct => waitlistProduct.Id == productId);
                if (borrowProduct == null)
                {
                    throw new Exception($"AddUserToWaitlist: No ProductWaitList with id: {productId}");
                }

                // Get the next position in the queue for the new user
                int positionInQueue = await this.GetWaitlistSize(borrowProduct.Id) + 1;

                // Create the new entry in the UserWaitList table with preferred end date
                UserWaitList userWaitListToBeAdded = new UserWaitList
                {
                    UserWaitListID = 0, // This will be auto-generated by the database
                    UserID = userId,
                    ProductWaitListID = borrowProduct.Id,
                    PositionInQueue = positionInQueue,
                    JoinedTime = DateTime.UtcNow,
                    PreferredEndDate = preferredEndDate
                };

                this.dbContext.UserWaitList.Add(userWaitListToBeAdded);
                await this.dbContext.SaveChangesAsync();

                Console.WriteLine($"[DEBUG] AddUserToWaitlist with preferred end date completed successfully for userId: {userId}, productId: {productId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AddUserToWaitlist with preferred end date failed: {ex.Message}");
                throw; // Re-throw the exception to maintain the original behavior
            }
        }

        /// <summary>
        /// Removes a user from the waitlist and adjusts the queue positions.
        /// </summary>
        /// <param name="userId">The ID of the user to be removed from the waitlist. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the waitlist product entry does not exist.</exception>
        public async Task RemoveUserFromWaitlist(int userId, int productId)
        {
            // Check if the WaitlistProduct exists and get the WaitListProductID
            int waitListProductID = await this.dbContext.BorrowProducts.Where(waitlistProduct => waitlistProduct.Id == productId).Select(waitlistProduct => waitlistProduct.Id).FirstOrDefaultAsync();
            if (waitListProductID == 0)
            {
                throw new Exception($"RemoveUserFromWaitlist: No ProductWaitList with id: {productId}");
            }

            // Check if the UserWaitList exists
            UserWaitList userWaitListToBeRemoved = await this.dbContext.UserWaitList.FirstOrDefaultAsync(userWaitList => userWaitList.UserID == userId && userWaitList.ProductWaitListID == waitListProductID)
                                                        ?? throw new Exception($"RemoveUserFromWaitlist: No UserWaitList with userId: {userId} and productWaitListId: {waitListProductID}");
            this.dbContext.UserWaitList.Remove(userWaitListToBeRemoved);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves all users in a waitlist for a given waitlist product ID.
        /// </summary>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the users in the waitlist.</returns>
        /// <exception cref="Exception">Thrown when there is no ProductWaitList entry with the given productId.</exception>
        public async Task<List<UserWaitList>> GetUsersInWaitlist(int productId)
        {
            // Check if the WaitlistProduct exists and get the WaitListProductID
            int waitListProductID = await this.dbContext.BorrowProducts.Where(waitlistProduct => waitlistProduct.Id == productId).Select(waitlistProduct => waitlistProduct.Id).FirstOrDefaultAsync();
            if (waitListProductID == 0)
            {
                throw new Exception($"GetUsersInWaitlist: No ProductWaitList with id: {productId}");
            }

            List<UserWaitList> usersInWaitlist = await this.dbContext.UserWaitList.Where(userWaitList => userWaitList.ProductWaitListID == waitListProductID).ToListAsync();
            return usersInWaitlist;
        }

        /// <summary>
        /// Gets all waitlists that a user is part of.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the waitlists the user is part of.</returns>
        public async Task<List<UserWaitList>> GetUserWaitlists(int userId)
        {
            List<UserWaitList> userWaitlists = await this.dbContext.UserWaitList.Where(userWaitList => userWaitList.UserID == userId).ToListAsync();
            return userWaitlists;
        }

        /// <summary>
        /// Gets the number of users in a product's waitlist.
        /// </summary>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>The number of users in the waitlist.</returns>
        /// <exception cref="Exception">Thrown when there is no ProductWaitList entry with the given productId.</exception>
        public async Task<int> GetWaitlistSize(int productId)
        {
            // Check if the WaitlistProduct exists and get the WaitListProductID
            int waitListProductID = await this.dbContext.BorrowProducts.Where(waitlistProduct => waitlistProduct.Id == productId).Select(waitlistProduct => waitlistProduct.Id).FirstOrDefaultAsync();
            if (waitListProductID == 0)
            {
                throw new Exception($"GetWaitlistSize: No ProductWaitList with id: {productId}");
            }

            // Get the number of users in the waitlist
            int waitlistSize = await this.dbContext.UserWaitList.CountAsync(userWaitList => userWaitList.ProductWaitListID == waitListProductID);
            return waitlistSize;
        }

        /// <summary>
        /// Checks if a user is in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>True if the user is in the waitlist, otherwise false.</returns>
        /// <exception cref="Exception">Thrown when there is no ProductWaitList entry with the given productId.</exception>
        public async Task<bool> IsUserInWaitlist(int userId, int productId)
        {
            // Check if the WaitlistProduct exists and get the WaitListProductID
            int waitListProductID = await this.dbContext.BorrowProducts.Where(waitlistProduct => waitlistProduct.Id == productId).Select(waitlistProduct => waitlistProduct.Id).FirstOrDefaultAsync();
            if (waitListProductID == 0)
            {
                throw new Exception($"IsUserInWaitlist: No ProductWaitList with id: {productId}");
            }

            // Check if UserWaitList entry exists with the given userId and fetched waitListProductID
            bool isInWaitlist = await this.dbContext.UserWaitList.AnyAsync(userWaitList => userWaitList.UserID == userId && userWaitList.ProductWaitListID == waitListProductID);
            return isInWaitlist;
        }

        /// <summary>
        /// Gets the position of a user in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>The position of the user in the waitlist, or -1 if the user is not in the waitlist.</returns>
        /// <exception cref="Exception">Thrown when there is no ProductWaitList entry with the given productId.</exception>
        public async Task<int> GetUserWaitlistPosition(int userId, int productId)
        {
            // Check if the WaitlistProduct exists and get the WaitListProductID
            int waitListProductID = await this.dbContext.BorrowProducts.Where(waitlistProduct => waitlistProduct.Id == productId).Select(waitlistProduct => waitlistProduct.Id).FirstOrDefaultAsync();
            if (waitListProductID == 0)
            {
                throw new Exception($"GetUserWaitlistPosition: No ProductWaitList with id: {productId}");
            }

            // Get the position of the user in the waitlist
            int positionInQueue = await this.dbContext.UserWaitList.Where(userWaitList => userWaitList.UserID == userId && userWaitList.ProductWaitListID == waitListProductID).Select(userWaitList => userWaitList.PositionInQueue).FirstOrDefaultAsync();
            return positionInQueue != 0 ? positionInQueue : -1;
        }

        /// <summary>
        /// Retrieves all users in a waitlist for a given product, ordered by their position in the queue.
        /// </summary>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the users in the waitlist, ordered by their position in the queue.</returns>
        /// <exception cref="Exception">Thrown when there is no ProductWaitList entry with the given productId.</exception>
        public async Task<List<UserWaitList>> GetUsersInWaitlistOrdered(int productId)
        {
            try
            {
                List<UserWaitList> usersInWaitlist = await this.GetUsersInWaitlist(productId);
                List<UserWaitList> orderedUsers = usersInWaitlist.OrderBy(userWaitList => userWaitList.PositionInQueue).ToList();
                return orderedUsers;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetUsersInWaitlistOrdered: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a specific user's waitlist entry for a product.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>The user's waitlist entry or null if not found.</returns>
        /// <exception cref="Exception">Thrown when there is no ProductWaitList entry with the given productId.</exception>
        public async Task<UserWaitList?> GetUserWaitlistEntry(int userId, int productId)
        {
            // Check if the WaitlistProduct exists and get the WaitListProductID
            int waitListProductID = await this.dbContext.BorrowProducts.Where(waitlistProduct => waitlistProduct.Id == productId).Select(waitlistProduct => waitlistProduct.Id).FirstOrDefaultAsync();
            if (waitListProductID == 0)
            {
                throw new Exception($"GetUserWaitlistEntry: No ProductWaitList with id: {productId}");
            }

            // Get the waitlist entry for the user
            UserWaitList? userWaitlistEntry = await this.dbContext.UserWaitList.FirstOrDefaultAsync(userWaitList => userWaitList.UserID == userId && userWaitList.ProductWaitListID == waitListProductID);
            return userWaitlistEntry;
        }
    }
}
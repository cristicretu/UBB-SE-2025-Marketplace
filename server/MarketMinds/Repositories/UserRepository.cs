// -----------------------------------------------------------------------
// <copyright file="UserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Server.Repository
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Provides methods for interacting with the Users table in the database.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context to be used by the repository.</param>
        public UserRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Connects to the database and adds a new user.
        /// </summary>
        /// <param name="user">The user to be added to the database.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task AddUser(User user)
        {
            this.dbContext.Users.Add(user);
            await this.dbContext.SaveChangesAsync();

            switch (user.Role)
            {
                case UserRole.Buyer:
                    Buyer buyer = new Buyer
                    {
                        User = user,
                    };
                    this.dbContext.Buyers.Add(buyer);
                    break;
                case UserRole.Seller:
                    Seller seller = new Seller(user);
                    this.dbContext.Sellers.Add(seller);
                    break;
            }

            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a user from the database by their ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation, with a result of the user if found, or null if not found.</returns>
        public async Task<User?> GetUserById(int id)
        {
            return await this.dbContext.Users.FirstOrDefaultAsync(user => user.UserId == id);
        }

        /// <summary>
        /// Retrieves a user from the database by their username. If no user is found, returns null.
        /// </summary>
        /// <param name="username">The username of the user that we search for.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation, with a result of the user if found, or null if not found.</returns>
        public async Task<User?> GetUserByUsername(string username)
        {
            return await this.dbContext.Users.FirstOrDefaultAsync(user => user.Username == username);
        }

        /// <summary>
        /// Updates the failed login count for a specified user.
        /// </summary>
        /// <param name="user">The user whose failed login count is to be updated.</param>
        /// <param name="newValueOfFailedLogIns">The new value for the failed login count.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the user is not found.</exception>
        public async Task UpdateUserFailedLoginsCount(User user, int newValueOfFailedLogIns)
        {
            // Find the user by their primary key (UserId)
            User userToUpdate = await this.dbContext.Users.FindAsync(user.UserId)
                                    ?? throw new Exception("UpdateUserFailedLoginsCount: User not found");

            // Update the property on the tracked entity
            userToUpdate.FailedLogins = newValueOfFailedLogIns;

            // EF Core automatically tracks the change, just save it
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the details of an existing user in the database. The user is identified by their user ID and the other fields are updated with the values from the user object.
        /// </summary>
        /// <param name="user">The user object containing updated information.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task UpdateUser(User user)
        {
            this.dbContext.Users.Update(user);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a user from the database by their email address. If no user is found, returns null.
        /// </summary>
        /// <param name="email">The email address of the user to search for.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation. The task result contains the user if found or null if no user is found with the specified email address.</returns>
        public async Task<User?> GetUserByEmail(string email)
        {
            Debug.WriteLine("MAMA \n MAMA \n MAMA \n MAMA \n");
            return await this.dbContext.Users.FirstOrDefaultAsync(user => user.Email == email);
        }

        /// <summary>
        /// Checks if an email address already exists in the database.
        /// </summary>
        /// <param name="email">The email address to check for existence.</param>
        /// <returns>
        /// A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the email exists, otherwise false.
        /// </returns>
        public async Task<bool> EmailExists(string email)
        {
            return await this.dbContext.Users.AnyAsync(user => user.Email == email);
        }

        /// <summary>
        /// Checks if a username already exists in the database.
        /// </summary>
        /// <param name="username">The username to check for existence.</param>
        /// <returns>
        /// A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the username exists, otherwise false.
        /// </returns>
        public async Task<bool> UsernameExists(string username)
        {
            return await this.dbContext.Users.AnyAsync(user => user.Username == username);
        }

        /// <summary>
        /// Retrieves the count of failed login attempts for a specified user by their user ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// A <see cref="Task{int}"/> representing the result of the asynchronous operation.
        /// The task result contains the count of failed login attempts for the specified user.
        /// </returns>
        public async Task<int> GetFailedLoginsCountByUserId(int userId)
        {
            return await this.dbContext.Users
                .Where(user => user.UserId == userId)
                .Select(user => user.FailedLogins)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Updates the contact information (phone number) of an existing user in the database.
        /// </summary>
        /// <param name="user">The user object containing the updated phone number.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the result of the asynchronous operation.
        /// </returns>
        public async Task UpdateUserPhoneNumber(User user)
        {
            User? foundUser = await this.dbContext.Users.FirstOrDefaultAsync(foundUser => foundUser.UserId == user.UserId);
            if (foundUser != null)
            {
                foundUser.PhoneNumber = user.PhoneNumber;
                await this.dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Loads the contact information (phone number and email) of a user from the database by their user ID.
        /// </summary>
        /// <param name="user">The user object to populate with the contact information.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the result of the asynchronous operation.
        /// </returns>
        public async Task LoadUserPhoneNumberAndEmailById(User user)
        {
            User? foundUser = await this.dbContext.Users.FirstOrDefaultAsync(foundUser => foundUser.UserId == user.UserId);
            if (foundUser != null)
            {
                user.PhoneNumber = foundUser.PhoneNumber;
                user.Email = foundUser.Email;
            }
        }

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{List{User}}"/> representing the result of the asynchronous operation.
        /// The task result contains a list of all users in the database.
        /// </returns>
        public async Task<List<User>> GetAllUsers()
        {
            return await this.dbContext.Users.ToListAsync();
        }

        /// <summary>
        /// Retrieves the total number of users in the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{int}"/> representing the result of the asynchronous operation.
        /// The task result contains the total number of users in the database.
        /// </returns>
        public async Task<int> GetTotalNumberOfUsers()
        {
            return await this.dbContext.Users.CountAsync();
        }

        public Task<string> AuthorizationLogin()
        {
            throw new NotImplementedException();
        }
    }
}

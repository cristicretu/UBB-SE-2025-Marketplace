using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Server.DataAccessLayer;
using System.Diagnostics;

namespace Server.MarketMinds.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepository> _logger;
        private const int BuyerTypeValue = 1;
        private const int BaseBalance = 0;
        private const int BaseRating = 0;

        public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(user => user.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(user => user.Email == email);
        }

        public async Task<User> RegisterUserAsync(string username, string email, string passwordHash)
        {
            try
            {
                var user = new User(username, email, passwordHash)
                {
                    Balance = BaseBalance,
                    Rating = BaseRating,
                    UserType = BuyerTypeValue
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user.");
                throw;
            }
        }

        public async Task<User> FindUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Username == username);
        }



        // merge-nicusor
        /// <summary>
        /// Connects to the database and adds a new user.
        /// </summary>
        /// <param name="user">The user to be added to the database.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task AddUser(User user)
        {
            this._context.Users.Add(user);
            await this._context.SaveChangesAsync();

            switch (user.UserType)
            {
                case (int)UserRole.Buyer:
                    Buyer buyer = new Buyer
                    {
                        User = user,
                    };
                    this._context.Buyers.Add(buyer);
                    break;
                case (int)UserRole.Seller:
                    Seller seller = new Seller(user);
                    this._context.Sellers.Add(seller);
                    break;
            }

            await this._context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a user from the database by their ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation, with a result of the user if found, or null if not found.</returns>
        public async Task<User?> GetUserById(int id)
        {
            return await this._context.Users.FirstOrDefaultAsync(user => user.Id == id);
        }

        /// <summary>
        /// Retrieves a user from the database by their username. If no user is found, returns null.
        /// </summary>
        /// <param name="username">The username of the user that we search for.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation, with a result of the user if found, or null if not found.</returns>
        public async Task<User?> GetUserByUsername(string username)
        {
            return await this._context.Users.FirstOrDefaultAsync(user => user.Username == username);
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
            User userToUpdate = await this._context.Users.FindAsync(user.Id)
                                    ?? throw new Exception("UpdateUserFailedLoginsCount: User not found");

            // Update the property on the tracked entity
            userToUpdate.FailedLogIns = newValueOfFailedLogIns;

            // EF Core automatically tracks the change, just save it
            await this._context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the details of an existing user in the database. The user is identified by their user ID and the other fields are updated with the values from the user object.
        /// </summary>
        /// <param name="user">The user object containing updated information.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task UpdateUser(User user)
        {
            this._context.Users.Update(user);
            await this._context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a user from the database by their email address. If no user is found, returns null.
        /// </summary>
        /// <param name="email">The email address of the user to search for.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation. The task result contains the user if found or null if no user is found with the specified email address.</returns>
        public async Task<User?> GetUserByEmail(string email)
        {
            Debug.WriteLine("MAMA \n MAMA \n MAMA \n MAMA \n");
            return await this._context.Users.FirstOrDefaultAsync(user => user.Email == email);
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
            return await this._context.Users.AnyAsync(user => user.Email == email);
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
            return await this._context.Users.AnyAsync(user => user.Username == username);
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
            return await this._context.Users
                .Where(user => user.Id == userId)
                .Select(user => user.FailedLogIns)
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
            User? foundUser = await this._context.Users.FirstOrDefaultAsync(foundUser => foundUser.Id == user.Id);
            if (foundUser != null)
            {
                foundUser.PhoneNumber = user.PhoneNumber;
                await this._context.SaveChangesAsync();
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
            User? foundUser = await this._context.Users.FirstOrDefaultAsync(foundUser => foundUser.Id == user.Id);
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
            return await this._context.Users.ToListAsync();
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
            return await this._context.Users.CountAsync();
        }

        public Task<string> AuthorizationLogin()
        {
            throw new NotImplementedException();
        }
    }
} 
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IUserRepository
    {
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<User> RegisterUserAsync(string username, string email, string passwordHash);
        Task<User> FindUserByUsernameAsync(string username);
        
        // merge-nicusor
        /// <summary>
        /// Connects to the database and adds a new user.
        /// </summary>
        /// <param name="user">The user to be added to the database.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task AddUser(User user);

        /// <summary>
        /// Returns a user form the database by their id. If no user is found, returns null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<User?> GetUserById(int id);

        /// <summary>
        /// Retrieves a user from the database by their username. If no user is found, returns null.
        /// </summary>
        /// <param name="username">The username of the user that we search for.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation, with a result of the user if found, or null if not found.</returns>
        Task<User?> GetUserByUsername(string username);

        /// <summary>
        /// Updates the failed login count for a specified user.
        /// </summary>
        /// <param name="user">The user whose failed login count is to be updated.</param>
        /// <param name="newValueOfFailedLogIns">The new value for the failed login count.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateUserFailedLoginsCount(User user, int newValueOfFailedLogIns);

        /// <summary>
        /// Updates the details of an existing user in the database. The user is identified by their user ID and the other fields are updated with the values from the user object.
        /// </summary>
        /// <param name="user">The user object containing updated information.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateUser(User user);

        /// <summary>
        /// Retrieves a user from the database by their email address. If no user is found, returns null.
        /// </summary>
        /// <param name="email">The email address of the user to search for.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation. The task result contains the user if found or null if no user is found with the specified email address.</returns>
        Task<User?> GetUserByEmail(string email);

        /// <summary>
        /// Checks if an email address already exists in the database.
        /// </summary>
        /// <param name="email">The email address to check for existence.</param>
        /// <returns>
        /// A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the email exists, otherwise false.
        /// </returns>
        Task<bool> EmailExists(string email);

        /// <summary>
        /// Checks if a username already exists in the database.
        /// </summary>
        /// <param name="username">The username to check for existence.</param>
        /// <returns>
        /// A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the username exists, otherwise false.
        /// </returns>
        Task<bool> UsernameExists(string username);

        /// <summary>
        /// Retrieves the count of failed login attempts for a specified user by their user ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// A <see cref="Task{int}"/> representing the result of the asynchronous operation.
        /// The task result contains the count of failed login attempts for the specified user.
        /// </returns>
        Task<int> GetFailedLoginsCountByUserId(int userId);

        /// <summary>
        /// Updates the contact information (phone number) of an existing user in the database.
        /// </summary>
        /// <param name="user">The user object containing the updated phone number.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the result of the asynchronous operation.
        /// </returns>
        Task UpdateUserPhoneNumber(User user);

        /// <summary>
        /// Loads the contact information (phone number and email) of a user from the database by their user ID.
        /// </summary>
        /// <param name="user">The user object to populate with the contact information.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the result of the asynchronous operation.
        /// </returns>
        Task LoadUserPhoneNumberAndEmailById(User user);

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{List{User}}"/> representing the result of the asynchronous operation.
        /// The task result contains a list of all users in the database.
        /// </returns>
        Task<List<User>> GetAllUsers();

        /// <summary>
        /// Retrieves the total number of users in the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{int}"/> representing the result of the asynchronous operation.
        /// The task result contains the total number of users in the database.
        /// </returns>
        Task<int> GetTotalNumberOfUsers();

        Task<string> AuthorizationLogin();
    }
} 
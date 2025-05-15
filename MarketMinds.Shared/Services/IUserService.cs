// <copyright file="IUserService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Service
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Provides operations related to user management.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Hashes a password using SHA256.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password.</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies if the entered captcha matches the generated captcha.
        /// </summary>
        /// <param name="enteredCaptcha">The entered captcha value.</param>
        /// <param name="generatedCaptcha">The generated captcha value.</param>
        /// <returns>True if the entered captcha matches the generated captcha, otherwise false.</returns>
        bool VerifyCaptcha(string enteredCaptcha, string generatedCaptcha);

        /// <summary>
        /// Validates the email format for login.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email format is valid, otherwise false.</returns>
        bool IsEmailValidForLogin(string email);

        /// <summary>
        /// Registers a new user with the provided details.
        /// </summary>
        /// <param name="username">The username of the new user.</param>
        /// <param name="password">The password of the new user.</param>
        /// <param name="email">The email address of the new user.</param>
        /// <param name="phoneNumber">The phone number of the new user.</param>
        /// <param name="role">The role of the new user.</param>
        /// <returns>A <see cref="Task{bool}"/> representing the result of the asynchronous operation. The task result contains true if the user was successfully registered, otherwise false.</returns>
        Task<bool> RegisterUser(string username, string password, string email, string phoneNumber, int role);

        /// <summary>
        /// Checks if a user can log in with the provided email and password.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the user can log in, otherwise false.</returns>
        Task<bool> CanUserLogin(string email, string password);

        /// <summary>
        /// Updates the failed login count for a user.
        /// </summary>
        /// <param name="user">The user whose failed login count is to be updated.</param>
        /// <param name="newValueOfFailedLogIns">The new value for the failed login count.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateUserFailedLoginsCount(User user, int newValueOfFailedLogIns);

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation.
        /// The task result contains the user if found, or null if not found.</returns>
        Task<User?> GetUserByEmail(string email);

        /// <summary>
        /// Retrieves the count of failed login attempts for a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A <see cref="Task{int}"/> representing the result of the asynchronous operation.
        /// The task result contains the count of failed login attempts.</returns>
        Task<int> GetFailedLoginsCountByEmail(string email);

        /// <summary>
        /// Checks if an email address belongs to a user.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the email belongs to a user, otherwise false.</returns>
        Task<bool> IsUser(string email);

        /// <summary>
        /// Checks if a user is suspended based on their email address.
        /// </summary>
        /// <param name="email">The email address of the user to check.</param>
        /// <returns>A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the user is suspended, otherwise false.</returns>
        Task<bool> IsUserSuspended(string email);

        /// <summary>
        /// Suspends a user for a specified number of seconds.
        /// </summary>
        /// <param name="email">The email address of the user to suspend.</param>
        /// <param name="seconds">The number of seconds to suspend the user.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task SuspendUserForSeconds(string email, int seconds);

        /// <summary>
        /// Validates the login credentials and captcha.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="enteredCaptcha">The entered captcha value.</param>
        /// <param name="generatedCaptcha">The generated captcha value.</param>
        /// <returns>A <see cref="Task{string}"/> representing the result of the asynchronous operation.
        /// The task result contains a message indicating the login result.</returns>
        Task<string> ValidateLoginCredentials(string email, string password, string enteredCaptcha, string generatedCaptcha);

        /// <summary>
        /// Handles a failed login attempt for a user.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task HandleFailedLogin(string email);

        /// <summary>
        /// Resets the failed login count for a user.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task ResetFailedLogins(string email);

        /// <summary>
        /// Attempts to log in a user with the provided credentials and captcha.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="enteredCaptcha">The entered captcha value.</param>
        /// <param name="generatedCaptcha">The generated captcha value.</param>
        /// <returns>A <see cref="Task{(bool Success, string Message, User? User)}"/> representing the result of the asynchronous operation.
        /// The task result contains a tuple indicating the success status, a message, and the user object if login is successful.</returns>
        Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password, string enteredCaptcha, string generatedCaptcha);

        /// <summary>
        /// Retrieves all users from the repository.
        /// </summary>
        /// <returns>A <see cref="Task{List{User}}"/> representing the result of the asynchronous operation.
        /// The task result contains a list of all users.</returns>
        Task<List<User>> GetAllUsers();

        Task<string> AuthorizationLogin();
    }
}

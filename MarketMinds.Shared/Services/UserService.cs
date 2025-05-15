// <copyright file="UserService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Service
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using SharedClassLibrary.Helper;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Provides operations related to user management.
    /// </summary>
    public class UserService : IUserService
    {
        private const int MaxFailedLoginAttempts = 5;
        private const int SuspensionDurationInSeconds = 5;
        private IUserRepository userRepository;
        private IUserValidator userValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository to be used by the service.</param>
        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
            this.userValidator = new UserValidator();
        }

        /// <summary>
        /// Hashes a password using SHA256.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password.</returns>
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verifies if the entered captcha matches the generated captcha.
        /// </summary>
        /// <param name="enteredCaptcha">The entered captcha value.</param>
        /// <param name="generatedCaptcha">The generated captcha value.</param>
        /// <returns>True if the entered captcha matches the generated captcha, otherwise false.</returns>
        public bool VerifyCaptcha(string enteredCaptcha, string generatedCaptcha)
        {
            return enteredCaptcha == generatedCaptcha;
        }

        /// <summary>
        /// Validates the email format for login.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email format is valid, otherwise false.</returns>
        public bool IsEmailValidForLogin(string email)
        {
            return this.userValidator.IsValidEmail(email);
        }

        /// <summary>
        /// Registers a new user with the provided details.
        /// </summary>
        /// <param name="username">The username of the new user.</param>
        /// <param name="password">The password of the new user.</param>
        /// <param name="email">The email address of the new user.</param>
        /// <param name="phoneNumber">The phone number of the new user.</param>
        /// <param name="role">The role of the new user.</param>
        /// <returns>A <see cref="Task{bool}"/> representing the result of the asynchronous operation. The task result contains true if the user was successfully registered, otherwise false.</returns>
        public async Task<bool> RegisterUser(string username, string password, string email, string phoneNumber, int role)
        {
            if (!this.userValidator.IsValidUsername(username))
            {
                throw new Exception("Username must be at least 4 characters long.");
            }

            if (!this.userValidator.IsValidEmail(email))
            {
                throw new Exception("Invalid email address format.");
            }

            if (!this.userValidator.IsValidPassword(password))
            {
                throw new Exception("The password must be at least 8 characters long, have at least 1 uppercase letter, at least 1 digit and at least 1 special character.");
            }

            if (!this.userValidator.IsValidPhoneNumber(phoneNumber))
            {
                throw new Exception("The phone number should start with +40 area code followed by 9 digits.");
            }

            if (role != (int)UserRole.Buyer && role != (int)UserRole.Seller)
            {
                throw new Exception("Please select an account type (Buyer or Seller).");
            }

            if (await this.userRepository.UsernameExists(username))
            {
                throw new Exception("Username already exists.");
            }

            if (await this.userRepository.EmailExists(email))
            {
                throw new Exception("Email is already in use.");
            }

            var hashedPassword = this.HashPassword(password);

            var userRole = (UserRole)role;
            var newUser = new User(0, username, email, phoneNumber, hashedPassword, userRole, 0, null, false);

            await this.userRepository.AddUser(newUser);

            return true;
        }

        /// <summary>
        /// Checks if a user can log in with the provided email and password.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the user can log in, otherwise false.</returns>
        public async Task<bool> CanUserLogin(string email, string password)
        {
            if (await this.userRepository.EmailExists(email))
            {
                var user = await this.GetUserByEmail(email);
                if (user == null)
                {
                    return false;
                }

                if (user.Password.StartsWith("plain:"))
                {
                    return user.Password == "plain:" + password;
                }

                return user.Password == this.HashPassword(password);
            }

            return false;
        }

        /// <summary>
        /// Updates the failed login count for a user.
        /// </summary>
        /// <param name="user">The user whose failed login count is to be updated.</param>
        /// <param name="newValueOfFailedLogIns">The new value for the failed login count.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task UpdateUserFailedLoginsCount(User user, int newValueOfFailedLogIns)
        {
            await this.userRepository.UpdateUserFailedLoginsCount(user, newValueOfFailedLogIns);
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>A <see cref="Task{User?}"/> representing the result of the asynchronous operation.
        /// The task result contains the user if found, or null if not found.</returns>
        public async Task<User?> GetUserByEmail(string email)
        {

            Debug.WriteLine("GAYYY \n GAYAAA \n GAYAAA \n GAYAAA");

            return await this.userRepository.GetUserByEmail(email);
        }

        /// <summary>
        /// Retrieves the count of failed login attempts for a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A <see cref="Task{int}"/> representing the result of the asynchronous operation.
        /// The task result contains the count of failed login attempts.</returns>
        public async Task<int> GetFailedLoginsCountByEmail(string email)
        {
            var user = await this.GetUserByEmail(email);

            if (user is null)
            {
                throw new ArgumentNullException($"{email} is not a user");
            }

            int userId = user.UserId;
            return await this.userRepository.GetFailedLoginsCountByUserId(userId);
        }

        /// <summary>
        /// Checks if an email address belongs to a user.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the email belongs to a user, otherwise false.</returns>
        public async Task<bool> IsUser(string email)
        {
            return await this.userRepository.EmailExists(email);
        }

        /// <summary>
        /// Checks if a user is suspended based on their email address.
        /// </summary>
        /// <param name="email">The email address of the user to check.</param>
        /// <returns>A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the user is suspended, otherwise false.</returns>
        public async Task<bool> IsUserSuspended(string email)
        {
            var user = await this.GetUserByEmail(email);

            if (user is null)
            {
                throw new ArgumentNullException($"{email} is not a user");
            }

            if (user.BannedUntil.HasValue && DateTime.Compare(user.BannedUntil.Value, DateTime.Now) > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Suspends a user for a specified number of seconds.
        /// </summary>
        /// <param name="email">The email address of the user to suspend.</param>
        /// <param name="seconds">The number of seconds to suspend the user.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SuspendUserForSeconds(string email, int seconds)
        {
            var user = await this.GetUserByEmail(email);

            if (user is null)
            {
                throw new ArgumentNullException($"{email} is not a user");
            }

            user.BannedUntil = DateTime.Now.AddSeconds(seconds);
            await this.userRepository.UpdateUser(user);
        }

        /// <summary>
        /// Validates the login credentials and captcha.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="enteredCaptcha">The entered captcha value.</param>
        /// <param name="generatedCaptcha">The generated captcha value.</param>
        /// <returns>A <see cref="Task{string}"/> representing the result of the asynchronous operation.
        /// The task result contains a message indicating the login result.</returns>
        public async Task<string> ValidateLoginCredentials(string email, string password, string enteredCaptcha, string generatedCaptcha)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(enteredCaptcha))
            {
                return "Please fill in all fields.";
            }

            if (enteredCaptcha != generatedCaptcha)
            {
                return "Captcha verification failed.";
            }

            if (!await this.IsUser(email))
            {
                return "Email does not exist.";
            }

            var user = await this.GetUserByEmail(email);

            if (await this.IsUserSuspended(email) && user?.BannedUntil != null)
            {
                TimeSpan remainingTime = user.BannedUntil.Value - DateTime.Now;

                return $"Too many failed attempts. Try again in {remainingTime.Seconds}s";
            }

            if (!await this.CanUserLogin(email, password))
            {
                return "Login failed";
            }

            return "Success";
        }

        /// <summary>
        /// Handles a failed login attempt for a user.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task HandleFailedLogin(string email)
        {
            var user = await this.GetUserByEmail(email);
            if (user == null)
            {
                return;
            }

            int failedAttempts = await this.GetFailedLoginsCountByEmail(email) + 1;
            Debug.WriteLine(failedAttempts);
            await this.UpdateUserFailedLoginsCount(user, failedAttempts);

            if (failedAttempts >= MaxFailedLoginAttempts)
            {
                await this.SuspendUserForSeconds(email, SuspensionDurationInSeconds);
            }
        }

        /// <summary>
        /// Resets the failed login count for a user.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task ResetFailedLogins(string email)
        {
            var user = await this.GetUserByEmail(email);
            if (user != null)
            {
                await this.UpdateUserFailedLoginsCount(user, 0);
            }
        }

        /// <summary>
        /// Attempts to log in a user with the provided credentials and captcha.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="enteredCaptcha">The entered captcha value.</param>
        /// <param name="generatedCaptcha">The generated captcha value.</param>
        /// <returns>A <see cref="Task{(bool Success, string Message, User? User)}"/> representing the result of the asynchronous operation.
        /// The task result contains a tuple indicating the success status, a message, and the user object if login is successful.</returns>
        public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password, string enteredCaptcha, string generatedCaptcha)
        {
            if (string.IsNullOrEmpty(email))
            {
                return (false, "Email cannot be empty!", null);
            }

            if (string.IsNullOrEmpty(password))
            {
                return (false, "Password cannot be empty!", null);
            }

            if (!this.IsEmailValidForLogin(email))
            {
                return (false, "Email does not have the right format!", null);
            }

            if (!this.VerifyCaptcha(enteredCaptcha, generatedCaptcha))
            {
                return (false, "Captcha verification failed.", null);
            }

            if (!await this.IsUser(email))
            {
                return (false, "Email does not exist.", null);
            }

            var user = await this.GetUserByEmail(email);
            if (user == null)
            {
                return (false, "Email does not exist.", null);
            }

            if (user.IsBanned)
            {
                return (false, "User is banned.", null);
            }

            return (true, "Success", user);
        }

        /// <summary>
        /// Retrieves all users from the repository.
        /// </summary>
        /// <returns>A <see cref="Task{List{User}}"/> representing the result of the asynchronous operation.
        /// The task result contains a list of all users.</returns>
        public async Task<List<User>> GetAllUsers()
        {
            return await this.userRepository.GetAllUsers();
        }

        public async Task<string> AuthorizationLogin()
        {
            string token = await this.userRepository.AuthorizationLogin();
            AppConfig.AuthorizationToken = token;

            return token;
        }
    }
}

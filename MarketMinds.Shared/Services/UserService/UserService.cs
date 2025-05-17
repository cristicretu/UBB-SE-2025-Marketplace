using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Helper;
using System.Text;
using System.Security.Cryptography;

namespace MarketMinds.Shared.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly UserProxyRepository repository;
        private readonly JsonSerializerOptions jsonOptions;
        private IUserValidator userValidator;
        private const int MaxFailedLoginAttempts = 5;
        private const int SuspensionDurationInSeconds = 5;

        public UserService(IConfiguration configuration)
        {
            this.userValidator = new UserValidator();
            repository = new UserProxyRepository(configuration);
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
        }

        // merge-nicusor
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

            if (await this.repository.UsernameExists(username))
            {
                throw new Exception("Username already exists.");
            }

            if (await this.repository.EmailExists(email))
            {
                throw new Exception("Email is already in use.");
            }

            var hashedPassword = this.HashPassword(password);

            var userRole = (UserRole)role;
            var newUser = new User(
                username: username,
                email: email,
                phoneNumber: phoneNumber,
                userType: role,
                balance: 0,
                bannedUntil: DateTime.MinValue,
                isBanned: false,
                failedLogins: 0,
                passwordHash: hashedPassword);

            await this.repository.AddUser(newUser);

            return true;
        }

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            try
            {
                await repository.AuthenticateUserRawAsync(username, password);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
                return false;
            }
        }

        public async Task<User> GetUserByCredentialsAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            try
            {
                var json = await repository.AuthenticateUserRawAsync(username, password);
                var sharedUser = JsonSerializer.Deserialize<MarketMinds.Shared.Models.User>(json, jsonOptions);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by credentials: {ex.Message}");
                return null;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }

            try
            {
                var json = await repository.GetUserByUsernameRawAsync(username);
                var sharedUser = JsonSerializer.Deserialize<MarketMinds.Shared.Models.User>(json, jsonOptions);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by username: {ex.Message}");
                return null;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            try
            {
                var json = await repository.GetUserByEmailRawAsync(email);
                var sharedUser = JsonSerializer.Deserialize<MarketMinds.Shared.Models.User>(json, jsonOptions);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by email: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }

            try
            {
                var json = await repository.CheckUsernameRawAsync(username);
                var result = JsonSerializer.Deserialize<UsernameCheckResult>(json, jsonOptions);
                return result.Exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if username is taken: {ex.Message}");
                return true; // Default to true (username taken) if there's an error
            }
        }

        public async Task<User> RegisterUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(user.Username));
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(user.Email));
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(user.Password));
            }

            if (!this.userValidator.IsValidUsername(user.Username))
            {
                throw new Exception("Username must be at least 4 characters long.");
            }

            if (!this.userValidator.IsValidEmail(user.Email))
            {
                throw new Exception("Invalid email address format.");
            }

            if (!this.userValidator.IsValidPassword(user.Password))
            {
                throw new Exception("The password must be at least 8 characters long, have at least 1 uppercase letter, at least 1 digit and at least 1 special character.");
            }

            if (!this.userValidator.IsValidPhoneNumber(user.PhoneNumber))
            {
                throw new Exception("The phone number should start with +40 area code followed by 9 digits.");
            }

            if (user.UserType != (int)UserRole.Buyer && user.UserType != (int)UserRole.Seller)
            {
                throw new Exception("Please select an account type (Buyer or Seller).");
            }

            if (await this.repository.UsernameExists(user.Username))
            {
                throw new Exception("Username already exists.");
            }

            if (await this.repository.EmailExists(user.Email))
            {
                throw new Exception("Email is already in use.");
            }

            try
            {
                // Validate email is not already in use
                try
                {
                    var existingUserByEmail = await GetUserByEmailAsync(user.Email);
                    if (existingUserByEmail != null)
                    {
                        Debug.WriteLine($"[UserService] Email already in use: {user.Email}");
                        return null;
                    }
                }
                catch (Exception)
                {
                    // If we can't check email (API error), continue with registration
                }

                // Validate username is not already taken
                try
                {
                    bool usernameTaken = await IsUsernameTakenAsync(user.Username);
                    if (usernameTaken)
                    {
                        Debug.WriteLine($"[UserService] Username already taken: {user.Username}");
                        return null;
                    }
                }
                catch (Exception)
                {
                    // If we can't check username (API error), continue with registration
                }

                // Create registration request
                var registerRequest = new
                {
                    Username = user.Username,
                    Email = user.Email,
                    Password = user.Password
                };

                var json = await repository.RegisterUserRawAsync(registerRequest);
                var registeredSharedUser = JsonSerializer.Deserialize<MarketMinds.Shared.Models.User>(json, jsonOptions);
                return registeredSharedUser != null ? ConvertToDomainUser(registeredSharedUser) : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UserService] Error registering user: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[UserService] Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be a positive number.", nameof(userId));
            }

            try
            {
                var json = await repository.GetUserByIdRawAsync(userId);
                var sharedUser = JsonSerializer.Deserialize<MarketMinds.Shared.Models.User>(json, jsonOptions);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UserOrder>> GetUserOrdersAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be a positive number.", nameof(userId));
            }

            try
            {
                var json = await repository.GetUserOrdersRawAsync(userId);
                var orders = JsonSerializer.Deserialize<List<UserOrder>>(json, jsonOptions);
                return orders ?? new List<UserOrder>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user orders: {ex.Message}");
                return new List<UserOrder>();
            }
        }

        public async Task<double> GetBasketTotalAsync(int userId, int basketId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be a positive number.", nameof(userId));
            }

            if (basketId <= 0)
            {
                throw new ArgumentException("Basket ID must be a positive number.", nameof(basketId));
            }

            try
            {
                var json = await repository.GetBasketTotalRawAsync(userId, basketId);
                return JsonSerializer.Deserialize<double>(json, jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting basket total: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<Order>> CreateOrderFromBasketAsync(int userId, int basketId, double discountAmount = 0)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be a positive number.", nameof(userId));
            }

            if (basketId <= 0)
            {
                throw new ArgumentException("Basket ID must be a positive number.", nameof(basketId));
            }

            if (discountAmount < 0)
            {
                throw new ArgumentException("Discount amount cannot be negative.", nameof(discountAmount));
            }

            try
            {
                var json = await repository.CreateOrderFromBasketRawAsync(userId, basketId, discountAmount);
                var orders = JsonSerializer.Deserialize<List<Order>>(json, jsonOptions);
                return orders ?? new List<Order>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order from basket: {ex.Message}");
                return new List<Order>();
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            if (user.Id <= 0)
            {
                throw new ArgumentException("User ID must be a positive number.", nameof(user.Id));
            }

            try
            {
                var sharedUser = ConvertToSharedUser(user);
                return await repository.UpdateUserRawAsync(sharedUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return false;
            }
        }

        // Helper methods to convert between domain and shared models
        private User ConvertToDomainUser(MarketMinds.Shared.Models.User sharedUser)
        {
            if (sharedUser == null)
            {
                return null;
            }

            var user = new User
            {
                Id = sharedUser.Id,
                Username = sharedUser.Username,
                Email = sharedUser.Email,
                PasswordHash = sharedUser.PasswordHash,
                Password = sharedUser.Password,
                UserType = sharedUser.UserType,
                Balance = sharedUser.Balance,
            };

            if (sharedUser.GetType().GetProperty("Rating") != null)
            {
                user.Rating = sharedUser.Rating;
            }

            return user;
        }

        private MarketMinds.Shared.Models.User ConvertToSharedUser(User domainUser)
        {
            if (domainUser == null)
            {
                return null;
            }
            var user = new MarketMinds.Shared.Models.User
            {
                Id = domainUser.Id,
                Username = domainUser.Username,
                Email = domainUser.Email,
                PasswordHash = domainUser.PasswordHash,
                Password = domainUser.Password,
                UserType = domainUser.UserType,
                Balance = domainUser.Balance,
            };

            if (domainUser.GetType().GetProperty("Rating") != null)
            {
                user.Rating = domainUser.Rating;
            }

            return user;
        }

        private class UsernameCheckResult
        {
            public bool Exists { get; set; }
        }


        // merge-nicusor
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
        /// Checks if a user can log in with the provided email and password.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the user can log in, otherwise false.</returns>
        public async Task<bool> CanUserLogin(string email, string password)
        {
            if (await this.repository.EmailExists(email))
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
            await this.repository.UpdateUserFailedLoginsCount(user, newValueOfFailedLogIns);
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

            return await this.repository.GetUserByEmail(email);
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

            int userId = user.Id;
            return await this.repository.GetFailedLoginsCountByUserId(userId);
        }

        /// <summary>
        /// Checks if an email address belongs to a user.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>A <see cref="Task{bool}"/> representing the result of the asynchronous operation.
        /// The task result contains true if the email belongs to a user, otherwise false.</returns>
        public async Task<bool> IsUser(string email)
        {
            return await this.repository.EmailExists(email);
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
            await this.repository.UpdateUser(user);
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
            return await this.repository.GetAllUsers();
        }

        public async Task<string> AuthorizationLogin()
        {
            string token = await this.repository.AuthorizationLogin();
            AppConfig.AuthorizationToken = token;

            return token;
        }
    }
}

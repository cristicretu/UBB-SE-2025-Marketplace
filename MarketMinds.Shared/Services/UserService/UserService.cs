using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.IRepository;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly UserProxyRepository repository;
        private readonly JsonSerializerOptions jsonOptions;

        public UserService(IConfiguration configuration)
        {
            repository = new UserProxyRepository(configuration);
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
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

            return new User
            {
                Id = sharedUser.Id,
                Username = sharedUser.Username,
                Email = sharedUser.Email,
                PasswordHash = sharedUser.PasswordHash,
                Password = sharedUser.Password,
                UserType = sharedUser.UserType,
                Balance = sharedUser.Balance,
                Rating = sharedUser.Rating
            };
        }

        private MarketMinds.Shared.Models.User ConvertToSharedUser(User domainUser)
        {
            if (domainUser == null)
            {
                return null;
            }
            return new MarketMinds.Shared.Models.User
            {
                Id = domainUser.Id,
                Username = domainUser.Username,
                Email = domainUser.Email,
                PasswordHash = domainUser.PasswordHash,
                Password = domainUser.Password,
                UserType = domainUser.UserType,
                Balance = domainUser.Balance,
                Rating = domainUser.Rating
            };
        }

        private class UsernameCheckResult
        {
            public bool Exists { get; set; }
        }
    }
}

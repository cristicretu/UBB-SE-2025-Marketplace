using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.ProxyRepository
{
    public class UserProxyRepository : IAccountRepository
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;
        private static readonly int ERROR_CODE = -1;

        public UserProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("API base URL is null or empty");
            }

            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }

            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
            Console.WriteLine($"User Repository initialized with base address: {httpClient.BaseAddress}");

            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
        }

        // Raw data access methods
        public async Task<string> GetUserByIdRawAsync(int userId)
        {
            var response = await httpClient.GetAsync($"account/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetUserOrdersRawAsync(int userId)
        {
            var response = await httpClient.GetAsync($"account/{userId}/orders");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetBasketTotalRawAsync(int userId, int basketId)
        {
            var response = await httpClient.GetAsync($"account/{userId}/basket/{basketId}/total");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CreateOrderFromBasketRawAsync(int userId, int basketId, double discountAmount = 0)
        {
            var orderRequest = new
            {
                UserId = userId,
                BasketId = basketId,
                DiscountAmount = discountAmount
            };

            var response = await httpClient.PostAsJsonAsync($"account/orders", orderRequest);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<bool> UpdateUserRawAsync(User user)
        {
            var response = await httpClient.PutAsJsonAsync($"account/{user.Id}", user);
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<string> AuthenticateUserRawAsync(string username, string password)
        {
            var loginRequest = new
            {
                Username = username,
                Password = password
            };

            var response = await httpClient.PostAsJsonAsync("users/login", loginRequest);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetUserByUsernameRawAsync(string username)
        {
            var response = await httpClient.GetAsync($"users/{username}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetUserByEmailRawAsync(string email)
        {
            var response = await httpClient.GetAsync($"users/by-email/{email}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CheckUsernameRawAsync(string username)
        {
            var response = await httpClient.GetAsync($"users/check-username/{username}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> RegisterUserRawAsync(object registerRequest)
        {
            var response = await httpClient.PostAsJsonAsync("users/register", registerRequest);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // IAccountRepository implementation - these now just call the service methods
        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                var json = await GetUserByIdRawAsync(userId);
                return JsonSerializer.Deserialize<User>(json, jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UserOrder>> GetUserOrdersAsync(int userId)
        {
            try
            {
                var json = await GetUserOrdersRawAsync(userId);
                return JsonSerializer.Deserialize<List<UserOrder>>(json, jsonOptions) ?? new List<UserOrder>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user orders: {ex.Message}");
                return new List<UserOrder>();
            }
        }

        public async Task<double> GetBasketTotalAsync(int userId, int basketId)
        {
            try
            {
                var json = await GetBasketTotalRawAsync(userId, basketId);
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
            try
            {
                var json = await CreateOrderFromBasketRawAsync(userId, basketId, discountAmount);
                return JsonSerializer.Deserialize<List<Order>>(json, jsonOptions) ?? new List<Order>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order from basket: {ex.Message}");
                return new List<Order>();
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                return await UpdateUserRawAsync(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return false;
            }
        }

        // Additional authentication methods (not part of IAccountRepository)
        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                await AuthenticateUserRawAsync(username, password);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating credentials: {ex.Message}");
                return false;
            }
        }

        public async Task<User> GetUserByCredentialsAsync(string username, string password)
        {
            try
            {
                var json = await AuthenticateUserRawAsync(username, password);
                return JsonSerializer.Deserialize<User>(json, jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by credentials: {ex.Message}");
                return null;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                var json = await GetUserByUsernameRawAsync(username);
                return JsonSerializer.Deserialize<User>(json, jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by username: {ex.Message}");
                return null;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                var json = await GetUserByEmailRawAsync(email);
                return JsonSerializer.Deserialize<User>(json, jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by email: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            try
            {
                var json = await CheckUsernameRawAsync(username);
                var result = JsonSerializer.Deserialize<UsernameCheckResult>(json, jsonOptions);
                return result?.Exists ?? true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if username is taken: {ex.Message}");
                return true; // Default to true (username taken) if there's an error
            }
        }

        public async Task<User> RegisterUserAsync(User user)
        {
            try
            {
                // Basic null check - detailed validation in service layer
                if (user == null)
                {
                    return null;
                }

                var registerRequest = new
                {
                    Username = user.Username,
                    Email = user.Email,
                    Password = user.Password
                };

                var json = await RegisterUserRawAsync(registerRequest);
                return JsonSerializer.Deserialize<User>(json, jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering user: {ex.Message}");
                return null;
            }
        }

        private class UsernameCheckResult
        {
            public bool Exists { get; set; }
        }
    }
}

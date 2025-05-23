using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
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
        private const string ApiBaseRoute = "users";
        private const string AuthorizationBaseRoute = "authorization";


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


        // merge-nicusor
        public async Task AddUser(User user)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}", user);
            await this.ThrowOnError(nameof(AddUser), response);
        }

        public async Task<User?> GetUserById(int userId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{userId}");
            await this.ThrowOnError(nameof(GetUserById), response);

            // Check if the response content is empty
            if (response.Content.Headers.ContentLength == 0)
            {
                return null; // No content means no user
            }

            var user = await response.Content.ReadFromJsonAsync<User>();
            return user;
        }

        /// <inheritdoc />
        public async Task<bool> EmailExists(string email)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/email-exists?email={email}");
            await this.ThrowOnError(nameof(EmailExists), response);

            var found = await response.Content.ReadFromJsonAsync<bool>();
            return found;
        }

        /// <inheritdoc />
        public async Task<List<User>> GetAllUsers()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}");
            await this.ThrowOnError(nameof(GetAllUsers), response);

            var users = await response.Content.ReadFromJsonAsync<List<User>>();
            if (users == null)
            {
                users = new List<User>();
            }

            return users;
        }

        /// <inheritdoc />
        public async Task<int> GetFailedLoginsCountByUserId(int userId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/failed-logins-count/{userId}");
            await this.ThrowOnError(nameof(GetFailedLoginsCountByUserId), response);

            var failedLoginsCount = await response.Content.ReadFromJsonAsync<int>();
            return failedLoginsCount;
        }

        /// <inheritdoc />
        public async Task<int> GetTotalNumberOfUsers()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/count");
            await this.ThrowOnError(nameof(GetTotalNumberOfUsers), response);

            var userCount = await response.Content.ReadFromJsonAsync<int>();
            return userCount;
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByEmail(string email)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/email/{email}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // No user found with the given email
            }
            await this.ThrowOnError(nameof(GetUserByEmail), response);

            // Check if the response content is empty
            if (response.Content.Headers.ContentLength == 0)
            {
                return null; // No content means no user
            }

            var user = await response.Content.ReadFromJsonAsync<User>();
            return user;
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByUsername(string username)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/username/{username}");
            await this.ThrowOnError(nameof(GetUserByUsername), response);

            var user = await response.Content.ReadFromJsonAsync<User>();
            return user;
        }

        /// <inheritdoc />
        public async Task LoadUserPhoneNumberAndEmailById(User user)
        {
            int userId = user.Id;
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/phone-email/{user.Id}");
            await this.ThrowOnError(nameof(LoadUserPhoneNumberAndEmailById), response);

            var newUser = await response.Content.ReadFromJsonAsync<User>();
            if (newUser == null)
            {
                throw new InvalidOperationException($"Failed to load user data for UserId: {userId}. The API returned no data.");
            }

            user.PhoneNumber = newUser.PhoneNumber;
            user.Email = newUser.Email;
        }

        /// <inheritdoc />
        public async Task UpdateUser(User user)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}", user);
            await this.ThrowOnError(nameof(UpdateUser), response);
        }

        /// <inheritdoc />
        public async Task UpdateUserFailedLoginsCount(User user, int newValueOfFailedLogIns)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/update-failed-logins/{newValueOfFailedLogIns}", user);
            await this.ThrowOnError(nameof(UpdateUserFailedLoginsCount), response);
        }

        /// <inheritdoc />
        public async Task UpdateUserPhoneNumber(User user)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/update-phone-number", user);
            await this.ThrowOnError(nameof(UpdateUserPhoneNumber), response);
        }

        /// <inheritdoc />
        public async Task<bool> UsernameExists(string username)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/username-exists?username={username}");
            await this.ThrowOnError(nameof(UsernameExists), response);

            var found = await response.Content.ReadFromJsonAsync<bool>();
            return found;
        }

        private async Task ThrowOnError(string methodName, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = response.ReasonPhrase;
                }
                throw new Exception($"{methodName}: {errorMessage}");
            }
        }

        public async Task<string> AuthorizationLogin()
        {
            var response = await this.httpClient.PostAsync($"{AuthorizationBaseRoute}/login", null);
            var token = await response.Content.ReadAsStringAsync();
            return token;
        }
    }
}

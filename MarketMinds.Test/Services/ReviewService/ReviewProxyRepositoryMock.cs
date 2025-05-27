using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.UserService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarketMinds.Test.Services.ReviewService
{
    public class ReviewProxyRepositoryMock : ReviewProxyRepository
    {
        private List<Review> sellerReviews;
        private List<Review> buyerReviews;
        private int nextId = 1;

        public ReviewProxyRepositoryMock() : base(null)
        {
            sellerReviews = new List<Review>();
            buyerReviews = new List<Review>();
        }

        public void SetupSellerReviews(List<Review> reviews)
        {
            sellerReviews = reviews;
        }

        public void SetupBuyerReviews(List<Review> reviews)
        {
            buyerReviews = reviews;
        }

        public string GetReviewsBySellerRaw(int sellerId)
        {
            var filtered = sellerReviews.FindAll(r => r.SellerId == sellerId);
            return JsonSerializer.Serialize(filtered);
        }

        public string GetReviewsByBuyerRaw(int buyerId)
        {
            var filtered = buyerReviews.FindAll(r => r.BuyerId == buyerId);
            return JsonSerializer.Serialize(filtered);
        }

        public string CreateReviewRaw(Review reviewToCreate)
        {
            reviewToCreate.Id = nextId++;
            sellerReviews.Add(reviewToCreate);
            buyerReviews.Add(reviewToCreate);
            return JsonSerializer.Serialize(reviewToCreate);
        }

        public string EditReviewRaw(Review reviewToEdit)
        {
            // Update in seller reviews
            var sellerReview = sellerReviews.Find(r => r.Id == reviewToEdit.Id);
            if (sellerReview != null)
            {
                sellerReviews.Remove(sellerReview);
                sellerReviews.Add(reviewToEdit);
            }

            // Update in buyer reviews
            var buyerReview = buyerReviews.Find(r => r.Id == reviewToEdit.Id);
            if (buyerReview != null)
            {
                buyerReviews.Remove(buyerReview);
                buyerReviews.Add(reviewToEdit);
            }

            return JsonSerializer.Serialize(reviewToEdit);
        }

        public string DeleteReviewRaw(object deleteRequest)
        {
            var reviewId = (int)deleteRequest.GetType().GetProperty("ReviewId").GetValue(deleteRequest);
            var sellerId = (int)deleteRequest.GetType().GetProperty("SellerId").GetValue(deleteRequest);
            var buyerId = (int)deleteRequest.GetType().GetProperty("BuyerId").GetValue(deleteRequest);

            // Remove from seller reviews
            var sellerReview = sellerReviews.Find(r => r.Id == reviewId && r.SellerId == sellerId && r.BuyerId == buyerId);
            if (sellerReview != null)
            {
                sellerReviews.Remove(sellerReview);
            }

            // Remove from buyer reviews
            var buyerReview = buyerReviews.Find(r => r.Id == reviewId && r.SellerId == sellerId && r.BuyerId == buyerId);
            if (buyerReview != null)
            {
                buyerReviews.Remove(buyerReview);
            }

            return "{}";
        }

        // Helper methods to get current state (for assertions)
        public List<Review> GetCurrentSellerReviews()
        {
            return sellerReviews;
        }

        public List<Review> GetCurrentBuyerReviews()
        {
            return buyerReviews;
        }
    }

    public class UserServiceMock : IUserService
    {
        private Dictionary<int, User> users = new Dictionary<int, User>();
        private Dictionary<string, User> usersByEmail = new Dictionary<string, User>();
        private Dictionary<string, User> usersByUsername = new Dictionary<string, User>();
        private Dictionary<string, string> passwordResetTokens = new Dictionary<string, string>();
        private Dictionary<string, int> failedLoginCounts = new Dictionary<string, int>();
        private Dictionary<string, DateTime> suspendedUntil = new Dictionary<string, DateTime>();
        private User currentUser = null;

        public void SetupUsers(List<User> usersList)
        {
            users.Clear();
            usersByEmail.Clear();
            usersByUsername.Clear();
            foreach (var user in usersList)
            {
                users[user.Id] = user;
                if (!string.IsNullOrEmpty(user.Email))
                {
                    usersByEmail[user.Email.ToLower()] = user;
                }
                if (!string.IsNullOrEmpty(user.Username))
                {
                    usersByUsername[user.Username.ToLower()] = user;
                }
            }
        }

        public void SetCurrentUser(User user)
        {
            currentUser = user;
        }

        public Task<User> GetUserByIdAsync(int userId)
        {
            if (users.TryGetValue(userId, out var user))
            {
                return Task.FromResult(user);
            }
            return Task.FromResult<User>(null);
        }

        public Task<bool> AuthenticateUserAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Task.FromResult(false);
            }

            var lowerUsername = username.ToLower();
            if (usersByUsername.TryGetValue(lowerUsername, out var user) &&
                user.Password == HashPassword(password))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<User> GetUserByCredentialsAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Task.FromResult<User>(null);
            }

            var lowerUsername = username.ToLower();
            if (usersByUsername.TryGetValue(lowerUsername, out var user) &&
                user.Password == HashPassword(password))
            {
                return Task.FromResult(user);
            }

            return Task.FromResult<User>(null);
        }

        public Task<User> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Task.FromResult<User>(null);
            }

            var lowerUsername = username.ToLower();
            if (usersByUsername.TryGetValue(lowerUsername, out var user))
            {
                return Task.FromResult(user);
            }

            return Task.FromResult<User>(null);
        }

        public Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Task.FromResult<User>(null);
            }

            var lowerEmail = email.ToLower();
            if (usersByEmail.TryGetValue(lowerEmail, out var user))
            {
                return Task.FromResult(user);
            }

            return Task.FromResult<User>(null);
        }

        public Task<bool> IsUsernameTakenAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(usersByUsername.ContainsKey(username.ToLower()));
        }

        public Task<User> RegisterUserAsync(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email))
            {
                return Task.FromResult<User>(null);
            }

            var lowerEmail = user.Email.ToLower();
            var lowerUsername = user.Username.ToLower();

            if (usersByEmail.ContainsKey(lowerEmail) || usersByUsername.ContainsKey(lowerUsername))
            {
                return Task.FromResult<User>(null);
            }

            user.Id = users.Count > 0 ? users.Keys.Max() + 1 : 1;
            users[user.Id] = user;
            usersByEmail[lowerEmail] = user;
            usersByUsername[lowerUsername] = user;

            return Task.FromResult(user);
        }

        public string HashPassword(string password)
        {
            // Mock implementation - in a real scenario, use a proper hashing algorithm
            return $"hashed_{password}";
        }

        public bool VerifyCaptcha(string enteredCaptcha, string generatedCaptcha)
        {
            return enteredCaptcha == generatedCaptcha;
        }

        public bool IsEmailValidForLogin(string email)
        {
            // Simple validation for email format
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        }

        public Task<bool> RegisterUser(string username, string password, string email, string phoneNumber, int role)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(email))
            {
                return Task.FromResult(false);
            }

            var lowerEmail = email.ToLower();
            var lowerUsername = username.ToLower();

            if (usersByEmail.ContainsKey(lowerEmail) || usersByUsername.ContainsKey(lowerUsername))
            {
                return Task.FromResult(false);
            }

            var newUser = new User
            {
                Id = users.Count > 0 ? users.Keys.Max() + 1 : 1,
                Username = username,
                Password = HashPassword(password),
                Email = email
                // Set other properties as needed
            };

            users[newUser.Id] = newUser;
            usersByEmail[lowerEmail] = newUser;
            usersByUsername[lowerUsername] = newUser;

            return Task.FromResult(true);
        }

        public Task<bool> CanUserLogin(User user, string plainPassword)
        {
            if (user == null || string.IsNullOrEmpty(plainPassword))
            {
                return Task.FromResult(false);
            }

            // Check if user is suspended
            if (IsUserSuspended(user.Email).Result)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(user.Password == HashPassword(plainPassword));
        }

        public Task UpdateUserFailedLoginsCount(User user, int newValueOfFailedLogIns)
        {
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                failedLoginCounts[user.Email.ToLower()] = newValueOfFailedLogIns;
            }
            return Task.CompletedTask;
        }

        public Task<User?> GetUserByEmail(string email)
        {
            return GetUserByEmailAsync(email);
        }

        public Task<int> GetFailedLoginsCountByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Task.FromResult(0);
            }

            var lowerEmail = email.ToLower();
            if (failedLoginCounts.TryGetValue(lowerEmail, out var count))
            {
                return Task.FromResult(count);
            }

            return Task.FromResult(0);
        }

        public Task<bool> IsUser(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(usersByEmail.ContainsKey(email.ToLower()));
        }

        public Task<bool> IsUserSuspended(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Task.FromResult(false);
            }

            var lowerEmail = email.ToLower();
            if (suspendedUntil.TryGetValue(lowerEmail, out var suspendTime))
            {
                return Task.FromResult(DateTime.Now < suspendTime);
            }

            return Task.FromResult(false);
        }

        public Task SuspendUserForSeconds(string email, int seconds)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var lowerEmail = email.ToLower();
                suspendedUntil[lowerEmail] = DateTime.Now.AddSeconds(seconds);
            }
            return Task.CompletedTask;
        }

        public async Task<string> ValidateLoginCredentials(string email, string password, string enteredCaptcha, string generatedCaptcha)
        {
            if (!IsEmailValidForLogin(email))
            {
                return "Invalid email format";
            }

            if (string.IsNullOrEmpty(password))
            {
                return "Password cannot be empty";
            }

            // Check if user exists
            var user = await GetUserByEmail(email);
            if (user == null)
            {
                return "User not found";
            }

            // Check if user is suspended
            if (await IsUserSuspended(email))
            {
                return "Account is temporarily suspended";
            }

            // Check failed login count
            var failedCount = await GetFailedLoginsCountByEmail(email);
            if (failedCount >= 3 && !VerifyCaptcha(enteredCaptcha, generatedCaptcha))
            {
                return "Invalid captcha";
            }

            // Check password
            if (!await CanUserLogin(user, password))
            {
                await HandleFailedLogin(email);
                return "Invalid credentials";
            }

            // Reset failed logins on successful login
            await ResetFailedLogins(email);
            return "Success";
        }

        public async Task HandleFailedLogin(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return;
            }

            var lowerEmail = email.ToLower();
            var user = await GetUserByEmail(email);
            if (user != null)
            {
                var failedCount = await GetFailedLoginsCountByEmail(email);
                failedCount++;
                await UpdateUserFailedLoginsCount(user, failedCount);

                // Suspend after 5 failed attempts
                if (failedCount >= 5)
                {
                    await SuspendUserForSeconds(email, 300); // 5 minutes
                }
            }
        }

        public async Task ResetFailedLogins(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return;
            }

            var user = await GetUserByEmail(email);
            if (user != null)
            {
                await UpdateUserFailedLoginsCount(user, 0);
            }
        }

        public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password, string enteredCaptcha, string generatedCaptcha)
        {
            var result = await ValidateLoginCredentials(email, password, enteredCaptcha, generatedCaptcha);

            if (result == "Success")
            {
                var user = await GetUserByEmail(email);
                return (true, "Login successful", user);
            }

            return (false, result, null);
        }

        public Task<List<User>> GetAllUsers()
        {
            return Task.FromResult(users.Values.ToList());
        }

        public Task<string> AuthorizationLogin()
        {
            // Mock implementation for authorization login
            return Task.FromResult("mock_authorization_token");
        }

        // Methods already implemented in your code
        public Task<string> GetJwtTokenAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return Task.FromResult<string>(null);
            }

            var lowerEmail = email.ToLower();
            if (usersByEmail.TryGetValue(lowerEmail, out var user) && user.Password == password)
            {
                return Task.FromResult($"mock_jwt_token_for_{user.Id}");
            }

            return Task.FromResult<string>(null);
        }

        public Task<User> GetCurrentUserAsync()
        {
            return Task.FromResult(currentUser);
        }

        public Task<bool> CheckEmailExistsAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(usersByEmail.ContainsKey(email.ToLower()));
        }

        public Task<bool> CheckUsernameExistsAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(usersByUsername.ContainsKey(username.ToLower()));
        }

        public Task<bool> ChangePasswordAsync(string email, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                return Task.FromResult(false);
            }

            var lowerEmail = email.ToLower();
            if (usersByEmail.TryGetValue(lowerEmail, out var user) && user.Password == oldPassword)
            {
                user.Password = newPassword;
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<bool> RequestPasswordResetAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Task.FromResult(false);
            }

            var lowerEmail = email.ToLower();
            if (usersByEmail.ContainsKey(lowerEmail))
            {
                // Generate a simple token for testing
                string token = $"reset_token_{Guid.NewGuid()}";
                passwordResetTokens[lowerEmail] = token;
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
            {
                return Task.FromResult(false);
            }

            var lowerEmail = email.ToLower();
            if (usersByEmail.TryGetValue(lowerEmail, out var user) &&
                passwordResetTokens.TryGetValue(lowerEmail, out var storedToken) &&
                token == storedToken)
            {
                user.Password = newPassword;
                passwordResetTokens.Remove(lowerEmail); // Token used, remove it
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }

}
